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

            Point3d P0 = new Point3d();
            P0 = ed.GetPointOnScreen("请指定圆心: ");

            db.AddCircleModeSpace(P0, 1200);
            db.SetTextStyleCurrent("SMEDI");
            db.AddTextToModeSpace(P0, 350, "DN1200");
            ed.WriteMessage("\n绘制完成");

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

            SelectionSet ss1 = null, ss2 = null, ss3 = null; ;

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
            TypedValue[] typeArrText = new TypedValue[4];
            typeArrText.SetValue(new TypedValue((int)DxfCode.Operator, "<OR"), 0);
            typeArrText.SetValue(new TypedValue((int)DxfCode.Start, "TEXT"), 1);
            typeArrText.SetValue(new TypedValue((int)DxfCode.Start, "MTEXT"), 2);
            typeArrText.SetValue(new TypedValue((int)DxfCode.Operator, "OR>"), 3);
            SelectionFilter selFtrText = new SelectionFilter(typeArrText);

            // 直线过滤器
            TypedValue[] typeArrLine = new TypedValue[4];
            typeArrLine.SetValue(new TypedValue((int)DxfCode.Operator, "<OR"), 0);
            typeArrLine.SetValue(new TypedValue((int)DxfCode.Start, "LINE"), 1);
            typeArrLine.SetValue(new TypedValue((int)DxfCode.Start, "LWPOLYLINE"), 2);
            typeArrLine.SetValue(new TypedValue((int)DxfCode.Operator, "OR>"), 3);
            SelectionFilter selFtrLine = new SelectionFilter(typeArrLine);

            ss1 = doc.GetSelectionSet("请选择第一列数字", selFtrText);
            if (ss1 != null) ss2 = doc.GetSelectionSet("请选择第二列数字", selFtrText);
            if (ss2 != null) ss3 = doc.GetSelectionSet("请选择插入数字栏两侧边线", selFtrLine);

            if (ss1 != null && ss2 != null && ss3 != null && ss1.Count == ss2.Count && ss3.Count == 2)
            {
                //开启事务
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    Entity entline1 = trans.GetObject(ss3.GetObjectIds()[0], OpenMode.ForRead) as Entity;
                    if (entline1 != null && entline1.GetType() == typeof(Line))
                    {
                        Line line1 = entline1 as Line;
                        Point3d p1 = line1.StartPoint;
                        ed.WriteMessage("\n" + p1.X.ToString());
                    }
                    else if (entline1 != null && entline1.GetType() == typeof(Polyline))
                    {
                        Polyline line1 = entline1 as Polyline;
                        Point3d p1 = line1.StartPoint;
                        ed.WriteMessage("\n" + p1.X.ToString());
                    }
                    // 遍历选择集内的对象
                    foreach (SelectedObject obj in ss1)
                    {
                        // 确认返回的是合法的 SelectedObject 对象
                        if (obj != null)
                        {
                            // 以读模式打开所选对象
                            Entity ent = trans.GetObject(obj.ObjectId, OpenMode.ForRead) as Entity;
                            if (ent != null && ent.GetType() == typeof(DBText))
                            {
                                DBText text = ent as DBText;
                                Point3d position = text.Position;
                                ed.WriteMessage("\n" + text.TextString);
                                ed.WriteMessage("\n" + position.X.ToString());
                            }
                            else if (ent != null && ent.GetType() == typeof(MText))
                            {
                                MText text = ent as MText;
                                Point3d position = text.Location;
                                ed.WriteMessage("\n" + text.Text);
                                ed.WriteMessage("\n" + position.X.ToString());
                            }
                        }
                    }
                }
            }
            else ed.WriteMessage("操作有误或两次选取数字数量不符, 请重新选取! ");
        }
    }
}
