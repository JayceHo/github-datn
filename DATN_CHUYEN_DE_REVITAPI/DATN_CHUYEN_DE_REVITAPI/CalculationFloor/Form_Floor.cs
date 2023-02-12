using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;

using DataTable = System.Data.DataTable;
using Autodesk.Revit.UI;
using System.Reflection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using OfficeOpenXml;

namespace DATN_CHUYEN_DE_REVITAPI.CalculationFloor
{
    public partial class Form_Floor : System.Windows.Forms.Form
    {
        private UIApplication uiapp;
        private UIDocument uidoc;
        private Document doc;
        private ExternalCommandData commandData;
        private string message;
        private ElementSet elements;
        public Form_Floor(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            InitializeComponent();
            this.commandData = commandData;
            this.elements = elements;
            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            doc = uidoc.Document;

        }
        public Form_Floor()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CreateRebarSlabAuto createRebarSlabAuto = new CreateRebarSlabAuto();
            createRebarSlabAuto.Execute(commandData, ref message, elements);
            Close();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            double cover = Global.Btbv, Rb = Global.Rb, CosiR = Global.CosiR, aphaR = Global.AlphaR, phi = Global.LopDuoi, Rs = Global.Rs, hsan = 150,
                abotrix = 10, abotriy = 10;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(openFileDialog1.FileName)))
                {
                    ExcelWorksheet excelWorksheet1 = excelPackage.Workbook.Worksheets[1];
                    DataTable dataTable1 = new DataTable();
                    for (int i = excelWorksheet1.Dimension.Start.Column; i <= excelWorksheet1.Dimension.End.Column; i++)
                    {
                        dataTable1.Columns.Add(excelWorksheet1.Cells[1, i].Value.ToString());
                    }
                    for (int i = excelWorksheet1.Dimension.Start.Row + 1; i <= excelWorksheet1.Dimension.End.Row; i++)
                    {
                        List<string> listRows1 = new List<string>();
                        for (int j = excelWorksheet1.Dimension.Start.Column; j <= excelWorksheet1.Dimension.End.Column; j++)
                        {
                            listRows1.Add(excelWorksheet1.Cells[i, j].Value.ToString());
                        }
                        dataTable1.Rows.Add(listRows1.ToArray());
                    }

                    ExcelWorksheet excelWorksheet2 = excelPackage.Workbook.Worksheets[2];
                    DataTable dataTable2 = new DataTable();
                    for (int i = excelWorksheet2.Dimension.Start.Column; i <= excelWorksheet2.Dimension.End.Column; i++)
                    {
                        dataTable2.Columns.Add(excelWorksheet2.Cells[1, i].Value.ToString());
                    }
                    dataTable2.Columns.Add("aplhaM");
                    dataTable2.Columns.Add("cosi");
                    dataTable2.Columns.Add("As tính toán");
                    dataTable2.Columns.Add("Muy");
                    dataTable2.Columns.Add("Phi thép");
                    dataTable2.Columns.Add("a tính toán");
                    dataTable2.Columns.Add("a chọn");
                    dataTable2.Columns.Add("As chọn");

                    for (int i = excelWorksheet2.Dimension.Start.Row + 1; i <= excelWorksheet2.Dimension.End.Row; i++)
                    {
                        List<string> listRows2 = new List<string>();
                        for (int j = excelWorksheet2.Dimension.Start.Column; j <= excelWorksheet2.Dimension.End.Column; j++)
                        {
                            listRows2.Add(excelWorksheet2.Cells[i, j].Value.ToString());
                        }
                        dataTable2.Rows.Add(listRows2.ToArray());

                    }
                    int i1 = 0;
                    try
                    {
                        while (dataTable2.Rows[i1][0] != null)
                        {

                            dataTable2.Rows[i1][2] = Math.Round(Math.Abs(double.Parse(dataTable2.Rows[i1][1].ToString()) * 1000 / ((Rb * (hsan - cover) * (hsan - cover)))), 3);
                            dataTable2.Rows[i1][3] = Math.Round((1 + Math.Sqrt(1 - 2 * double.Parse(dataTable2.Rows[i1][2].ToString()))) / 2, 3);
                            dataTable2.Rows[i1][4] = Math.Round(Math.Abs((double.Parse(dataTable2.Rows[i1][1].ToString()) * 10000) / (double.Parse(dataTable2.Rows[i1][3].ToString()) * Rs * (hsan - cover))), 3);
                            dataTable2.Rows[i1][5] = Math.Round(100 * (double.Parse(dataTable2.Rows[i1][4].ToString())) / (10 * (hsan - cover)), 3);
                            dataTable2.Rows[i1][6] = phi;
                            dataTable2.Rows[i1][7] = Math.Round((10 * Math.PI * phi * phi / 4) / (double.Parse(dataTable2.Rows[i1][4].ToString())), 3);
                            int a = (int)Math.Floor(double.Parse(dataTable2.Rows[i1][7].ToString()) / 10) * 10;
                            if (a >= 200) a = 200;
                            if (150 <= a && a < 200) a = 150;
                            if (100 <= a && a < 150) a = 100;
                            dataTable2.Rows[i1][8] = a;
                            dataTable2.Rows[i1][9] = Math.Round((1000 * 0.007853982 * Math.Pow(phi, 2)) / (double.Parse(dataTable2.Rows[i1][8].ToString())), 3);
                            if (dataTable2.Rows[i1][0] == dataTable2.Rows[i1 + 1][0])
                            {
                                abotrix = Math.Min(double.Parse(dataTable2.Rows[i1][8].ToString()), double.Parse(dataTable2.Rows[i1 + 1][8].ToString()));
                            }
                            i1++;

                        }

                    }
                    catch (Exception)
                    {

                    };

                    ExcelWorksheet excelWorksheet3 = excelPackage.Workbook.Worksheets[3];
                    DataTable dataTable3 = new DataTable();
                    for (int i = excelWorksheet3.Dimension.Start.Column; i <= excelWorksheet3.Dimension.End.Column; i++)
                    {
                        dataTable3.Columns.Add(excelWorksheet3.Cells[1, i].Value.ToString());
                    }
                    dataTable3.Columns.Add("aplhaM");
                    dataTable3.Columns.Add("cosi");
                    dataTable3.Columns.Add("As tính toán");
                    dataTable3.Columns.Add("Muy");
                    dataTable3.Columns.Add("Phi thép");
                    dataTable3.Columns.Add("a tính toán");
                    dataTable3.Columns.Add("a chọn");
                    dataTable3.Columns.Add("As chọn");

                    for (int i = excelWorksheet3.Dimension.Start.Row + 1; i <= excelWorksheet3.Dimension.End.Row; i++)
                    {
                        List<string> listRows2 = new List<string>();
                        for (int j = excelWorksheet3.Dimension.Start.Column; j <= excelWorksheet3.Dimension.End.Column; j++)
                        {
                            listRows2.Add(excelWorksheet3.Cells[i, j].Value.ToString());
                        }
                        dataTable3.Rows.Add(listRows2.ToArray());

                    }
                    int i2 = 0;
                    try
                    {
                        while (dataTable3.Rows[i2][0] != null)
                        {
                            dataTable3.Rows[i2][2] = Math.Round(Math.Abs(double.Parse(dataTable3.Rows[i2][1].ToString()) * 1000 / ((Rb * (hsan - cover) * (hsan - cover)))), 3);
                            dataTable3.Rows[i2][3] = Math.Round((1 + Math.Sqrt(1 - 2 * double.Parse(dataTable3.Rows[i2][2].ToString()))) / 2, 3);
                            dataTable3.Rows[i2][4] = Math.Round(Math.Abs((double.Parse(dataTable3.Rows[i2][1].ToString()) * 10000) / (double.Parse(dataTable3.Rows[i2][3].ToString()) * Rs * (hsan - cover))), 3);
                            dataTable3.Rows[i2][5] = Math.Round(100 * (double.Parse(dataTable3.Rows[i2][4].ToString())) / (10 * (hsan - cover)), 3);
                            dataTable3.Rows[i2][6] = phi;
                            dataTable3.Rows[i2][7] = Math.Round((10 * Math.PI * phi * phi / 4) / (double.Parse(dataTable3.Rows[i2][4].ToString())), 3);
                            int a = (int)Math.Floor(double.Parse(dataTable3.Rows[i2][7].ToString()) / 10) * 10;
                            if (a >= 200) a = 200;
                            if (150 <= a && a < 200) a = 150;
                            if (100 <= a && a < 150) a = 100;
                            dataTable3.Rows[i2][8] = a;
                            dataTable3.Rows[i2][9] = Math.Round((1000 * 0.007853982 * Math.Pow(phi, 2)) / (double.Parse(dataTable3.Rows[i2][8].ToString())), 3);

                            if (dataTable3.Rows[i2][0] == dataTable3.Rows[i2 + 1][0])
                            {
                                abotriy = Math.Min(double.Parse(dataTable2.Rows[i2][8].ToString()), double.Parse(dataTable2.Rows[i2 + 1][8].ToString()));
                            }

                            i2++;

                        }

                    }

                    catch (Exception)
                    {

                    };
                    dgvX.DataSource = dataTable2;
                    dgvY.DataSource = dataTable3;

                    Global.dataTable1 = dataTable1;
                    Global.dataTable2 = dataTable2;
                    Global.dataTable3 = dataTable3;
                }




            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form_Setting form_Setting = new Form_Setting();
            form_Setting.Tao();
            form_Setting.ShowDialog();
        }
    }
}
