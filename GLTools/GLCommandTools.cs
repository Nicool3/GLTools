using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(GLTools.GLCommandTools))]

namespace GLTools
{
    public class GLCommandTools
    {
       
        /// <summary>
        /// 测试
        /// </summary>
        [CommandMethod("ZHZH")]
        public void test()
        {
            // 获取当前文档和数据库
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            PromptEntityOptions peo1 = new PromptEntityOptions("/n请选择: ");
            PromptEntityResult per1 = ed.GetEntity(peo1);
            if (per1.Status != PromptStatus.OK) { return; }
            ObjectId objid1 = per1.ObjectId;
            bool status0 = false;
            string content0 = "";
            Point3d p0 = new Point3d(0, 0, 0);

            objid1.GetTextAttr(out status0, out content0, out p0);
            ed.WriteMessage(content0.FindMileageNumber());

        }

        /// <summary>
        /// 测试
        /// </summary>
        [CommandMethod("CSCS")]
        public void test0()
        {
            // 获取当前文档和数据库
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // 平面图中需将坐标系置为WCS
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            // 文字过滤器
            List<string> strlist = new List<string> { "TEXT", "MTEXT" };
            SelectionFilter selFtrText = strlist.GetTypeFilter("OR");
            SelectionSet ss = doc.GetSelectionSet("请选择节点文字", selFtrText);

            List<string> namelist = new List<string> { };
            List<string> mileagelist = new List<string> { };

            foreach (SelectedObject obj in ss)
            {
                if (obj != null)
                {
                    bool status0 = false;
                    string content0 = "";
                    Point3d p0 = new Point3d(0, 0, 0);
                    try{
                        obj.ObjectId.GetTextAttr(out status0, out content0, out p0);
                        // 如果文字中不包含构筑物内容则跳过
                        if (content0.IsBuildingName() == false) continue;
                        // 如果文字中包含桩号则记录
                        if (content0.IsMileageNumber() == true)
                        {
                            namelist.Add(content0.Split(' ')[0]);
                            mileagelist.Add(content0.FindMileageNumber());
                            continue;
                        }
                        double mindis = 100;
                        string mincontent = "";
                        foreach (SelectedObject subobj in ss)
                        {
                            bool substatus = false;
                            string subcontent = "";
                            Point3d subp = new Point3d(0, 0, 0);
                            subobj.ObjectId.GetTextAttr(out substatus, out subcontent, out subp);
                            if (subcontent.IsMileageNumber() == false) continue;
                            double subdis = p0.GetDistance2dBetweenTwoPoint(subp);
                            if (subdis < mindis && subdis > 0.01)
                            {
                                mindis = subdis;
                                mincontent = subcontent;
                            }
                        }
                        namelist.Add(content0);
                        mileagelist.Add(mincontent);
                    }
                    catch
                    {
                        ed.WriteMessage("\n出现错误! ");
                    }
                }
            }

            Table table = new Table();
            Point3d position = ed.GetPointOnScreen("请指定表格插入点: ");
            table.Position = position; // 设置插入点
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
                table.Cells[i, 0].TextString = namelist[i-1];
                table.Cells[i, 1].TextString = mileagelist[i-1];
            }

            db.AddEntityToModeSpace(table);

            //PromptEntityOptions peo1 = new PromptEntityOptions("/n请选择第一条曲线: ");
            //PromptEntityResult per1 = ed.GetEntity(peo1);
            //if (per1.Status != PromptStatus.OK) { return; }
            //ObjectId objid1 = per1.ObjectId;

            //PromptEntityOptions peo2 = new PromptEntityOptions("/n请选择第二条曲线: ");
            //PromptEntityResult per2 = ed.GetEntity(peo2);
            //if (per2.Status != PromptStatus.OK) { return; }
            //ObjectId objid2 = per2.ObjectId;

            //Point3d m_pt = db.GetLineIntersection(objid1, objid2);
            //ed.WriteMessage("/n第一条曲线与第二条曲线交点:{0}", m_pt);

            //db.AddCircleModeSpace(position, 1200);
            //db.SetTextStyleCurrent("SMEDI");
            //db.AddTextToModeSpace("DN1200", position);
            //ed.WriteMessage("\n绘制完成");

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
            if (db.initialized(doc))
            {
                overFlag = ed.GetKeywordOnScreen("已检测到初始化结果, 是否覆盖? ");
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
            // 获取当前文档和数据库
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            SelectionSet ss1 = null, ss2 = null, ss3 = null;

            if (db.initialized(doc))
            {
                // 读取初始化数据
                double? X1, Y1, SC_BG, SC_ZH;
                X1 = db.ReadNumberFromNOD(doc, "X1");
                Y1 = db.ReadNumberFromNOD(doc, "Y1");
                SC_BG = db.ReadNumberFromNOD(doc, "SC_BG");
                SC_ZH = db.ReadNumberFromNOD(doc, "SC_ZH");
            }
            else
            {
                ed.WriteMessage("\n本图还未进行初始化, 请先对标高和桩号进行初始化");
                return;
            }

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
                bool status1, status2;
                Point3d startp1, endp1, startp2, endp2;

                ObjectId lineId1 = ss3.GetObjectIds()[0].GetLineAttr(out status1, out startp1, out endp1);
                ObjectId lineId2 = ss3.GetObjectIds()[1].GetLineAttr(out status2, out startp2, out endp2);

                double insertpy = (startp1.Y + startp2.Y) / 2;

                // 遍历选择集内的对象
                for (int i = 0; i < ss1.Count; i++)
                {
                    // 确认返回的是合法的 SelectedObject 对象
                    if (ss1.GetObjectIds()[i] != null & ss2.GetObjectIds()[i] != null)
                    {
                        bool tstatus1, tstatus2;
                        string content1, content2;
                        Point3d position1, position2;

                        ObjectId textId1 = ss1.GetObjectIds()[i].GetTextAttr(out tstatus1, out content1, out position1);
                        ObjectId textId2 = ss2.GetObjectIds()[i].GetTextAttr(out tstatus2, out content2, out position2);

                        if (tstatus1 == true && tstatus2 == true)
                        {
                            Point3d insertp = new Point3d(position1.X, insertpy, position1.Z);
                            double result = Convert.ToDouble(content1) - Convert.ToDouble(content2);
                            db.AddTextToModeSpace(result.ToString("#.000"), insertp, 3.5, Math.PI*0.5);
                        }
                    }
                }
            }
            else ed.WriteMessage("操作有误或两次选取数字数量不符, 请重新选取! ");
        }
    }
}
