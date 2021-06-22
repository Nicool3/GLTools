using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;


[assembly: CommandClass(typeof(GLTools.GLCommandTools))]

namespace GLTools
{

    public class GLCommandTools
    {
        // 获取当前文档和数据库
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database db = Application.DocumentManager.MdiActiveDocument.Database;
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        /// <summary>
        /// 测试
        /// </summary>
        [CommandMethod("CSCS")]
        public void test()
        {
            SelectionSet ss = null;
            double? offsetDistance = null;

            // 多段线过滤器
            string str = "LINE";
            SelectionFilter filterLine = str.GetSingleTypeFilter();

            offsetDistance = ed.GetNumberOnScreen("请输入偏移距离: ");  // 偏移距离
            double? maxdis = ed.GetNumberOnScreen("请输入最大壁厚或板厚: ");
            ss = doc.GetSelectionSet("请选择4条直线", filterLine);

            List<Point3d> interPoints = db.GetAllLineIntersection(ss);
            ed.WriteMessage(ss.Count + "\n");
            List<Point3d> assignedPoints = new List<Point3d> { };
            List<Point3d[]> groupPoints = new List<Point3d[]> { };
            foreach (Point3d p in interPoints)
            {
                if (assignedPoints.Contains(p) == false)
                {
                    ed.WriteMessage(p.ToString()+"\n");
                    List<Point3d> tempPoints = new List<Point3d> { };
                    foreach (Point3d subp in interPoints)
                    {
                        if ((p.X == subp.X || p.Y == subp.Y) && (p.GetDistanceBetweenTwoPoint(subp) < (double)maxdis))
                        {
                            tempPoints.Add(p);
                            ed.WriteMessage(subp.ToString() + "\n");
                            //db.AddCircleToModeSpace(p, 10);
                        }

                        else if ((p.X != subp.X && p.Y != subp.Y) && (p.GetDistanceBetweenTwoPoint(subp) < (double)maxdis * 1.415))
                        {
                            tempPoints.Add(p);
                            ed.WriteMessage(subp.ToString() + "\n");
                            //db.AddCircleToModeSpace(p, 10);
                        }
                    }
                    if (tempPoints.Count() == 4)
                    {
                        Point3d[] ps = tempPoints.ToArray();
                        groupPoints.Add(ps);
                        assignedPoints.AddRange(tempPoints);
                        foreach (Point3d ppp in ps)
                        {
                            db.AddCircleToModeSpace(ppp, 10);
                        }
                    }
                }
                break;
            }
        }

        /// <summary>
        /// 管廊纵断面工具-标高及桩号初始化
        /// </summary>
        [CommandMethod("GLCSH")]
        public void ini_bg_and_zh()
        {
            // 获取当前文档和数据库
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            // 判断是否已初始化, 如果已初始化判断是否覆盖
            bool overFlag = false;
            if (db.initialized())
            {
                overFlag = ed.GetBoolKeywordOnScreen("已检测到初始化结果, 是否覆盖? ");
                if (overFlag)
                {
                    db.WriteDataToNOD(ed);
                }
            }
            else
            {
                db.WriteDataToNOD(ed);
            }
        }

