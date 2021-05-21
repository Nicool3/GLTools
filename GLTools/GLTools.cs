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


namespace GLTools
{
    public static class GLTools
    {
        /// <summary>
        /// 测试
        /// </summary>
        [CommandMethod("CSCS")]
        public static void CSCS()
        {
            // 获取当前文档和数据库
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            //Point3d P0 = new Point3d();
            //P0 = ed.GetPointOnScreen("请指定圆心: ");

            //db.AddCircleModeSpace(P0, 1200);
            //db.SetTextStyleCurrent("SMEDI");
            //db.AddTextToModeSpace(P0, 350, "DN1200");
            //ed.WriteMessage("\n绘制完成");
            //double X1 = 0;
            //db.WriteDoubleToNOD("X1", P0.X);
            //X1 = db.ReadDoubleFromNOD(doc, "X1");
            //ed.WriteMessage(X1.ToString());
        }

        /// <summary>
        /// 管廊纵断面工具-标高及桩号初始化
        /// </summary>
        [CommandMethod("GLCSH")]
        public static void GLCSH()
        {
            // 获取当前文档和数据库
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            Point3d P_BG1, P_BG2, P_ZH1, P_ZH2;
            double BG1, BG2, ZH1, ZH2;
            P_BG1 = ed.GetPointOnScreen("选择标高基准点1");
            BG1 = ed.GetNumberOnScreen("输入标高基准点1标高: ");
            P_BG2 = ed.GetPointOnScreen("选择标高基准点2");
            BG2 = ed.GetNumberOnScreen("输入标高基准点2标高: ");
            P_ZH1 = ed.GetPointOnScreen("选择桩号基准点1");
            ZH1 = ed.GetNumberOnScreen("输入桩号基准点1桩号(不含字母): ");
            P_ZH2 = ed.GetPointOnScreen("选择桩号基准点2");
            ZH2 = ed.GetNumberOnScreen("输入桩号基准点2桩号(不含字母): ");

            ed.WriteMessage((BG2 - BG1).ToString());
            ed.WriteMessage((P_BG2.Y - P_BG1.Y).ToString());
        }
    }
}
