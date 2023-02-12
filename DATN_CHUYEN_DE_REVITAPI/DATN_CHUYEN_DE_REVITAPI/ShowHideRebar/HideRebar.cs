using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;

namespace DATN_CHUYEN_DE_REVITAPI
{
    [TransactionAttribute(TransactionMode.Manual)]
    class HideRebar : IExternalCommand
    {
        UIApplication uiapp;
        UIDocument uidoc;
        Autodesk.Revit.ApplicationServices.Application app;
        public Document doc;
        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            app = uiapp.Application;
            doc = uidoc.Document;
            

            if (!doc.ActiveView.GetType().Equals(typeof(View3D)))
            {
                TaskDialog.Show("THXD", "View hiện tại không phải là view 3D");
                return Result.Cancelled;
            }

            try
            {
                // Create an ElementOwnerView filter with id of active view
                ElementOwnerViewFilter elementOwnerViewFilter = new ElementOwnerViewFilter(doc.ActiveView.Id);

                List<Rebar> listRebar = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Rebar)
                            .WhereElementIsNotElementType()
                    .OfClass(typeof(Rebar)).Cast<Rebar>().ToList();

                List<AreaReinforcement> listAreaRebar = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_AreaRein)
                            .WhereElementIsNotElementType()
                            .Cast<AreaReinforcement>()
                    .ToList();

                
                using (Transaction tran = new Transaction(doc))
                {
                    tran.Start("HideRebar");
                    int k = 0;
                    // Hide Rebar of Area 
                    foreach (AreaReinforcement item in listAreaRebar)
                    {
                        IList<ElementId> rebarInSystemIds = item.GetRebarInSystemIds();

                        for (int i = 0; i < rebarInSystemIds.Count; i++)
                        {
                            RebarInSystem ris = doc.GetElement(rebarInSystemIds[0]) as RebarInSystem;
                            ris.SetSolidInView(doc.ActiveView as View3D, false);
                            ris.SetUnobscuredInView(doc.ActiveView, false);
                        }
                    }
                    // Hide Rebar of Stregh
                    foreach (Element e in listRebar)
                    {
                        Rebar r = e as Rebar;
                        r.SetSolidInView(doc.ActiveView as View3D, false);
                        r.SetUnobscuredInView(doc.ActiveView, false);
                        k++;
                    }

                    
                    tran.Commit();
                    TaskDialog.Show("THXD", "Ẩn thành công");
                }

            }
            catch (Exception ex)
            {
                TaskDialog.Show("THXD", ex.Message);
                return Result.Failed;
            }
            return Result.Succeeded;
        }
        
    }

}
