using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace GLTools
{
    public static partial class GetObjectTools
    {
        /// <summary>
        /// 屏幕取点
        /// </summary>
        public static Point3d GetPointOnScreen(this Editor ed, string opt)
        {
            PromptPointResult PPtRes;
            PromptPointOptions PPtOpts = new PromptPointOptions("");
            PPtOpts.Message = opt;
            PPtRes = ed.GetPoint(PPtOpts);
            return PPtRes.Value;
        }

        /// <summary>
        /// 输入数字
        /// </summary>
        public static double GetNumberOnScreen(this Editor ed, string opt)
        {
            PromptDoubleResult PPtRes;
            PromptDoubleOptions PPtOpts = new PromptDoubleOptions("");
            PPtOpts.Message = opt;
            PPtRes = ed.GetDouble(PPtOpts);
            return PPtRes.Value;
        }

    }
}