        /// <summary>
        /// 管廊纵断面工具-两行数值相减
        /// </summary>
        [CommandMethod("SZXJ")]
        public void text_minus()
        {

            SelectionSet ss1 = null, ss2 = null, ss3 = null;

            // 文字过滤器
            List<string> strlist = new List<string> { "TEXT", "MTEXT" };
            SelectionFilter selFtrText = strlist.GetTypeFilter("OR");

            // 直线过滤器
            List<string> linelist = new List<string> { "LINE", "LWPOLYLINE" };
            SelectionFilter selFtrLine = linelist.GetTypeFilter("OR");

            ss1 = doc.GetSelectionSet("请选择被减列数字", selFtrText);
            if (ss1 != null) ss2 = doc.GetSelectionSet("请选择减列数字", selFtrText);
            if (ss2 != null) ss3 = doc.GetSelectionSet("请选择插入数字栏两侧边线", selFtrLine);

            if (ss1 != null && ss2 != null && ss3 != null && ss1.Count == ss2.Count && ss3.Count == 2)
            {

                LineData l1 = db.GetLineData(ss3.GetObjectIds()[0]);
                LineData l2 = db.GetLineData(ss3.GetObjectIds()[1]);

                double insertpy = (l1.StartPoint.Y + l2.StartPoint.Y) / 2;

                // 遍历选择集内的对象
                for (int i = 0; i < ss1.Count; i++)
                {
                    // 确认返回的是合法的 SelectedObject 对象
                    if (ss1.GetObjectIds()[i] != null & ss2.GetObjectIds()[i] != null)
                    {
                        TextData t1 = db.GetTextData(ss1.GetObjectIds()[i]);
                        TextData t2 = db.GetTextData(ss2.GetObjectIds()[i]);

                        try
                        {
                            Point3d insertp = new Point3d(t1.Position.X, insertpy, t1.Position.Z);
                            double result = Convert.ToDouble(t1.Content) - Convert.ToDouble(t2.Content);
                            db.AddTextToModeSpace(result.ToString("#.000"), insertp, 3.5, Math.PI * 0.5);
                        }
                        catch
                        {
                            ed.WriteMessage("出现错误! ");
                        }
                    }
                }
            }
            else ed.WriteMessage("操作有误或两次选取数字数量不符, 请重新选取! ");
        }

        /// <summary>
        /// 管廊平面工具-生成节点桩号表格
        /// </summary>
        [CommandMethod("JDZH")]
        public void create_jd_zh()
        {
            // 平面图中需将坐标系置为WCS
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            // 文字过滤器
            List<string> strlist = new List<string> { "TEXT", "MTEXT" };
            SelectionFilter selFtrText = strlist.GetTypeFilter("OR");
            SelectionSet ss = doc.GetSelectionSet("请选择节点文字", selFtrText);

            List<StructureData> sdatalist = new List<StructureData> { };

            if (ss != null)
            {
                foreach (SelectedObject obj in ss)
                {
                    if (obj != null)
                    {
                        try
                        {
                            TextData tdata = db.GetTextData(obj.ObjectId);
                            Point3d p0 = tdata.Position;
                            double r0 = tdata.Rotation;
                            // 如果文字中不包含构筑物内容则跳过
                            if (tdata.Content.IsStructureName() == false) continue;
                            // 如果文字中本身包含桩号则记录
                            else if (tdata.Content.FindMileageNumber() != "")
                            {
                                StructureData sdata = new StructureData();
                                sdata.Name = tdata.Content.Split(' ')[0];
                                sdata.Mileage = tdata.Content.FindMileageNumber();
                                sdata.MileageHead = sdata.Mileage.ToArray()[0].ToString();
                                sdatalist.Add(sdata);
                                continue;
                            }
                            // 如果文字中包含构筑物内容但本身不包含桩号则按邻近内容查找
                            else
                            {
                                StructureData sdata = new StructureData();
                                sdata.Name = tdata.Content;

                                double mindis = 10;
                                string mincontent = "";
                                foreach (SelectedObject subobj in ss)
                                {
                                    TextData subdata = db.GetTextData(subobj.ObjectId);
                                    Point3d psub = subdata.Position;
                                    double rsub = subdata.Rotation;
                                    if (subdata.Content.IsMileageNumber() == false || (rsub - r0) > 0.05)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        double subdis = p0.GetDistance2dBetweenTwoPoint(psub);
                                        if (subdis < mindis && subdis > 0.01)
                                        {
                                            mindis = subdis;
                                            mincontent = subdata.Content;
                                        }
                                    }
                                }

                                sdata.Mileage = mincontent;
                                sdata.MileageHead = mincontent != "" ? sdata.Mileage.ToArray()[0].ToString() : "Unknown";
                                sdatalist.Add(sdata);
                            }
                        }
                        catch (Autodesk.AutoCAD.Runtime.Exception e)
                        {
                            throw e;
                        }
                    }
                }
            }

            // 节点桩号按桩号头分类
            HashSet<string> headset = new HashSet<string>(from sdata in sdatalist select sdata.MileageHead);
            var headlist = headset.OrderBy(s => s).ToList();

