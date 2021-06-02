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
        /// 输入字符串
        /// </summary>
        public static string GetStringOnScreen(this Editor ed, string message)
        {
            PromptResult Res;
            PromptStringOptions Opts = new PromptStringOptions(message);
            Res = ed.GetString(Opts);
            return Res.StringResult;
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