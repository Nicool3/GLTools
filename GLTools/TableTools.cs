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
using Autodesk.AutoCAD.Colors;

namespace GLTools
{
    public static class TableTools
    {
        public static void AddTable(this Database db)
        {
            Table table = new Table();

            table.SetSize(10, 5); // 表格大小
            table.SetRowHeight(10); // 设置行高
            table.SetColumnWidth(50); // 设置列宽
            table.Columns[0].Width = 20; // 设置第一列宽度为20
            table.Position = new Point3d(100, 100, 0); // 设置插入点
            table.Cells[0, 0].TextString = "测试表格数据统计";
            table.Cells[0, 0].TextHeight = 6; //设置文字高度
            Color color = Color.FromColorIndex(ColorMethod.ByAci, 3); // 声明颜色
            table.Cells[0, 0].BackgroundColor = color; // 设置背景颜色
            color = Color.FromColorIndex(ColorMethod.ByAci, 1);
            table.Cells[0, 0].ContentColor = color; //内容颜色

            table.Cells[1, 1].TextString = "11111";
            table.Cells[2, 3].TextString = "23333";

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                btr.AppendEntity(table);
                trans.AddNewlyCreatedDBObject(table, true);
                trans.Commit();
            }
        }
    }
}
