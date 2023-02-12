using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Structure;

namespace DATN_CHUYEN_DE_REVITAPI.CreateBeam
{
    [TransactionAttribute(TransactionMode.Manual)]
    class CreateFormBeam : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            Form_BarBeam form_BarBeam = new Form_BarBeam(commandData, ref message, elements);
            form_BarBeam.ShowDialog();

            if (Global.IsFormBeamOk == true)
            {
               
                try
                {

                    IList<Reference> newrefs1 = new List<Reference>();
                    beamISelFilter SF = new beamISelFilter();
                    IList<Reference> newrefs = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, SF, "ADD/REMOVE Beam elements") as IList<Reference>;

                    IList<ElementId> beamIDS = new List<ElementId>();
                    foreach (Reference ref1 in newrefs)
                    {
                        ElementId eid = ref1.ElementId;
                        beamIDS.Add(eid);
                    }

                    uidoc.Selection.SetElementIds(beamIDS);
                    uidoc.ShowElements(beamIDS);
                    uidoc.RefreshActiveView();

                    //   IList<Reference> newrefs = new List<Reference>();
                    IList<Element> element = new List<Element>();
                    foreach (ElementId elementId in beamIDS)
                    {
                        Element e1 = doc.GetElement(elementId);
                        element.Add(e1);
                        Reference R = new Reference(e1);
                        newrefs.Add(R);
                    }

                    
                    foreach (Element element1 in element)
                    {
                        CreateRebarBeam(doc, uidoc, element1);
                    }

                    return Result.Succeeded;

                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    return Result.Failed;
                }
            }
            return Result.Succeeded;
        }
        public class beamISelFilter : ISelectionFilter
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
        public Result CreateRebarBeam(Document doc, UIDocument uidoc, Element element)
        {


            //Get information of element
            ElementId elementIdType = element.GetTypeId();
            ElementType elementType = doc.GetElement(elementIdType) as ElementType;

            //Get Rebar shape
            RebarShape rebarShape = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarShape))
                .Cast<RebarShape>()
                .First(x => x.Name == "M_00");
            //Get Rebar Type
            RebarBarType barType = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .Cast<RebarBarType>()
                .First(x => x.Name == Global.Thepduoi.ToString());
            // Get Rebar shape
            RebarShape rebarShape1 = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarShape))
                .Cast<RebarShape>()
                .First(x => x.Name == "M_17");
            //Get Rebar Type
            RebarBarType barType1 = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .Cast<RebarBarType>()
                .First(x => x.Name == Global.Theptren.ToString());
            //Get Rebar shape
            RebarShape rebarShape2 = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarShape))
                .Cast<RebarShape>()
                .First(x => x.Name == Global.kieucotdaiBeam);
            //Get Rebar Type
            RebarBarType barType2 = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .Cast<RebarBarType>()
                .First(x => x.Name == Global.StirrupBeam.ToString());


            Parameter bParam = elementType.LookupParameter("b");
            double width = bParam.AsDouble();
            Parameter hParam = elementType.LookupParameter("h");
            double height = hParam.AsDouble();
            Parameter lParam = element.LookupParameter("Length");
            double length = lParam.AsDouble();
            double length1 = lParam.AsDouble() - 2 * UnitUtils.ConvertToInternalUnits(20, DisplayUnitType.DUT_MILLIMETERS);
            Location loc = element.Location;
            LocationCurve locCur = loc as LocationCurve;
            Curve curve = locCur.Curve;
            

            Parameter zJus = element.LookupParameter("z Justification");

            XYZ vecxT = curve.GetEndPoint(1) - curve.GetEndPoint(0);
            XYZ vecyT = XYZ.BasisZ.CrossProduct(vecxT); // tạo 1 vecto vuông góc
            if (element.LookupParameter("y Justification").AsValueString() == "Right") // Kiểm tra đường dóng của dầm
            {
                curve = GeomUtil.OffsetCurve(curve, vecyT, width / 2); //Trả về một đoạn thẳng là kết quả của tịnh tiến một đoạn thẳng theo một vector và khoảng cách cho trước
            }
            else if (element.LookupParameter("y Justification").AsValueString() == "Left")
            {
                curve = GeomUtil.OffsetCurve(curve, -vecyT, width / 2);
            }
            Line line = curve as Line;
            XYZ vectorX = line.Direction;
            XYZ vectorZ = XYZ.BasisZ;
            XYZ vectorY = vectorZ.CrossProduct(vectorX);
            XYZ pnt = line.GetEndPoint(0);
            int zPos = zJus.AsInteger();
            double a = 0;
            switch (zPos)
            {
                case 0: // Top
                    a = 1;
                    break;
                case 1: // Center  
                case 2: // Origin  
                    a = 0.5;
                    break;
                case 3: // Bottom  
                    a = 0;
                    break;
            }
            XYZ origin = pnt - vectorY * width / 2 - vectorZ * height * a;
            List<XYZ> sectionPnts = new List<XYZ>
                        {
                                    origin,
                            origin + vectorY * width,
                            origin + vectorY * width + vectorZ * height,
                            origin + vectorZ * height
                          };
            try
            {

                using (Transaction trans = new Transaction(doc, "Create Rebar Beam"))
                {
                    trans.Start();
                    //Get Parameter Rebar Cover
                    FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc);
                    filteredElementCollector.OfClass(typeof(RebarCoverType));
                    List<RebarCoverType> rebarCoverTypes = filteredElementCollector.Cast<RebarCoverType>().ToList<RebarCoverType>();
                    RebarCoverType rct = rebarCoverTypes[1];
                    foreach (RebarCoverType r1 in rebarCoverTypes)
                    {
                        if (r1.Name == Global.Beam_RebarCover) rct = r1;
                    }
                    Parameter top = element.LookupParameter("Rebar Cover - Top Face");
                    Parameter bot = element.LookupParameter("Rebar Cover - Bottom Face");
                    Parameter side = element.LookupParameter("Rebar Cover - Other Faces");
                    top.Set(rct.Id);
                    bot.Set(rct.Id);
                    side.Set(rct.Id);
                    // Beam duoi len tren
                    if (vectorX.Y == 1)
                    {
                        //PHIA DUOI : Duoi phai rebar1, duoi trai rebar2, cung chieu

                        XYZ origin1 = new XYZ(sectionPnts[0].X - barType.BarDiameter / 2 - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                            sectionPnts[0].Y + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                            sectionPnts[0].Z + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;
                        Rebar rebar1 = Rebar.CreateFromRebarShape(doc, rebarShape, barType, element, origin1, vectorX, vectorY);
                        RebarShapeDrivenAccessor rebarShapeDrivenAccessor1 = rebar1.GetShapeDrivenAccessor();
                        XYZ xmoi = vectorX * length1;
                        XYZ ymoi = vectorY;

                        rebarShapeDrivenAccessor1.ScaleToBox(origin1, xmoi, vectorZ);

                        XYZ origin2 = new XYZ(sectionPnts[1].X + barType.BarDiameter / 2 + barType2.BarDiameter + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                           sectionPnts[1].Y + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                           sectionPnts[1].Z + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;
                        Rebar rebar2 = Rebar.CreateFromRebarShape(doc, rebarShape, barType, element, origin1, vectorX, vectorY);
                        RebarShapeDrivenAccessor rebarShapeDrivenAccessor2 = rebar2.GetShapeDrivenAccessor();
                        rebarShapeDrivenAccessor2.ScaleToBox(origin2, xmoi, vectorZ);

                        double khoangcachxduoi = Math.Round((origin1.X - origin2.X) * (width + 2 * (barType.BarDiameter / 2 - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))) / (Global.nudThepduoi), 3);
                        if (Global.nudThepduoi >= 3)
                        {
                            for (int i = 1; i <= Global.nudThepduoi - 2; i++)
                            {
                                XYZ xnew1 = new XYZ(origin2.X + khoangcachxduoi * i, origin2.Y, origin2.Z);
                                Rebar thep1 = Rebar.CreateFromRebarShape(doc, rebarShape, barType, element, xnew1, vectorX, vectorY);
                                RebarShapeDrivenAccessor rebarShapeDrivenAccessornew1 = thep1.GetShapeDrivenAccessor();
                                rebarShapeDrivenAccessornew1.ScaleToBox(xnew1, xmoi, vectorZ);
                            }
                        }

                        //PHIA TREN tren trai,tren phai
                        XYZ origin3 = new XYZ(sectionPnts[2].X + barType.BarDiameter / 2 + barType2.BarDiameter + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                           sectionPnts[2].Y + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                           sectionPnts[2].Z - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;
                        Rebar rebar3 = Rebar.CreateFromRebarShape(doc, rebarShape1, barType1, element, origin3, vectorX, vectorY);
                        RebarShapeDrivenAccessor rebarShapeDrivenAccessor3 = rebar3.GetShapeDrivenAccessor();
                        rebarShapeDrivenAccessor3.ScaleToBox(origin3, xmoi, -vectorZ);

                        XYZ origin4 = new XYZ(sectionPnts[3].X - barType.BarDiameter / 2 - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                              sectionPnts[3].Y + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                              sectionPnts[3].Z - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;
                        Rebar rebar4 = Rebar.CreateFromRebarShape(doc, rebarShape1, barType1, element, origin4, vectorX, vectorY);
                        RebarShapeDrivenAccessor rebarShapeDrivenAccessor4 = rebar4.GetShapeDrivenAccessor();
                        rebarShapeDrivenAccessor4.ScaleToBox(origin4, xmoi, -vectorZ);
                        double khoangcachxtren = (origin4.X - origin3.X) * (width + 2 * (barType.BarDiameter / 2 - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))) / (Global.nudTheptren);
                        if (Global.nudTheptren >= 3)
                        {
                            for (int i = 1; i <= Global.nudTheptren - 2; i++)
                            {
                                XYZ xnew1 = new XYZ(origin3.X + khoangcachxtren * i, origin3.Y, origin3.Z);
                                Rebar thep1 = Rebar.CreateFromRebarShape(doc, rebarShape1, barType1, element, xnew1, vectorX, vectorY);
                                RebarShapeDrivenAccessor rebarShapeDrivenAccessornew1 = thep1.GetShapeDrivenAccessor();
                                rebarShapeDrivenAccessornew1.ScaleToBox(xnew1, xmoi, -vectorZ);
                            }
                        }

                        // thep dai
                        XYZ origin0 = new XYZ(sectionPnts[0].X - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                               sectionPnts[0].Y + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                               sectionPnts[0].Z + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;


                        switch (int.Parse(Global.botricotdaiBeam))
                        {
                            case 0:
                                {
                                    Rebar stirrup = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, origin0, vectorY, vectorZ);
                                    //Create Layout
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup = stirrup.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A1beam, DisplayUnitType.DUT_MILLIMETERS), length1 / 4, true, false, true);
                                    rebarShapeDrivenAccessorStirup.ScaleToBox(origin0, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));

                                    XYZ xStirrup1 = new XYZ(origin0.X, origin0.Y + length1 / 4, origin0.Z);
                                    //Create Layout 2
                                    Rebar stirrup1 = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, xStirrup1, vectorY, vectorX);
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup1 = stirrup1.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup1.ScaleToBox(xStirrup1, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));
                                    rebarShapeDrivenAccessorStirup1.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A2beam, DisplayUnitType.DUT_MILLIMETERS), length1 / 2, true, false, true);

                                    XYZ xStirrup2 = new XYZ(origin0.X, origin0.Y + 3 * length1 / 4, origin0.Z);
                                    //Create Layout 3
                                    Rebar stirrup2 = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, xStirrup2, vectorY, vectorX);
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup2 = stirrup2.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup2.ScaleToBox(xStirrup2, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));
                                    rebarShapeDrivenAccessorStirup2.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A3beam, DisplayUnitType.DUT_MILLIMETERS), length1 / 4, true, true, false);

                                    break;
                                };
                            case 1:
                                {
                                    Rebar stirrup = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, origin0, vectorY, vectorZ);
                                    //Create Layout
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup = stirrup.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A1beam, DisplayUnitType.DUT_MILLIMETERS), 0.4 * length1, true, false, true);
                                    rebarShapeDrivenAccessorStirup.ScaleToBox(origin0, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));

                                    XYZ xStirrup1 = new XYZ(origin0.X, origin0.Y + 0.4 * length1, origin0.Z);
                                    //Create Layout 2
                                    Rebar stirrup1 = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, xStirrup1, vectorY, vectorX);
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup1 = stirrup1.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup1.ScaleToBox(xStirrup1, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));
                                    rebarShapeDrivenAccessorStirup1.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A2beam, DisplayUnitType.DUT_MILLIMETERS), 0.6 * length1, true, true, false);

                                    break;
                                }
                            case 2:
                                {
                                    Rebar stirrup = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, origin0, vectorY, vectorZ);
                                    //Create Layout
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup = stirrup.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A1beam, DisplayUnitType.DUT_MILLIMETERS), length1, true, false, false);
                                    rebarShapeDrivenAccessorStirup.ScaleToBox(origin0, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));
                                    break;
                                }
                        }
                    }
                    // Beam duoi len tren
                    if (vectorX.Y == -1)
                    {
                        //PHIA DUOI : Duoi phai rebar1, duoi trai rebar2, cung chieu

                        XYZ origin1 = new XYZ(sectionPnts[0].X + barType.BarDiameter / 2 + barType2.BarDiameter + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                            sectionPnts[0].Y - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                            sectionPnts[0].Z + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;
                        Rebar rebar1 = Rebar.CreateFromRebarShape(doc, rebarShape, barType, element, origin1, vectorX, vectorY);
                        RebarShapeDrivenAccessor rebarShapeDrivenAccessor1 = rebar1.GetShapeDrivenAccessor();
                        XYZ xmoi = vectorX * length1;
                        XYZ ymoi = vectorY;

                        rebarShapeDrivenAccessor1.ScaleToBox(origin1, xmoi, vectorZ);

                        XYZ origin2 = new XYZ(sectionPnts[1].X - barType.BarDiameter / 2 - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                           sectionPnts[1].Y - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                           sectionPnts[1].Z + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;
                        Rebar rebar2 = Rebar.CreateFromRebarShape(doc, rebarShape, barType, element, origin1, vectorX, vectorY);
                        RebarShapeDrivenAccessor rebarShapeDrivenAccessor2 = rebar2.GetShapeDrivenAccessor();
                        rebarShapeDrivenAccessor2.ScaleToBox(origin2, xmoi, vectorZ);

                        double khoangcachxduoi = Math.Round((origin1.X - origin2.X) * (width + 2 * (barType.BarDiameter / 2 - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))) / (Global.nudThepduoi), 3);
                        if (Global.nudThepduoi >= 3)
                        {
                            for (int i = 1; i <= Global.nudThepduoi - 2; i++)
                            {
                                XYZ xnew1 = new XYZ(origin2.X + khoangcachxduoi * i, origin2.Y, origin2.Z);
                                Rebar thep1 = Rebar.CreateFromRebarShape(doc, rebarShape, barType, element, xnew1, vectorX, vectorY);
                                RebarShapeDrivenAccessor rebarShapeDrivenAccessornew1 = thep1.GetShapeDrivenAccessor();
                                rebarShapeDrivenAccessornew1.ScaleToBox(xnew1, xmoi, vectorZ);
                            }
                        }

                        //PHIA TREN tren trai,tren phai
                        XYZ origin3 = new XYZ(sectionPnts[2].X - barType.BarDiameter / 2 - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                           sectionPnts[2].Y - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                           sectionPnts[2].Z - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;
                        Rebar rebar3 = Rebar.CreateFromRebarShape(doc, rebarShape1, barType1, element, origin3, vectorX, vectorY);
                        RebarShapeDrivenAccessor rebarShapeDrivenAccessor3 = rebar3.GetShapeDrivenAccessor();
                        rebarShapeDrivenAccessor3.ScaleToBox(origin3, xmoi, -vectorZ);

                        XYZ origin4 = new XYZ(sectionPnts[3].X + barType.BarDiameter / 2 + barType2.BarDiameter + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                              sectionPnts[3].Y - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                              sectionPnts[3].Z - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;
                        Rebar rebar4 = Rebar.CreateFromRebarShape(doc, rebarShape1, barType1, element, origin4, vectorX, vectorY);
                        RebarShapeDrivenAccessor rebarShapeDrivenAccessor4 = rebar4.GetShapeDrivenAccessor();
                        rebarShapeDrivenAccessor4.ScaleToBox(origin4, xmoi, -vectorZ);
                        double khoangcachxtren = Math.Round((origin4.X - origin3.X) * (width + 2 * (barType.BarDiameter / 2 - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))) / (Global.nudTheptren), 3);
                        if (Global.nudTheptren >= 3)
                        {
                            for (int i = 1; i <= Global.nudTheptren - 2; i++)
                            {
                                XYZ xnew1 = new XYZ(origin3.X + khoangcachxtren * i, origin3.Y, origin3.Z);
                                Rebar thep1 = Rebar.CreateFromRebarShape(doc, rebarShape1, barType1, element, xnew1, vectorX, vectorY);
                                RebarShapeDrivenAccessor rebarShapeDrivenAccessornew1 = thep1.GetShapeDrivenAccessor();
                                rebarShapeDrivenAccessornew1.ScaleToBox(xnew1, xmoi, -vectorZ);
                            }
                        }

                        // thep dai
                        XYZ origin0 = new XYZ(sectionPnts[0].X + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                               sectionPnts[0].Y - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                               sectionPnts[0].Z + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;


                        switch (int.Parse(Global.botricotdaiBeam))
                        {
                            case 0:
                                {
                                    Rebar stirrup = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, origin0, vectorY, vectorZ);
                                    //Create Layout
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup = stirrup.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A1beam, DisplayUnitType.DUT_MILLIMETERS), length1 / 4, true, false, true);
                                    rebarShapeDrivenAccessorStirup.ScaleToBox(origin0, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));

                                    XYZ xStirrup1 = new XYZ(origin0.X, origin0.Y - length1 / 4, origin0.Z);
                                    //Create Layout 2
                                    Rebar stirrup1 = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, xStirrup1, vectorY, vectorX);
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup1 = stirrup1.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup1.ScaleToBox(xStirrup1, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));
                                    rebarShapeDrivenAccessorStirup1.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A2beam, DisplayUnitType.DUT_MILLIMETERS), length1 / 2, true, false, true);

                                    XYZ xStirrup2 = new XYZ(origin0.X, origin0.Y - 3 * length1 / 4, origin0.Z);
                                    //Create Layout 3
                                    Rebar stirrup2 = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, xStirrup2, vectorY, vectorX);
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup2 = stirrup2.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup2.ScaleToBox(xStirrup2, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));
                                    rebarShapeDrivenAccessorStirup2.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A3beam, DisplayUnitType.DUT_MILLIMETERS), length1 / 4, true, true, false);

                                    break;
                                };
                            case 1:
                                {
                                    Rebar stirrup = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, origin0, vectorY, vectorZ);
                                    //Create Layout
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup = stirrup.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A1beam, DisplayUnitType.DUT_MILLIMETERS), 0.4 * length1, true, false, true);
                                    rebarShapeDrivenAccessorStirup.ScaleToBox(origin0, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));

                                    XYZ xStirrup1 = new XYZ(origin0.X, origin0.Y - 0.4 * length1, origin0.Z);
                                    //Create Layout 2
                                    Rebar stirrup1 = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, xStirrup1, vectorY, vectorX);
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup1 = stirrup1.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup1.ScaleToBox(xStirrup1, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));
                                    rebarShapeDrivenAccessorStirup1.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A2beam, DisplayUnitType.DUT_MILLIMETERS), 0.6 * length1, true, true, false);

                                    break;
                                }
                            case 2:
                                {
                                    Rebar stirrup = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, origin0, vectorY, vectorZ);
                                    //Create Layout
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup = stirrup.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A1beam, DisplayUnitType.DUT_MILLIMETERS), length1, true, false, false);
                                    rebarShapeDrivenAccessorStirup.ScaleToBox(origin0, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));
                                    break;
                                }
                        }
                    }
                    if (vectorX.X == 1)
                    {
                        //PHIA DUOI : Duoi phai rebar1, duoi trai rebar2, cung chieu

                        XYZ origin1 = new XYZ(sectionPnts[0].X + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                            sectionPnts[0].Y + barType.BarDiameter / 2 + barType2.BarDiameter + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                            sectionPnts[0].Z + barType2.BarDiameter + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;
                        Rebar rebar1 = Rebar.CreateFromRebarShape(doc, rebarShape, barType, element, origin1, vectorX, vectorY);
                        RebarShapeDrivenAccessor rebarShapeDrivenAccessor1 = rebar1.GetShapeDrivenAccessor();
                        XYZ xmoi = vectorX * length1;
                        XYZ ymoi = vectorY;

                        rebarShapeDrivenAccessor1.ScaleToBox(origin1, xmoi, vectorZ);

                        XYZ origin2 = new XYZ(sectionPnts[1].X + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                           sectionPnts[1].Y - barType.BarDiameter / 2 - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                           sectionPnts[1].Z + barType2.BarDiameter + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;
                        Rebar rebar2 = Rebar.CreateFromRebarShape(doc, rebarShape, barType, element, origin1, vectorX, vectorY);
                        RebarShapeDrivenAccessor rebarShapeDrivenAccessor2 = rebar2.GetShapeDrivenAccessor();
                        rebarShapeDrivenAccessor2.ScaleToBox(origin2, xmoi, vectorZ);

                        double khoangcachyduoi = Math.Round((origin1.Y - origin2.Y) * (width + 2 * (barType.BarDiameter / 2 - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))) / (Global.nudThepduoi), 3);
                        if (Global.nudThepduoi >= 3)
                        {
                            for (int i = 1; i <= Global.nudThepduoi - 2; i++)
                            {
                                XYZ xnew1 = new XYZ(origin2.X, origin2.Y + khoangcachyduoi * i, origin2.Z);
                                Rebar thep1 = Rebar.CreateFromRebarShape(doc, rebarShape, barType, element, xnew1, vectorX, vectorY);
                                RebarShapeDrivenAccessor rebarShapeDrivenAccessornew1 = thep1.GetShapeDrivenAccessor();
                                rebarShapeDrivenAccessornew1.ScaleToBox(xnew1, xmoi, vectorZ);
                            }
                        }

                        //PHIA TREN tren trai,tren phai
                        XYZ origin3 = new XYZ(sectionPnts[2].X + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                           sectionPnts[2].Y - barType.BarDiameter / 2 - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                           sectionPnts[2].Z - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;
                        Rebar rebar3 = Rebar.CreateFromRebarShape(doc, rebarShape1, barType1, element, origin3, vectorX, vectorY);
                        RebarShapeDrivenAccessor rebarShapeDrivenAccessor3 = rebar3.GetShapeDrivenAccessor();
                        rebarShapeDrivenAccessor3.ScaleToBox(origin3, xmoi, -vectorZ);

                        XYZ origin4 = new XYZ(sectionPnts[3].X + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                              sectionPnts[3].Y + barType.BarDiameter / 2 + barType2.BarDiameter + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                              sectionPnts[3].Z - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;
                        Rebar rebar4 = Rebar.CreateFromRebarShape(doc, rebarShape1, barType1, element, origin4, vectorX, vectorY);
                        RebarShapeDrivenAccessor rebarShapeDrivenAccessor4 = rebar4.GetShapeDrivenAccessor();
                        rebarShapeDrivenAccessor4.ScaleToBox(origin4, xmoi, -vectorZ);
                        double khoangcachytren = Math.Round((origin4.Y - origin3.Y) * (width + 2 * (barType.BarDiameter / 2 - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))) / (Global.nudTheptren), 3);
                        if (Global.nudTheptren >= 3)
                        {
                            for (int i = 1; i <= Global.nudTheptren - 2; i++)
                            {
                                XYZ xnew1 = new XYZ(origin3.X, origin3.Y + khoangcachytren * i, origin3.Z);
                                Rebar thep1 = Rebar.CreateFromRebarShape(doc, rebarShape1, barType1, element, xnew1, vectorX, vectorY);
                                RebarShapeDrivenAccessor rebarShapeDrivenAccessornew1 = thep1.GetShapeDrivenAccessor();
                                rebarShapeDrivenAccessornew1.ScaleToBox(xnew1, xmoi, -vectorZ);
                            }
                        }

                        // thep dai
                        XYZ origin0 = new XYZ(sectionPnts[0].X + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                               sectionPnts[0].Y + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                               sectionPnts[0].Z + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;


                        switch (int.Parse(Global.botricotdaiBeam))
                        {
                            case 0:
                                {
                                    Rebar stirrup = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, origin0, vectorY, vectorZ);
                                    //Create Layout
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup = stirrup.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A1beam, DisplayUnitType.DUT_MILLIMETERS), length1 / 4, true, false, true);
                                    rebarShapeDrivenAccessorStirup.ScaleToBox(origin0, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));

                                    XYZ xStirrup1 = new XYZ(origin0.X + length1 / 4, origin0.Y, origin0.Z);
                                    //Create Layout 2
                                    Rebar stirrup1 = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, xStirrup1, vectorY, vectorX);
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup1 = stirrup1.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup1.ScaleToBox(xStirrup1, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));
                                    rebarShapeDrivenAccessorStirup1.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A2beam, DisplayUnitType.DUT_MILLIMETERS), length1 / 2, true, false, true);

                                    XYZ xStirrup2 = new XYZ(origin0.X + 3 * length1 / 4, origin0.Y, origin0.Z);
                                    //Create Layout 3
                                    Rebar stirrup2 = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, xStirrup2, vectorY, vectorX);
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup2 = stirrup2.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup2.ScaleToBox(xStirrup2, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));
                                    rebarShapeDrivenAccessorStirup2.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A3beam, DisplayUnitType.DUT_MILLIMETERS), length1 / 4, true, true, false);

                                    break;
                                };
                            case 1:
                                {
                                    Rebar stirrup = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, origin0, vectorY, vectorZ);
                                    //Create Layout
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup = stirrup.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A1beam, DisplayUnitType.DUT_MILLIMETERS), 0.4 * length1, true, false, true);
                                    rebarShapeDrivenAccessorStirup.ScaleToBox(origin0, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));

                                    XYZ xStirrup1 = new XYZ(origin0.X + 0.4 * length1, origin0.Y, origin0.Z);
                                    //Create Layout 2
                                    Rebar stirrup1 = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, xStirrup1, vectorY, vectorX);
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup1 = stirrup1.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup1.ScaleToBox(xStirrup1, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));
                                    rebarShapeDrivenAccessorStirup1.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A2beam, DisplayUnitType.DUT_MILLIMETERS), 0.6 * length1, true, true, false);

                                    break;
                                }
                            case 2:
                                {
                                    Rebar stirrup = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, origin0, vectorY, vectorZ);
                                    //Create Layout
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup = stirrup.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A1beam, DisplayUnitType.DUT_MILLIMETERS), length1, true, false, false);
                                    rebarShapeDrivenAccessorStirup.ScaleToBox(origin0, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));
                                    break;
                                }
                        }
                    }
                    if (vectorX.X == -1)
                    {
                        //PHIA DUOI : Duoi phai rebar1, duoi trai rebar2, cung chieu

                        XYZ origin1 = new XYZ(sectionPnts[0].X - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                            sectionPnts[0].Y - barType.BarDiameter / 2 - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                            sectionPnts[0].Z + barType2.BarDiameter + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;
                        Rebar rebar1 = Rebar.CreateFromRebarShape(doc, rebarShape, barType, element, origin1, vectorX, vectorY);
                        RebarShapeDrivenAccessor rebarShapeDrivenAccessor1 = rebar1.GetShapeDrivenAccessor();
                        XYZ xmoi = vectorX * length1;
                        XYZ ymoi = vectorY;

                        rebarShapeDrivenAccessor1.ScaleToBox(origin1, xmoi, vectorZ);

                        XYZ origin2 = new XYZ(sectionPnts[1].X - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                           sectionPnts[1].Y + barType.BarDiameter / 2 + barType2.BarDiameter + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                           sectionPnts[1].Z + barType2.BarDiameter + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;
                        Rebar rebar2 = Rebar.CreateFromRebarShape(doc, rebarShape, barType, element, origin1, vectorX, vectorY);
                        RebarShapeDrivenAccessor rebarShapeDrivenAccessor2 = rebar2.GetShapeDrivenAccessor();
                        rebarShapeDrivenAccessor2.ScaleToBox(origin2, xmoi, vectorZ);

                        double khoangcachyduoi = Math.Round((origin1.Y - origin2.Y) * (width + 2 * (barType.BarDiameter / 2 - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))) / (Global.nudThepduoi), 3);
                        if (Global.nudThepduoi >= 3)
                        {
                            for (int i = 1; i <= Global.nudThepduoi - 2; i++)
                            {
                                XYZ xnew1 = new XYZ(origin2.X, origin2.Y + khoangcachyduoi * i, origin2.Z);
                                Rebar thep1 = Rebar.CreateFromRebarShape(doc, rebarShape, barType, element, xnew1, vectorX, vectorY);
                                RebarShapeDrivenAccessor rebarShapeDrivenAccessornew1 = thep1.GetShapeDrivenAccessor();
                                rebarShapeDrivenAccessornew1.ScaleToBox(xnew1, xmoi, vectorZ);
                            }
                        }

                        //PHIA TREN tren trai,tren phai
                        XYZ origin3 = new XYZ(sectionPnts[2].X - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                               sectionPnts[2].Y + barType.BarDiameter / 2 + barType2.BarDiameter + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                               sectionPnts[2].Z - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;
                        Rebar rebar3 = Rebar.CreateFromRebarShape(doc, rebarShape1, barType1, element, origin3, vectorX, vectorY);
                        RebarShapeDrivenAccessor rebarShapeDrivenAccessor3 = rebar3.GetShapeDrivenAccessor();
                        rebarShapeDrivenAccessor3.ScaleToBox(origin3, xmoi, -vectorZ);

                        XYZ origin4 = new XYZ(sectionPnts[3].X - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                              sectionPnts[3].Y - barType.BarDiameter / 2 - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                              sectionPnts[3].Z - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;
                        Rebar rebar4 = Rebar.CreateFromRebarShape(doc, rebarShape1, barType1, element, origin4, vectorX, vectorY);
                        RebarShapeDrivenAccessor rebarShapeDrivenAccessor4 = rebar4.GetShapeDrivenAccessor();
                        rebarShapeDrivenAccessor4.ScaleToBox(origin4, xmoi, -vectorZ);
                        double khoangcachytren = Math.Round((origin4.Y - origin3.Y) * (width + 2 * (barType.BarDiameter / 2 - barType2.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))) / (Global.nudTheptren), 3);
                        if (Global.nudTheptren >= 3)
                        {
                            for (int i = 1; i <= Global.nudTheptren - 2; i++)
                            {
                                XYZ xnew1 = new XYZ(origin3.X, origin3.Y + khoangcachytren * i, origin3.Z);
                                Rebar thep1 = Rebar.CreateFromRebarShape(doc, rebarShape1, barType1, element, xnew1, vectorX, vectorY);
                                RebarShapeDrivenAccessor rebarShapeDrivenAccessornew1 = thep1.GetShapeDrivenAccessor();
                                rebarShapeDrivenAccessornew1.ScaleToBox(xnew1, xmoi, -vectorZ);
                            }
                        }

                        // thep dai
                        XYZ origin0 = new XYZ(sectionPnts[0].X + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                                   sectionPnts[0].Y - UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS),
                                   sectionPnts[0].Z + UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)) as XYZ;


                        switch (int.Parse(Global.botricotdaiBeam))
                        {
                            case 0:
                                {
                                    Rebar stirrup = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, origin0, vectorY, vectorZ);
                                    //Create Layout
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup = stirrup.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A1beam, DisplayUnitType.DUT_MILLIMETERS), length1 / 4, true, false, true);
                                    rebarShapeDrivenAccessorStirup.ScaleToBox(origin0, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));

                                    XYZ xStirrup1 = new XYZ(origin0.X - length1 / 4, origin0.Y, origin0.Z);
                                    //Create Layout 2
                                    Rebar stirrup1 = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, xStirrup1, vectorY, vectorX);
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup1 = stirrup1.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup1.ScaleToBox(xStirrup1, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));
                                    rebarShapeDrivenAccessorStirup1.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A2beam, DisplayUnitType.DUT_MILLIMETERS), length1 / 2, true, false, true);

                                    XYZ xStirrup2 = new XYZ(origin0.X - 3 * length1 / 4, origin0.Y, origin0.Z);
                                    //Create Layout 3
                                    Rebar stirrup2 = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, xStirrup2, vectorY, vectorX);
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup2 = stirrup2.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup2.ScaleToBox(xStirrup2, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));
                                    rebarShapeDrivenAccessorStirup2.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A3beam, DisplayUnitType.DUT_MILLIMETERS), length1 / 4, true, true, false);

                                    break;
                                };
                            case 1:
                                {
                                    Rebar stirrup = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, origin0, vectorY, vectorZ);
                                    //Create Layout
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup = stirrup.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A1beam, DisplayUnitType.DUT_MILLIMETERS), 0.4 * length1, true, false, true);
                                    rebarShapeDrivenAccessorStirup.ScaleToBox(origin0, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));

                                    XYZ xStirrup1 = new XYZ(origin0.X - 0.4 * length1, origin0.Y, origin0.Z);
                                    //Create Layout 2
                                    Rebar stirrup1 = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, xStirrup1, vectorY, vectorX);
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup1 = stirrup1.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup1.ScaleToBox(xStirrup1, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));
                                    rebarShapeDrivenAccessorStirup1.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A2beam, DisplayUnitType.DUT_MILLIMETERS), 0.6 * length1, true, true, false);

                                    break;
                                }
                            case 2:
                                {
                                    Rebar stirrup = Rebar.CreateFromRebarShape(doc, rebarShape2, barType2, element, origin0, vectorY, vectorZ);
                                    //Create Layout
                                    RebarShapeDrivenAccessor rebarShapeDrivenAccessorStirup = stirrup.GetShapeDrivenAccessor();
                                    rebarShapeDrivenAccessorStirup.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A1beam, DisplayUnitType.DUT_MILLIMETERS), length1, true, false, false);
                                    rebarShapeDrivenAccessorStirup.ScaleToBox(origin0, vectorY * (width - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS))
                                        , vectorZ * (height - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverBeam, DisplayUnitType.DUT_MILLIMETERS)));
                                    break;
                                }
                        }


                    }
                    trans.Commit();
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show(message);
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
}