            string method = ed.GetStringKeywordOnScreen("请选择排列方式: ", "1Name", "按节点名称(1)", "2Mile", "按桩号(2)");
            if (method != null && method != "ESC")
            {
                ed.WriteMessage("共有" + headlist.Count() + "个桩号节点表格, 请依次点击生成\n");

                foreach (string head in headlist)
                {
                    // 选取指定桩号头的数据并按节点排序
                    var subdatalist_sorted = (from sdata in sdatalist where sdata.MileageHead == head orderby sdata.Name select sdata).ToList();

                    if (method == "2Mile")
                    {
                        // 选取指定桩号头的数据并按桩号排序
                        subdatalist_sorted = (from sdata in sdatalist where sdata.MileageHead == head orderby sdata.Mileage select sdata).ToList();
                    }

                    Table table = new Table();
                    Point3d? position = ed.GetPointOnScreen("请指定" + head + "段表格插入点: ");
                    if (position != null) table.Position = (Point3d)position; // 设置插入点
                    table.SetSize(subdatalist_sorted.Count() + 1, 2); // 表格大小
                    table.CellType(1, 1);
                    table.Cells.TextStyleId = db.GetTextStyleId("SMEDI");
                    table.Cells.TextHeight = 3.5;
                    table.Cells.Alignment = CellAlignment.MiddleCenter;
                    table.SetRowHeight(6); // 设置行高
                    table.SetColumnWidth(50); // 设置列宽

                    table.Cells[0, 0].TextString = head + "段节点汇总表\n";

                    for (int i = 1; i <= subdatalist_sorted.Count(); i++)
                    {
                        table.Cells[i, 0].TextString = subdatalist_sorted[i - 1].Name;
                        table.Cells[i, 1].TextString = subdatalist_sorted[i - 1].Mileage;
                    }
                    db.AddEntityToModeSpace(table);
                }
            }
        }

        /// <summary>
        /// 管廊纵断面工具-拾取线生成桩号处管廊标高
        /// </summary>
        [CommandMethod("QXBG")]
        public void create_bg_for_line()
        {
            if (db.initialized())
            {
                // 读取初始化数据
                double Y1, BG1, SC_BG;
                try
                {
                    Y1 = (double)db.ReadNumberFromNOD("Y1");
                    BG1 = (double)db.ReadNumberFromNOD("BG1");
                    SC_BG = (double)db.ReadNumberFromNOD("SC_BG");
                }
                catch (Autodesk.AutoCAD.Runtime.Exception e)
                {
                    throw e;
                }

                ObjectId GLLineId = doc.GetEntityOnScreen("请选择管廊线: ");
                if (GLLineId != ObjectId.Null)
                {
                    SelectionSet ssVLine = doc.GetSelectionSet("请选择竖向网格线: ");
                    if (ssVLine != null)
                    {
                        SelectionSet ss2 = doc.GetSelectionSet("请选择插入数字栏两侧边线");
                        if (ss2 != null && ss2.Count == 2)
                        {
                            LineData l1 = db.GetLineData(ss2.GetObjectIds()[0]);
                            LineData l2 = db.GetLineData(ss2.GetObjectIds()[1]);
                            double insertpy = (l1.StartPoint.Y + l2.StartPoint.Y) / 2;

                            foreach (ObjectId VLineId in ssVLine.GetObjectIds())
                            {
                                Point3d p = db.GetLineIntersection(GLLineId, VLineId);
                                Point3d insertp = new Point3d(p.X, insertpy, 0);
                                try
                                {
                                    double BG = BG1 + (p.Y - Y1) * SC_BG;
                                    db.AddTextToModeSpace(BG.ToString("#.000"), insertp, 3.5, Math.PI * 0.5);
                                }
                                catch
                                {
                                    ed.WriteMessage("出现错误! ");
                                }
                            }
                        }
                        else ed.WriteMessage("数字栏两侧边线选择有误, 请重新选择! ");
                    }
                }
            }
            else
            {
                ed.WriteMessage("\n本图还未进行初始化, 请先对标高和桩号进行初始化");
                return;
            }
        }

