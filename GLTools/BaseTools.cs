using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PdfDocument = iTextSharp.text.pdf.PdfDocument;

namespace GLTools
{
    public static partial class BaseTools
    {
        /// <summary>
        /// 角度转化为弧度
        /// </summary>
        /// <param name="degree">角度值</param>
        /// <returns></returns>
        public static double DegreeToAngle(this Double degree)
        {
            return degree * Math.PI / 180;
        }
        /// <summary>
        /// 弧度转换角度
        /// </summary>
        /// <param name="angle">弧度制</param>
        /// <returns></returns>
        public static double AngleToDegree(this Double angle)
        {
            return angle * 180 / Math.PI;
        }

        /// <summary>
        /// 判断三个点是否在同一直线上
        /// </summary>
        /// <param name="firstPoint">第一个点</param>
        /// <param name="secondPoint">第二个点</param>
        /// <param name="thirdPoint">第三个点</param>
        /// <returns></returns>
        public static bool IsOnOneLine(this Point3d firstPoint, Point3d secondPoint, Point3d thirdPoint)
        {
            Vector3d v21 = secondPoint.GetVectorTo(firstPoint);
            Vector3d v23 = secondPoint.GetVectorTo(thirdPoint);
            if (v21.GetAngleTo(v23) == 0 || v21.GetAngleTo(v23) == Math.PI)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static double GetAngleToXAxis(this Point3d startPoint, Point3d endPoint)
        {
            // 声明一个与X轴平行的向量
            Vector3d temp = new Vector3d(1, 0, 0);
            // 获取起点到终点的向量
            Vector3d VsToe = startPoint.GetVectorTo(endPoint);
            return VsToe.Y > 0 ? temp.GetAngleTo(VsToe) : -temp.GetAngleTo(VsToe);
        }

        /// <summary>
        /// 获取两点的3d距离
        /// </summary>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <returns></returns>
        public static double GetDistanceBetweenTwoPoint(this Point3d point1, Point3d point2)
        {
            return (Math.Sqrt(Math.Pow((point1.X - point2.X), 2) + Math.Pow((point1.Y - point2.Y), 2) + Math.Pow((point1.Z + point2.Z), 2)));
        }

        /// <summary>
        /// 获取两点的2d距离
        /// </summary>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <returns></returns>
        public static double GetDistance2dBetweenTwoPoint(this Point3d point1, Point3d point2)
        {
            return (Math.Sqrt(Math.Pow((point1.X - point2.X), 2) + Math.Pow((point1.Y - point2.Y), 2)));
        }


        /// <summary>
        /// 获取两点的中心点
        /// </summary>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <returns></returns>
        public static Point3d GetCenterPointBetweenTwoPoint(this Point3d point1, Point3d point2)
        {
            return new Point3d((point1.X + point2.X) / 2, (point1.Y + point2.Y) / 2, (point1.Z + point2.Z) / 2);
        }

        /// <summary>
        /// 获取多点的中点
        /// </summary>
        /// <returns></returns>
        public static Point3d GetCenterPointFromPointList(this List<Point3d> pointList)
        {
            double x = pointList.Average(s => s.X);
            double y = pointList.Average(s => s.Y);
            double z = pointList.Average(s => s.Z);
            return new Point3d(x, y, z);
        }

        /// <summary>
        /// 求两直线交点
        /// </summary>
        public static Point3d GetLineIntersection(this Database db, ObjectId LineId1, ObjectId LineId2)
        {
            // 启动事务
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                Curve Line1 = (Curve)trans.GetObject(LineId1, OpenMode.ForRead);
                Curve Line2 = (Curve)trans.GetObject(LineId2, OpenMode.ForRead);

                Point3dCollection points = new Point3dCollection();
                Line1.IntersectWith(Line2, Intersect.OnBothOperands, new Plane(), points, IntPtr.Zero, IntPtr.Zero);

                return points[0];
                // 关闭事务
            }
        }

