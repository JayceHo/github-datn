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
using Autodesk.Revit.UI.Selection;
using System.Windows.Forms;
using Form = System.Windows.Forms.Form;

namespace DATN_CHUYEN_DE_REVITAPI.CreateBeam
{
    [Transaction(TransactionMode.Manual)]
    public partial class Form_BarBeam : Form
    {
        private UIApplication uiapp;
        private UIDocument uidoc;
        private Document doc;
        private ExternalCommandData commandData;
        private string message;
        private ElementSet elements;
        List<RebarBarType> m_rebarBarTypes = new List<RebarBarType>();
        BindingSource m_barTypesBinding = new BindingSource();
        BindingSource m_barTypesBinding1 = new BindingSource();
        BindingSource m_barTypesBinding2 = new BindingSource();
        BindingSource rebarCoverbinding = new BindingSource();
        List<RebarCoverType> rebarCoverTypes = new List<RebarCoverType>();
        List<double> RebarCoverDouble = new List<double>();
        public Form_BarBeam(ExternalCommandData commandData, ref string message, ElementSet elements)
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

            m_barTypesBinding1.DataSource = m_rebarBarTypes;
            m_barTypesBinding.DataSource = m_rebarBarTypes;
            m_barTypesBinding2.DataSource = m_rebarBarTypes;

            cbbThepduoi.DataSource = m_barTypesBinding;
            cbbThepduoi.DisplayMember = "Name";
            cbbTheptren.DataSource = m_barTypesBinding1;
            cbbTheptren.DisplayMember = "Name";
            cbbStirrupBeam.DataSource = m_barTypesBinding2;
            cbbStirrupBeam.DisplayMember = "Name";
            filteredElementCollector = new FilteredElementCollector(doc);
            filteredElementCollector.OfClass(typeof(RebarCoverType));
            rebarCoverTypes = filteredElementCollector.Cast<RebarCoverType>().ToList<RebarCoverType>();
            foreach (var rCT in rebarCoverTypes)
            {
                RebarCoverDouble.Add(Math.Round(UnitUtils.ConvertFromInternalUnits(rCT.CoverDistance, DisplayUnitType.DUT_MILLIMETERS), 1));
            }
            rebarCoverbinding.DataSource = rebarCoverTypes;
            cbbRebarCover.DataSource = rebarCoverbinding;
            cbbRebarCover.DisplayMember = "Name";

            return Result.Succeeded;
        }

        private void Form_Stirrup_Load(object sender, EventArgs e)
        {
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
            cbbBoTri.Text = "2";
            tbA1.Text = "100";
            tbA2.Text = "200";
            tbA3.Text = "100";

            nudThepduoi.Value = 2;
            nudTheptren.Value = 2;
            tbUonphaiduoi.Text = "0";
            tbUontraiduoi.Text = "0";
            tbUonphaitren.Text = "300";
            tbUontraitren.Text = "300";
            Execute(commandData, ref message, elements);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cbbKieuCotDai_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                e.Graphics.DrawImage(imageList1.Images[e.Index], e.Bounds.Left, e.Bounds.Top);
            }
        }

        private void cbbKieuCotDai_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = imageList1.ImageSize.Height;
            e.ItemHeight = imageList1.ImageSize.Width;
        }

        private void cbbBoTri_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                e.Graphics.DrawImage(imageList2.Images[e.Index], e.Bounds.Left, e.Bounds.Top);
            }
        }

        private void cbbBoTri_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = imageList2.ImageSize.Height;
            e.ItemHeight = imageList2.ImageSize.Width;
        }

        private void cbbBoTri_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbbBoTri.SelectedIndex == 0)
            {
                tbA1.Enabled = true;
                tbA2.Enabled = true;
                tbA3.Enabled = true;
            }
            if (cbbBoTri.SelectedIndex == 1)
            {
                tbA3.Enabled = false;
            }
            if (cbbBoTri.SelectedIndex == 2)
            {
                tbA1.Enabled = false;
                tbA3.Enabled = false;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Global.Thepduoi = double.Parse(cbbThepduoi.Text);
            Global.nudThepduoi = double.Parse(nudThepduoi.Text);
            Global.Thepduoiuontrai = double.Parse(tbUontraiduoi.Text);
            Global.Thepduoiuonphai = double.Parse(tbUonphaiduoi.Text);

            Global.Theptren = double.Parse(cbbTheptren.Text);
            Global.nudTheptren = double.Parse(nudTheptren.Text);
            Global.Theptrenuontrai = double.Parse(tbUontraitren.Text);
            Global.Theptrenuonphai = double.Parse(tbUonphaitren.Text);

            Global.StirrupBeam = double.Parse(cbbStirrupBeam.Text);

            Global.Beam_RebarCover = cbbRebarCover.Text;
            Global.CoverBeam = RebarCoverDouble[cbbRebarCover.SelectedIndex];
            Global.kieucotdaiBeam = cbbKieuCotDai.Text;
            Global.botricotdaiBeam = cbbBoTri.Text;
            Global.A2beam = double.Parse(tbA2.Text);
            if (tbA1.Enabled = true) { Global.A1beam = double.Parse(tbA1.Text); }
            if (tbA3.Enabled = true) { Global.A3beam = double.Parse(tbA3.Text); }

            Global.IsFormBeamOk = true;
            this.Close();

        }
    }
}
