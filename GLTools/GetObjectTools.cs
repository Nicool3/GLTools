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
        public static Point3d? GetPointOnScreen(this Editor ed, string message)
        {
            PromptPointResult Res;
            PromptPointOptions Opts = new PromptPointOptions(message);
            Res = ed.GetPoint(Opts);
            if (Res.Status == PromptStatus.OK) return Res.Value;
            else return null;
        }

        /// <summary>
        /// 输入数字
        /// </summary>
        public static double? GetNumberOnScreen(this Editor ed, string message)
        {
            PromptDoubleResult Res;
            PromptDoubleOptions Opts = new PromptDoubleOptions(message);
            Res = ed.GetDouble(Opts);
            if (Res.Status == PromptStatus.OK) return Res.Value;
            else return null;
        }

        /// <summary>
        /// 输入字符串
        /// </summary>
        public static string GetStringOnScreen(this Editor ed, string message)
        {
            PromptResult Res;
            PromptStringOptions Opts = new PromptStringOptions(message);
            Res = ed.GetString(Opts);
            if (Res.Status == PromptStatus.OK) return Res.StringResult;
            else return null;
        }

        /// <summary>
        /// 输入布尔类型关键字
        /// </summary>
        public static bool GetBoolKeywordOnScreen(this Editor ed, string message)
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
        /// 输入字符串类型关键字
        /// </summary>
        public static string GetStringKeywordOnScreen(this Editor ed, string message,
            string str1, string str1display, string str2, string str2display)
        {
            PromptKeywordOptions Opts = new PromptKeywordOptions(message);
            Opts.Keywords.Add(str1, str1, str1display);
            Opts.Keywords.Add(str2, str2, str2display);
            Opts.Keywords.Add("Esc", "Esc", "退出(Esc)");
            Opts.Keywords.Default = str1;
            Opts.AllowNone = true;

            PromptResult Res = ed.GetKeywords(Opts);
            switch (Res.Status)
            {
                case PromptStatus.OK:
                    return Res.StringResult;

                case PromptStatus.Cancel:
                    return null;

                default:
                    return null;
            }
        }

        /// <summary>
        /// 获取选择集
        /// </summary>
        public static SelectionSet GetSelectionSet(this Document doc, string message, SelectionFilter selFtr=null)
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
        /// 获取实体
        /// </summary>
        public static ObjectId GetEntityOnScreen(this Document doc, string message)
        {
            ObjectId Id = ObjectId.Null;
            PromptEntityOptions peo = new PromptEntityOptions(message);
            PromptEntityResult per = doc.Editor.GetEntity(peo);
            if (per.Status == PromptStatus.OK) Id = per.ObjectId;
            return Id;
        }

        /// <summary>
        /// 获取实体包围盒-选择集
        /// </summary>
        public static Point2d[] GetGeometricExtents(this Database db, SelectionSet sSet)
        {
            // 范围对象
            Extents3d extend = new Extents3d();

            Point2d[] result = new Point2d[2];

            // 判断选择集是否为空
            if (sSet != null)
            {
                // 遍历选择对象
                foreach (SelectedObject selObj in sSet)
                {
                    // 确认返回的是合法的SelectedObject对象  
                    if (selObj != null) //
                    {
                        //开启事务处理
                        using (Transaction trans = db.TransactionManager.StartTransaction())
                        {
                            Entity ent = trans.GetObject(selObj.ObjectId, OpenMode.ForRead) as Entity;
                            // 获取多个实体合在一起的获取其总范围
                            extend.AddExtents(ent.GeometricExtents);

                            trans.Commit();
                        }
                    }
                }
                if (extend != null)
                {
                    // 绘制包围盒
                    result[0] = new Point2d(extend.MinPoint.X, extend.MinPoint.Y);  // 范围最大点
                    result[1] = new Point2d(extend.MaxPoint.X, extend.MaxPoint.Y);  // 范围最小点
                    
                }
            }
            return result;
        }

        /// <summary>
        /// 获取实体包围盒-实体
        /// </summary>
        public static Point2d[] GetGeometricExtents(this Database db, Entity ent)
        {
            // 范围对象
            Extents3d extend = new Extents3d();

            Point2d[] result = new Point2d[2];

            //开启事务处理
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 获取多个实体合在一起的获取其总范围
                extend.AddExtents(ent.GeometricExtents);
                trans.Commit();
            }

            if (extend != null)
            {
                // 绘制包围盒
                result[0] = new Point2d(extend.MinPoint.X, extend.MinPoint.Y);  // 范围最大点
                result[1] = new Point2d(extend.MaxPoint.X, extend.MaxPoint.Y);  // 范围最小点
            }
            return result;
        }

        /// <summary>
        /// 生成单类型过滤器
        /// </summary>
        public static SelectionFilter GetSingleTypeFilter(this string str)
        {
            TypedValue[] typelist = new TypedValue[1];
            typelist.SetValue(new TypedValue((int)DxfCode.Start, str), 0);
            SelectionFilter typefilter = new SelectionFilter(typelist);
            return typefilter;
        }

        /// <summary>
        /// 生成多类型过滤器
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