        /// <summary>
        /// 管廊纵断面工具-选点生成桩号和标高
        /// </summary>
        [CommandMethod("QDZHBG")]
        public void bg_and_zh()
        {
            if (db.initialized())
            {
                // 读取初始化数据
                double X1, Y1, BG1, ZH1, SC_BG, SC_ZH;
                try
                {
                    Y1 = (double)db.ReadNumberFromNOD("Y1");
                    X1 = (double)db.ReadNumberFromNOD("X1");
                    BG1 = (double)db.ReadNumberFromNOD("BG1");
                    ZH1 = (double)db.ReadNumberFromNOD("ZH1");
                    SC_BG = (double)db.ReadNumberFromNOD("SC_BG");
                    SC_ZH = (double)db.ReadNumberFromNOD("SC_ZH");
                }
                catch (Autodesk.AutoCAD.Runtime.Exception e)
                {
                    throw e;
                }

                bool outBG = false, outZH = false;
                double ZHpy = 0, BGpy = 0;
                SelectionSet ssBG = doc.GetSelectionSet("请选择插入标高栏两侧边线, 如无需插入标高则(ESC): ");
                SelectionSet ssZH = doc.GetSelectionSet("请选择插入桩号栏两侧边线, 如无需插入桩号则(ESC): ");
                if (ssBG != null)
                {
                    if (ssBG.Count == 2)
                    {
                        outBG = true;
                        LineData BGl1 = db.GetLineData(ssBG.GetObjectIds()[0]);
                        LineData BGl2 = db.GetLineData(ssBG.GetObjectIds()[1]);
                        BGpy = (BGl1.StartPoint.Y + BGl2.StartPoint.Y) / 2;
                    }
                    else
                    {
                        ed.WriteMessage("标高栏两侧边线选择有误, 请重新选择! ");
                        return;
                    }
                }
                string ZHhead = "A";
                if (ssZH != null)
                {
                    if (ssZH.Count == 2)
                    {
                        outZH = true;
                        LineData ZHl1 = db.GetLineData(ssZH.GetObjectIds()[0]);
                        LineData ZHl2 = db.GetLineData(ssZH.GetObjectIds()[1]);
                        ZHpy = (ZHl1.StartPoint.Y + ZHl2.StartPoint.Y) / 2;
                        ZHhead = ed.GetStringOnScreen("请输入桩号段字母(A/B/C...): ");
                    }
                    else ed.WriteMessage("桩号栏两侧边线选择有误, 请重新选择! ");
                }

                bool flag = true;
                while (flag)
                {
                    Point3d? p0 = ed.GetPointOnScreen("请选择需要标注的点");
                    if (p0 != null)
                    {
                        Point3d p = (Point3d)p0;
                        double BG = Math.Round(BG1 + (p.Y - Y1) * SC_BG, 3);
                        double ZH = Math.Round(ZH1 + (p.X - X1) * SC_ZH, 3);

                        if (outBG)
                        {
                            string BGstr = BG.ToString("#.000");
                            db.AddTextToModeSpace(BGstr, new Point3d(p.X, BGpy, p.Z), 3.5, Math.PI * 0.5);
                        }
                        if (outZH)
                        {
                            double ZHtail = ZH - (Math.Floor(ZH / 1000)) * 1000;
                            int ZHmile = (int)Math.Floor(ZH / 1000);
                            string ZHstr = ZHhead + "0+000";
                            if (Math.Abs(Math.Floor(ZHtail) - ZHtail) < 0.001)
                            {
                                ZHstr = ZHhead + ZHmile.ToString() + "+" + ZHtail.ToString("000");
                                db.AddTextToModeSpace(ZHstr, new Point3d(p.X, ZHpy, p.Z), 3.5, Math.PI * 0.5);
                            }
                            else
                            {
                                ZHstr = ZHhead + ZHmile.ToString() + "+" + ZHtail.ToString("000.000");
                                db.AddTextToModeSpace(ZHstr, new Point3d(p.X, ZHpy, p.Z), 3.5, Math.PI * 0.5);
                            }
                        }
                    }
                    else flag = false;
                }
            }
            else
            {
                ed.WriteMessage("\n本图还未进行初始化, 请先对标高和桩号进行初始化");
                return;
            }
        }