        /// <summary>
        /// 求多条直线及多段线的全部交点，包括多段线的中间顶点
        /// </summary>
        public static List<Point3d> GetAllLineIntersection(this Database db, SelectionSet ss)
        {
            List<Point3d> result = new List<Point3d> { };
            // 启动事务
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                if (ss != null)
                {
                    foreach (SelectedObject obj in ss)
                    {
                        Entity ent = trans.GetObject(obj.ObjectId, OpenMode.ForRead) as Entity;
                        if (ent.GetType() == typeof(Line) || ent.GetType() == typeof(Polyline))
                        {
                            Curve Line1 = (Curve)trans.GetObject(obj.ObjectId, OpenMode.ForRead);
                            foreach (SelectedObject subobj in ss)
                            {
                                Entity subent = trans.GetObject(obj.ObjectId, OpenMode.ForRead) as Entity;
                                if ((subobj.ObjectId != obj.ObjectId) && (subent.GetType() == typeof(Line) || subent.GetType() == typeof(Polyline)))
                                {
                                    Curve Line2 = (Curve)trans.GetObject(subobj.ObjectId, OpenMode.ForRead);
                                    Point3dCollection points = new Point3dCollection();
                                    Line1.IntersectWith(Line2, Intersect.OnBothOperands, new Plane(), points, IntPtr.Zero, IntPtr.Zero);
                                    foreach (Point3d p in points)
                                    {
                                        if (result.Contains(p) == false) result.Add(p);
                                    }
                                }
                            }
                        }
                        if (ent.GetType() == typeof(Polyline))
                        {
                            Polyline pline = ent as Polyline;
                            for (int i = 0; i < pline.NumberOfVertices; i++)
                            {
                                Point3d p = pline.GetPoint3dAt(i);
                                if (p != pline.StartPoint && p != pline.EndPoint && result.Contains(p) == false) result.Add(p);
                            }
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 将多段线转换为直线
        /// </summary>
        public static ObjectId[] ConvertPlineToLine(this Database db, Polyline pline)
        {
            List<Line> lines = new List<Line> { };
            if (pline != null)
            {
                int count = pline.NumberOfVertices;
                if (pline.Closed == true)
                {
                    for (int i = 0; i < count; i++)
                    {
                        lines.Add(new Line(pline.GetPoint3dAt(i), pline.GetPoint3dAt((i + 1) % count)));
                    }
                }
                if (pline.Closed == false)
                {
                    for (int i = 0; i < count - 1; i++)
                    {
                        lines.Add(new Line(pline.GetPoint3dAt(i), pline.GetPoint3dAt(i + 1)));
                    }
                }
            }
            return db.AddEntityToModeSpace(lines.ToArray());
        }

        /// <summary>
        /// 判断直线集中是否有重叠部分
        /// </summary>
        public static bool IsOverlapLine(this List<Line> lineList)
        {
            bool flag = false;

            foreach (Line line in lineList)
            {
                Point3d p0 = line.StartPoint;
                Point3d p1 = line.EndPoint;
                Vector3d v = p0.GetVectorTo(p1);

                foreach (Line subline in lineList)
                {
                    if (subline != line)
                    {
                        Point3d subp0 = subline.StartPoint;
                        Point3d subp1 = subline.EndPoint;
                        Vector3d subv = subp0.GetVectorTo(subp1);
                        Vector3d vp0subp0 = p0.GetVectorTo(subp0);
                        Vector3d vp0subp1 = p0.GetVectorTo(subp1);
                        Vector3d vp1subp0 = p1.GetVectorTo(subp0);
                        Vector3d vp1subp1 = p1.GetVectorTo(subp1);
                        if (v.GetAngleTo(subv) == 0 || v.GetAngleTo(subv) == Math.PI)
                        {
                            if (vp0subp0.GetAngleTo(vp1subp0) == Math.PI || vp0subp1.GetAngleTo(vp1subp1) == Math.PI ||
                                p0 == subp0 || p0 == subp1 || p1 == subp0 || p1 == subp1)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                }
                if (flag) break;
            }
            return flag;
        }


        /// <summary>
        /// 将多条直线合并去重
        /// </summary>
        public static ObjectId[] OverkillLine(this Database db, SelectionSet ss)
        {
            List<Line> rawlines = new List<Line> { };
            List<Line> lines = new List<Line> { };

            if (ss != null)
            {
                foreach (SelectedObject obj in ss)
                {
                    if (obj != null)
                    {
                        Entity ent = obj.ObjectId.GetObject(OpenMode.ForRead) as Entity;
                        if (ent.GetType() == typeof(Line))
                        {
                            Line rawline = ent as Line;
                            rawlines.Add(rawline);
                        }
                    }
                }
            }

            foreach (Line rawline in rawlines)
            {
                Point3d p0 = rawline.StartPoint;
                Point3d p1 = rawline.EndPoint;
                Vector3d v = p0.GetVectorTo(p1);

                foreach (Line subrawline in rawlines)
                {
                    if (subrawline != rawline)
                    {
                        Point3d subp0 = subrawline.StartPoint;
                        Point3d subp1 = subrawline.EndPoint;
                        Vector3d subv = subp0.GetVectorTo(subp1);
                        Vector3d subv1 = p0.GetVectorTo(subp0);

                        if ((v.GetAngleTo(subv) == 0 || v.GetAngleTo(subv) == Math.PI) &&
                            (v.GetAngleTo(subv1) == 0 || v.GetAngleTo(subv1) == Math.PI))
                        {
                            subrawline.ObjectId.ChangeEntityColor(3);
                        }
                    }
                }
            }

            return db.AddEntityToModeSpace(lines.ToArray());
        }

        /// <summary>
        /// 判断文字是否为桩号
        /// </summary>
        public static bool IsMileageNumber(this string str)
        {
            string pattern = @"^[A-G][0-9]+[+][0-9]{3}[.]?[0-9]*$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(str);
        }

        /// <summary>
        /// 返回文字中的桩号内容
        /// </summary>
        public static string FindMileageNumber(this string str)
        {
            string result = "";
            string pattern = @"[A-G][0-9]+[+][0-9]{3}[.]?[0-9]*";
            Regex regex = new Regex(pattern);
            if (regex.IsMatch(str)) result = regex.Match(str).Value;
            return result;
        }

        /// <summary>
        /// 判断文字是否为节点名称
        /// </summary>
        public static bool IsStructureName(this string str)
        {
            string[] KeyNames = { "人员出入口", "通风口", "投料口", "接出口", "交叉口", "端头井", "缝", "变坡", "防火墙" };
            int KeyNameCounts = KeyNames.Count();
            bool[] Flags = new bool[KeyNameCounts];
            for (int i = 0; i < KeyNameCounts; i++)
            {
                if (str.Contains(KeyNames[i])) Flags[i] = true;
            }
            return Flags.Contains(true);
        }

        /// <summary>
        /// 拆分PDF
        /// </summary>
        public static void PDFSplit(string inFile, string[] outFileArray)
        {
            if (outFileArray.Count() != new PdfReader(inFile).NumberOfPages)
                throw new System.Exception("所选择的PDF页数与图号、图名数量不符, 请检查后重试");

            using (var reader = new PdfReader(inFile))
            {
                // 注意起始页是从1开始的
                for (int i = 1; i <= new PdfReader(inFile).NumberOfPages; i++)
                {
                    using (var sourceDocument = new iTextSharp.text.Document(reader.GetPageSizeWithRotation(i)))
                    {
                        var pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(outFileArray[i - 1], System.IO.FileMode.Create));
                        sourceDocument.Open();
                        var importedPage = pdfCopyProvider.GetImportedPage(reader, i);
                        pdfCopyProvider.AddPage(importedPage);
                    }
                }
            }
        }

        public static double MileTextToNumber(this string str)
        {
            if (str.Contains("K") && str.Contains("+"))
            {
                string[] mileStrArray = str.Split('K', '+');
                double mile = Convert.ToDouble(mileStrArray[mileStrArray.Length - 2]) * 1000 + Convert.ToDouble(mileStrArray[mileStrArray.Length - 1]);
                return mile;
            }
            return -1;
        }

        public static string MileNumberToText(this double num, string headStr = "")
        {
            double tailNum = num - (Math.Floor(num / 1000)) * 1000;
            int mileNum = (int)Math.Floor(num / 1000);
            string str = headStr + "K0+000";

            if (Math.Abs(Math.Floor(tailNum) - tailNum) < 0.001)
            {
                str = headStr + "K" + mileNum.ToString() + "+" + tailNum.ToString("000");
            }
            else
            {
                str = headStr + "K" + mileNum.ToString() + "+" + tailNum.ToString("000.000");
            }

            return str;
        }
    }
}