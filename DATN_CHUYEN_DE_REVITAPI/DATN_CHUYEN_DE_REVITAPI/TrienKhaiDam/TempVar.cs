using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN_CHUYEN_DE_REVITAPI
{
    //Dùng để lưu các biến tạm của dự án
    class TempVar
    {
        public XYZ TempPoint { get; set; }
        public XYZ vecx { get; set; }
        public XYZ vecy { get; set; }
        public XYZ vecz { get; set; }
        public double Width { get; set; }
        public double High { get; set; }
        public Element beam { get; set; }
        public Plane SectionPlane { get; set; }
        public XYZ Mid { get; set; }
        public Reference TopBeamRef { get; set; }
        public TempVar() { }
        public int SubNumber { get; set; }

        public static readonly TempVar Instance = new TempVar();
    }
}
