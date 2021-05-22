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

            // 判断DB中是否已存在经过初始化的数据
            string[] DBNameArray = { "X1", "Y1", "SC_BG", "SC_ZH" };
            double?[] DBNumberArray = new double?[4];

            for (int i = 0; i < 4; i++)
            {
                DBNumberArray[i] = db.ReadNumberFromNOD(doc, DBNameArray[i]);
            }
            bool emptyFlag = DBNumberArray.Any(x => string.IsNullOrEmpty(x.ToString()));
            bool overFlag = false;
            if (!emptyFlag)
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
    }
}
