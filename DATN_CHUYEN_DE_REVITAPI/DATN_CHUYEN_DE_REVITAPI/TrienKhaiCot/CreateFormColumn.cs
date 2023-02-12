using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace DATN_CHUYEN_DE_REVITAPI.TrienKhaiCot
{
    [TransactionAttribute(TransactionMode.Manual)]
    class CreateFormColumn : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;


            Form_Column form_Column = new Form_Column(commandData, ref message, elements);
            form_Column.ShowDialog();

            if (Global.IsFormColumnOk == true)
            {


                try
                {
                    //Select all columns in the project
                    IList<Reference> newrefs1 = new List<Reference>();
                    columnISelFilter SF = new columnISelFilter();
                    IList<Reference> newrefs = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, SF, "ADD/REMOVE column elements") as IList<Reference>;

                    //FilteredElementCollector collector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralColumns).WhereElementIsNotElementType();

                    //SET A
                    // IList<ElementId> columnIDS = collector.ToElementIds() as IList<ElementId>;

                    //Highlight all columns in the project
                    IList<ElementId> columnIDS = new List<ElementId>();
                    foreach (Reference ref1 in newrefs)
                    {
                        ElementId eid = ref1.ElementId;
                        columnIDS.Add(eid);
                    }

                    uidoc.Selection.SetElementIds(columnIDS);
                    uidoc.ShowElements(columnIDS);
                    uidoc.RefreshActiveView();

                    //   IList<Reference> newrefs = new List<Reference>();
                    IList<Element> element = new List<Element>();
                    foreach (ElementId elementId in columnIDS)
                    {
                        Element e1 = doc.GetElement(elementId);
                        element.Add(e1);
                        Reference R = new Reference(e1);
                        newrefs.Add(R);
                    }

                    // columnISelFilter SF = new columnISelFilter();

                    //SET B
                    // IList<Reference> refIDS = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, SF, "ADD/REMOVE column elements", newrefs) as IList<Reference>;

                    foreach (Element element1 in element)
                    {
                        CreateRebarColumn(doc, uidoc, element1);
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
        public class columnISelFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralColumns)
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
        public Result CreateRebarColumn(Document doc, UIDocument uidoc, Element element)
        {

            try
            {
                Parameter Length = element.get_Parameter(BuiltInParameter.INSTANCE_LENGTH_PARAM);
                double a = Length.AsDouble();

                //Get information of element
                ElementId elementIdType = element.GetTypeId();
                ElementType elementType = doc.GetElement(elementIdType) as ElementType;



                //Get Rebar shape
                RebarShape rebarshape = new FilteredElementCollector(doc)
                    .OfClass(typeof(RebarShape))
                    .Cast<RebarShape>()
                    .First(x => x.Name == "M_00");
                //Get Rebar Type
                RebarBarType Bartype = new FilteredElementCollector(doc)
                    .OfClass(typeof(RebarBarType))
                    .Cast<RebarBarType>()
                    .First(x => x.Name == Global.Rebar.ToString());
                //Get Rebar shape
                RebarShape rebarShape1 = new FilteredElementCollector(doc)
                    .OfClass(typeof(RebarShape))
                    .Cast<RebarShape>()
                    .First(x => x.Name == Global.kieucotdai);
                //Get Rebar Type
                RebarBarType barType1 = new FilteredElementCollector(doc)
                    .OfClass(typeof(RebarBarType))
                    .Cast<RebarBarType>()
                    .First(x => x.Name == Global.StirrupColumn.ToString());


                LocationPoint locPoint = element.Location as LocationPoint;
                XYZ centerPoint = locPoint.Point;

                XYZ xvec = new XYZ(0, 1, 0);
                XYZ yvec = new XYZ(1, 0, 0);

                using (Transaction trans = new Transaction(doc, "Create bar: "))
                {
                    trans.Start();
                    //Get Parameter Rebar Cover
                    FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc);
                    filteredElementCollector.OfClass(typeof(RebarCoverType));
                    List<RebarCoverType> rebarCoverTypes = filteredElementCollector.Cast<RebarCoverType>().ToList<RebarCoverType>();
                    RebarCoverType rct = rebarCoverTypes[1];
                    foreach (RebarCoverType r1 in rebarCoverTypes)
                    {
                        if (r1.Name == Global.Colum_RebarCover) rct = r1;
                    }
                    Parameter top = element.LookupParameter("Rebar Cover - Top Face");
                    Parameter bot = element.LookupParameter("Rebar Cover - Bottom Face");
                    Parameter side = element.LookupParameter("Rebar Cover - Other Faces");
                    top.Set(rct.Id);
                    bot.Set(rct.Id);
                    side.Set(rct.Id);

                    BoundingBoxXYZ boundingBox = element.get_BoundingBox(null);

                    XYZ x1 = new XYZ(boundingBox.Min.X + Bartype.BarDiameter + UnitUtils.ConvertToInternalUnits(Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS),
                        boundingBox.Min.Y + Bartype.BarDiameter + UnitUtils.ConvertToInternalUnits(Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS),
                        boundingBox.Min.Z);
                    XYZ x2 = new XYZ(boundingBox.Max.X - Bartype.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS),
                        boundingBox.Min.Y + Bartype.BarDiameter + UnitUtils.ConvertToInternalUnits(Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS),
                        boundingBox.Min.Z);
                    XYZ x3 = new XYZ(boundingBox.Max.X - Bartype.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS),
                        boundingBox.Max.Y - Bartype.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS),
                        boundingBox.Min.Z);
                    XYZ x4 = new XYZ(boundingBox.Min.X + Bartype.BarDiameter + UnitUtils.ConvertToInternalUnits(Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS),
                        boundingBox.Max.Y - Bartype.BarDiameter - UnitUtils.ConvertToInternalUnits(Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS),
                        boundingBox.Min.Z);

                    LocationPoint location = element.Location as LocationPoint;
                    XYZ x0 = location.Point;


                    double khoangcachx = (boundingBox.Max.X - boundingBox.Min.X) / Global.Nb;
                    if (Global.Nb >= 3)
                    {

                        for (int i = 1; i <= Global.Nb - 2; i++)
                        {
                            XYZ xnew1 = new XYZ(x1.X + khoangcachx * i, x1.Y, x1.Z);
                            XYZ xnew4 = new XYZ(x4.X + khoangcachx * i, x4.Y, x4.Z);
                            Rebar thep1 = Rebar.CreateFromRebarShape(doc, rebarshape, Bartype, element, xnew1, XYZ.BasisZ, XYZ.BasisX);
                            Rebar thep2 = Rebar.CreateFromRebarShape(doc, rebarshape, Bartype, element, xnew4, XYZ.BasisZ, XYZ.BasisX);
                            RebarShapeDrivenAccessor rebarShapeDrivenAccessornew1 = thep1.GetShapeDrivenAccessor();
                            RebarShapeDrivenAccessor rebarShapeDrivenAccessornew2 = thep2.GetShapeDrivenAccessor();
                            XYZ phuongx = new XYZ(0, 0, boundingBox.Max.Z - boundingBox.Min.Z + Global.noithep * Bartype.BarDiameter);
                            rebarShapeDrivenAccessornew1.ScaleToBox(xnew1, phuongx, XYZ.BasisX);
                            rebarShapeDrivenAccessornew2.ScaleToBox(xnew4, phuongx, XYZ.BasisX);
                        }
                    }
                    double khoangcachy = (boundingBox.Max.Y - boundingBox.Min.Y) / Global.Nh;
                    if (Global.Nh >= 3)
                    {

                        for (int i = 1; i <= Global.Nh - 2; i++)
                        {
                            XYZ xnew1 = new XYZ(x1.X, x1.Y + khoangcachy * i, x1.Z);
                            XYZ xnew2 = new XYZ(x2.X, x2.Y + khoangcachy * i, x2.Z);
                            Rebar thep1 = Rebar.CreateFromRebarShape(doc, rebarshape, Bartype, element, xnew1, XYZ.BasisZ, XYZ.BasisY);
                            Rebar thep2 = Rebar.CreateFromRebarShape(doc, rebarshape, Bartype, element, xnew2, XYZ.BasisZ, XYZ.BasisY);
                            RebarShapeDrivenAccessor rebarShapeDrivenAccessornew1 = thep1.GetShapeDrivenAccessor();
                            RebarShapeDrivenAccessor rebarShapeDrivenAccessornew2 = thep2.GetShapeDrivenAccessor();
                            XYZ phuongx = new XYZ(0, 0, boundingBox.Max.Z - boundingBox.Min.Z + Global.noithep * Bartype.BarDiameter);
                            rebarShapeDrivenAccessornew1.ScaleToBox(xnew1, phuongx, XYZ.BasisY);
                            rebarShapeDrivenAccessornew2.ScaleToBox(xnew2, phuongx, XYZ.BasisY);
                        }
                    }

                    XYZ rebarLineEnd1 = new XYZ(x1.X, x1.Y, x1.Z + a + Global.noithep * Bartype.BarDiameter);
                    Line rebarLine1 = Line.CreateBound(x1, rebarLineEnd1);
                    XYZ rebarLineEnd2 = new XYZ(x2.X, x2.Y, x2.Z + a + Global.noithep * Bartype.BarDiameter);
                    Line rebarLine2 = Line.CreateBound(x2, rebarLineEnd2);
                    XYZ rebarLineEnd3 = new XYZ(x3.X, x3.Y, x3.Z + a + Global.noithep * Bartype.BarDiameter);
                    Line rebarLine3 = Line.CreateBound(x3, rebarLineEnd3);
                    XYZ rebarLineEnd4 = new XYZ(x4.X, x4.Y, x4.Z + a + Global.noithep * Bartype.BarDiameter);
                    Line rebarLine4 = Line.CreateBound(x4, rebarLineEnd4);
                    // Create the line rebar

                    IList<Curve> curves1 = new List<Curve>();
                    IList<Curve> curves2 = new List<Curve>();
                    IList<Curve> curves3 = new List<Curve>();
                    IList<Curve> curves4 = new List<Curve>();

                    curves1.Add(rebarLine1);
                    curves2.Add(rebarLine2);
                    curves3.Add(rebarLine3);
                    curves4.Add(rebarLine4);


                    Rebar rebar1 = Rebar.CreateFromCurvesAndShape(doc, rebarshape, Bartype, null, null, element, x0, curves1,
                        RebarHookOrientation.Right, RebarHookOrientation.Left);
                    Rebar rebar2 = Rebar.CreateFromCurvesAndShape(doc, rebarshape, Bartype, null, null, element, x0, curves2,
                        RebarHookOrientation.Right, RebarHookOrientation.Left);
                    Rebar rebar3 = Rebar.CreateFromCurvesAndShape(doc, rebarshape, Bartype, null, null, element, x0, curves3,
                        RebarHookOrientation.Right, RebarHookOrientation.Left);
                    Rebar rebar4 = Rebar.CreateFromCurvesAndShape(doc, rebarshape, Bartype, null, null, element, x0, curves4,
                        RebarHookOrientation.Right, RebarHookOrientation.Left);
                    // Create Stirrup
                    XYZ xvec1 = new XYZ((boundingBox.Max.X - boundingBox.Min.X) - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverColumn
                        , DisplayUnitType.DUT_MILLIMETERS), 0, 0);
                    XYZ yvec1 = new XYZ(0, (boundingBox.Max.Y - boundingBox.Min.Y) - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverColumn
                        , DisplayUnitType.DUT_MILLIMETERS), 0);

                    switch (int.Parse(Global.botricotdai))
                    {
                        case 0:
                            {
                                Rebar stirrup = Rebar.CreateFromRebarShape(doc, rebarShape1, barType1, element, x1, XYZ.BasisX, XYZ.BasisY);
                                //Create Layout
                                RebarShapeDrivenAccessor rebarShapeDrivenAccessor = stirrup.GetShapeDrivenAccessor();
                                XYZ orgin = new XYZ(boundingBox.Min.X + UnitUtils.ConvertToInternalUnits(Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS),
                                boundingBox.Min.Y + UnitUtils.ConvertToInternalUnits(Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS),
                                boundingBox.Min.Z + UnitUtils.ConvertToInternalUnits(Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS));

                                rebarShapeDrivenAccessor.ScaleToBox(orgin, xvec1, yvec1);
                                rebarShapeDrivenAccessor.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A1, DisplayUnitType.DUT_MILLIMETERS), a / 4, true, true, true);

                                XYZ xmoi = new XYZ(orgin.X, orgin.Y, orgin.Z + a / 4);

                                //Create Layout 2
                                Rebar stirrup1 = Rebar.CreateFromRebarShape(doc, rebarShape1, barType1, element, x1, xvec1, yvec1);
                                RebarShapeDrivenAccessor rebarShapeDrivenAccessor1 = stirrup1.GetShapeDrivenAccessor();
                                rebarShapeDrivenAccessor1.ScaleToBox(xmoi, xvec1, yvec1);
                                rebarShapeDrivenAccessor1.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A2, DisplayUnitType.DUT_MILLIMETERS), a / 2, true, true, true);

                                XYZ xmoi2 = new XYZ(orgin.X, orgin.Y, orgin.Z + 3 * a / 4);
                                //Create Layout 3
                                Rebar stirrup2 = Rebar.CreateFromRebarShape(doc, rebarShape1, barType1, element, x1, xvec1, yvec1);
                                RebarShapeDrivenAccessor rebarShapeDrivenAccessor2 = stirrup2.GetShapeDrivenAccessor();
                                rebarShapeDrivenAccessor2.ScaleToBox(xmoi2, xvec1, yvec1);
                                rebarShapeDrivenAccessor2.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A3, DisplayUnitType.DUT_MILLIMETERS),
                                    a / 4 - UnitUtils.ConvertToInternalUnits(2 * Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS), true, true, true);

                                break;
                            };
                        case 1:
                            {
                                Rebar stirrup = Rebar.CreateFromRebarShape(doc, rebarShape1, barType1, element, x1, XYZ.BasisX, XYZ.BasisY);
                                //Create Layout
                                RebarShapeDrivenAccessor rebarShapeDrivenAccessor = stirrup.GetShapeDrivenAccessor();
                                XYZ orgin = new XYZ(boundingBox.Min.X + UnitUtils.ConvertToInternalUnits(Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS),
                                boundingBox.Min.Y + UnitUtils.ConvertToInternalUnits(Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS),
                                boundingBox.Min.Z + UnitUtils.ConvertToInternalUnits(Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS));

                                rebarShapeDrivenAccessor.ScaleToBox(orgin, xvec1, yvec1);
                                rebarShapeDrivenAccessor.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A1, DisplayUnitType.DUT_MILLIMETERS), 0.4 * a, true, true, true);

                                XYZ xmoi = new XYZ(orgin.X, orgin.Y, orgin.Z + 0.4 * a);

                                //Create Layout 2
                                Rebar stirrup1 = Rebar.CreateFromRebarShape(doc, rebarShape1, barType1, element, x1, xvec1, yvec1);
                                RebarShapeDrivenAccessor rebarShapeDrivenAccessor1 = stirrup1.GetShapeDrivenAccessor();
                                rebarShapeDrivenAccessor1.ScaleToBox(xmoi, xvec1, yvec1);
                                rebarShapeDrivenAccessor1.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A2, DisplayUnitType.DUT_MILLIMETERS), 0.6 * a - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS) + barType1.BarDiameter, true, true, true);
                                break;
                            }
                        case 2:
                            {
                                Rebar stirrup = Rebar.CreateFromRebarShape(doc, rebarShape1, barType1, element, x1, XYZ.BasisX, XYZ.BasisY);
                                //Create Layout
                                RebarShapeDrivenAccessor rebarShapeDrivenAccessor = stirrup.GetShapeDrivenAccessor();
                                XYZ orgin = new XYZ(boundingBox.Min.X + UnitUtils.ConvertToInternalUnits(Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS),
                                boundingBox.Min.Y + UnitUtils.ConvertToInternalUnits(Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS),
                                boundingBox.Min.Z + UnitUtils.ConvertToInternalUnits(Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS));

                                rebarShapeDrivenAccessor.ScaleToBox(orgin, xvec1, yvec1);
                                rebarShapeDrivenAccessor.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(Global.A2, DisplayUnitType.DUT_MILLIMETERS), a - 2 * UnitUtils.ConvertToInternalUnits(Global.CoverColumn, DisplayUnitType.DUT_MILLIMETERS) + barType1.BarDiameter, true, true, true);
                                break;
                            }
                    }

                    trans.Commit();
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {

                return Result.Failed;
            }
        }
    }
}
