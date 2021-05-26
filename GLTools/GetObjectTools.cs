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
        /// 获取单行及多行文字相关属性
        /// </summary>
        /// <param name="textId">文字对象ID</param>
        /// <param name="status">读取状态</param>
        /// <param name="content">文字内容</param>
        /// <param name="position">文字位置</param>
        public static ObjectId GetTextAttr(this ObjectId textId, out bool status, out string content, out Point3d position)
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
                    status = true;
                    content = text.TextString;
                    position = text.Position;
                }
                else if (ent != null && ent.GetType() == typeof(MText))
                {
                    MText mtext = ent as MText;
                    status = true;
                    content = mtext.Text;
                    position = mtext.Location;
                }
                else
                {
                    status = false;
                    content = "";
                    position = new Point3d(0, 0, 0);
                }
                trans.Commit();
            }
            return textId;
        }

        /// <summary>
        /// 获取直线及多段线相关属性
        /// </summary>
        /// <param name="lineId">文字对象ID</param>
        /// <param name="status">读取状态</param>
        /// <param name="startpoint">文字内容</param>
        /// <param name="endpoint">文字位置</param>
        public static ObjectId GetLineAttr(this ObjectId lineId, out bool status, out Point3d startpoint, out Point3d endpoint)
        {
            // 图形数据库
            Database db = HostApplicationServices.WorkingDatabase;
            // 开启事务处理
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                Entity ent = trans.GetObject(lineId, OpenMode.ForRead) as Entity;

                if (ent != null && ent.GetType() == typeof(Line))
                {
                    Line line = ent as Line;
                    status = true;
                    startpoint = line.StartPoint;
                    endpoint = line.EndPoint;
                }
                else if (ent != null && ent.GetType() == typeof(Polyline))
                {
                    Polyline pline = ent as Polyline;
                    status = true;
                    startpoint = pline.StartPoint;
                    endpoint = pline.EndPoint;
                }
                else
                {
                    status = false;
                    startpoint = new Point3d(0, 0, 0);
                    endpoint = new Point3d(0, 0, 0);
                }
                trans.Commit();
            }
            return lineId;
        }

        /// <summary>
        /// 生成过滤器
        /// </summary>
        public static SelectionFilter GetTypeFilter(this List<string> strlist, string opt)
        {
            int n = strlist.Count();
            string startopt = "<" + opt;
            string endopt = opt + ">";
            TypedValue[] typelist = new TypedValue[n+2];
            typelist.SetValue(new TypedValue((int)DxfCode.Operator, startopt), 0);
            for (int i=1; i<n+1; i++)
            {
                typelist.SetValue(new TypedValue((int)DxfCode.Start, strlist[i-1]), i);
            }
            typelist.SetValue(new TypedValue((int)DxfCode.Operator, endopt), n+1);

            SelectionFilter typefilter = new SelectionFilter(typelist);
            return typefilter;
        }
    }
}