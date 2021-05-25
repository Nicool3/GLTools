using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace GLTools
{
    public static partial class GetObjectTools
    {
        /// <summary>
        /// 屏幕取点
        /// </summary>
        public static Point3d GetPointOnScreen(this Editor ed, string message)
        {
            PromptPointResult Res;
            PromptPointOptions Opts = new PromptPointOptions("");
            Opts.Message = message;
            Res = ed.GetPoint(Opts);
            return Res.Value;
        }

        /// <summary>
        /// 输入数字
        /// </summary>
        public static double GetNumberOnScreen(this Editor ed, string message)
        {
            PromptDoubleResult Res;
            PromptDoubleOptions Opts = new PromptDoubleOptions("");
            Opts.Message = message;
            Res = ed.GetDouble(Opts);
            return Res.Value;
        }

        /// <summary>
        /// 输入关键字
        /// </summary>
        public static bool GetKeywordOnScreen(this Editor ed, string message)
        {
            PromptKeywordOptions Opts = new PromptKeywordOptions("");
            Opts.Message = message;
            Opts.Keywords.Add("Y", "Y", "是(Y)");
            Opts.Keywords.Add("N", "N", "否(N)");
            Opts.Keywords.Default = "N";
            Opts.AllowNone = true;

            PromptResult Res = ed.GetKeywords(Opts);
            if (Res.StringResult == "Y") return true;
            else return false;
        }

        /// <summary>
        /// 获取选择集
        /// </summary>
        public static SelectionSet GetSelectionSet(this Document doc, string message, SelectionFilter selFtr)
        {
            Database db = doc.Database;

            // 请求选择对象
            SelectionSet ss1 = null;
            PromptSelectionOptions Opts = new PromptSelectionOptions();
            Opts.MessageForAdding = message;
            PromptSelectionResult ssp = doc.Editor.GetSelection(Opts, selFtr);

            // 如果状态OK，表示已选择对象
            if (ssp.Status == PromptStatus.OK) ss1 = ssp.Value;
            return ss1;
        }

        /// <summary>
        /// 获取文字相关属性
        /// </summary>
        public static ObjectId GetTextAttr(this ObjectId textId, out bool flag, out string content, out Point3d position)
        {
            // 图形数据库
            Database db = HostApplicationServices.WorkingDatabase;
            // 开启事务处理
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                Entity ent = trans.GetObject(textId, OpenMode.ForRead) as Entity;

                if (ent != null && ent.GetType() == typeof(DBText))
                {
                    DBText text = ent as DBText;
                    flag = true;
                    content = text.TextString;
                    position = text.Position;
                }
                else if (ent != null && ent.GetType() == typeof(MText))
                {
                    MText text = ent as MText;
                    flag = true;
                    content = text.Text;
                    position = text.Location;
                }
                else
                {
                    flag = false;
                    content = "";
                    position = new Point3d(0, 0, 0);
                }
                trans.Commit();
            }
            return textId;
        }
    }
}