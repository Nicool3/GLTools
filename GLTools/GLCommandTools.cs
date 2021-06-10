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
        /// 测试桩号
        /// </summary>
        [CommandMethod("TESTZH")]
        public void testzh()
        {
            ObjectId id = doc.GetEntityOnScreen("请选择");
            string content = db.GetTextData(id).Content;

            ed.WriteMessage("IsMileageNumber? " + content.IsMileageNumber() + "\n");
            ed.WriteMessage("FindMileageNumber? " + content.FindMileageNumber() + "\n");
        }

        /// <summary>
        /// 测试多段线
        /// </summary>
        [CommandMethod("TESTTEMP")]
        public void testtemp()
        {
            SelectionSet ss = doc.GetSelectionSet("请选择");
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 打开块表
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                // 打开块表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                foreach (SelectedObject obj in ss)
                {
                    Entity ent = trans.GetObject(obj.ObjectId, OpenMode.ForWrite) as Entity;
                    if (ent.GetType()==typeof(Line))
                    {
                        ed.WriteMessage("Find 1\n");
                        
                        Line line1 = ent as Line;
                        if (line1.StartPoint.Y > line1.EndPoint.Y)
                        {
                            line1.StartPoint = new Point3d(line1.StartPoint.X, line1.StartPoint.Y - 3, line1.StartPoint.Z);
                            line1.EndPoint = new Point3d(line1.EndPoint.X, line1.EndPoint.Y + 3, line1.EndPoint.Z);
                        }
                        else
                        {
                            line1.StartPoint = new Point3d(line1.StartPoint.X, line1.StartPoint.Y + 3, line1.StartPoint.Z);
                            line1.EndPoint = new Point3d(line1.EndPoint.X, line1.EndPoint.Y - 3, line1.EndPoint.Z);
                        }
                        
                    }
                }
                trans.Commit();
            }
        }

        /// <summary>
        /// 测试多段线
        /// </summary>
        [CommandMethod("TESTREC")]
        public void testpline()
        {
            ObjectId id = doc.GetEntityOnScreen("请选择");
            /*
            PLineData data = db.GetPLineData(id);
            for (int i = 0; i < data.VertexCount; i++)
            {
                ed.WriteMessage(data.Vectors[i].ToString()+"\n");
            }
            ed.WriteMessage("start: "+data.StartPoint.ToString() + "\n");
            ed.WriteMessage("end: " + data.EndPoint.ToString() + "\n");

            //ed.WriteMessage(data.Vectors[0].DotProduct(data.Vectors[1]).ToString() + "\n");
            //ed.WriteMessage(data.Vectors[1].DotProduct(data.Vectors[2]).ToString() + "\n");
            //ed.WriteMessage(data.Vectors[2].DotProduct(data.Vectors[3]).ToString() + "\n");
            ed.WriteMessage(id.ToString() + "\n");
            */
            //RectangleData data = db.GetRectangleData(id);
            db.SetRectangleWidth(id, 10);
            /*
            ed.WriteMessage(db.IsRectangle(id).ToString() + "\n");
            ed.WriteMessage(data.Width.ToString()+"\n");
            ed.WriteMessage(data.Height.ToString() + "\n");
            ed.WriteMessage(data.IsClockWise.ToString() + "\n");
            ed.WriteMessage(data.BasePointIndex.ToString() + "\n");
            ed.WriteMessage(data.BasePoint.ToString() + "\n");
            */
        }

        /// <summary>
        /// 测试
        /// </summary>
        [CommandMethod("CSVERSION")]
        public void testtest()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ed.WriteMessage(version);
            ed.WriteMessage("CSVERSION_1");
        }

        /// <summary>
        /// 测试
        /// </summary>
        [CommandMethod("CSCS")]
        public void test()
        {
            ed.WriteMessage(Math.Abs(Math.Sin(Math.PI*1.5)).ToString());
            ed.WriteMessage(Math.Sin(Math.PI * 1.5).ToString());
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
                            db.AddTextToModeSpace(result.ToString("#.000"), insertp, 3.5, Math.PI*0.5);
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
                                sdata.MileageHead = mincontent!="" ? sdata.Mileage.ToArray()[0].ToString():"Unknown";
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
                            db.AddTextToModeSpace(BGstr, new Point3d(p.X, BGpy, p.Z),3.5,Math.PI*0.5);
                        }
                        if (outZH)
                        {
                            double ZHtail = ZH - (Math.Floor(ZH/1000))*1000;
                            int ZHmile = (int)Math.Floor(ZH / 1000);
                            string ZHstr = ZHhead+"0+000";
                            if(Math.Floor(ZHtail)- ZHtail < 0.001)
                            {
                                ZHstr = ZHhead + ZHmile.ToString() + "+"+ ZHtail.ToString("000");
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
    }
}
