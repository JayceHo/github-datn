using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Structure;
using System.Data;
using System.Windows.Forms;

namespace DATN_CHUYEN_DE_REVITAPI.CalculationFloor
{
    [TransactionAttribute(TransactionMode.Manual)]
    class CreateRebarSlabAuto : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get Document
            Document doc = uidoc.Document;

            //Get Reference of Element

            FilteredElementCollector collector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Floors).WhereElementIsNotElementType();

            //SET A
            IList<ElementId> elementID = collector.ToElementIds() as IList<ElementId>;
            IList<ElementId> floorID = new List<ElementId>();
            IList<Reference> newrefs = new List<Reference>();
            IList<Element> element = new List<Element>();
            try
            {
                foreach (ElementId elementid in elementID)
                {
                    Element e1 = doc.GetElement(elementid);
                    Reference R = new Reference(e1);
                    Parameter markValue = e1.LookupParameter("Mark");
                    for (int i = 0; i <= Global.dataTable1.Rows.Count - 1; i++)
                    {
                        if (markValue.AsString() == Global.dataTable1.Rows[i][1].ToString())
                        {
                            floorID.Add(elementid);
                            newrefs.Add(R);
                            element.Add(e1);
                        }
                    }

                }

                uidoc.Selection.SetElementIds(floorID);
                uidoc.ShowElements(floorID);
                uidoc.RefreshActiveView();
            }
            catch (Exception e)
            {
                MessageBox.Show("Ban phai nhap du lieu da.");
            }

            foreach (Element element1 in element)
            {
                Reference R = new Reference(element1);
                Parameter markValue = element1.LookupParameter("Mark");
                string axtren = "300", axduoi = "300", aytren = "300", ayduoi = "300";
                try
                {
                    for (int i = 0; i <= Global.dataTable1.Rows.Count - 1; i++)
                    {

                        if ((markValue.AsString() == Global.dataTable1.Rows[i][1].ToString()))
                        {
                            for (int j = 0; j <= Global.dataTable2.Rows.Count - 1; j++)
                            {
                                if ((Global.dataTable1.Rows[i][0].ToString() == Global.dataTable2.Rows[j][0].ToString()))
                                {
                                    if (double.Parse(Global.dataTable2.Rows[j][1].ToString()) <= 0) axduoi = Global.dataTable2.Rows[j][8].ToString();
                                    if (double.Parse(Global.dataTable2.Rows[j][1].ToString()) > 0) axtren = Global.dataTable2.Rows[j][8].ToString();
                                }

                            }

                            for (int j = 0; j <= Global.dataTable2.Rows.Count - 1; j++)
                            {
                                if ((Global.dataTable1.Rows[i][0].ToString() == Global.dataTable3.Rows[j][0].ToString()))
                                {
                                    if (double.Parse(Global.dataTable3.Rows[j][1].ToString()) <= 0) ayduoi = Global.dataTable3.Rows[j][8].ToString();
                                    if (double.Parse(Global.dataTable3.Rows[j][1].ToString()) > 0) aytren = Global.dataTable3.Rows[j][8].ToString();
                                }

                            }
                        }
                    }
                }
                catch (Exception e) { };

                tao1(doc, uidoc, element1, axtren, aytren, axduoi, ayduoi);
            }
            return Result.Succeeded;
        }
        public Result tao1(Document doc, UIDocument uidoc, Element element, string axtren, string aytren, string axduoi, string ayduoi)
        {

            //Get Rebar shape
            RebarShape rebarshape = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarShape))
                .Cast<RebarShape>()
                .First(x => x.Name == "M_00");
            //Get Rebar Type
            RebarBarType barType = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .Cast<RebarBarType>()
                .First(x => x.Name == Global.LopDuoi.ToString());

            // Get Rebar shape
            RebarShape rebarshape2 = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarShape))
                .Cast<RebarShape>()
                .First(x => x.Name == "M_02");
            //Get Rebar Type
            RebarBarType barType2 = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .Cast<RebarBarType>()
                .First(x => x.Name == Global.LopTren.ToString());
            double cover = Global.Btbv;
            using (Transaction trans = new Transaction(doc, "Create bar: "))
            {
                trans.Start();

                BoundingBoxXYZ boundingBox = element.get_BoundingBox(null);
                XYZ x1 = new XYZ(boundingBox.Min.X + barType.BarDiameter + UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS),
                    boundingBox.Min.Y + barType.BarDiameter + UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS),
                    boundingBox.Min.Z + barType.BarDiameter + UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS));

                XYZ rebarLineEnd1 = new XYZ(boundingBox.Max.X - barType.BarDiameter - UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS)
                    , boundingBox.Min.Y + barType.BarDiameter + UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS)
                    , boundingBox.Min.Z + barType.BarDiameter + UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS));
                Line rebarLine1 = Line.CreateBound(x1, rebarLineEnd1);

                XYZ x2 = new XYZ(boundingBox.Min.X + barType.BarDiameter + UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS),
                    boundingBox.Min.Y + barType.BarDiameter + UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS),
                    boundingBox.Min.Z + barType.BarDiameter + UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS));
                XYZ rebarLineEnd2 = new XYZ(boundingBox.Min.X + barType.BarDiameter + UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS),
                    boundingBox.Max.Y - barType.BarDiameter - UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS),
                    boundingBox.Min.Z + barType.BarDiameter + UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS));
                Line rebarLine2 = Line.CreateBound(x2, rebarLineEnd2);

                XYZ x3 = new XYZ(boundingBox.Min.X + barType.BarDiameter + UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS),
                    boundingBox.Min.Y + barType.BarDiameter + UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS),
                    boundingBox.Max.Z - barType.BarDiameter - UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS));

                XYZ rebarLineEnd3 = new XYZ(boundingBox.Max.X - barType.BarDiameter - UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS)
                    , boundingBox.Min.Y + barType.BarDiameter + UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS)
                    , boundingBox.Max.Z - barType.BarDiameter - UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS));
                Line rebarLine3 = Line.CreateBound(x3, rebarLineEnd3);

                XYZ x4 = new XYZ(boundingBox.Min.X + barType.BarDiameter + UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS),
                    boundingBox.Min.Y + barType.BarDiameter + UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS),
                    boundingBox.Max.Z - barType.BarDiameter - UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS));
                XYZ rebarLineEnd4 = new XYZ(boundingBox.Min.X + barType.BarDiameter + UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS),
                    boundingBox.Max.Y - barType.BarDiameter - UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS),
                    boundingBox.Max.Z - barType.BarDiameter - UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS));
                Line rebarLine4 = Line.CreateBound(x4, rebarLineEnd4);
                // Create the line rebar
                IList<Curve> curves1 = new List<Curve>();
                curves1.Add(rebarLine1);
                IList<Curve> curves2 = new List<Curve>();
                curves2.Add(rebarLine2);
                IList<Curve> curves3 = new List<Curve>();
                curves3.Add(rebarLine3);
                IList<Curve> curves4 = new List<Curve>();
                curves4.Add(rebarLine4);


                Rebar rebar1 = Rebar.CreateFromCurvesAndShape(doc, rebarshape, barType, null, null, element, XYZ.BasisY, curves1,
                RebarHookOrientation.Right, RebarHookOrientation.Left);

                Rebar rebar2 = Rebar.CreateFromCurvesAndShape(doc, rebarshape, barType, null, null, element, XYZ.BasisX, curves2,
                RebarHookOrientation.Right, RebarHookOrientation.Left);


                XYZ zam = new XYZ(0, 0, -1);
                Rebar rebar3 = Rebar.CreateFromRebarShape(doc, rebarshape2, barType2, element, x3, XYZ.BasisX, zam);

                Rebar rebar4 = Rebar.CreateFromRebarShape(doc, rebarshape2, barType2, element, x4, XYZ.BasisY, zam);

                RebarShapeDrivenAccessor rebarShapeDrivenAccessor1 = rebar1.GetShapeDrivenAccessor();
                rebarShapeDrivenAccessor1.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(double.Parse(axduoi), DisplayUnitType.DUT_MILLIMETERS), boundingBox.Max.Y - boundingBox.Min.Y -
                    2 * barType.BarDiameter - 2 * UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS), true, true, true);

                RebarShapeDrivenAccessor rebarShapeDrivenAccessor2 = rebar2.GetShapeDrivenAccessor();
                rebarShapeDrivenAccessor2.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(double.Parse(ayduoi), DisplayUnitType.DUT_MILLIMETERS), boundingBox.Max.X - boundingBox.Min.X
                    - 2 * barType.BarDiameter - 2 * UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS), true, true, true);

                RebarShapeDrivenAccessor rebarShapeDrivenAccessor3 = rebar3.GetShapeDrivenAccessor();
                rebarShapeDrivenAccessor3.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(double.Parse(axtren), DisplayUnitType.DUT_MILLIMETERS), boundingBox.Max.Y - boundingBox.Min.Y
                   - 2 * barType2.BarDiameter - 2 * UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS), true, true, true);

                XYZ phuongx = new XYZ(boundingBox.Max.X - boundingBox.Min.X - 2 * barType.BarDiameter - 2 * UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS), 0, 0);
                rebarShapeDrivenAccessor3.ScaleToBox(x3, phuongx, zam);

                RebarShapeDrivenAccessor rebarShapeDrivenAccessor4 = rebar4.GetShapeDrivenAccessor();
                rebarShapeDrivenAccessor4.SetLayoutAsMaximumSpacing(UnitUtils.ConvertToInternalUnits(double.Parse(aytren), DisplayUnitType.DUT_MILLIMETERS), boundingBox.Max.X - boundingBox.Min.X
                   - 2 * barType2.BarDiameter - 2 * UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS), true, true, true);


                XYZ phuongy = new XYZ(0, boundingBox.Max.Y - boundingBox.Min.Y - 2 * barType.BarDiameter - 2 * UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS), 0);
                XYZ x5 = new XYZ(boundingBox.Max.X - barType.BarDiameter - UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS),
                       boundingBox.Min.Y + barType.BarDiameter + UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS),
                       boundingBox.Max.Z - barType.BarDiameter - UnitUtils.ConvertToInternalUnits(cover, DisplayUnitType.DUT_MILLIMETERS));
                rebarShapeDrivenAccessor4.ScaleToBox(x5, phuongy, zam);



                trans.Commit();
                return Result.Succeeded;
            }

            return Result.Succeeded;
        }
    }
}
