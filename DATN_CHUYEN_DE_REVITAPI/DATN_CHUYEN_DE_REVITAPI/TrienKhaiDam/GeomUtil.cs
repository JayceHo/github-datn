using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN_CHUYEN_DE_REVITAPI
{
    /// <summary>
    /// Kiểu dữ liệu chứa các phương thức tĩnh xử lý các đối tượng hình học Revit
    /// </summary>
    class GeomUtil
    {
        const string revit = "Revit";

        /// <summary>
        /// Sai số tọa độ cho phép trong Revit, nếu sai số nhỏ hơn sai số trên thì Revit hiểu là trùng nhau
        /// </summary>
        const double Precision = 0.00001;

        /// <summary>
        /// Hệ số chuyển đổi từ feet sang meter
        /// </summary>
        const double FEET_TO_METERS = 0.3048;

        /// <summary>
        /// Hệ số chuyển đổi từ feet sang centimeter
        /// </summary>
        const double FEET_TO_CENTIMETERS = FEET_TO_METERS * 100;

        /// <summary>
        /// Hệ số chuyển đổi từ feet sang milimeter
        /// </summary>
        const double FEET_TO_MILIMETERS = FEET_TO_METERS * 1000;

        /// <summary>
        /// Hàm chuyển đổi từ feet sang meter
        /// </summary>
        /// <param name="feet">Giá trị đang xét</param>
        /// <returns></returns>
        public static double feet2Meter(double feet)
        {
            return feet * FEET_TO_METERS;
        }

        /// <summary>
        /// Hàm chuyển đổi từ feet sang milimeter
        /// </summary>
        /// <param name="feet">Giá trị đang xét</param>
        /// <returns></returns>
        public static double feet2Milimeter(double feet)
        {
            return feet * FEET_TO_MILIMETERS;
        }

        /// <summary>
        /// Hàm chuyển đổi từ feet sang meter
        /// </summary>
        /// <param name="meter">Giá trị đang xét</param>
        /// <returns></returns>
        public static double meter2Feet(double meter)
        {
            return meter / FEET_TO_METERS;
        }

        /// <summary>
        /// Hàm chuyển đổi từ milimeter sang feet
        /// </summary>
        /// <param name="milimeter">Giá trị đang xét</param>
        /// <returns></returns>
        public static double milimeter2Feet(double milimeter)
        {
            return milimeter / FEET_TO_MILIMETERS;
        }

        /// <summary>
        /// Hàm chuyển đổi từ độ Radian sang độ Degree
        /// </summary>
        /// <param name="rad">Giá trị đang xét</param>
        /// <returns></returns>
        public static double radian2Degree(double rad)
        {
            return rad * 180 / Math.PI;
        }

        /// <summary>
        /// Hàm chuyển đổi từ độ Degree sang độ Radian
        /// </summary>
        /// <param name="deg">Giá trị đang xét</param>
        /// <returns></returns>
        public static double degree2Radian(double deg)
        {
            return deg * Math.PI / 180;
        }

        /// <summary>
        /// Kiểm tra 2 giá trị có bằng nhau hay không trong môi trường Revit
        /// True: bằng nhau, False: khác nhau
        /// </summary>
        /// <param name="d1">Giá trị 1</param>
        /// <param name="d2">Giá trị 2</param>
        /// <returns></returns>
        public static bool IsEqual(double d1, double d2)
        {
            //get the absolute value;
            double diff = Math.Abs(d1 - d2);
            return diff < Precision;
        }

        /// <summary>
        /// Kiểm tra 2 điểm, vector có giống nhau hay không trong môi trường Revit
        /// True: giống nhau, False: khác nhau
        /// </summary>
        /// <param name="first">Điểm, vector 1</param>
        /// <param name="second">Điểm, vector 2</param>
        /// <returns></returns>
        public static bool IsEqual(Autodesk.Revit.DB.XYZ first, Autodesk.Revit.DB.XYZ second)
        {
            bool flag = true;
            flag = flag && IsEqual(first.X, second.X);
            flag = flag && IsEqual(first.Y, second.Y);
            flag = flag && IsEqual(first.Z, second.Z);
            return flag;
        }

        /// <summary>
        /// Kiểm tra 2 điểm, vector 2d có giống nhau hay không trong môi trường Revit
        /// True: giống nhau, False: khác nhau
        /// </summary>
        /// <param name="first">Điểm, vector 2d 1</param>
        /// <param name="second">Điểm, vector 2d 2</param>
        /// <returns></returns>
        public static bool IsEqual(Autodesk.Revit.DB.UV first, Autodesk.Revit.DB.UV second)
        {
            bool flag = true;
            flag = flag && IsEqual(first.U, second.U);
            flag = flag && IsEqual(first.V, second.V);
            return flag;
        }

        /// <summary>
        /// Kiểm tra 2 đoạn thẳng có giống nhau hay không trong môi trường Revit
        /// True: giống nhau, False: khác nhau
        /// </summary>
        /// <param name="first">Đoạn thẳng 1</param>
        /// <param name="second">Đoạn thẳng 2</param>
        /// <returns></returns>
        public static bool IsEqual(Curve first, Curve second)
        {
            if (IsEqual(first.GetEndPoint(0), second.GetEndPoint(0)))
            {
                return IsEqual(first.GetEndPoint(1), second.GetEndPoint(1));
            }
            if (IsEqual(first.GetEndPoint(1), second.GetEndPoint(0)))
            {
                return IsEqual(first.GetEndPoint(0), second.GetEndPoint(1));
            }
            return false;
        }

        /// <summary>
        /// Kiểm tra điểm của vector 1 có nhỏ hơn điểm của vector 2 hay không trong môi trường Revit
        /// Cách thức so sánh theo thứ tự Z -> Y -> X
        /// True: nhỏ hơn, False: lớn hơn hoặc bằng
        /// </summary>
        /// <param name="first">Điểm, vector 1</param>
        /// <param name="second">Điểm, vector 2</param>
        /// <returns></returns>
        public static bool IsSmaller(XYZ first, XYZ second)
        {
            if (IsEqual(first, second)) return false;
            if (IsEqual(first.Z, second.Z))
            {
                if (IsEqual(first.Y, second.Y))
                {
                    return (first.X < second.X);
                }
                return (first.Y < second.Y);
            }
            return (first.Z < second.Z);
        }

        /// <summary>
        /// Kiểm tra giá trị 1 có nhỏ hơn giá trị 2 hay không trong môi trường Revit
        /// True: nhỏ hơn, False: lớn hơn hoặc bằng
        /// </summary>
        /// <param name="first">Giá trị 1</param>
        /// <param name="second">Giá trị 2</param>
        /// <returns></returns>
        public static bool IsSmaller(double x, double y)
        {
            if (IsEqual(x, y)) return false;
            return x < y;
        }

        /// <summary>
        /// Kiểm tra điểm của vector 1 có lớn hơn điểm của vector 2 hay không trong môi trường Revit
        /// Cách thức so sánh theo thứ tự Z -> Y -> X
        /// True: lớn hơn, False: nhỏ hơn hoặc bằng
        /// </summary>
        /// <param name="first">Điểm, vector 1</param>
        /// <param name="second">Điểm, vector 2</param>
        /// <returns></returns>
        public static bool IsBigger(XYZ first, XYZ second)
        {
            if (IsEqual(first, second)) return false;
            if (IsEqual(first.Z, second.Z))
            {
                if (IsEqual(first.Y, second.Y))
                {
                    return (first.X > second.X);
                }
                return (first.Y > second.Y);
            }
            return (first.Z > second.Z);
        }

        /// <summary>
        /// Kiểm tra giá trị 1 có nhỏ hơn giá trị 2 hay không trong môi trường Revit
        /// True: lớn hơn, False: nhỏ hơn hoặc bằng
        /// </summary>
        /// <param name="first">Giá trị 1</param>
        /// <param name="second">Giá trị 2</param>
        /// <returns></returns>
        public static bool IsBigger(double first, double second)
        {
            if (IsEqual(first, second)) return false;
            return first > second;
        }

        /// <summary>
        /// Kiểm tra 2 vector có cùng hướng và cùng phương với nhau hay không trong môi trường Revit
        /// True: cùng phương và hướng, False: khác phương hoặc hướng
        /// </summary>
        /// <param name="firstVec">Vector 1</param>
        /// <param name="secondVec">Vector 2</param>
        /// <returns></returns>
        public static bool IsSameDirection(Autodesk.Revit.DB.XYZ firstVec, Autodesk.Revit.DB.XYZ secondVec)
        {
            Autodesk.Revit.DB.XYZ first = UnitVector(firstVec);
            Autodesk.Revit.DB.XYZ second = UnitVector(secondVec);
            double dot = DotMatrix(first, second);
            return (IsEqual(dot, 1));
        }

        /// <summary>
        /// Kiểm tra 2 vector 2d có cùng hướng và cùng phương với nhau hay không trong môi trường Revit
        /// True: cùng phương và hướng, False: khác phương hoặc hướng
        /// </summary>
        /// <param name="firstVec">Vector 2d 1</param>
        /// <param name="secondVec">Vector 2d 2</param>
        /// <returns></returns>
        public static bool IsSameDirection(UV firstVec, UV secondVec)
        {
            Autodesk.Revit.DB.UV first = UnitVector(firstVec);
            Autodesk.Revit.DB.UV second = UnitVector(secondVec);
            double dot = DotMatrix(first, second);
            return (IsEqual(dot, 1));
        }

        /// <summary>
        /// Kiểm tra 2 vector có cùng phương với nhau hay không trong môi trường Revit
        /// True: cùng phương và hướng, False: khác phương hoặc hướng
        /// </summary>
        /// <param name="firstVec">Vector 1</param>
        /// <param name="secondVec">Vector 2</param>
        /// <returns></returns>
        public static bool IsSameOrOppositeDirection(XYZ firstVec, XYZ secondVec)
        {
            Autodesk.Revit.DB.XYZ first = UnitVector(firstVec);
            Autodesk.Revit.DB.XYZ second = UnitVector(secondVec);

            // if the dot product of two unit vectors is equal to 1, return true
            double dot = DotMatrix(first, second);
            return (IsEqual(dot, 1) || IsEqual(dot, -1));
        }

        /// <summary>
        /// Kiểm tra 2 vector 2d có cùng hướng với nhau hay không trong môi trường Revit
        /// True: cùng phương và hướng, False: khác phương hoặc hướng
        /// </summary>
        /// <param name="firstVec">Vector 2d 1</param>
        /// <param name="secondVec">Vector 2d 2</param>
        /// <returns></returns>
        public static bool IsSameOrOppositeDirection(UV firstVec, UV secondVec)
        {
            Autodesk.Revit.DB.UV first = UnitVector(firstVec);
            Autodesk.Revit.DB.UV second = UnitVector(secondVec);

            // if the dot product of two unit vectors is equal to 1, return true
            double dot = DotMatrix(first, second);
            return (IsEqual(dot, 1) || IsEqual(dot, -1));
        }

        /// <summary>
        /// Kiểm tra 2 vector có cùng hướng và ngược phương với nhau hay không trong môi trường Revit
        /// True: cùng phương và ngược hướng, False: các trường hợp còn lại
        /// </summary>
        /// <param name="firstVec">Vector 1</param>
        /// <param name="secondVec">Vector 2</param>
        /// <returns></returns>
        public static bool IsOppositeDirection(Autodesk.Revit.DB.XYZ firstVec, Autodesk.Revit.DB.XYZ secondVec)
        {
            // get the unit vector for two vectors
            Autodesk.Revit.DB.XYZ first = UnitVector(firstVec);
            Autodesk.Revit.DB.XYZ second = UnitVector(secondVec);
            // if the dot product of two unit vectors is equal to -1, return true
            double dot = DotMatrix(first, second);
            return (IsEqual(dot, -1));
        }

        /// <summary>
        /// Kiểm tra 2 vector 2d có cùng hướng và ngược phương với nhau hay không trong môi trường Revit
        /// True: cùng phương và ngược hướng, False: các trường hợp còn lại
        /// </summary>
        /// <param name="firstVec">Vector 2d 1</param>
        /// <param name="secondVec">Vector 2d 2</param>
        /// <returns></returns>
        public static bool IsOppositeDirection(Autodesk.Revit.DB.UV firstVec, Autodesk.Revit.DB.UV secondVec)
        {
            // get the unit vector for two vectors
            Autodesk.Revit.DB.UV first = UnitVector(firstVec);
            Autodesk.Revit.DB.UV second = UnitVector(secondVec);

            // if the dot product of two unit vectors is equal to -1, return true
            double dot = DotMatrix(first, second);
            return (IsEqual(dot, -1));
        }

        /// <summary>
        /// Trả về vector là tích có hướng của 2 vector cho trước
        /// </summary>
        /// <param name="p1">Vector 1</param>
        /// <param name="p2">Vector 2</param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.XYZ CrossMatrix(Autodesk.Revit.DB.XYZ p1, Autodesk.Revit.DB.XYZ p2)
        {
            //get the coordinate of the XYZ
            double u1 = p1.X;
            double u2 = p1.Y;
            double u3 = p1.Z;

            double v1 = p2.X;
            double v2 = p2.Y;
            double v3 = p2.Z;

            double x = v3 * u2 - v2 * u3;
            double y = v1 * u3 - v3 * u1;
            double z = v2 * u1 - v1 * u2;

            return new Autodesk.Revit.DB.XYZ(x, y, z);
        }

        /// <summary>
        /// Trả về vector đơn vị của vector cho trước
        /// </summary>
        /// <param name="vector">Vector cho trước</param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.XYZ UnitVector(Autodesk.Revit.DB.XYZ vector)
        {
            // calculate the distance from grid origin to the XYZ
            double length = GetLength(vector);

            // changed the vector into the unit length
            double x = vector.X / length;
            double y = vector.Y / length;
            double z = vector.Z / length;
            return new Autodesk.Revit.DB.XYZ(x, y, z);
        }

        /// <summary>
        /// Trả về vector đơn vị của vector 2d cho trước
        /// </summary>
        /// <param name="vector">Vector 2d cho trước</param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.UV UnitVector(Autodesk.Revit.DB.UV vector)
        {
            // calculate the distance from grid origin to the XYZ
            double length = GetLength(vector);

            // changed the vector into the unit length
            double x = vector.U / length;
            double y = vector.V / length;
            return new Autodesk.Revit.DB.UV(x, y);
        }

        /// <summary>
        /// Trả về chiều dài của vector cho trước
        /// </summary>
        /// <param name="vector">Vector cho trước</param>
        /// <returns></returns>
        public static double GetLength(Autodesk.Revit.DB.XYZ vector)
        {
            double x = vector.X;
            double y = vector.Y;
            double z = vector.Z;
            return Math.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>
        /// Trả về chiều dài đại số của vector cho trước
        /// </summary>
        /// <param name="vector">Vector cho trước</param>
        /// <param name="checkPositive">Kiểu dữ liệu bool yêu cầu đại số vector hay không (true: có, false: không)</param>
        /// <returns></returns>
        public static double GetLength(Autodesk.Revit.DB.XYZ vector, bool checkPositive)
        {
            double len = GetLength(vector);
            if (!checkPositive) return len;
            return IsSmaller(-vector, vector) ? len : -len;
        }

        /// <summary>
        /// Trả về chiều dài của vector 2d cho trước
        /// </summary>
        /// <param name="vector">Vector 2d cho trước</param>
        /// <returns></returns>
        public static double GetLength(Autodesk.Revit.DB.UV vector)
        {
            double x = vector.U;
            double y = vector.V;
            return Math.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// Trả về chiều dài của đoạn thẳng cho trước
        /// </summary>
        /// <param name="line">Đoạn thẳng cho trước</param>
        /// <returns></returns>
        public static double GetLength(Autodesk.Revit.DB.Line line)
        {
            XYZ first = line.GetEndPoint(0);
            XYZ second = line.GetEndPoint(1);
            XYZ vec = SubXYZ(first, second);
            return GetLength(vec);
        }

        /// <summary>
        /// Trả về khoảng cách của 2 tọa độ cho trước
        /// </summary>
        /// <param name="p1">Tọa độ 1</param>
        /// <param name="p2">Tọa độ 2</param>
        /// <returns></returns>
        public static double GetLength(XYZ p1, XYZ p2)
        {
            return GetLength(SubXYZ(p1, p2));
        }

        /// <summary>
        /// Trả về tọa độ là trung điểm của 2 tọa độ cho trước
        /// </summary>
        /// <param name="first">Tọa độ 1</param>
        /// <param name="second">Tọa độ 2</param>
        /// <returns></returns>
        public static XYZ GetMiddlePoint(XYZ first, XYZ second)
        {
            return (first + second) / 2;
        }

        /// <summary>
        /// Trả về vector đi từ điểm 2 về điểm 1
        /// </summary>
        /// <param name="p1">Điểm 1</param>
        /// <param name="p2">Điểm 2</param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.XYZ SubXYZ(Autodesk.Revit.DB.XYZ p1, Autodesk.Revit.DB.XYZ p2)
        {
            double x = p1.X - p2.X;
            double y = p1.Y - p2.Y;
            double z = p1.Z - p2.Z;

            return new Autodesk.Revit.DB.XYZ(x, y, z);
        }

        /// <summary>
        /// Trả về vector 2d đi từ điểm 2 về điểm 1
        /// </summary>
        /// <param name="p1">Điểm 2d 1</param>
        /// <param name="p2">Điểm 2d 2</param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.UV SubXYZ(Autodesk.Revit.DB.UV p1, Autodesk.Revit.DB.UV p2)
        {
            double x = p1.U - p2.U;
            double y = p1.V - p2.V;

            return new Autodesk.Revit.DB.UV(x, y);
        }

        /// <summary>
        /// Trả về vector là tổng 2 vector cho trước
        /// </summary>
        /// <param name="p1">Vector 1</param>
        /// <param name="p2">Vector 2</param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.XYZ AddXYZ(Autodesk.Revit.DB.XYZ p1, Autodesk.Revit.DB.XYZ p2)
        {
            double x = p1.X + p2.X;
            double y = p1.Y + p2.Y;
            double z = p1.Z + p2.Z;

            return new Autodesk.Revit.DB.XYZ(x, y, z);
        }

        /// <summary>
        /// Trả về vector 2d là tổng 2 vector 2d cho trước
        /// </summary>
        /// <param name="p1">Vector 2d 1</param>
        /// <param name="p2">Vector 2d 2</param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.UV AddXYZ(Autodesk.Revit.DB.UV p1, Autodesk.Revit.DB.UV p2)
        {
            double x = p1.U + p2.U;
            double y = p1.V + p2.V;

            return new Autodesk.Revit.DB.UV(x, y);
        }

        /// <summary>
        /// Trả về vector là kết quả của nhân vector với một giá trị
        /// </summary>
        /// <param name="vector">Vector cho trước</param>
        /// <param name="rate">Giá trị cho trước</param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.XYZ MultiplyVector(Autodesk.Revit.DB.XYZ vector, double rate)
        {
            double x = vector.X * rate;
            double y = vector.Y * rate;
            double z = vector.Z * rate;

            return new Autodesk.Revit.DB.XYZ(x, y, z);
        }


        public static Autodesk.Revit.DB.XYZ TransformPoint(Autodesk.Revit.DB.XYZ point, Transform transform)
        {
            //get the coordinate value in X, Y, Z axis
            double x = point.X;
            double y = point.Y;
            double z = point.Z;

            //transform basis of the old coordinate system in the new coordinate system
            Autodesk.Revit.DB.XYZ b0 = transform.get_Basis(0);
            Autodesk.Revit.DB.XYZ b1 = transform.get_Basis(1);
            Autodesk.Revit.DB.XYZ b2 = transform.get_Basis(2);
            Autodesk.Revit.DB.XYZ origin = transform.Origin;

            //transform the origin of the old coordinate system in the new coordinate system
            double xTemp = x * b0.X + y * b1.X + z * b2.X + origin.X;
            double yTemp = x * b0.Y + y * b1.Y + z * b2.Y + origin.Y;
            double zTemp = x * b0.Z + y * b1.Z + z * b2.Z + origin.Z;

            return new Autodesk.Revit.DB.XYZ(xTemp, yTemp, zTemp);
        }

        public static Autodesk.Revit.DB.Curve TransformCurve(Autodesk.Revit.DB.Curve curve, Transform transform)
        {
            return Autodesk.Revit.DB.Line.CreateBound(TransformPoint(curve.GetEndPoint(0), transform), TransformPoint(curve.GetEndPoint(1), transform));
        }

        /// <summary>
        /// Trả về một điểm là kết quả của tịnh tiến một điểm theo một vector và khoảng cách trước
        /// </summary>
        /// <param name="point">Điểm cho trước</param>
        /// <param name="direction">Vector cho trước</param>
        /// <param name="offset">Khoảng cách cho trước</param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.XYZ OffsetPoint(Autodesk.Revit.DB.XYZ point, Autodesk.Revit.DB.XYZ direction, double offset)
        {
            Autodesk.Revit.DB.XYZ directUnit = UnitVector(direction);
            Autodesk.Revit.DB.XYZ offsetVect = MultiplyVector(directUnit, offset);
            return AddXYZ(point, offsetVect);
        }

        /// <summary>
        /// Trả về một đoạn thẳng là kết quả của tịnh tiến một đoạn thẳng theo một vector và khoảng cách cho trước
        /// </summary>
        /// <param name="c">Đoạn thẳng cho trước</param>
        /// <param name="direction">Vector cho trước</param>
        /// <param name="offset">Khoảng cách cho trước</param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.Curve OffsetCurve(Autodesk.Revit.DB.Curve c, Autodesk.Revit.DB.XYZ direction, double offset)
        {
            c = Autodesk.Revit.DB.Line.CreateBound(OffsetPoint(c.GetEndPoint(0), direction, offset), OffsetPoint(c.GetEndPoint(1), direction, offset));
            return c;
        }

        /// <summary>
        /// Trả về một tập hợp các đoạn thẳng là kết quả của tịnh tiến tập hợp đoạn thẳng theo một vector và khoảng cách cho trước
        /// </summary>
        /// <param name="cs">Tập hợp các đoạn thẳng cho trước</param>
        /// <param name="direction">Vector cho trước</param>
        /// <param name="offset">Khoảng cách cho trước</param>
        /// <returns></returns>
        public static List<Curve> OffsetListCurve(List<Curve> cs, Autodesk.Revit.DB.XYZ direction, double offset)
        {
            for (int i = 0; i < cs.Count; i++)
            {
                cs[i] = OffsetCurve(cs[i], direction, offset);
            }
            return cs;
        }

        /// <summary>
        /// Trả về một polygon là kết quả của tịnh tiến một polygon theo một vector và khoảng cách cho trước
        /// </summary>
        /// <param name="pl">Polygon cho trước</param>
        /// <param name="direction">Vector cho trước</param>
        /// <param name="offset">Khoảng cách cho trước</param>
        /// <returns></returns>
        public static Polygon OffsetPolygon(Polygon pl, Autodesk.Revit.DB.XYZ direction, double offset)
        {
            List<Curve> cs = new List<Curve>();
            foreach (Curve c in pl.ListCurve)
            {
                Curve c1 = OffsetCurve(c, direction, offset);
                cs.Add(c1);
            }
            return new Polygon(cs);
        }

        /// <summary>
        /// Trả về giá trị là tích vô hướng của 2 vector
        /// </summary>
        /// <param name="p1">Vector 1</param>
        /// <param name="p2">Vector 2</param>
        /// <returns></returns>
        public static double DotMatrix(Autodesk.Revit.DB.XYZ p1, Autodesk.Revit.DB.XYZ p2)
        {
            //get the coordinate of the Autodesk.Revit.DB.XYZ 
            double v1 = p1.X;
            double v2 = p1.Y;
            double v3 = p1.Z;

            double u1 = p2.X;
            double u2 = p2.Y;
            double u3 = p2.Z;

            return v1 * u1 + v2 * u2 + v3 * u3;
        }

        /// <summary>
        /// Trả về giá trị là tích vô hướng của 2 vector 2d
        /// </summary>
        /// <param name="p1">Vector 2d 1</param>
        /// <param name="p2">Vector 2d 2</param>
        /// <returns></returns>
        public static double DotMatrix(Autodesk.Revit.DB.UV p1, Autodesk.Revit.DB.UV p2)
        {
            //get the coordinate of the Autodesk.Revit.DB.XYZ 
            double v1 = p1.U;
            double v2 = p1.V;


            double u1 = p2.U;
            double u2 = p2.V;


            return v1 * u1 + v2 * u2;
        }

        /// <summary>
        /// Hàm làm tròn lên giá trị và ép về lại kiểu int
        /// </summary>
        /// <param name="d">Giá trị đang xét</param>
        /// <returns></returns>
        public static int RoundUp(double d)
        {
            return Math.Round(d, 0) < d ? (int)(Math.Round(d, 0) + 1) : (int)(Math.Round(d, 0));
        }

        /// <summary>
        /// Hàm làm tròn xuống giá trị và ép về lại kiểu int
        /// </summary>
        /// <param name="d">Giá trị đang xét</param>
        /// <returns></returns>
        public static int RoundDown(double d)
        {
            return Math.Round(d, 0) < d ? (int)(Math.Round(d, 0)) : (int)(Math.Round(d, 0) - 1);
        }

        /// <summary>
        /// Trả về góc radian của 2 vector đang xét
        /// </summary>
        /// <param name="vec1">Vector 1</param>
        /// <param name="vec2">Vector 2</param>
        /// <returns></returns>
        public static double GetRadianAngle(XYZ vec1, XYZ vec2)
        {
            return Math.Acos(DotMatrix(UnitVector(vec1), UnitVector(vec2)));
        }

        /// <summary>
        /// Trả về góc degree của 2 vector đang xét
        /// </summary>
        /// <param name="vec1">Vector 1</param>
        /// <param name="vec2">Vector 2</param>
        /// <returns></returns>
        public static double GetDegreeAngle(XYZ vec1, XYZ vec2)
        {
            return radian2Degree(GetRadianAngle(vec1, vec2));
        }

        public static double GetAngle(XYZ targetVec, XYZ vecX, XYZ vecY)
        {
            vecX = vecX.Normalize(); vecY = vecY.Normalize();
            XYZ vecZ = vecX.CrossProduct(vecY);
            double dot = vecZ.DotProduct(targetVec);

            if (!IsEqual(vecX.DotProduct(vecY), 0)) throw new Exception("Two basis is not perpecular!");
            if (!IsEqual(vecX.CrossProduct(vecY).DotProduct(targetVec), 0))
            {
                throw new Exception("TargetVector is not planar with two basis!");
            }
            double angle = 0;
            double angle1 = vecX.AngleTo(targetVec);
            double angle2 = Math.PI - angle1;
            XYZ assVec = vecX * Math.Cos(angle1) + vecY * Math.Sin(angle1);
            if (IsSameDirection(assVec, targetVec))
            {
                angle = angle1;
            }
            else if (IsOppositeDirection(assVec, targetVec))
            {
                angle = -angle2;
            }
            else
            {
                assVec = vecX * Math.Cos(angle2) + vecY * Math.Sin(angle2);
                if (IsSameDirection(assVec, targetVec))
                {
                    angle = angle2;
                }
                else if (IsOppositeDirection(assVec, targetVec))
                {
                    angle = -angle1;
                }
                else throw new Exception("The code should never go here!");
            }
            return angle;
        }
        public static UV GetUVCoordinate(XYZ targetVec, XYZ vecX, XYZ vecY)
        {
            vecX = vecX.Normalize(); vecY = vecY.Normalize();
            double len = targetVec.GetLength();
            double angle = GetAngle(targetVec, vecX, vecY);
            return new UV(Math.Cos(angle) * len, Math.Sin(angle) * len);
        }
        public static double GetAngle2(XYZ targetVec, XYZ vecX, XYZ vecY, XYZ vecZ)
        {
            vecX = vecX.Normalize(); vecY = vecY.Normalize(); vecZ = vecZ.Normalize();
            if (!IsEqual(vecX.DotProduct(vecY), 0)) throw new Exception("Two basis X and Y is not perpecular!");
            if (!IsEqual(vecX.CrossProduct(vecY), vecZ)) throw new Exception("Three basises are not a coordiante!");
            Plane pl = Plane.CreateByOriginAndBasis(XYZ.Zero, vecX, vecY);
            XYZ pjPnt = CheckGeometry.GetProjectPoint(pl, targetVec);
            return GetAngle(targetVec, pjPnt, vecZ);
        }

        public static XYZ GetPositionVector(XYZ targetVec, XYZ vecX, XYZ vecY)
        {
            XYZ vec = null;
            double ang = GetAngle(targetVec, vecX, vecY);
            if (IsEqual(Math.Abs(ang), Math.PI / 2) || ang < Math.PI / 2)
            {
                vec = targetVec;
            }
            else if (ang > Math.PI / 2)
            {
                vec = -targetVec;
            }
            return vec;
        }
    }

    /// <summary>
    /// Kiểu dữ liệu để sắp xếp tập hợp các tọa độ theo thứ tự Z -> Y -> X
    /// </summary>
    public class ZYXComparer : IComparer<XYZ>
    {
        int IComparer<XYZ>.Compare(XYZ first, XYZ second)
        {
            // first compare z coordinate, then y coordiante, at last x coordinate
            if (GeomUtil.IsEqual(first.Z, second.Z))
            {
                if (GeomUtil.IsEqual(first.Y, second.Y))
                {
                    if (GeomUtil.IsEqual(first.X, second.X))
                    {
                        return 0; // Equal
                    }
                    return (first.X > second.X) ? 1 : -1;
                }
                return (first.Y > second.Y) ? 1 : -1;
            }
            return (first.Z > second.Z) ? 1 : -1;
        }
    }

    /// <summary>
    /// Kiểu dữ liệu để sắp xếp tập hợp các tọa độ theo thứ tự Y -> X
    /// </summary>
    public class YXComparer : IComparer<XYZ>
    {
        int IComparer<XYZ>.Compare(XYZ first, XYZ second)
        {
            if (GeomUtil.IsEqual(first.Y, second.Y))
            {
                if (GeomUtil.IsEqual(first.X, second.X))
                {
                    return 0; // Equal
                }
                return (first.X > second.X) ? 1 : -1;
            }
            return (first.Y > second.Y) ? 1 : -1;
        }
    }

    /// <summary>
    /// Kiểu dữ liệu để sắp xếp tập hợp các tọa độ theo thứ tự X -> Y
    /// </summary>
    public class XYComparer : IComparer<XYZ>
    {
        int IComparer<XYZ>.Compare(XYZ first, XYZ second)
        {
            if (GeomUtil.IsEqual(first.X, second.X))
            {
                if (GeomUtil.IsEqual(first.Y, second.Y))
                {
                    return 0; // Equal
                }
                return (first.Y > second.Y) ? 1 : -1;
            }
            return (first.X > second.X) ? 1 : -1;
        }
    }

    /// <summary>
    /// Hàm sắp xếp các đối tượng trong Revit dựa trên tọa độ điểm đặt các đối tượng
    /// Các tọa độ được sắp xếp theo thứ tự X -> Y
    /// </summary>
    public class ElementLocationComparer : IComparer<Element>
    {
        private Plane Plane;
        public ElementLocationComparer()
        {

        }
        public ElementLocationComparer(Plane pl)
        {
            Plane = pl;
        }
        int IComparer<Element>.Compare(Element x, Element y)
        {
            XYZ loc1 = null;
            XYZ loc2 = null;
            if (x is Wall)
            {
                WallGeometryInfo wgi = new WallGeometryInfo(x as Wall); loc1 = wgi.TopPolygon.CentralXYZPoint;
            }
            if (x is FamilyInstance)
            {
                ColumnGeometryInfo cgi = new ColumnGeometryInfo(x as FamilyInstance); loc1 = cgi.TopPolygon.CentralXYZPoint;
            }
            if (y is Wall)
            {
                WallGeometryInfo wgi = new WallGeometryInfo(y as Wall); loc2 = wgi.TopPolygon.CentralXYZPoint;
            }
            if (y is FamilyInstance)
            {
                ColumnGeometryInfo cgi = new ColumnGeometryInfo(y as FamilyInstance); loc2 = cgi.TopPolygon.CentralXYZPoint;
            }
            loc1 = new XYZ(loc1.X, loc1.Y, 0); loc2 = new XYZ(loc2.X, loc2.Y, 0);
            if (Plane != null)
            {
                loc1 = CheckGeometry.GetProjectPoint(Plane, loc1); loc2 = CheckGeometry.GetProjectPoint(Plane, loc2);
            }

            if (GeomUtil.IsEqual(loc1.X, loc2.X))
            {
                if (GeomUtil.IsEqual(loc1.Y, loc2.Y))
                {
                    return 0; // Equal
                }
                return (loc1.Y > loc2.Y) ? 1 : -1;
            }
            return (loc1.X > loc2.X) ? 1 : -1;
        }
    }

    public class Angle
    {
        public double Radian { get; set; }
        public double Degree { get; set; }
        public XYZ TargetVector { get; set; }
        public Angle(XYZ targetVec, XYZ baseVec)
        {
            this.TargetVector = targetVec;
            this.Radian = this.TargetVector.AngleTo(baseVec);
            this.Degree = this.Radian * 180 / Math.PI;
        }
        public Angle(XYZ targetVec, XYZ xVec, XYZ yVec)
        {
            this.TargetVector = targetVec;
            this.Radian = this.TargetVector.AngleTo(xVec);
            XYZ assTargetVec = xVec * Math.Cos(Radian) + yVec * Math.Sin(Radian);
            if (GeomUtil.IsSameDirection(assTargetVec, this.TargetVector))
            {

            }
            else if (GeomUtil.IsOppositeDirection(assTargetVec, this.TargetVector))
            {
                this.Radian *= -1;
            }
            else
            {
                this.Radian = Math.PI - this.Radian;
                assTargetVec = xVec * Math.Cos(Radian) + yVec * Math.Sin(Radian);
                if (GeomUtil.IsSameDirection(assTargetVec, this.TargetVector))
                {

                }
                else if (GeomUtil.IsOppositeDirection(assTargetVec, this.TargetVector))
                {
                    this.Radian *= -1;
                }
                else
                {
                    throw new Exception("Wrong Calculation!");
                }
            }
            this.Degree = this.Radian * 180 / Math.PI;
        }
    }
}
