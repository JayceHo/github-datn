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
using System.Data;
using Autodesk.Revit.UI.Selection;
using System.Windows.Forms;
using Form = System.Windows.Forms.Form;

namespace DATN_CHUYEN_DE_REVITAPI.TrienKhaiCot
{
    public partial class Form_Column : Form
    {
        private UIApplication uiapp;

        private UIDocument uidoc;
        private Document doc;
        private ExternalCommandData commandData;
        private string message;
        private ElementSet elements;
        List<RebarBarType> m_rebarBarTypes = new List<RebarBarType>();
        List<RebarShape> m_rebarShapes = new List<RebarShape>();
        List<RebarCoverType> rebarCoverTypes = new List<RebarCoverType>();
        List<RebarHookType> m_rebarHookTypes = new List<RebarHookType>();
        BindingSource m_barTypesBinding = new BindingSource();
        BindingSource m_barTypesBinding1 = new BindingSource();
        BindingSource m_shapesBinding = new BindingSource();
        BindingSource rebarCoverbinding = new BindingSource();
        BindingSource rebarHookTypebinding = new BindingSource();
        List<double> RebarCoverDouble = new List<double>();
        public Form_Column(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            InitializeComponent();
            this.commandData = commandData;
            this.elements = elements;
            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            doc = uidoc.Document;

        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc);
            filteredElementCollector.OfClass(typeof(RebarBarType));
            m_rebarBarTypes = filteredElementCollector.Cast<RebarBarType>().ToList<RebarBarType>();


            filteredElementCollector = new FilteredElementCollector(doc);
            filteredElementCollector.OfClass(typeof(RebarShape));
            m_rebarShapes = filteredElementCollector.Cast<RebarShape>().ToList<RebarShape>();

            m_barTypesBinding.DataSource = m_rebarBarTypes;
            m_shapesBinding.DataSource = m_rebarShapes;
            m_barTypesBinding1.DataSource = m_rebarBarTypes;

            cbbRebar.DataSource = m_barTypesBinding;
            cbbRebar.DisplayMember = "Name";
            //  cbbRebar.Sorted = true;
            cbbStirrup.DataSource = m_barTypesBinding1;
            cbbStirrup.DisplayMember = "Name";
            //  cbbStirrup.Sorted = true;

            filteredElementCollector = new FilteredElementCollector(doc);
            filteredElementCollector.OfClass(typeof(RebarCoverType));
            rebarCoverTypes = filteredElementCollector.Cast<RebarCoverType>().ToList<RebarCoverType>();
            foreach (var rCT in rebarCoverTypes)
            {
                RebarCoverDouble.Add(Math.Round(UnitUtils.ConvertFromInternalUnits(rCT.CoverDistance, DisplayUnitType.DUT_MILLIMETERS), 1));
            }
            filteredElementCollector = new FilteredElementCollector(doc);
            filteredElementCollector.OfClass(typeof(RebarHookType));
            m_rebarHookTypes = filteredElementCollector.Cast<RebarHookType>().ToList<RebarHookType>();
            rebarHookTypebinding.DataSource = m_rebarHookTypes;

            rebarCoverbinding.DataSource = rebarCoverTypes;
            // cbbRebarCover.Sorted = true;
            cbbRebarCover.DataSource = rebarCoverbinding;
            cbbRebarCover.DisplayMember = "Name";
            // cbbHook1.DataSource = rebarHookTypebinding;
            // cbbHook1.DisplayMember = "Name";
            // cbbHook2.DataSource = m_rebarHookTypes;
            //  cbbHook2.DisplayMember = "Name";
            return Result.Succeeded;
        }
       
        private void comboBox2_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                e.Graphics.DrawImage(imageList1.Images[e.Index], e.Bounds.Left, e.Bounds.Top);
            }
        }

        private void comboBox2_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = imageList1.ImageSize.Height;
            e.ItemHeight = imageList1.ImageSize.Width;
        }

        private void comboBox3_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                e.Graphics.DrawImage(imageList2.Images[e.Index], e.Bounds.Left, e.Bounds.Top);
            }
        }

        private void comboBox3_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = imageList2.ImageSize.Height;
            e.ItemHeight = imageList2.ImageSize.Width;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnVe_Click(object sender, EventArgs e)
        {
            Global.Rebar = double.Parse(cbbRebar.Text);
            Global.Nb = int.Parse(nudNb.Text);
            Global.Nh = int.Parse(nudNh.Text);
            Global.StirrupColumn = double.Parse(cbbStirrup.Text);
            Global.Colum_RebarCover = cbbRebarCover.Text;
            Global.CoverColumn = RebarCoverDouble[cbbRebarCover.SelectedIndex];
            Global.kieucotdai = cbbKieuCotDai.Text;
            Global.botricotdai = cbbBoTri.Text;
            Global.noithep = double.Parse(tbNoiThep.Text);
            Global.A2 = double.Parse(tbA2.Text);
            if (tbA1.Enabled = true) { Global.A1 = double.Parse(tbA1.Text); }
            if (tbA3.Enabled = true) { Global.A3 = double.Parse(tbA3.Text); }

            Global.IsFormColumnOk = true;
            this.Close();

        }

        private void cbbRebarCover_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cbbBoTri_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (cbbBoTri.SelectedIndex == 0)
            {
                picKieu3.Visible = true;
                picKieu1.Visible = false;
                picKieu2.Visible = false;
                tbA1.Enabled = true;
                tbA2.Enabled = true;
                tbA3.Enabled = true;
            }
            if (cbbBoTri.SelectedIndex == 1)
            {
                picKieu3.Visible = false;
                picKieu1.Visible = false;
                picKieu2.Visible = true;
                tbA3.Enabled = false;
            }
            if (cbbBoTri.SelectedIndex == 2)
            {
                picKieu3.Visible = false;
                picKieu1.Visible = true;
                picKieu2.Visible = false;
                tbA1.Enabled = false;
                tbA3.Enabled = false;
            }
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void Form_Column_Load(object sender, EventArgs e)
        {
            picKieu3.Visible = true;
            picKieu1.Visible = false;
            picKieu2.Visible = false;
            Execute(commandData, ref message, elements);
            List<string> img = new List<string>() { "M_T1", "M_T2", "M_T6" };

            cbbKieuCotDai.DataSource = img;
            cbbKieuCotDai.Width = (int)imageList1.ImageSize.Width + 64;
            cbbKieuCotDai.MaxDropDownItems = imageList1.Images.Count;

            List<string> img1 = new List<string>();
            for (int i = 0; i <= imageList2.Images.Count - 1; i++)
            {
                string a = i.ToString();
                img1.Add(a);
            }
            cbbBoTri.DataSource = img1;
            cbbBoTri.Width = (int)imageList2.ImageSize.Width + 64;
            cbbBoTri.MaxDropDownItems = imageList2.Images.Count;
            //Dim img[imageList1.Images.Count - 1];
            // cbbStirrup.Items.AddRange(img[1]);
            tbNoiThep.Text = "40";
            tbA1.Text = "100";
            tbA2.Text = "200";
            tbA3.Text = "100";
        }
    }
}