        /// <summary>
        /// 文字镜像
        /// </summary>
        [CommandMethod("WZJX")]
        public void mirror_text_by_line()
        {

            // 单行文字及直线过滤器
            List<string> lst = new List<string> { "TEXT", "LINE" };
            SelectionFilter selftr = lst.GetTypeFilter("OR");

            SelectionSet ss = doc.GetSelectionSet("请选择直线及文字", selftr);
            if (ss != null)
            {
                List<BasicEntityData> entlst = new List<BasicEntityData>();
                List<int> orientlst = new List<int>();
                foreach (SelectedObject obj in ss)
                {
                    if (obj != null)
                    {
                        entlst.Add(db.GetBasicEntityData(obj.ObjectId));
                        orientlst.Add(db.GetBasicEntityData(obj.ObjectId).Orientation);
                    }
                }

                // 检查是否元素均为同一方向
                HashSet<int> orientset = new HashSet<int>(orientlst);
                if (orientset.Count() == 1)
                {
                    string method = orientset.Contains(0) ? "X" : "Y";
                    BlockTools tool = new BlockTools();
                    List<BasicEntityData> entlst_sorted = tool.SortEntityDataList(entlst, method);

                    int n = entlst_sorted.Count();
                    if (n % 2 == 0)
                    {
                        try
                        {
                            for (int i = 0; i < n; i = i + 2)
                            {
                                BasicEntityData entity1 = entlst_sorted[i];
                                BasicEntityData entity2 = entlst_sorted[i + 1];
                                if (entity1.Type == "TEXT" && entity2.Type == "LINE")
                                {
                                    LineData l2 = db.GetLineData(entity2.Id);
                                    entity1.Id.MirrorEntity(l2.StartPoint, l2.EndPoint, true);
                                    entity1.Id.MirrorText();
                                }
                                else if (entity1.Type == "LINE" && entity2.Type == "TEXT")
                                {
                                    LineData l1 = db.GetLineData(entity1.Id);
                                    entity2.Id.MirrorEntity(l1.StartPoint, l1.EndPoint, true);
                                    entity2.Id.MirrorText();
                                }
                                else
                                {
                                    ed.WriteMessage("\n所选文字和直线数量不匹配, 请重新选择! ");
                                }
                            }
                        }
                        catch (Autodesk.AutoCAD.Runtime.Exception e)
                        {
                            throw e;
                        }
                    }
                    else
                    {
                        ed.WriteMessage("\n所选文字和直线数量不匹配, 请重新选择! ");
                    }
                }
                else
                {
                    ed.WriteMessage("\n所选元素方向不一致, 请重新选择! ");
                }

            }
        }

