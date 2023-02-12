using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Diagnostics;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace DATN_CHUYEN_DE_REVITAPI
{
    [TransactionAttribute(TransactionMode.Manual)]
    class Load_Data : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapplication = commandData.Application;
            UIDocument uidocument = uiapplication.ActiveUIDocument;
            Document document = uidocument.Document;

            // set family is null
            Family family = null;
 
            // set path to folder for desired family

            DirectoryInfo multiPath = new DirectoryInfo(@"C:\Users\trant\OneDrive\Desktop\DATN\Family\");
            if (multiPath == null)
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    multiPath = new DirectoryInfo(dialog.FileName);
                }
            }


            FileInfo[] files = multiPath.GetFiles("*.rfa"); // grabs the files
            try
            {
                using (Transaction trans = new Transaction(document, "LoadFamily"))
                {
                    trans.Start();
                    // create for loop(vòng lặp) to go through all the families and print out
                    foreach (FileInfo file in files)
                    {
                        document.LoadFamily(multiPath + file.Name, out family);
                        Debug.Print(multiPath + file.Name);
                    }
                    Autodesk.Revit.UI.TaskDialog.Show("Done", string.Format("Family đã được load xong!"));
                    trans.Commit();
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                Autodesk.Revit.UI.TaskDialog.Show("Error", message);
                return Result.Failed;
            }
        }
    }
}
