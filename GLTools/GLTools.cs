﻿using System;
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
        /// 绘制钢筋
        /// </summary>
        [CommandMethod("TTT")]
        public static void TTT()
        {
            // 获取当前文档和数据库
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            Point3d P0 = new Point3d();
            P0 = ed.GetPointOnScreen("请指定圆心: ");

            db.AddCircleModeSpace(P0, 1200);
            db.SetTextStyleCurrent("SMEDI");
            db.AddTextToModeSpace(P0, 350, "DN1200");
            ed.WriteMessage("\n绘制完成");
            db.WriteDataToNOD("X1", P0.X);
            db.ReadDataFromNOD(doc, "X1");

        }
    }
}