        /// <summary>
        /// 管廊配筋图绘制工具-选择壁板边界绘制剖面配筋图
        /// </summary>
        [CommandMethod("PJT")]
        public void PJT()
        {
            SelectionSet ss = null;
            double? offsetDistance = null;

            // 多段线过滤器
            string str = "LWPOLYLINE";
            SelectionFilter filterPline = str.GetSingleTypeFilter();

            ss = doc.GetSelectionSet("请选择需要绘制配筋图的多段线", filterPline);
            offsetDistance = ed.GetNumberOnScreen("请输入保护层厚度: ");  // 偏移距离
            if (ss != null && offsetDistance!=null)
            {

                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    List<ObjectId> listPlineId = new List<ObjectId> { };
                    List<double> listPlineArea = new List<double> { };
                    foreach (SelectedObject obj in ss)
                    {
                        if (obj != null)
                        {
                            Entity ent = trans.GetObject(obj.ObjectId, OpenMode.ForRead) as Entity;
                            if (ent.GetType() == typeof(Polyline))
                            {
                                Polyline pline = ent as Polyline;
                                double plineArea = pline.Area;
                                listPlineId.Add(obj.ObjectId);
                                listPlineArea.Add(plineArea);
                            }                            
                        }
                    }
                    int indexMax = listPlineArea.IndexOf(listPlineArea.Max());
                    double plineAreaMax = listPlineArea[indexMax];
                    ObjectId plineAreaMaxId = listPlineId[indexMax];

                    List<ObjectId> listOffsetPlineId = new List<ObjectId> { };
                    ObjectId maxId = ObjectId.Null;
                    // 设置当前图层
                    db.SetLayerCurrent("结-钢筋", 1);

                    foreach (ObjectId objId in listPlineId)
                    {
                        Polyline pline = trans.GetObject(objId, OpenMode.ForRead) as Polyline;
                        if (objId == plineAreaMaxId)
                        {
                            Curve plineOffset = pline.GetOffsetCurves((double)offsetDistance)[0] as Curve;
                            ObjectId newId = db.AddEntityToModeSpace(plineOffset);
                            maxId = newId;
                            newId.ChangeEntityLayer("结-钢筋");
                        }
                        else
                        {
                            Curve plineOffset = pline.GetOffsetCurves(-(double)offsetDistance)[0] as Curve;
                            ObjectId newId = db.AddEntityToModeSpace(plineOffset);
                            listOffsetPlineId.Add(newId);
                            newId.ChangeEntityLayer("结-钢筋");
                        }
                    }

                    Polyline maxPline = trans.GetObject(maxId, OpenMode.ForRead) as Polyline;
                    PLineData maxdata = db.GetPLineData(maxId);

                    // 连接最外圈多段线本身内部可连接处
                    for (int i = 0; i < maxdata.VertexCount; i++)
                    {
                        Point3d p = maxdata.VertexPoints[i];
                        Vector3d v = maxdata.Vectors[i];
                        Line line = new Line(p, p + v);
                        Point3dCollection points = new Point3dCollection();
                        maxPline.IntersectWith(line, Intersect.ExtendBoth, new Plane(), points, IntPtr.Zero, IntPtr.Zero);
                        if (points.Count > 2)
                        {
                            foreach (Point3d point in points)
                            {
                                if (maxdata.VertexPoints.Contains(point) == false)
                                { 
                                    ObjectId newId = db.AddLineToModeSpace(p, point);
                                }
                            }
                            
                        }
                    }

                    // 连接内圈多段线和最外圈多段线可连接处
                    foreach (ObjectId objId in listOffsetPlineId)
                    {
                        PLineData data = db.GetPLineData(objId);
                        for(int i=0; i< data.VertexCount; i++)
                        {
                            Point3d p = data.VertexPoints[i];
                            Vector3d v = data.Vectors[i];
                            Line line = new Line(p, p+v);
                            Point3dCollection points = new Point3dCollection();
                            maxPline.IntersectWith(line, Intersect.ExtendBoth, new Plane(), points, IntPtr.Zero, IntPtr.Zero);
                            ObjectId newId = db.AddLineToModeSpace(points[0], points[points.Count-1]);
                        }
                    }
                    trans.Commit();
                }
            }
        }

        /// <summary>
        /// 管廊配筋图绘制工具-绘制转角点筋
        /// </summary>
        [CommandMethod("DJDJ")]
        public void DJDJ()
        {
            SelectionSet ss = null;
            double? offsetDistance = null;

            // 多段线过滤器
            string str = "LINE";
            SelectionFilter filterLine = str.GetSingleTypeFilter();

            offsetDistance = ed.GetNumberOnScreen("请输入偏移距离: ");  // 偏移距离
            ss = doc.GetSelectionSet("请选择4条直线", filterLine);
            // 设置当前图层
            db.SetLayerCurrent("结-钢筋", 1);

            if (offsetDistance != null && ss.Count == 4)
            {
                while (ss != null && ss.Count == 4)
                {
                    List<Point3d> interPoints = db.GetAllLineIntersection(ss);
                    if(interPoints.Count()==4)
                    {
                        using (Transaction trans = db.TransactionManager.StartTransaction())
                        {
                            // 求返回点集的中点
                            double sumX = 0, sumY = 0, sumZ = 0;
                            foreach (Point3d p in interPoints)
                            {
                                sumX += p.X; sumY += p.Y; sumZ += p.Z;
                            }
                            Point3d midPoint = new Point3d(sumX / 4, sumY / 4, sumZ / 4);

                            Point3d? p0raw = null, p1raw = null, p2raw = null, p3raw = null;
                            foreach (Point3d p in interPoints)
                            {
                                Line line = new Line(midPoint, p);
                                // 此处未考虑壁板斜交倾角很大的情况
                                if (line.Angle > Math.PI && line.Angle < Math.PI * 1.5) p0raw = p;
                                else if (line.Angle > Math.PI * 0.5 && line.Angle < Math.PI) p1raw = p;
                                else if (line.Angle > 0 && line.Angle < Math.PI * 0.5) p2raw = p;
                                else if (line.Angle > Math.PI * 1.5 && line.Angle < Math.PI * 2) p3raw = p;
                            }
                            if (p0raw != null && p1raw != null && p2raw != null && p3raw != null)
                            {
                                Point3d p0 = (Point3d)p0raw; Point3d p1 = (Point3d)p1raw; Point3d p2 = (Point3d)p2raw; Point3d p3 = (Point3d)p3raw;
                                Polyline pline = db.DrawPolyLine(true, 0,
                                new Point2d(p0.X, p0.Y), new Point2d(p1.X, p1.Y),
                                new Point2d(p2.X, p2.Y), new Point2d(p3.X, p3.Y));

                                Curve curveOffset = pline.GetOffsetCurves((double)offsetDistance)[0] as Curve;
                                if (curveOffset.GetType() == typeof(Polyline)) {
                                    Polyline plineOffset = curveOffset as Polyline;
                                    for (int i = 0; i < plineOffset.NumberOfVertices; i++)
                                    {
                                        Point3d p = plineOffset.GetPoint3dAt(i);
                                        db.AddCircleToModeSpace(p, (double)offsetDistance/2);
                                    }
                                }
                            }
                            trans.Commit();
                        }
                    }

                    ss = doc.GetSelectionSet("请选择4条直线", filterLine);
                }
            }
            else if (offsetDistance != null && ss.Count > 4)
            {
                double? maxdis = ed.GetNumberOnScreen("请输入最大壁厚或板厚: ");
                if (maxdis != null) { 
                    List<Point3d> interPoints = db.GetAllLineIntersection(ss);
                    List<Point3d> assignedPoints = new List<Point3d> { };
                    List<Point3d[]> groupPoints = new List<Point3d[]> { };
                    foreach (Point3d p in interPoints)
                    {
                        if (assignedPoints.Contains(p) == false) {
                            List<Point3d> tempPoints = new List<Point3d> { };
                            foreach (Point3d subp in interPoints)
                            {
                                if ((p.X==subp.X||p.Y==subp.Y)&&(p.GetDistanceBetweenTwoPoint(subp) < (double)maxdis)) tempPoints.Add(p);
                                else if ((p.X != subp.X && p.Y != subp.Y) && (p.GetDistanceBetweenTwoPoint(subp) < (double)maxdis*1.415)) tempPoints.Add(p);
                            }
                            if (tempPoints.Count() == 4)
                            {
                                Point3d[] ps = tempPoints.ToArray();
                                groupPoints.Add(ps);
                                assignedPoints.AddRange(tempPoints);
                            }
                        }
                    }
                    foreach (Point3d[] ps in groupPoints)
                    {
                        // 求返回点集的中点
                        double sumX = 0, sumY = 0, sumZ = 0;
                        foreach (Point3d p in ps)
                        {
                            sumX += p.X; sumY += p.Y; sumZ += p.Z;
                        }
                        Point3d midPoint = new Point3d(sumX / 4, sumY / 4, sumZ / 4);

                        Point3d? p0raw = null, p1raw = null, p2raw = null, p3raw = null;

                        foreach (Point3d p in ps)
                        {
                            Line line = new Line(midPoint, p);
                            // 此处未考虑壁板斜交倾角很大的情况
                            if (line.Angle > Math.PI && line.Angle < Math.PI * 1.5) p0raw = p;
                            else if (line.Angle > Math.PI * 0.5 && line.Angle < Math.PI) p1raw = p;
                            else if (line.Angle > 0 && line.Angle < Math.PI * 0.5) p2raw = p;
                            else if (line.Angle > Math.PI * 1.5 && line.Angle < Math.PI * 2) p3raw = p;
                        }
                        if (p0raw != null && p1raw != null && p2raw != null && p3raw != null)
                        {
                            Point3d p0 = (Point3d)p0raw; Point3d p1 = (Point3d)p1raw; Point3d p2 = (Point3d)p2raw; Point3d p3 = (Point3d)p3raw;
                            Polyline pline = db.DrawPolyLine(true, 0,
                            new Point2d(p0.X, p0.Y), new Point2d(p1.X, p1.Y),
                            new Point2d(p2.X, p2.Y), new Point2d(p3.X, p3.Y));

                            Curve curveOffset = pline.GetOffsetCurves((double)offsetDistance)[0] as Curve;
                            if (curveOffset.GetType() == typeof(Polyline))
                            {
                                Polyline plineOffset = curveOffset as Polyline;
                                for (int i = 0; i < plineOffset.NumberOfVertices; i++)
                                {
                                    Point3d p = plineOffset.GetPoint3dAt(i);
                                    db.AddCircleToModeSpace(p, (double)offsetDistance / 2);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
