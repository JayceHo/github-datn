using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DATN_CHUYEN_DE_REVITAPI.TrienKhaiDam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DATN_CHUYEN_DE_REVITAPI
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        // Các field hiển thị trong Schedule
        private static BuiltInParameter[] s_skipParameters = new BuiltInParameter[] { BuiltInParameter.REBAR_BAR_DIAMETER,
            BuiltInParameter.REBAR_NUMBER,  BuiltInParameter.REBAR_ELEM_QUANTITY_OF_BARS, BuiltInParameter.REBAR_ELEM_BAR_SPACING,
        BuiltInParameter.REBAR_ELEM_TOTAL_LENGTH , BuiltInParameter.REBAR_ELEM_LENGTH, BuiltInParameter.REBAR_ELEM_HOST_MARK};

        UIApplication uiapp;
        UIDocument uidoc;
        Autodesk.Revit.ApplicationServices.Application app;
        public Document doc;
        Selection sel;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Lấy về địa chỉ file thực thi bin.debug
            String assemblyFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            String assemplyDirPath = System.IO.Path.GetDirectoryName(assemblyFilePath);

            // Lấy về các thông số ban đầu
            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            app = uiapp.Application;
            doc = uidoc.Document;
            sel = uidoc.Selection;

            // Khởi tạo Form ban đầu
            Window window = new Window();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Height = 600; window.Width = 1000;
            window.Title = "Triển Khai Thép Dầm";
            window.ResizeMode = ResizeMode.NoResize;
            SettingsForm settingsFrm = new SettingsForm(doc, window);
            window.Content = settingsFrm;
            window.ShowDialog();

            try
            {
                // Nếu bấm OK thì thực hiện đoạn code này
                if (Global.IsFormOK == true)
                {
                    // Chọn 1 dầm trong Revit
                    Reference rf = sel.PickObject(ObjectType.Element, new BeamSelectionFilter());
                    Element beam = doc.GetElement(rf);
                    TempVar.Instance.SubNumber = 1;
                    List<XYZ> PickPointList = new List<XYZ>();

                    // Lấy về giá trị Mark của dầm
                    Parameter markBeam = beam.LookupParameter("Mark");
                    string StrmarkBeam = markBeam.AsValueString();
                    if (StrmarkBeam == "")
                    {
                        using (Transaction trans = new Transaction(doc, "SetPara"))
                        {
                            trans.Start();
                            markBeam.Set("D1");
                            trans.Commit();
                        }
                        StrmarkBeam = "D1";
                        
                    }
                    Global.strTenDam = StrmarkBeam;

                    // Tạo Schedule 
                    ICollection<ViewSchedule> schedules = CreateSchedules(uidoc);
                    
                    // Tạo Sheet
                    FilteredElementCollector collector = new FilteredElementCollector(doc); //Create a filter to get all the title block types.
                    collector.OfCategory(BuiltInCategory.OST_TitleBlocks);
                    collector.WhereElementIsElementType();
                    Transaction t = new Transaction(doc, "Create sheet");
                    t.Start();
                    ElementId titleBlockId = collector.First(x => x.Name == Global.strTitleBlock).Id;//Get ElementId of first title block type.
                    ViewSheet newSheet = ViewSheet.Create(doc, titleBlockId);//Create sheet by gotten title block type.
                    newSheet.Name = Global.strSheetName;
                    doc.Regenerate();
                    t.Commit();

                    // Lấy về vị trí trên trái của TitleBlock trong sheet
                    collector = new FilteredElementCollector(doc);//Find titleblock of the newly created sheet.
                    collector.OfCategory(BuiltInCategory.OST_TitleBlocks);
                    collector.OwnedByView(newSheet.Id);
                    XYZ upperLeft = new XYZ();//Declare a XYZ to be used as the upperLeft point of schedule sheet instance to be created.
                    Element titleBlock = collector.FirstElement();
                    BoundingBoxXYZ bbox = titleBlock.get_BoundingBox(newSheet);//Get bounding box of the title block.
                    upperLeft = new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Min.Z);//Get upperLeft (TRÊN TRÁI) point of the bounding box.

                    // Bỏ Schedule vào trong Sheet
                    foreach (ViewSchedule schedule in schedules)
                    {
                        Transaction tx = new Transaction(doc, "Populate sheet"); // Điền vào
                        tx.Start();
                        
                        if (titleBlockId != ElementId.InvalidElementId)//If there is an existing title block.
                        {
                            upperLeft = upperLeft + new XYZ(6.0 / 12.0, -9.5 / 12.0, 0);//Move the point to the postion that is 2 inches right and 2 inches down from the original upperLeft point.
                            ScheduleSheetInstance placedInstance = ScheduleSheetInstance.Create(doc, newSheet.Id, schedule.Id, upperLeft);//Create a new schedule sheet instance that makes the sheet
                        }
                        
                        tx.Commit();
                    }
                    upperLeft = new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Min.Z); // reset lại point trên trái

                    // Bỏ Legend vào Sheet                    
                    List<View> ViewLegendList = new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>()
                .Where(x => x.ViewType == ViewType.Legend).ToList();
                    
                    foreach (var VT in ViewLegendList)
                    {
                        if (VT.Name == Global.strLegend)
                        {
                            Transaction T = new Transaction(doc, "LegendtoSheet");
                            T.Start();
                            upperLeft = upperLeft + new XYZ(12.0 / 12.0, -6.5 / 12.0, 0);
                            Viewport viewportLegend = Viewport.Create(doc, newSheet.Id, VT.Id, upperLeft);
                            
                            T.Commit();
                        }

                    }

                    upperLeft = new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Min.Z); // reset lại point trên trái

                    // Set WorkPlan (Mặt phẳng làm việc)
                    Plane plane = Plane.CreateByNormalAndOrigin(doc.ActiveView.ViewDirection, doc.ActiveView.Origin);
                    Transaction WorkPlane = new Transaction(doc, "workplane");
                    WorkPlane.Start();
                    if (doc.ActiveView.SketchPlane == null)
                    {
                        doc.ActiveView.SketchPlane = SketchPlane.Create(doc, plane);
                    }
                    doc.ActiveView.HideActiveWorkPlane(); 
                    WorkPlane.Commit();

                    // Chọn các điểm làm mặt cắt
                    while (true)
                    {
                        try // đường origin của dầm sẽ ở giữa dầm chừ muốn chọn 3 điểm tự động sẽ suy ra tọa độ X,Y của đường origin mà ra
                        {
                            XYZ PickPoint = sel.PickPoint();
                            PickPointList.Add(PickPoint);
                        }
                        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                        {
                            goto Outloop;
                        }
                    }
                Outloop:
                    // Tạo mặt các ngang
                    using (Transaction tx = new Transaction(doc,"CreateSectionMCN"))
                    {
                        tx.Start();
                        Reference r = Reference.ParseFromStableRepresentation(doc, Settings1.Default.ViewType);
                        ViewCreate ViewDetail = new ViewCreate(PickPointList, r.ElementId, beam, doc);
                        View sectionMCN = ViewDetail.CreateViewSection();
                        // Xác định vị trí để đưa MCN vào sheet
                        upperLeft = upperLeft + new XYZ(2.5 / 12.0, -3.5 / 12.0, 0);
                        Viewport viewportMCN = Viewport.Create(doc, newSheet.Id, sectionMCN.Id, upperLeft);
                        
                        //bool newViewportTypeParameterShowLabel = doc.GetElement(viewportMCN.GetTypeId()).get_Parameter(BuiltInParameter.VIEWPORT_ATTR_SHOW_LABEL).Set(1);
                        FamilySymbol famSym = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Where(q => q.Name == "THXD_View Title").First() as FamilySymbol;
                        // Set viewtitle for viewport
                        bool elementType = doc.GetElement(viewportMCN.GetTypeId()).get_Parameter(BuiltInParameter.VIEWPORT_ATTR_LABEL_TAG).Set(famSym.Id);
                        bool unshowLine  = doc.GetElement(viewportMCN.GetTypeId()).get_Parameter(BuiltInParameter.VIEWPORT_ATTR_SHOW_EXTENSION_LINE).Set(0);
                        tx.Commit();

                        
                    }
                    upperLeft = new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Min.Z); // reset lại point trên trái
                    // Tạo mặt cắt dọc
                    using (Transaction tx = new Transaction(doc, "CreateDetailMCD"))
                    {

                        tx.Start();

                        Reference r = Reference.ParseFromStableRepresentation(doc, Settings1.Default.ViewType);

                        // dùng để lấy về thông tin của các point
                        ViewCreate ViewDetail = new ViewCreate(PickPointList, r.ElementId, beam, doc);// truyền thông số ban đầu cho quá trình tạo view
                        List<View> SectionViewlist = new List<View>();
                        
                        SectionViewlist = ViewDetail.getListView();
                        tx.Commit();

                        // Xác định vị trí để đưa MCD vào sheet
                        upperLeft = upperLeft + new XYZ(3.5 / 12.0, -7.0 / 12.0, 0);
                        XYZ spaceMCD = new XYZ(0, 0, 0);

                        // Tạo DIM và Tag và nét cắt cho view mặt cắt dọc
                        foreach (View view in SectionViewlist)
                        {

                            uidoc.ActiveView = view;
                            tx.Start();

                            DimCreat dimCreat = new DimCreat();
                            dimCreat.CreateDIM(doc, beam, view, ViewDetail.section1, ViewDetail.vecx);
                            TagCreat rebarTag = new TagCreat();
                            rebarTag.TagRebar(app, doc, view);
                            NETCAT NETCAT = new NETCAT();
                            NETCAT.NETCATCreat(doc, beam, view);
                            // Xóa Line trong quá trình tạo mc
                            ElementCategoryFilter LineFilter = new ElementCategoryFilter(BuiltInCategory.OST_Lines);
                            FilteredElementCollector LineCol = new FilteredElementCollector(doc, view.Id).WherePasses(LineFilter);
                            foreach (Element lineEle in LineCol)
                            {
                                doc.Delete(lineEle.Id);
                            }

                            // Đưa cái MCD vào trong sheet
                            upperLeft = upperLeft + spaceMCD;
                            Viewport viewportMCD = Viewport.Create(doc, newSheet.Id, view.Id, upperLeft);
                            spaceMCD = spaceMCD + new XYZ(4.0 / 12.0, 0, 0);
                            tx.Commit();

                        }

                    }
                    uidoc.ActiveView = newSheet;
                }
                
                return Result.Succeeded;
            }
            catch (Exception ex )
            { 
                message = ex.Message;
                return Result.Failed;
            }
            

        }
       
        public bool ShouldSkip(ElementId parameterId)// Kiểm tra ElementID vừa nhập vào có thuộc điều kiện ở trên cùng kh (skipParameter)
        {
            foreach (BuiltInParameter bip in s_skipParameters)
            {
                if (new ElementId(bip) == parameterId)
                    return false;
            }
            return true;
        }
        public ICollection<ViewSchedule> CreateSchedules(UIDocument uiDocument)
        {
            Transaction t = new Transaction(doc, "Create Schedules");
            t.Start();

            List<ViewSchedule> schedules = new List<ViewSchedule>();
            //Create an empty view schedule of rebar category.
            ViewSchedule schedule = ViewSchedule.CreateSchedule(doc, new ElementId(BuiltInCategory.OST_Rebar), ElementId.InvalidElementId);
            schedule.Name = "Thống kê thép dầm " + Global.strTenDam;
            schedules.Add(schedule);

            //Iterate all the schedulable field gotten from the rebar view schedule. Lặp lại tất cả trường có thể lập lịch nhận được từ schedule xem tường.
            foreach (SchedulableField schedulableField in schedule.Definition.GetSchedulableFields())
            {
                //Judge (xét) if the FieldType is ScheduleFieldType.Instance.
                if (schedulableField.FieldType == ScheduleFieldType.Instance)
                {
                    //Get ParameterId of SchedulableField.
                    ElementId parameterId = schedulableField.ParameterId;

                    //If the ParameterId is id of BuiltInParameter.ALL_MODEL_MARK then ignore(bỏ qua) next operation.
                    if (ShouldSkip(parameterId)) // Kiểm tra parameterID vừa nhập vào có trong biến hệ thống có điều kiện kh cần skip
                        continue;

                    //Add a new schedule field to the view schedule by using the SchedulableField as argument of AddField method of Autodesk.Revit.DB.ScheduleDefinition class.
                    ScheduleField field = schedule.Definition.AddField(schedulableField);

                    if (field.ColumnHeading == "Rebar Number")
                    {
                        field.ColumnHeading = "Số hiệu";
                    }
                    else
                    if (field.ColumnHeading == "Bar Length")
                    {
                        field.ColumnHeading = "Chiều dài thanh";
                    }
                    else
                    if (field.ColumnHeading == "Spacing")
                    {
                        field.ColumnHeading = "Khoảng cách thép";
                    }
                    else
                    if (field.ColumnHeading == "Quantity")
                    {
                        field.ColumnHeading = "Số lượng";
                    }
                    else
                    if (field.ColumnHeading == "Total Bar Length")
                    {
                        field.ColumnHeading = "Tổng chiều dài";
                    }
                    else
                    if (field.ColumnHeading == "Bar Diameter")
                    {
                        field.ColumnHeading = "Đường kính";
                    }
                    else
                    if (field.ColumnHeading == "Host Mark")
                    {
                        field.IsHidden = true;
                    }

                    //Judge if the parameterId is a BuiltInParameter.
                    if (Enum.IsDefined(typeof(BuiltInParameter), parameterId.IntegerValue))
                    {
                        BuiltInParameter bip = (BuiltInParameter)parameterId.IntegerValue;
                        //Get the StorageType of BuiltInParameter.
                        StorageType st = doc.get_TypeOfStorage(bip);
                        //if StorageType is String or ElementId, set GridColumnWidth of schedule field to three times of current GridColumnWidth. 
                        //And set HorizontalAlignment property to left.
                        if (st == StorageType.String || st == StorageType.ElementId)
                        {
                            field.GridColumnWidth = 3 * field.GridColumnWidth;
                            field.HorizontalAlignment = ScheduleHorizontalAlignment.Left;
                        }
                        //For other StorageTypes, set HorizontalAlignment property to center.
                        else
                        {
                            field.HorizontalAlignment = ScheduleHorizontalAlignment.Center;
                        }
                    }


                    //Filter the view schedule by volume
                    if (field.ParameterId == new ElementId(BuiltInParameter.HOST_VOLUME_COMPUTED))
                    {
                        double volumeFilterInCubicFt = 0.8 * Math.Pow(3.2808399, 3.0);
                        ScheduleFilter filter = new ScheduleFilter(field.FieldId, ScheduleFilterType.GreaterThan, volumeFilterInCubicFt);
                        schedule.Definition.AddFilter(filter);
                    }

                    //Group and sort the view schedule by type
                    if (field.ParameterId == new ElementId(BuiltInParameter.ELEM_TYPE_PARAM))
                    {
                        ScheduleSortGroupField sortGroupField = new ScheduleSortGroupField(field.FieldId);
                        sortGroupField.ShowHeader = true;
                        schedule.Definition.AddSortGroupField(sortGroupField);
                    }
                }
            }

            t.Commit();
            return schedules;
        }

        public class NETCAT
        {
            public NETCAT() { }
            public void NETCATCreat(Document doc, Element beam, View view)
            {
                // 
                int counNetCat = 0;
                // lọc lấy sàn trong ViewF
                ElementCategoryFilter FloorFil = new ElementCategoryFilter(BuiltInCategory.OST_Floors);
                // Tạo list point kiểm tra
                List<XYZ> LeftPointList = new List<XYZ>();
                List<XYZ> RightPointList = new List<XYZ>();
                List<BoundingBoxContainsPointFilter> BoundLeftPointFilterList = new List<BoundingBoxContainsPointFilter>();
                List<BoundingBoxContainsPointFilter> BoundRightPointFilterList = new List<BoundingBoxContainsPointFilter>();

                XYZ LeftbotPoint = TempVar.Instance.TempPoint - TempVar.Instance.vecy * TempVar.Instance.High - TempVar.Instance.vecx * (TempVar.Instance.Width + GeomUtil.milimeter2Feet(200));
                double offset = GeomUtil.milimeter2Feet(25);
                XYZ p1 = LeftbotPoint - TempVar.Instance.vecx * offset - TempVar.Instance.vecz * offset;
                XYZ p2 = p1 + TempVar.Instance.vecx * offset * 2;
                XYZ p3 = p2 + TempVar.Instance.vecz * offset * 2;
                XYZ p4 = p3 - TempVar.Instance.vecx * offset * 2;
                Line l11 = Line.CreateBound(p1, p2);
                Line l21 = Line.CreateBound(p2, p3);
                Line l31 = Line.CreateBound(p3, p4);
                Line l41 = Line.CreateBound(p4, p1);
                CurveLoop cloop = new CurveLoop();
                cloop.Append(l11); cloop.Append(l21); cloop.Append(l31); cloop.Append(l41);
                Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { cloop }, XYZ.BasisZ, TempVar.Instance.High);
                ElementIntersectsSolidFilter eisFilter = new ElementIntersectsSolidFilter(solid);
                List<Element> LeftFloorCol = new FilteredElementCollector(doc, view.Id).WherePasses(FloorFil).WherePasses(eisFilter).ToList();

                // tạo family detail
                ElementCategoryFilter DetailItemFilter = new ElementCategoryFilter(BuiltInCategory.OST_DetailComponents);
                FilteredElementCollector DetailCol = new FilteredElementCollector(doc).WherePasses(DetailItemFilter).WhereElementIsElementType();

                List<FamilySymbol> symbols = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>().Where(x => x.FamilyName == "HB_NetCat" && x.Name == "TL 1/25").ToList();



                XYZ RightBotPoint = TempVar.Instance.TempPoint - TempVar.Instance.vecy * TempVar.Instance.High + TempVar.Instance.vecx * (TempVar.Instance.Width + GeomUtil.milimeter2Feet(200));
                //XYZ RightTopPoint = TempVar.Instance.TempPoint + TempVar.Instance.vecx * (TempVar.Instance.Width + GeomUtil.milimeter2Feet(200));
                CurveLoop Circle = new CurveLoop();
                List<CurveLoop> curveloops = new List<CurveLoop>();


                Circle.Append(Arc.Create(RightBotPoint, GeomUtil.milimeter2Feet(200), 0, Math.PI, TempVar.Instance.vecx, TempVar.Instance.vecz));
                Circle.Append(Arc.Create(RightBotPoint, GeomUtil.milimeter2Feet(200), Math.PI, 2 * Math.PI, TempVar.Instance.vecx, TempVar.Instance.vecz));
                curveloops.Add(Circle);
                Solid CylinderSolid = GeometryCreationUtilities.CreateExtrusionGeometry(curveloops, XYZ.BasisZ, TempVar.Instance.High);
                ElementIntersectsSolidFilter RightSolidFilter = new ElementIntersectsSolidFilter(CylinderSolid);
                List<Element> RightFloorCol = new FilteredElementCollector(doc, view.Id).WherePasses(FloorFil).WherePasses(RightSolidFilter).ToList();
                //VẼ NÉT CẮT trái
                if (LeftFloorCol.Count() > 0)
                    foreach (Element item in LeftFloorCol)
                    {

                        Floor FloorElement = item as Floor;
                        double FloorElaTop = Double.Parse(FloorElement.LookupParameter("Elevation at Top").AsValueString());

                        double BeamElatop = Double.Parse(beam.LookupParameter("Elevation at Top").AsValueString());
                        // chênh lệch cao độ dầm sàn
                        double delta = BeamElatop - FloorElaTop;
                        double FloorThickness = Double.Parse(FloorElement.LookupParameter("Thickness").AsValueString());

                        // Xác định 4 point 2 bên của dầm cần để Nét Cắt
                        XYZ Point1 = TempVar.Instance.TempPoint - TempVar.Instance.vecx.Normalize() * (GeomUtil.milimeter2Feet(100) + TempVar.Instance.Width / 2) - TempVar.Instance.vecy.Normalize() * GeomUtil.milimeter2Feet(delta);
                        XYZ Point2 = Point1 - TempVar.Instance.vecy.Normalize() * GeomUtil.milimeter2Feet(FloorThickness);
                        //XYZ Point3 = TempVar.Instance.TempPoint + TempVar.Instance.vecx.Normalize() * (GeomUtil.milimeter2Feet(100) + TempVar.Instance.Width / 2) - TempVar.Instance.vecy.Normalize() * GeomUtil.milimeter2Feet(delta);
                        //XYZ Point4 = Point3 - TempVar.Instance.vecy.Normalize() * GeomUtil.milimeter2Feet(FloorThickness);

                        Line l1 = Line.CreateBound(Point1, Point2);
                        var detailLine1 = doc.Create.NewDetailCurve(view, l1);
                        var BaseLine1 = detailLine1.GeometryCurve as Line;
                        //Line l3 = Line.CreateBound(Point4, Point3);
                        //var detailLine3 = doc.Create.NewDetailCurve(view, l3);
                        //var BaseLine3 = detailLine3.GeometryCurve as Line;



                        // kiểm tra point 1 và point3 có này trong Boundingbox của Floor ko

                        Options opt = new Options();
                        opt.ComputeReferences = true;
                        GeometryElement geoElem = FloorElement.get_Geometry(opt);
                        Solid s = null;
                        foreach (GeometryObject FloorGeoObj in geoElem)
                        {
                            s = FloorGeoObj as Solid;
                        }

                        if (!symbols[0].IsActive)
                        {
                            symbols[0].Activate();
                            doc.Regenerate();
                        }


                        FamilyInstance DetailInstance = doc.Create.NewFamilyInstance(BaseLine1, symbols.First(), view);
                        //DetailInstance.ChangeTypeId()
                        counNetCat = counNetCat + 1;



                    }
                // VẼ NÉT CẮT PHẢI
                #region MyRegion


                if (RightFloorCol.Count() > 0)
                    foreach (Element item in RightFloorCol)
                    {
                        Floor FloorElement = item as Floor;
                        double FloorElaTop = Double.Parse(FloorElement.LookupParameter("Elevation at Top").AsValueString());

                        double BeamElatop = Double.Parse(beam.LookupParameter("Elevation at Top").AsValueString());
                        // chênh lệch cao độ dầm sàn
                        double delta = BeamElatop - FloorElaTop;
                        double FloorThickness = Double.Parse(FloorElement.LookupParameter("Thickness").AsValueString());

                        // Xác định 4 point 2 bên của dầm cần để Nét Cắt
                        //XYZ Point1 = TempVar.Instance.TempPoint - TempVar.Instance.vecx.Normalize() * (GeomUtil.milimeter2Feet(100) + TempVar.Instance.Width / 2) - TempVar.Instance.vecy.Normalize() * GeomUtil.milimeter2Feet(delta);
                        //XYZ Point2 = Point1 - TempVar.Instance.vecy.Normalize() * GeomUtil.milimeter2Feet(FloorThickness);
                        XYZ Point3 = TempVar.Instance.TempPoint + TempVar.Instance.vecx.Normalize() * (GeomUtil.milimeter2Feet(100) + TempVar.Instance.Width / 2) - TempVar.Instance.vecy.Normalize() * GeomUtil.milimeter2Feet(delta);
                        XYZ Point4 = Point3 - TempVar.Instance.vecy.Normalize() * GeomUtil.milimeter2Feet(FloorThickness);

                        //Line l1 = Line.CreateBound(Point1, Point2);
                        //var detailLine1 = doc.Create.NewDetailCurve(view, l1);
                        //var BaseLine1 = detailLine1.GeometryCurve as Line;
                        Line l3 = Line.CreateBound(Point4, Point3);
                        var detailLine3 = doc.Create.NewDetailCurve(view, l3);
                        var BaseLine3 = detailLine3.GeometryCurve as Line;



                        // kiểm tra point 1 và point3 có này trong Boundingbox của Floor ko

                        Options opt = new Options();
                        opt.ComputeReferences = true;
                        GeometryElement geoElem = FloorElement.get_Geometry(opt);
                        Solid s = null;
                        foreach (GeometryObject FloorGeoObj in geoElem)
                        {
                            s = FloorGeoObj as Solid;
                        }

                        if (!symbols[0].IsActive)
                        {
                            symbols[0].Activate();
                            doc.Regenerate();
                        }


                        // Point 3 nằm trong sàn

                        FamilyInstance DetailInstance2 = doc.Create.NewFamilyInstance(BaseLine3, symbols.First(), view);
                        counNetCat = counNetCat + 1;
                        #endregion

                    }

            }
        }

        public class TagCreat
        {
            public TagCreat() { }
            public void TagRebar(Autodesk.Revit.ApplicationServices.Application app, Document doc, View view)
            {
                //ElementCategoryFilter RebarCat = new ElementCategoryFilter(BuiltInCategory.OST_Rebar);
                ElementClassFilter RebarClassFIlter = new ElementClassFilter(typeof(Rebar));
                List<Rebar> Rebarcol = new FilteredElementCollector(doc, view.Id).WherePasses(RebarClassFIlter).Cast<Rebar>().ToList(); // lấy về danh sách các rebar có trong view
                //.Cast<Rebar>().Where(x => x.GetHostId().IntegerValue == TempVar.Instance.beam.Id.IntegerValue).ToList();
                int S = 0;
                Double DeltaSingleRebar_bot = 0;
                Double DeltaSingleRebar_top = 0;
                Double DeltaMultiRebar_bot = 0;
                Double DeltaMultiRebar_top = 0;
                
                foreach (Rebar rebar in Rebarcol)
                {

                    if (rebar.Quantity > 1 && rebar.LookupParameter("Style").AsValueString() == "Standard") //Thep chủ số lượng multi
                    {
                        //Trích dẫn danh sách Type của MultiReference
                        ElementClassFilter mFilterRAT = new ElementClassFilter(typeof(MultiReferenceAnnotationType));
                        List<MultiReferenceAnnotationType> mRAT = new FilteredElementCollector(doc).WherePasses(mFilterRAT)
                        .Cast<MultiReferenceAnnotationType>().ToList();


                        MultiReferenceAnnotationOptions Option = new MultiReferenceAnnotationOptions(mRAT[Settings1.Default.MultiStandardRebar]);

                        //xác định tọa độ các điểm cho mulTag

                        //
                        XYZ vecMT = TempVar.Instance.Mid - TempVar.Instance.TempPoint;
                        double Distance = (vecMT).GetLength();
                        if (GeomUtil.IsSameDirection(vecMT, TempVar.Instance.vecz))
                        {
                            Distance = -Distance;
                        }
                        else
                        {
                        }
                        List<Curve> CurveRebarList = rebar.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, 0) as List<Curve>;
                        int MainCurveIndex = 0;
                        for (int i = 0; i < CurveRebarList.Count; i++)
                        {
                            if ((CurveRebarList[i] as Arc) != null)
                            {
                                continue;
                            }
                            Polygon pl = new BeamGeometryInfo(TempVar.Instance.beam as FamilyInstance).CentralVerticalSectionPolygon;
                            pl = GeomUtil.OffsetPolygon(pl, TempVar.Instance.vecz, Distance);
                            LineComparePolygonResult res = new LineComparePolygonResult(pl, CheckGeometry.ConvertLine(CurveRebarList[i]));
                            if (res.Type == LineComparePolygonType.OverlapOrIntersect || res.Type == LineComparePolygonType.PerpendicularIntersectFace
                                || res.Type == LineComparePolygonType.PerpendicularIntersectPlane)
                            {
                                MainCurveIndex = i;
                                goto ExitForLoop2;
                            }
                        }
                    ExitForLoop2:
                        // xác định RebarPoint
                        XYZ RebarPoint = GetIntersection(CheckGeometry.ConvertLine(CurveRebarList[MainCurveIndex]), TempVar.Instance.SectionPlane);

                        //Curve rebarCurve=rebar

                        double DistanceXY = 0;
                        double valX = Distancexy(TempVar.Instance.TempPoint, RebarPoint);
                        Line l = Line.CreateBound(XYZ.Zero, TempVar.Instance.vecx);
                        XYZ vec = CheckGeometry.GetProjectPoint(l, TempVar.Instance.TempPoint) - CheckGeometry.GetProjectPoint(l, RebarPoint);
                        if (valX >= 0)
                        {
                            if (GeomUtil.IsSameDirection(vec, TempVar.Instance.vecx))
                            {
                                if (valX <= TempVar.Instance.Width / 2)
                                {
                                    DistanceXY = TempVar.Instance.Width / 2 - valX;
                                }
                                else
                                {
                                    DistanceXY = TempVar.Instance.Width / 2 - valX;
                                }
                            }
                            else
                            {
                                if (valX <= TempVar.Instance.Width / 2)
                                {
                                    DistanceXY = TempVar.Instance.Width / 2 + valX;
                                }
                                else
                                {
                                    DistanceXY = TempVar.Instance.Width / 2 + valX;
                                }
                            }
                        }

                        /// kiểm tra thép nằm lớp dưới hay lớp trên
                        XYZ HeadPosition;
                        XYZ DimOriginPoint1;
                        XYZ MidBeamPoint = TempVar.Instance.TempPoint - TempVar.Instance.vecy * TempVar.Instance.High / 2;


                        if (RebarPoint.Z - MidBeamPoint.Z > 0) // THÉP NẰM NỬA TRÊN DẦM
                        {
                            HeadPosition = RebarPoint + TempVar.Instance.vecy.Normalize() * GeomUtil.milimeter2Feet(DeltaMultiRebar_top + 120) - TempVar.Instance.vecx.Normalize() * (DistanceXY + GeomUtil.milimeter2Feet(150));
                            //xác định line Origin
                            DimOriginPoint1 = RebarPoint + TempVar.Instance.vecy.Normalize() * GeomUtil.milimeter2Feet(DeltaMultiRebar_top + 120);
                            // 
                            DeltaMultiRebar_top = DeltaMultiRebar_top + 60;
                        }
                        else
                        {
                            HeadPosition = RebarPoint - TempVar.Instance.vecy.Normalize() * GeomUtil.milimeter2Feet(75 + DeltaMultiRebar_bot) - TempVar.Instance.vecx.Normalize() * (DistanceXY + GeomUtil.milimeter2Feet(150));
                            //xác định line Origin
                            DimOriginPoint1 = RebarPoint - TempVar.Instance.vecy.Normalize() * GeomUtil.milimeter2Feet(75 + DeltaMultiRebar_bot);
                            // 
                            DeltaMultiRebar_bot = DeltaMultiRebar_bot + 60;
                        }
                        Option.TagHeadPosition = HeadPosition; ;
                        Option.DimensionLineOrigin = DimOriginPoint1;
                        Option.DimensionLineDirection = TempVar.Instance.vecx;
                        Option.DimensionPlaneNormal = view.ViewDirection;

                        List<ElementId> SubrebarList = new List<ElementId>();
                        for (int i = 0; i < rebar.Quantity; i++)
                        {
                            SubrebarList.Add(rebar.Id);
                        }
                        try
                        {
                            Option.SetElementsToDimension(SubrebarList);
                            MultiReferenceAnnotation.Create(doc, view.Id, Option);
                        }
                        catch
                        {

                            TaskDialog.Show("Cảnh Báo", "Lỗi thép");
                        }
                    }
                    // Trường Hợp Thép Đơn
                    else if (rebar.Quantity == 1 && rebar.LookupParameter("Style").AsValueString() == "Standard")
                    {

                        List<Curve> CurveRebarList = rebar.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, 0) as List<Curve>;
                        int MainCurveIndex = 0;
                        XYZ vecMT = TempVar.Instance.Mid - TempVar.Instance.TempPoint;
                        double Distance = (vecMT).GetLength();
                        if (GeomUtil.IsSameDirection(vecMT, TempVar.Instance.vecz))
                        {
                            Distance = -Distance;
                        }

                        for (int i = 0; i < CurveRebarList.Count; i++)
                        {

                            
                            if ((CurveRebarList[i] as Arc) != null)
                            {
                                continue;
                            }
                            Polygon pl = new BeamGeometryInfo(TempVar.Instance.beam as FamilyInstance).CentralVerticalSectionPolygon;
                            pl = GeomUtil.OffsetPolygon(pl, TempVar.Instance.vecz, Distance);
                            LineComparePolygonResult res = new LineComparePolygonResult(pl, CheckGeometry.ConvertLine(CurveRebarList[i]));
                            if (res.Type == LineComparePolygonType.OverlapOrIntersect || res.Type == LineComparePolygonType.PerpendicularIntersectPlane
                                || res.Type == LineComparePolygonType.PerpendicularIntersectFace)
                            {
                                MainCurveIndex = i;

                                goto ExitForLoop;
                            }

                        }
                    ExitForLoop:

                        // xác định RebarPoint
                        XYZ RebarPoint = GetIntersection(CurveRebarList[MainCurveIndex] as Line, TempVar.Instance.SectionPlane);

                        //Curve rebarCurve = rebar

                        XYZ pnt = rebar.GetShapeDrivenAccessor().ComputeDrivingCurves()[0].GetEndPoint(0);  //rebar.ComputeDrivingCurves()[0].GetEndPoint(0);
                        IndependentTag tag = IndependentTag.Create(doc, view.Id, new Reference(rebar), false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, RebarPoint);

                        tag.LeaderEndCondition = LeaderEndCondition.Free;
                        tag.HasLeader = true;
                        TagCreat tagCreat = new TagCreat();
                        tagCreat.ChangeTag(doc, tag, Settings1.Default.SingleStandardRebar); // đổi tag

                        #region Edit Tag
                        double DistanceXY = 0;
                        double valX = Distancexy(TempVar.Instance.TempPoint, RebarPoint);
                        Line l = Line.CreateBound(XYZ.Zero, TempVar.Instance.vecx);
                        XYZ vec = CheckGeometry.GetProjectPoint(l, TempVar.Instance.TempPoint) - CheckGeometry.GetProjectPoint(l, RebarPoint);
                        if (valX >= 0)
                        {
                            if (GeomUtil.IsSameDirection(vec, TempVar.Instance.vecx))
                            {
                                if (valX <= TempVar.Instance.Width / 2)
                                {
                                    DistanceXY = TempVar.Instance.Width / 2 - valX;
                                }
                                else
                                {
                                    DistanceXY = TempVar.Instance.Width / 2 - valX;
                                }
                            }
                            else
                            {
                                if (valX <= TempVar.Instance.Width / 2)
                                {
                                    DistanceXY = TempVar.Instance.Width / 2 + valX;
                                }
                                else
                                {
                                    DistanceXY = TempVar.Instance.Width / 2 + valX;
                                }
                            }
                        }
                        XYZ MidBeamPoint = TempVar.Instance.TempPoint - TempVar.Instance.vecy * TempVar.Instance.High / 2;
                        //kiểm tra thép nằm lớp trên/dưới?

                        if (RebarPoint.Z - MidBeamPoint.Z < 0) // nằm dưới
                        {
                            XYZ elbow = RebarPoint + TempVar.Instance.vecy.Normalize() * GeomUtil.milimeter2Feet(100 + DeltaSingleRebar_bot);
                            XYZ HeadPosition = RebarPoint + TempVar.Instance.vecy.Normalize() * GeomUtil.milimeter2Feet(100 + DeltaSingleRebar_bot) - TempVar.Instance.vecx.Normalize() * (DistanceXY + GeomUtil.milimeter2Feet(150));
                            tag.LeaderElbow = elbow;
                            tag.TagHeadPosition = HeadPosition;

                            DeltaSingleRebar_bot -= 50;
                        }
                        else //nằm trên
                        {
                            XYZ elbow = RebarPoint - TempVar.Instance.vecy.Normalize() * GeomUtil.milimeter2Feet(80 - DeltaSingleRebar_top);
                            XYZ HeadPosition = RebarPoint - TempVar.Instance.vecy.Normalize() * GeomUtil.milimeter2Feet(80 - DeltaSingleRebar_top) - TempVar.Instance.vecx.Normalize() * (DistanceXY + GeomUtil.milimeter2Feet(150));
                            tag.LeaderElbow = elbow;
                            tag.TagHeadPosition = HeadPosition;
                            DeltaSingleRebar_top += 50;
                        }

                        tag.LeaderEnd = RebarPoint;
                        #endregion

                    }

                    else if (rebar.LookupParameter("Style").AsValueString() == "Stirrup / Tie") //Thép Stirrup
                    {
                        IndependentTag tagStir = IndependentTag.Create(doc, view.Id, new Reference(rebar), true, Autodesk.Revit.DB.TagMode.TM_ADDBY_CATEGORY, Autodesk.Revit.DB.TagOrientation.Horizontal, TempVar.Instance.TempPoint);
                        tagStir.LeaderEndCondition = LeaderEndCondition.Attached; ;
                        tagStir.TagHeadPosition = TempVar.Instance.TempPoint - TempVar.Instance.vecx.Normalize() * (GeomUtil.milimeter2Feet(150) + TempVar.Instance.Width / 2) - TempVar.Instance.vecy.Normalize() * (GeomUtil.milimeter2Feet(S - 50) + TempVar.Instance.High / 2);
                        S += 50; // mỗi thanh thép đai sẽ cách nhau 50
                        TagCreat tagCreat2 = new TagCreat();
                        tagCreat2.ChangeTag(doc, tagStir, Settings1.Default.StirruptRebar); // change tag



                    }

                }
            }
            public double Distancexy(XYZ a, XYZ b)
            {
                double Denx = Math.Pow(a.X - b.X, 2);
                double Deny = Math.Pow(a.Y - b.Y, 2);

                return Math.Sqrt(Denx + Deny);
            }

            public double Position(XYZ a, XYZ b)
            {
                if (GeomUtil.IsEqual(a.Z, b.Z))
                {
                    if (GeomUtil.IsEqual(a.Y, b.Y))
                    {
                        if (GeomUtil.IsEqual(a.X, b.X))
                        {
                            return 0;
                        }
                        return a.X > b.X ? 1 : -1;
                    }
                    return a.Y > b.Y ? 1 : -1;
                }
                return a.Z > b.Z ? 1 : -1;
            }

            public XYZ GetIntersection(Line line, Plane plan)
            {
                XYZ p0 = line.GetEndPoint(0);
                XYZ p0_0 = CheckGeometry.GetProjectPoint(plan, p0);
                double h = Line.CreateBound(p0, p0_0).Length;
                XYZ vecto = line.Direction;
                double angle = Math.PI - vecto.AngleTo(plan.Normal);
                double len = 0;
                try
                {
                    len = h / Math.Cos(angle);
                }
                catch (Exception)
                {
                    throw new InvalidOperationException("Cos(angle) == 0 , lỗi chia cho 0");
                }
                XYZ interPoint = new XYZ();
                if (plan.Normal.DotProduct(vecto) < 0)
                {
                    interPoint = GeomUtil.OffsetPoint(p0, vecto / vecto.GetLength(), len);
                }
                else
                {
                    interPoint = GeomUtil.OffsetPoint(p0, vecto / vecto.GetLength(), -len);
                }
                return interPoint;
            }
            public void ChangeTag(Document doc, IndependentTag tag, string Id)
            {
                Element TagEle = doc.GetElement(Id);
                tag.ChangeTypeId(TagEle.Id);
            }
        }
        public class DimCreat
        {
            public DimCreat() { }
            public void CreateDIM(Document doc, Element beam, View View, XYZ section1, XYZ directionH)
            {
                //tìm đối tượng join với beam trong Activeview
                //B1: tạo filtercolector
                #region MyRegion2                
                ElementCategoryFilter FloorCategoryfilter = new ElementCategoryFilter(BuiltInCategory.OST_Floors);

                #endregion           

                XYZ LeftbotPoint = TempVar.Instance.TempPoint - TempVar.Instance.vecy * TempVar.Instance.High - TempVar.Instance.vecx * (TempVar.Instance.Width + GeomUtil.milimeter2Feet(200));
                double offset = GeomUtil.milimeter2Feet(25);
                XYZ p1 = LeftbotPoint - TempVar.Instance.vecx * offset - TempVar.Instance.vecz * offset;
                XYZ p2 = p1 + TempVar.Instance.vecx * offset * 2;
                XYZ p3 = p2 + TempVar.Instance.vecz * offset * 2;
                XYZ p4 = p3 - TempVar.Instance.vecx * offset * 2;
                Line l11 = Line.CreateBound(p1, p2);
                Line l21 = Line.CreateBound(p2, p3);
                Line l31 = Line.CreateBound(p3, p4);
                Line l41 = Line.CreateBound(p4, p1);
                CurveLoop cloop = new CurveLoop();
                cloop.Append(l11); cloop.Append(l21); cloop.Append(l31); cloop.Append(l41);
                Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { cloop }, XYZ.BasisZ, TempVar.Instance.High);
                ElementIntersectsSolidFilter LeftSolidFilter = new ElementIntersectsSolidFilter(solid);

                // tạo family detail

                XYZ RightBotPoint = TempVar.Instance.TempPoint - TempVar.Instance.vecy * TempVar.Instance.High + TempVar.Instance.vecx * (TempVar.Instance.Width + GeomUtil.milimeter2Feet(200));
                //XYZ RightTopPoint = TempVar.Instance.TempPoint + TempVar.Instance.vecx * (TempVar.Instance.Width + GeomUtil.milimeter2Feet(200));
                CurveLoop Circle = new CurveLoop();
                List<CurveLoop> curveloops = new List<CurveLoop>();

                Circle.Append(Arc.Create(RightBotPoint, GeomUtil.milimeter2Feet(200), 0, Math.PI, TempVar.Instance.vecx, TempVar.Instance.vecz));
                Circle.Append(Arc.Create(RightBotPoint, GeomUtil.milimeter2Feet(200), Math.PI, 2 * Math.PI, TempVar.Instance.vecx, TempVar.Instance.vecz));
                curveloops.Add(Circle);
                Solid CylinderSolid = GeometryCreationUtilities.CreateExtrusionGeometry(curveloops, XYZ.BasisZ, TempVar.Instance.High);
                ElementIntersectsSolidFilter RightSolidFilter = new ElementIntersectsSolidFilter(CylinderSolid);
                LogicalOrFilter FinalFIlter = new LogicalOrFilter(RightSolidFilter, LeftSolidFilter);
                List<Element> filcol = new FilteredElementCollector(doc, View.Id).WherePasses(FinalFIlter).ToList();


                List<Reference> referenceArray1 = new List<Reference>();
                ReferenceArray referenceArray1a = new ReferenceArray();   // Dim chiều cao chi tiết
                List<Reference> referenceArray2 = new List<Reference>();
                ReferenceArray referenceArray2a = new ReferenceArray(); //Dim Chiều Cao Tổng
                List<Reference> referenceArray3 = new List<Reference>();
                ReferenceArray referenceArray3a = new ReferenceArray(); //Dim chiều rộng

                FamilyInstance fiBeam = beam as FamilyInstance;
                // lấy face của Beam bằng cách ép về solid rồi kiểm tra từng face
                Options opt = new Options();
                opt.ComputeReferences = true;
                GeometryElement geoElem = beam.get_Geometry(opt);
                Solid s = null;
                List<PlanarFace> PlanarPlLists = new List<PlanarFace>();
                foreach (GeometryObject geoObj in geoElem)
                {
                    s = geoObj as Solid;
                    if (s == null)
                    {
                        continue;
                    }
                    foreach (Face f in s.Faces)
                    {
                        XYZ vec = f.ComputeNormal(UV.Zero);
                        if (GeomUtil.IsSameOrOppositeDirection(vec, XYZ.BasisZ))
                        {
                            Reference rf = f.Reference;
                            if (rf != null)
                            {
                                referenceArray1.Add(rf);
                            }

                            //Add 2 face of Beam to list 2
                            if (rf != null)
                            {
                                referenceArray2.Add(rf);
                            }
                        }
                    }

                    //lấy 2 face 2 BÊN của beam
                    foreach (Face f in s.Faces)
                    {
                        XYZ vec = f.ComputeNormal(UV.Zero);
                        if (GeomUtil.IsSameOrOppositeDirection(vec, directionH.Normalize()))
                        {
                            Reference rf = f.Reference;
                            if (rf != null)
                            {
                                referenceArray3.Add(rf);
                            }
                            //Add 2 face of Beam to list 2                            
                        }
                    }
                    break;
                    //
                }

                //lầy face của floor trong filtercollector
                foreach (Element elements in filcol)
                {
                    Floor floor = elements as Floor;
                    Reference top = HostObjectUtils.GetTopFaces(floor).First();
                    Reference bot = HostObjectUtils.GetBottomFaces(floor).First();
                    if (top != null)
                    {
                        referenceArray1.Add(top);
                    }
                    if (bot != null)
                    {
                        referenceArray1.Add(bot);
                    }
                }

                //kiểm tra các face trùng nhau trong tập referenceArray
                List<int> indexToRemove = new List<int>();
                for (int i = 0; i < referenceArray1.Count; i++)
                {
                    for (int j = i + 1; j < referenceArray1.Count; j++)
                    {
                        PlanarFace pf1 = doc.GetElement(referenceArray1[i]).GetGeometryObjectFromReference(referenceArray1[i]) as PlanarFace;
                        PlanarFace pf2 = doc.GetElement(referenceArray1[j]).GetGeometryObjectFromReference(referenceArray1[j]) as PlanarFace;
                        XYZ orgPoint1 = pf1.Origin;
                        XYZ orgPoint2 = pf2.Origin;
                        XYZ vectemp = orgPoint2 - orgPoint1;
                        if (GeomUtil.IsEqual(XYZ.BasisZ.DotProduct(vectemp), 0))
                        {
                            //PlanarPlLists.RemoveAt(i);
                            if (!indexToRemove.Contains(i))
                                indexToRemove.Add(i);
                        }

                    }
                }
                //kiểm tra các face trùng nhau trong tập referenceArray2
                List<int> indexToRemove2 = new List<int>();
                for (int i = 0; i < referenceArray2.Count; i++)
                {
                    for (int j = i + 1; j < referenceArray2.Count; j++)
                    {
                        PlanarFace pf1 = doc.GetElement(referenceArray2[i]).GetGeometryObjectFromReference(referenceArray2[i]) as PlanarFace;
                        PlanarFace pf2 = doc.GetElement(referenceArray2[j]).GetGeometryObjectFromReference(referenceArray2[j]) as PlanarFace;
                        XYZ orgPoint1 = pf1.Origin;
                        XYZ orgPoint2 = pf2.Origin;
                        XYZ vectemp = orgPoint2 - orgPoint1;
                        if (GeomUtil.IsEqual(XYZ.BasisZ.DotProduct(vectemp), 0))
                        {
                            //PlanarPlLists.RemoveAt(i);
                            if (!indexToRemove2.Contains(i))
                                indexToRemove2.Add(i);
                        }

                    }
                } //kiểm tra các face trùng nhau trong tập referenceArray3
                List<int> indexToRemove3 = new List<int>();
                if (referenceArray3.Count > 2)
                {
                    for (int i = 0; i < referenceArray3.Count; i++)
                    {
                        for (int j = i + 1; j < referenceArray3.Count; j++)
                        {
                            PlanarFace pf1 = doc.GetElement(referenceArray3[i]).GetGeometryObjectFromReference(referenceArray3[i]) as PlanarFace;
                            PlanarFace pf2 = doc.GetElement(referenceArray3[j]).GetGeometryObjectFromReference(referenceArray3[j]) as PlanarFace;
                            XYZ orgPoint1 = pf1.Origin;
                            XYZ orgPoint2 = pf2.Origin;
                            XYZ vectemp = orgPoint2 - orgPoint1;
                            if (GeomUtil.IsEqual(XYZ.BasisZ.DotProduct(vectemp), 0))
                            {
                                //PlanarPlLists.RemoveAt(i);
                                if (!indexToRemove3.Contains(i))
                                    indexToRemove3.Add(i);
                            }
                        }
                    }
                    // Xóa các Face trùng nhau trong ReferencArray3
                    if (indexToRemove3.Count != 0)
                    {
                        for (int i = indexToRemove3.Count - 1; i >= 0; --i)
                        {
                            referenceArray3.RemoveAt(indexToRemove3[i]);
                        }
                    }
                }

                // Xóa các Face trùng nhau trong ReferencArray
                if (indexToRemove.Count != 0)
                {
                    for (int i = indexToRemove.Count - 1; i >= 0; --i)
                    {
                        referenceArray1.RemoveAt(indexToRemove[i]);
                    }
                }
                // Xóa các Face trùng nhau trong ReferencArray2
                if (indexToRemove2.Count != 0)
                {
                    for (int i = indexToRemove2.Count - 1; i >= 0; --i)
                    {
                        referenceArray2.RemoveAt(indexToRemove2[i]);
                    }
                }

                //CHuyển List Planar-> ReferenceArray3
                for (int i = 0; i < referenceArray1.Count; i++)
                {
                    referenceArray1a.Append(referenceArray1[i]);
                }
                for (int i = 0; i < referenceArray2.Count; i++)
                {
                    referenceArray2a.Append(referenceArray2[i]);
                }
                for (int i = 0; i < referenceArray3.Count; i++)
                {
                    referenceArray3a.Append(referenceArray3[i]);
                }

                using (SubTransaction subT = new SubTransaction(doc))
                {
                    subT.Start();
                    // đọc tiết diện dầm
                    FamilySymbol type = doc.GetElement(beam.GetTypeId()) as FamilySymbol;
                    double Width;
                    try
                    {
                        Width = type.LookupParameter("b").AsDouble();
                    }
                    catch
                    {
                        try
                        {
                            Width = type.LookupParameter("b_dam").AsDouble();
                        }
                        catch
                        {

                            Width = type.LookupParameter("b_input").AsDouble();
                        }


                    }
                    double Height;
                    try
                    {
                        Height = type.LookupParameter("h").AsDouble();
                    }
                    catch
                    {
                        try
                        {
                            Height = type.LookupParameter("h_dam").AsDouble();
                        }
                        catch
                        {
                            Height = type.LookupParameter("h_input").AsDouble();

                        }

                    }
                    //
                    FilteredElementCollector DimesionTypeCollector = new FilteredElementCollector(doc);
                    DimesionTypeCollector.OfClass(typeof(DimensionType));
                    DimensionType DimType = DimesionTypeCollector.Cast<DimensionType>()
                    .Where(x => x.Name == Settings1.Default.DIMStyle && x.FamilyName == "Linear Dimension Style").First();

                    XYZ dimPoint1 = section1 + (GeomUtil.milimeter2Feet(150) + Width / 2) * directionH.Normalize();
                    XYZ dimPoint2 = section1 + (GeomUtil.milimeter2Feet(250) + Width / 2) * directionH.Normalize();
                    XYZ dimPoint3 = section1 - (GeomUtil.milimeter2Feet(100) + Height) * XYZ.BasisZ;
                    if (referenceArray1a.Size > 1)
                    {
                        Dimension dimCreate1 = doc.Create.NewDimension(View, Line.CreateBound(dimPoint1, dimPoint1 + XYZ.BasisZ), referenceArray1a);
                        dimCreate1.DimensionType = DimType;
                    }
                    if (referenceArray2a.Size > 1)
                    {
                        Dimension dimCreate2 = doc.Create.NewDimension(View, Line.CreateBound(dimPoint2, dimPoint2 + XYZ.BasisZ), referenceArray2a);
                        dimCreate2.DimensionType = DimType;
                    }

                    if (referenceArray3a.Size > 1)
                    {
                        Dimension dimCreate3 = doc.Create.NewDimension(View, Line.CreateBound(dimPoint3, dimPoint3 + directionH), referenceArray3a);
                        dimCreate3.DimensionType = DimType;
                    }

                    subT.Commit();
                }
            }

        }
    }

    // class dùng để chỉ chọn dầm trong revit
    public class BeamSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralFraming)
            {
                return true;
            }
            return false;
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }

    public class ViewCreate
    {
        public XYZ section1;
        public XYZ section2;

        List<XYZ> PickPointList;
        ElementId ViewTypeId;
        Element beam;

        Document doc;
        public XYZ vecx;
        public ViewCreate(List<XYZ> PickPointList, ElementId ViewTypeId, Element beam, Document doc) // truyền thông số ban đầu cho quá trình tạo view
        {
            this.PickPointList = PickPointList;
            this.ViewTypeId = ViewTypeId;
            this.beam = beam;
            this.doc = doc;

        }

        public View CreateViewSection()
        {
            Parameter levelofBeam = beam.LookupParameter("Reference Level");
            //Get view of Beam
            View viewofBeam = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_Views)
                        .WhereElementIsNotElementType()
                        .Cast<View>()
                        .First(x => x.Name == levelofBeam.AsValueString());
            /// Lấy về tọa độ của dầm
            BoundingBoxXYZ box = beam.get_BoundingBox(viewofBeam);

            FamilyInstance instance = beam as FamilyInstance;

            AnalyticalModel model = instance.GetAnalyticalModel();

            Curve curve = model.GetCurve();

            Line line = curve as Line;
            // Determine view family type to use

            ViewFamilyType vft
              = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault<ViewFamilyType>(x =>
                 ViewFamily.Section == x.ViewFamily);

            // Determine section box

            XYZ p0 = line.GetEndPoint(0);
            XYZ q0 = line.GetEndPoint(1);

            XYZ p = new XYZ(p0.X, box.Min.Y, p0.Z);
            XYZ q = new XYZ(q0.X, box.Min.Y, q0.Z);

            XYZ v = q0 - p0;
            double minZ = box.Min.Z; // lấy z min (1: -2,62 ; 2:
            double maxZ = box.Max.Z; // lấy z max (1: 0,0000000000023 ; 2:

            double w = v.GetLength(); // Lấy chiều dài của line (1: 29,5 ; 2:
            double h = maxZ - minZ; //(1: 2,62 ; 2:
            double d = box.Max.Y - box.Min.Y; // lấy bề rộng của beam (1: 29,5 ; 2:
            double offset = 0.1 * w; // (1: 2,95 ; 2:

            XYZ min = new XYZ(-w + 4 * offset, minZ - 0.5 * offset + milimeter2Feet(00), -d / 1.1); //chinh canh duoi(-32.480314960629919;-35.104986876638115;-1.4763779527559056)
                                                                                                    //min: (mặt bằng cạnh bên trái, mặt đứng cạnh dưới, mặt bằng cạnh trên)
            XYZ max = new XYZ(w - 4 * offset, maxZ + 0.5 * offset - milimeter2Feet(000), 0); // chinh canh tren (26.574803149606296; -33.136482939630234;0.032808398950131233)
                                                                                             //max: (mặt bằng cạnh bên phải, mặt đứng cạnh trên, mặt bằng cạnh dưới)
            XYZ midpoint = p + 0.5 * v + new XYZ(0, 0, milimeter2Feet(00000)); // midpoint = 
            XYZ walldir = v.Normalize(); //walldir : x = 1
            XYZ up = XYZ.BasisZ; // xác định section chiếu vào mặt đứng hay mặt bằng. Z là mặt đứng

            XYZ viewdir = walldir.CrossProduct(up); //viewdir : y = -1

            Transform t = Transform.Identity;
            t.Origin = midpoint;
            t.BasisX = walldir;
            t.BasisY = up;
            t.BasisZ = viewdir;

            BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
            sectionBox.Transform = t;
            sectionBox.Min = min;
            sectionBox.Max = max;
            ViewSection section = ViewSection.CreateSection(doc, vft.Id, sectionBox);
            var cropRegionManager = section.GetCropRegionShapeManager();
            string SectionName = "Mặt cắt ngang";
            section.Name = SectionName;
            ApplyViewTemplateToActiveViewMCN(doc, section);
            section.CropBoxActive = true;
            section.CropBoxVisible = false;
            return section;
        }
        public List<View> getListView()
        {
            //Lấy cái beam đã chọn
            TempVar.Instance.beam = beam;
            LocationCurve lc = beam.Location as LocationCurve;
            Curve BeamCurve = lc.Curve; // Lấy về đường Curve của beam

            // Kiểm tra location Curve của beam nằm bên left hay bên right
            XYZ vecxT = BeamCurve.GetEndPoint(1) - BeamCurve.GetEndPoint(0);
            XYZ vecyT = XYZ.BasisZ.CrossProduct(vecxT); // tạo 1 vecto vuông góc

            double width; 
            //Lấy về bề rộng của dầm
            try
            {
                width = (doc.GetElement(beam.GetTypeId()) as FamilySymbol).LookupParameter("b").AsDouble();
            }
            catch (Exception)
            {
                try
                {
                    width = (doc.GetElement(beam.GetTypeId()) as FamilySymbol).LookupParameter("b_dam").AsDouble();
                }
                catch
                {
                    width = (doc.GetElement(beam.GetTypeId()) as FamilySymbol).LookupParameter("b_input").AsDouble();
                }

            }
            // Dời đường Curve của dầm về tâm
            if (beam.LookupParameter("y Justification").AsValueString() == "Right") // Kiểm tra đường dóng của dầm
            {
                BeamCurve = GeomUtil.OffsetCurve(BeamCurve, vecyT, width / 2); //Trả về một đoạn thẳng là kết quả của tịnh tiến một đoạn thẳng theo một vector và khoảng cách cho trước
            }
            else if (beam.LookupParameter("y Justification").AsValueString() == "Left")
            {
                BeamCurve = GeomUtil.OffsetCurve(BeamCurve, -vecyT, width / 2);
            }


            Line ln = BeamCurve as Line;
            // ???
            XYZ vecz_TEMP = (-ln.GetEndPoint(0) + ln.GetEndPoint(1)).Normalize();

            ///kiểm tra vector chỉ phương của dầm
            if (vecz_TEMP.X * vecz_TEMP.Y > 0)
            {
                if (vecz_TEMP.X < 0)
                {
                    vecz_TEMP = -vecz_TEMP;
                }
            }
            else if (vecz_TEMP.X * vecz_TEMP.Y < 0)
            {
                if (vecz_TEMP.X > 0)
                {
                    vecz_TEMP = -vecz_TEMP;
                }
            }
            else if (vecz_TEMP.X * vecz_TEMP.Y == 0)
            {
                if (vecz_TEMP.X == 0)
                {
                    if (vecz_TEMP.Y < 0)
                    {
                        vecz_TEMP = -vecz_TEMP;
                    }
                }
                else if (vecz_TEMP.X < 0)
                {
                    vecz_TEMP = -vecz_TEMP;
                }
            }

            XYZ vecy_TEMP = XYZ.BasisZ;
            XYZ vecx_TEMP = vecy_TEMP.CrossProduct(vecz_TEMP);
            ///

            FamilySymbol type = doc.GetElement(beam.GetTypeId()) as FamilySymbol;
            string BeamName = type.Name.ToString();
            double Width;
            try
            {
                Width = type.LookupParameter("b").AsDouble();
            }
            catch
            {
                try
                {
                    Width = type.LookupParameter("b_dam").AsDouble();
                }
                catch
                {
                    Width = type.LookupParameter("b_input").AsDouble();
                }
            }
            double Height;
            try
            {
                Height = type.LookupParameter("h").AsDouble();
            }
            catch
            {
                try
                {
                    Height = type.LookupParameter("h_dam").AsDouble(); ;
                }
                catch
                {
                    Height = type.LookupParameter("h_input").AsDouble();
                }

            }

            double Length = ln.Length;
            TempVar.Instance.Width = Width;
            TempVar.Instance.High = Height;
            XYZ vecy = XYZ.BasisZ;

            // Xác định 2 điểm đầu và cuối của đường ở giữa dầm
            XYZ startpoint = BeamCurve.GetEndPoint(0);
            XYZ endpoint = BeamCurve.GetEndPoint(1);

            XYZ vecz = vecz_TEMP;
            TempVar.Instance.vecy = vecy; //vector y của dầm, lưu vào class Temp
            TempVar.Instance.vecx = vecx = vecy.CrossProduct(vecz); // vector x của dầm, lưu vào class Temp
            TempVar.Instance.vecz = vecz;
            TempVar.Instance.Mid = (startpoint + endpoint) / 2;

            // CrossProduct là xác định vecto còn lại trong XYZ khi biết được 2 vecto
            // DotProduct là tích vô hướng
            // CrossProduct là tích có hướng


            List<View> SectionViewList = new List<View>();
            

            foreach (XYZ PickPoint in PickPointList)
            {
                //tao mp cat qua driving curve
                //Plane pl1 = new Plane(vecy, vecz, PickPoint);
                
                Plane pl1 = Plane.CreateByOriginAndBasis(PickPoint, vecx, vecy); // Tạo mặt phẳng X và Y
                TempVar.Instance.SectionPlane = pl1;
                // điểm offset từ pick point
                XYZ PickPointOffset = GeomUtil.OffsetPoint(PickPoint, vecz, GeomUtil.milimeter2Feet(150)); 

                Plane pl2 = Plane.CreateByOriginAndBasis(PickPointOffset, vecx, vecy);

                // lấy giao điểm của drivingcurve và 2 plane vừa tạo
                XYZ SectionPoint1 = CheckGeometry.GetProjectPoint(pl1, endpoint);

                XYZ SectionPoint2 = CheckGeometry.GetProjectPoint(pl2, endpoint);

                TempVar.Instance.TempPoint = SectionPoint1;

                section1 = SectionPoint1;
                section2 = SectionPoint2;

                XYZ MinBoundPoint = new XYZ(-Width / 2 - GeomUtil.milimeter2Feet(300), -Height * 2.2 - GeomUtil.milimeter2Feet(200), 0);
                XYZ MaxBoundPoint = new XYZ(Width / 2 + GeomUtil.milimeter2Feet(200), GeomUtil.milimeter2Feet(200), GeomUtil.milimeter2Feet(200));


                //XYZ MinBoundPoint1 = new XYZ(-Width / 2 - GeomUtil.milimeter2Feet(300), Height * 2.2 + GeomUtil.milimeter2Feet(200), 0);
                //XYZ MaxBoundPoint1 = new XYZ(Width / 2 + GeomUtil.milimeter2Feet(200), -GeomUtil.milimeter2Feet(200), GeomUtil.milimeter2Feet(200));


                Transform tf = Transform.Identity;
                tf.BasisX = vecx;
                tf.BasisY = vecy;
                tf.BasisZ = vecz;
                tf.Origin = SectionPoint1;
                BoundingBoxXYZ ViewBoundBox = new BoundingBoxXYZ() { Transform = tf, Min = MinBoundPoint, Max = MaxBoundPoint };

                ViewSection test = ViewSection.CreateDetail(doc, ViewTypeId, ViewBoundBox); // tạo 1 section
                doc.Regenerate(); // update element in doc to reflect all changes
                // Gán tên cho Detail view
                string Mark = beam.LookupParameter("Mark").AsString();
                string Type = doc.GetElement(beam.GetTypeId()).Name;
                string SectionName = Mark + "_" + Type + "_MCD " +"(" + TempVar.Instance.SubNumber + ")";
                TempVar.Instance.SubNumber = TempVar.Instance.SubNumber + 1;
                test.Name = SectionName;


                ApplyViewTemplateToActiveView(doc, test);
                SectionViewList.Add(test);
                
                test.CropBoxActive = true;
                test.CropBoxVisible = false;

            }

            return SectionViewList;
        }

        public void ApplyViewTemplateToActiveView(Document doc, View view)
        {

            View viewTemplate = (from v in new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                                 where v.IsTemplate == true && v.Name == Settings1.Default.ViewTemplate
                                 select v)
                .First();
            view.ViewTemplateId = viewTemplate.Id;

        }

        public void ApplyViewTemplateToActiveViewMCN(Document doc, View view)
        {

            View viewTemplate = (from v in new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                                 where v.IsTemplate == true && v.Name == Settings1.Default.ViewTemplateMCN
                                 select v)
                .First();
            view.ViewTemplateId = viewTemplate.Id;

        }

        /// <summary>
        /// Hệ số chuyển đổi từ feet sang meter
        /// </summary>
        const double FEET_TO_METERS = 0.3048;
        /// <summary>
        /// Hệ số chuyển đổi từ feet sang milimeter
        /// </summary>
        const double FEET_TO_MILIMETERS = FEET_TO_METERS * 1000;
        public static double milimeter2Feet(double milimeter)
        {
            return milimeter / FEET_TO_MILIMETERS;
        }

    }
}
