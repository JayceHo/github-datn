using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DATN_CHUYEN_DE_REVITAPI.CalculationFloor
{
    [TransactionAttribute(TransactionMode.Manual)]
    class CreateFormFloor : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            Form_Floor form_Floor = new Form_Floor(commandData, ref message, elements);
            form_Floor.ShowDialog();

            return Result.Succeeded;
        }
    }
}
