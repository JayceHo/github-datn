using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Autodesk.Revit.Attributes;
namespace DATN_CHUYEN_DE_REVITAPI.TrienKhaiCot
{
    [TransactionAttribute(TransactionMode.Manual)]
    class ExternalApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            //Create tab
            string nameTab = "Bộ công cụ THXD";
            application.CreateRibbonTab(nameTab);

            //create panel
            RibbonPanel panel = application.CreateRibbonPanel(nameTab, "Bố trí cốt thép");
            
            //create button
            string path = Assembly.GetExecutingAssembly().Location;
            PushButtonData button = new PushButtonData("btnButton", "Rebar Column", path, "DATN_CHUYEN_DE_REVITAPI.TrienKhaiCot.CreateFormColumn");

            button.ToolTip = "Bố trí cốt thép cột";
            //add button to panel
            PushButton btn = panel.AddItem(button) as PushButton;

            //add icon to button

            Uri uriSource = new Uri(@"C:\Users\ASUS-PRO\Downloads\GOPCODE\DATN_CHUYEN_DE_REVITAPI\DATN_CHUYEN_DE_REVITAPI\icon\column.png");
            System.Windows.Media.Imaging.BitmapImage image = new System.Windows.Media.Imaging.BitmapImage(uriSource);
            btn.LargeImage = image;

            //create button
            PushButtonData button1 = new PushButtonData("btnButton1", "Rebar Beam", path, "DATN_CHUYEN_DE_REVITAPI.CreateBeam.CreateFormBeam");

            button1.ToolTip = "Bố trí cốt thép dầm";
            //add button to panel
            PushButton btn1 = panel.AddItem(button1) as PushButton;

            //add icon to button

            Uri uriSource1 = new Uri(@"C:\Users\ASUS-PRO\Downloads\GOPCODE\DATN_CHUYEN_DE_REVITAPI\DATN_CHUYEN_DE_REVITAPI\icon\beam.png");
            System.Windows.Media.Imaging.BitmapImage image1 = new System.Windows.Media.Imaging.BitmapImage(uriSource1);
            btn1.LargeImage = image1;

            //create button
            PushButtonData button2 = new PushButtonData("btnButton2", "Rebar Floor", path, "DATN_CHUYEN_DE_REVITAPI.CalculationFloor.CreateFormFloor");

            //add button to panel
            PushButton btn2 = panel.AddItem(button2) as PushButton;

            //add icon to button

            Uri uriSource2 = new Uri(@"C:\Users\ASUS-PRO\Downloads\GOPCODE\DATN_CHUYEN_DE_REVITAPI\DATN_CHUYEN_DE_REVITAPI\icon\floor.png");
            System.Windows.Media.Imaging.BitmapImage image2 = new System.Windows.Media.Imaging.BitmapImage(uriSource2);
            btn2.LargeImage = image2;
            // trien khai dam
            RibbonPanel panel1 = application.CreateRibbonPanel(nameTab, "Triển khai cốt thép");
            //create button
            PushButtonData button3 = new PushButtonData("btnButton3", "Detailing Beam", path, "DATN_CHUYEN_DE_REVITAPI.Command");

            //add button to panel
            PushButton btn3 = panel1.AddItem(button3) as PushButton;

            //add icon to button

            Uri uriSource3 = new Uri(@"C:\Users\ASUS-PRO\Downloads\GOPCODE\DATN_CHUYEN_DE_REVITAPI\DATN_CHUYEN_DE_REVITAPI\icon\beamlo.png");
            System.Windows.Media.Imaging.BitmapImage image3 = new System.Windows.Media.Imaging.BitmapImage(uriSource3);
            btn3.LargeImage = image3;
            RibbonPanel panel2 = application.CreateRibbonPanel(nameTab, "Công cụ hỗ trợ");
            //create button
            PushButtonData button4 = new PushButtonData("btnButton4", "Show Rebar", path, "DATN_CHUYEN_DE_REVITAPI.ShowRebar3D");

            //add button to panel
            PushButton btn4 = panel2.AddItem(button4) as PushButton;

            //add icon to button

            Uri uriSource4 = new Uri(@"C:\Users\ASUS-PRO\Downloads\GOPCODE\DATN_CHUYEN_DE_REVITAPI\DATN_CHUYEN_DE_REVITAPI\icon\show.png");
            System.Windows.Media.Imaging.BitmapImage image4 = new System.Windows.Media.Imaging.BitmapImage(uriSource4);
            btn4.LargeImage = image4;

            //create button
            PushButtonData button5 = new PushButtonData("btnButton5", "Hide Rebar", path, "DATN_CHUYEN_DE_REVITAPI.HideRebar");

            //add button to panel
            PushButton btn5 = panel2.AddItem(button5) as PushButton;

            //add icon to button

            Uri uriSource5 = new Uri(@"C:\Users\ASUS-PRO\Downloads\GOPCODE\DATN_CHUYEN_DE_REVITAPI\DATN_CHUYEN_DE_REVITAPI\icon\hide.png");
            System.Windows.Media.Imaging.BitmapImage image5 = new System.Windows.Media.Imaging.BitmapImage(uriSource5);
            btn5.LargeImage = image5;
            return Result.Succeeded;

        }
    }
}
