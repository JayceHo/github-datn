using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DATN_CHUYEN_DE_REVITAPI.CalculationFloor
{
    public partial class Form_Setting : Form
    {
        public Form_Setting()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Global.Rb = double.Parse(tbRb.Text);
            Global.Rs = double.Parse(tbRs.Text);
            Global.LopTren = double.Parse(cbbLopTren.Text);
            Global.LopDuoi = double.Parse(cbbLopDuoi.Text);
            Global.Btbv = double.Parse(tbBtbv.Text);
            Global.CosiR = double.Parse(tbCosiR.Text);
            Global.AlphaR = double.Parse(tbAlphaR.Text);
            Close();
        }
        private double[] arrRb = new[] { 7.5, 8.5, 11.5, 14.5, 17 };
        private int[] arrRs = new[] { 225, 280, 365 };
        public void Tao()
        {

            List<string> phi1 = new List<string>() { "8", "10", "12", "16", "18", "20" };
            List<string> phi2 = new List<string>() { "8", "10", "12", "16", "18", "20" };
            List<string> beTong = new List<string>() { "B12,5", "B15", "B20", "B25", "B30" };
            List<string> thep = new List<string>() { "CI", "CII", "CIII" };
            cbbLopTren.DataSource = phi1;
            cbbLopDuoi.DataSource = phi2;
            cbbBetong.DataSource = beTong;
            cbbThep.DataSource = thep;
            tbBtbv.Text = "25";



        }


        private void cbbBetong_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbRb.Text = arrRb[cbbBetong.SelectedIndex].ToString();
            tbCosiR.Text = ((0.85 - 0.008 * double.Parse(tbRb.Text)) / (1 + (double.Parse(tbRs.Text) / 400 * (1 - (0.85 - 0.008 * double.Parse(tbRb.Text)) / 1.1)))).ToString();
            tbAlphaR.Text = (double.Parse(tbCosiR.Text) * (1 - double.Parse(tbCosiR.Text) / 2)).ToString();

        }

        
    }
}
