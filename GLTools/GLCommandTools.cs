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
        [CommandMethod("CSCS")]
        public void test0()
        {
            // 获取当前文档和数据库
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            SelectionSet ss1 = null;
            Point3d position;
            string content, textstyle;
            bool status;
            double height, rotation;

            // 文字过滤器
            List<string> strlist = new List<string> { "TEXT", "MTEXT" };
            SelectionFilter selFtrText = strlist.GetTypeFilter("OR");

            ss1 = doc.GetSelectionSet("请选择一列数字", selFtrText);
            foreach (ObjectId objId in ss1.GetObjectIds())
            {
                // db.SetTextStyleCurrent("SMEDI");
                objId.GetTextAttr(out status, out content, out position, out height, out rotation, out textstyle);
                ed.WriteMessage("\n" + status.ToString());
                ed.WriteMessage("\n" + content);
                ed.WriteMessage("\n" + position.X.ToString());
                ed.WriteMessage("\n" + height.ToString());
                ed.WriteMessage("\n" + rotation.ToString());
                ed.WriteMessage("\n" + textstyle);
            }


            //Point3d P0 = new Point3d();
            //P0 = ed.GetPointOnScreen("请指定圆心: ");

            //db.AddCircleModeSpace(P0, 1200);
            //db.SetTextStyleCurrent("SMEDI");
            //db.AddTextToModeSpace(P0, 350, "DN1200");
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
                        string content1, content2, textstyle;
                        double height, rotation;
                        Point3d position1, position2;

                        ObjectId textId1 = ss1.GetObjectIds()[i].GetTextAttr(out tstatus1, out content1, out position1, out height, out rotation, out textstyle);
                        ObjectId textId2 = ss2.GetObjectIds()[i].GetTextAttr(out tstatus2, out content2, out position2, out height, out rotation, out textstyle);

                        if (tstatus1 == true && tstatus2 == true)
                        {
                            Point3d insertp = new Point3d(position1.X, insertpy, position1.Z);
                            double result = Convert.ToDouble(content1) - Convert.ToDouble(content2);
                            db.SetTextStyleCurrent(textstyle);
                            db.AddTextToModeSpace(result.ToString(), insertp, height, rotation);
                        }
                    }
                }
            }
            else ed.WriteMessage("操作有误或两次选取数字数量不符, 请重新选取! ");
        }
    }
}
