using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DATN_CHUYEN_DE_REVITAPI.TrienKhaiDam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DATN_CHUYEN_DE_REVITAPI
{
    /// <summary>
    /// Interaction logic for SettingsForm.xaml
    /// </summary>
    public partial class SettingsForm : UserControl
    {
        public Document mDoc { get; set; }
        public Window mWindow { get; set; }
        public FamilySymbol mSym { get; set; }
        public SettingsForm(Document mDoc, Window window)
        {
            InitializeComponent();
            this.mDoc = mDoc;
            this.mWindow = window;
        }

        //List cho tag thép đơn
        List<FamilySymbol> listTagSingleRebar = new List<FamilySymbol>();
        List<string> listShowTagSingleRebar = new List<string>();
        //List cho tag thép đa
        List<MultiReferenceAnnotationType> listTagMultiRebar = new List<MultiReferenceAnnotationType>();
        List<string> listShowTagMultiRebar = new List<string>();
        //List cho tag thép đai
        List<FamilySymbol> listTagStirrup = new List<FamilySymbol>();
        List<string> listShowTagStirrup = new List<string>();
        //List cho Dim style
        List<DimensionType> listDimtype = new List<DimensionType>();
        List<string> listShowDimtype = new List<string>();
        //List cho View TemPlate
        List<View> listViewTemplate = new List<View>();
        List<View> listViewTemplateMCN = new List<View>();
        List<string> listShowViewTemplate = new List<string>();
        // List Spot Dimmention
        List<SpotDimensionType> lisSpotDim = new List<SpotDimensionType>();
        List<string> listShowSpotDim = new List<string>();
        // List cho View Type
        List<ViewFamilyType> listViewType = new List<ViewFamilyType>();
        List<string> listShowViewType = new List<string>();
        // List cho Title Block
        List<FamilySymbol> listTitleBlock = new List<FamilySymbol>();
        List<string> listShowTitleBlock = new List<string>();

        // List cho Legend
        List<View> listLegends = new List<View>();
        List<string> listShowLegends = new List<string>();

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.mSym = cbSStandardRebar.SelectedItem as FamilySymbol;

            int indexCbSStandardRebar = cbSStandardRebar.SelectedIndex;
            int MultiRebarTagIndex = cbMStandardRebar.SelectedIndex;
            int indexCbStirupTag = cbStirrupt.SelectedIndex;
            int indexCbDimtyle = cbDIMStyle.SelectedIndex;
            int indexCbViewTemplate = cbViewTemplate.SelectedIndex;
            int indexCbViewTemplateMCN = cbViewTemplateMCN.SelectedIndex;
            int indexCbViewType = cbViewType.SelectedIndex;

            int indexCbTitleblock = cbTitleblock.SelectedIndex;
            int indexCbLegend = cbLegends.SelectedIndex;

            string indexTxtSheetName = txtSheetName.Text;


            if (indexCbLegend > -1)
            {
                Global.strLegend = cbLegends.Text;
            }
            

            if (indexTxtSheetName != "")
            {
                Global.strSheetName = indexTxtSheetName;
            }

            if (indexCbTitleblock > -1)
            {

                Global.strTitleBlock = cbTitleblock.SelectedItem.ToString();

            }

            if (indexCbSStandardRebar > -1)
            {
                Reference singleStandard = new Reference(listTagSingleRebar[indexCbSStandardRebar]);
                string strSingleStandard = singleStandard.ConvertToStableRepresentation(mDoc);
                Settings1.Default.SingleStandardRebar = listTagSingleRebar[indexCbSStandardRebar].UniqueId;

            }

            
            if (indexCbStirupTag > -1)
            {
                Reference Stirup = new Reference(listTagStirrup[indexCbStirupTag]);
                string strStirup = Stirup.ConvertToStableRepresentation(mDoc);
                Settings1.Default.StirruptRebar = listTagStirrup[indexCbStirupTag].UniqueId;

            }

            if (indexCbDimtyle > -1)
            {
                Settings1.Default.DIMStyle = listDimtype[cbDIMStyle.SelectedIndex].Name;
            }
            if (indexCbViewTemplate > -1)
            {
                Settings1.Default.ViewTemplate = listViewTemplate[cbViewTemplate.SelectedIndex].Name;
            }
            if (indexCbViewTemplateMCN > 1)
            {
                Settings1.Default.ViewTemplateMCN = listViewTemplate[cbViewTemplateMCN.SelectedIndex].Name;
            }
            if (indexCbViewType > -1)
            {
                Reference viewType = new Reference(listViewType[indexCbViewType]);
                string strviewType = viewType.ConvertToStableRepresentation(mDoc);
                Settings1.Default.ViewType = listViewType[indexCbViewType].UniqueId;

            }

            // Lưu giá trị checkBox

            if (cbxDIM.IsChecked == true)
            {
                Global.checkBoxDIM = true;
            }
            if (cbxElevation.IsChecked == true)
            {
                Global.checkBoxElevation = true;
            }
            if (cbxNetCat.IsChecked == true)
            {
                Global.checkBoxNetCat = true;
            }
            if (cbxSchedule.IsChecked == true)
            {
                Global.checkBoxSchedule = true;
            }
            if (cbxTag.IsChecked == true)
            {
                Global.checkBoxTag = true;
            }
            //Family family = null;
            bool loadedFamily;

            Family netcat = null;
            using (Transaction t = new Transaction(mDoc, "load Family"))
            {
                t.Start();
                // Lấy về địa chỉ debug
                String assemblyFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                String assemplyDirPath = System.IO.Path.GetDirectoryName(assemblyFilePath);

                loadedFamily = mDoc.LoadFamily(assemplyDirPath + @"\HB_NetCat.rfa", out netcat);
                mDoc.Regenerate();
                t.Commit();
            }
            Global.IsFormOK = true;
            mWindow.Close();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            // TRUY VẤN REBAR TAG ( ONLY TYPE)
            ElementCategoryFilter tagSingleRebar = new ElementCategoryFilter(BuiltInCategory.OST_RebarTags);
            FilteredElementCollector filterSingleStandRebar = new FilteredElementCollector(mDoc).WherePasses(tagSingleRebar).WhereElementIsElementType();

            foreach (Element el in filterSingleStandRebar)
            {
                FamilySymbol Ftype = el as FamilySymbol;
                if (Ftype == null)
                {
                    continue;
                }
                Family Fam = Ftype.Family;
                listTagSingleRebar.Add(Ftype);
                listShowTagSingleRebar.Add(Fam.Name.ToString() + "-" + Ftype.Name);
            }
            cbSStandardRebar.ItemsSource = listShowTagSingleRebar; 
            int idSingleRebar = listShowTagSingleRebar.IndexOf("THXD_SingleRebarTag-2Ø20");
            cbSStandardRebar.SelectedIndex = idSingleRebar;


            // TRUY VẤN MULTI TAG

            ElementClassFilter mFilterRAT = new ElementClassFilter(typeof(MultiReferenceAnnotationType));
            List<MultiReferenceAnnotationType> mRAT = new FilteredElementCollector(mDoc).WherePasses(mFilterRAT)
            .Cast<MultiReferenceAnnotationType>().ToList();
            foreach (MultiReferenceAnnotationType el2 in mRAT)
            {
                listTagMultiRebar.Add(el2);
                listShowTagMultiRebar.Add(el2.Name);
            }
            cbMStandardRebar.ItemsSource = listShowTagMultiRebar;
            int idMutilRebar = listShowTagMultiRebar.IndexOf("THXD_MultiRebarTag");
            cbMStandardRebar.SelectedIndex = idMutilRebar;

            //  TRUY VẤN DIMENSION
            ElementClassFilter DimClass = new ElementClassFilter(typeof(DimensionType));
            List<DimensionType> DimTypeList = new FilteredElementCollector(mDoc).WherePasses(DimClass)
                .Cast<DimensionType>()
                .Where(x => !(x is SpotDimensionType) && x.FamilyName == "Linear Dimension Style").ToList();
            foreach (var item in DimTypeList)
            {
                listDimtype.Add(item);
                listShowDimtype.Add(item.Name);
            }
            listDimtype.Sort(new dimSort());
            cbDIMStyle.ItemsSource = listDimtype;
            cbDIMStyle.DisplayMemberPath = "Name";
            int idDIM = listShowDimtype.IndexOf("THXD_Arc Length - 3mm Arial 2");
            cbDIMStyle.SelectedIndex = idDIM;

            // TRUY VẤN STIRUPT TAG
            foreach (Element el in filterSingleStandRebar)
            {
                FamilySymbol Ftype = el as FamilySymbol;
                if (Ftype == null)
                {
                    continue;
                }
                Family Fam = Ftype.Family;
                listTagStirrup.Add(Ftype);
                listShowTagStirrup.Add(Fam.Name.ToString() + "-" + Ftype.Name);

            }
            cbStirrupt.ItemsSource = listShowTagStirrup;
            int idTagStirrup = listShowTagStirrup.IndexOf("THXD_StiruptRebarTag-Ø6@200");
            cbStirrupt.SelectedIndex = idTagStirrup;

            //TRUY VẤN VIEW TEMPLATE
            ElementClassFilter ViewFilter = new ElementClassFilter(typeof(View));
            List<View> ViewTemplateList = new FilteredElementCollector(mDoc).WherePasses(ViewFilter).Cast<View>()
                .Where(x => x.IsTemplate).ToList();
            foreach (var VT in ViewTemplateList)
            {
                listViewTemplate.Add(VT);
                listShowViewTemplate.Add(VT.Name);

            }
            cbViewTemplate.ItemsSource = listViewTemplate;
            cbViewTemplate.DisplayMemberPath = "Name";
            int idViewTemplate = listShowViewTemplate.IndexOf("THXD_DetailViews");
            cbViewTemplate.SelectedIndex = idViewTemplate;

            cbViewTemplateMCN.ItemsSource = listViewTemplate;
            cbViewTemplateMCN.DisplayMemberPath = "Name";
            int idViewTemplateMCN = listShowViewTemplate.IndexOf("THXD_SectionViews");
            cbViewTemplateMCN.SelectedIndex = idViewTemplateMCN;

            //TRUY VẤN Legends
            ElementClassFilter ViewLegend = new ElementClassFilter(typeof(View));
            List<View> ViewLegendList = new FilteredElementCollector(mDoc).OfClass(typeof(View)).Cast<View>()
                .Where(x => x.ViewType == ViewType.Legend).ToList();
            foreach (var VT in ViewLegendList)
            {
                listLegends.Add(VT);
                listShowLegends.Add(VT.Name);

            }
            cbLegends.ItemsSource = listLegends;
            cbLegends.DisplayMemberPath = "Name";
            int idLegends = listShowLegends.IndexOf("Ghi Chú Dầm");
            cbLegends.SelectedIndex = idLegends;

            //TRUY VẤN ELEVATION TYPE
            ElementClassFilter SpotDimClass = new ElementClassFilter(typeof(SpotDimensionType));
            List<SpotDimensionType> SpotDimList = new FilteredElementCollector(mDoc).WherePasses(SpotDimClass)
                .Where(x => x is SpotDimensionType && (x as SpotDimensionType).FamilyName == "Spot Elevations")
                .Cast<SpotDimensionType>().ToList();

            //TRUY VẤN VIEWTYPE
            List<ViewFamilyType> ViewTypelist = new FilteredElementCollector(mDoc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>()
                .Where(x => x.ViewFamily == ViewFamily.Detail).ToList();
            foreach (var ViewType in ViewTypelist)
            {
                listViewType.Add(ViewType);
            }
            cbViewType.ItemsSource = listViewType;
            cbViewType.DisplayMemberPath = "Name";
            cbViewType.SelectedIndex = 0;

            lisSpotDim.Sort(new dimSort());
            cbElevationType.ItemsSource = SpotDimList;
            cbElevationType.DisplayMemberPath = "Name";
            cbElevationType.SelectedIndex = 0;

            // TRUY VẤN TITLE BLOCK
            ElementCategoryFilter titleBlock = new ElementCategoryFilter(BuiltInCategory.OST_TitleBlocks);
            FilteredElementCollector filtertitleBlock = new FilteredElementCollector(mDoc).WherePasses(titleBlock).WhereElementIsElementType();

            foreach (Element el in filtertitleBlock)
            {
                FamilySymbol Ftype = el as FamilySymbol;
                if (Ftype == null)
                {
                    continue;
                }
                Family Fam = Ftype.Family;
                listTitleBlock.Add(Ftype);
                listShowTitleBlock.Add(Fam.Name.ToString());

            }
            cbTitleblock.ItemsSource = listShowTitleBlock;
            int idTitleBlock = listShowTitleBlock.IndexOf("THXD_A3");
            cbTitleblock.SelectedIndex = idTitleBlock;
        }

        public class dimSort : IComparer<DimensionType>
        {
            public int Compare(DimensionType x, DimensionType y)
            {
                return x.Name.CompareTo(y.Name);
            }

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Global.IsFormOK = false;
            mWindow.Close();
        }
    }
}
