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
        /// 测试多段线
        /// </summary>
        [CommandMethod("TESTREC")]
        public void testpline()
        {
            ObjectId id = doc.GetEntityOnScreen("请选择");
            //PLineData data = db.GetPLineData(id);
            //for (int i = 0; i < data.VertexCount; i++)
            //{
            //    ed.WriteMessage(data.Vectors[i].ToString()+"\n");
            //}
            //ed.WriteMessage(data.Vectors[0].DotProduct(data.Vectors[1]).ToString() + "\n");
            //ed.WriteMessage(data.Vectors[1].DotProduct(data.Vectors[2]).ToString() + "\n");
            //ed.WriteMessage(data.Vectors[2].DotProduct(data.Vectors[3]).ToString() + "\n");
            bool flag = db.IsRectangle(id);
            ed.WriteMessage(flag.ToString()+"\n");
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

            List<string> namelist = new List<string> { };
            List<string> mileagelist = new List<string> { };

            if (ss != null)
            {
                foreach (SelectedObject obj in ss)
                {
                    if (obj != null)
                    {
                        try
                        {
                            TextData data = db.GetTextData(obj.ObjectId);
                            // 如果文字中不包含构筑物内容则跳过
                            if (data.Content.IsBuildingName() == false) continue;
                            // 如果文字中包含桩号则记录
                            if (data.Content.IsMileageNumber() == true)
                            {
                                namelist.Add(data.Content.Split(' ')[0]);
                                mileagelist.Add(data.Content.FindMileageNumber());
                                continue;
                            }
                            double mindis = 100;
                            string mincontent = "";
                            foreach (SelectedObject subobj in ss)
                            {
                                TextData subdata = db.GetTextData(subobj.ObjectId);
                                if (subdata.Content.IsMileageNumber() == false) continue;
                                double subdis = data.Position.GetDistance2dBetweenTwoPoint(subdata.Position);
                                if (subdis < mindis && subdis > 0.01)
                                {
                                    mindis = subdis;
                                    mincontent = subdata.Content;
                                }
                            }
                            namelist.Add(data.Content);
                            mileagelist.Add(mincontent);
                        }
                        catch
                        {
                            ed.WriteMessage("\n出现错误! ");
                        }
                    }
                }
            }

            Table table = new Table();
            Point3d? position = ed.GetPointOnScreen("请指定表格插入点: ");
            if(position!=null) table.Position = (Point3d)position; // 设置插入点
            table.SetSize(namelist.Count() + 1, 2); // 表格大小
            table.CellType(1, 1);
            table.Cells.TextStyleId = db.GetTextStyleId("SMEDI");
            table.Cells.TextHeight = 3.5;
            table.Cells.Alignment = CellAlignment.MiddleCenter;
            table.SetRowHeight(6); // 设置行高
            table.SetColumnWidth(50); // 设置列宽

            table.Cells[0, 0].TextString = "节点汇总表";

            for (int i = 1; i <= namelist.Count(); i++)
            {
                table.Cells[i, 0].TextString = namelist[i - 1];
                table.Cells[i, 1].TextString = mileagelist[i - 1];
            }

            db.AddEntityToModeSpace(table);
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
        /// 测试
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
