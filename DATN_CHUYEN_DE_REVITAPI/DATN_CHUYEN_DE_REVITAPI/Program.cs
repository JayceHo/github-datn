using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN_CHUYEN_DE_REVITAPI
{
    class Program
    {
    }
    static class Global
    {
        public static string strSheetName;
        public static string strTitleBlock;
        public static string strLegend;
        public static bool checkBoxDIM;
        public static bool checkBoxTag;
        public static bool checkBoxNetCat;
        public static bool checkBoxElevation;
        public static bool checkBoxSchedule;
        public static bool IsFormOK = false;
        public static string strTenDam;

        public static DataTable dataTable1 = new DataTable("Data name");
        public static DataTable dataTable2 = new DataTable("Data Mx");
        public static DataTable dataTable3 = new DataTable("Data My");
        public static double Rb = 14.5;
        public static double Rs = 365;
        public static double LopTren = 8;
        public static double LopDuoi = 8;
        public static double Btbv = 25;
        public static double CosiR = 0.508;
        public static double AlphaR = 0.407;
        public static bool IsFormColumnOk = false;
        public static double Rebar;
        public static int Nb;
        public static int Nh;
        public static double StirrupColumn;
        public static string Colum_RebarCover;
        public static double CoverColumn;
        public static string kieucotdai;
        public static string botricotdai;
        public static double noithep;
        public static double A1 = 0;
        public static double A2 = 0;
        public static double A3 = 0;
        //FormBeam
        public static bool IsFormBeamOk = false;
        public static double Thepduoi;
        public static double nudThepduoi;
        public static double Thepduoiuontrai;
        public static double Thepduoiuonphai;
        public static double Theptren;
        public static double nudTheptren;
        public static double Theptrenuontrai;
        public static double Theptrenuonphai;
        public static double StirrupBeam;
        public static string Beam_RebarCover;
        public static double CoverBeam;
        public static string kieucotdaiBeam;
        public static string botricotdaiBeam;
        public static double A1beam = 0;
        public static double A2beam = 0;
        public static double A3beam = 0;
    }
}
