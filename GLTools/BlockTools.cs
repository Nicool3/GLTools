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

[assembly: CommandClass(typeof(GLTools.BlockTools))]

namespace GLTools
{
    /// <summary>
    /// 定义块属性结构体
    /// </summary>
    public struct BlockData
    {
        public ObjectId BlockId;
        public string BlockName;
        public double X;
        public double Y;
        public string ProjectName;
        public string DrawingName;
        public string DrawingNumber;
    }

    public class BlockTools
    {
        /// <summary>
        /// 获取块参照的信息
        /// </summary>
        public BlockData GetBlockData(Database db, ObjectId Id)
        {
            BlockData data = new BlockData();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockReference br = (BlockReference)Id.GetObject(OpenMode.ForRead);
                data.BlockId = Id;
                data.BlockName = br.Name;
                data.X = br.Position.X;
                data.Y = br.Position.Y;

                foreach (ObjectId item in br.AttributeCollection)
                {
                    AttributeReference AttRef = (AttributeReference)item.GetObject(OpenMode.ForRead);
                    if (AttRef.Tag.ToString() == "项目名称") data.ProjectName = AttRef.TextString;
                    else if (AttRef.Tag.ToString() == "图纸名称") data.DrawingName = AttRef.TextString;
                    else if (AttRef.Tag.ToString() == "图号") data.DrawingNumber = AttRef.TextString;
                }
                trans.Commit();
            }
            return data;
        }

        /// <summary>
        /// 块参照列表排序
        /// </summary>
        public List<BlockData> SortBlockDataList(List<BlockData> BlockList, string method="RowFirst")
        {
            List<BlockData> lst = new List<BlockData>(BlockList);
            var result = lst;
            switch (method)
            {
                case "RowFirst":
                    result = lst.OrderByDescending(s => s.Y).ThenBy(s => s.X).ToList();
                    break;

                case "ColumnFirst":
                    result = lst.OrderBy(s => s.X).ThenByDescending(s => s.Y).ToList();
                    break;

                default:
                    result = lst;
                    break;
            }
            return result;
        }

        /// <summary>
        /// 图框块图号重命名
        /// </summary>
        public void RenameDrawingNumber(Database db, List<BlockData> BlockList, string StartString)
        {
            try
            {
                string[] NameHeadArray = StartString.Split('-');
                string NameHead = string.Join("-", NameHeadArray.Take(NameHeadArray.Length - 1).ToArray());
                int StartNumber = Convert.ToInt32(NameHeadArray.Last());

                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    int i = StartNumber;
                    foreach (BlockData block in BlockList)
                    {
                        BlockReference br = (BlockReference)block.BlockId.GetObject(OpenMode.ForWrite);
                        foreach (ObjectId item in br.AttributeCollection)
                        {
                            AttributeReference AttRef = (AttributeReference)item.GetObject(OpenMode.ForWrite);
                            if (AttRef.Tag.ToString() == "图号")
                            {
                                AttRef.TextString= NameHead + "-" + i.ToString("00");
                            }
                        }
                        i++;
                    }
                    trans.Commit();
                }
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// 测试
        /// </summary>
        [CommandMethod("TKTK")]
        public void testTKTK()
        {
            // 获取当前文档和数据库
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            List<BlockData> lst = new List<BlockData>();

            SelectionSet ss = doc.GetSelectionSet("请选择");
            foreach (SelectedObject obj in ss)
            {
                lst.Add(GetBlockData(db, obj.ObjectId));
            }

            lst = SortBlockDataList(lst, "RowFirst");
            string str = ed.GetStringOnScreen("\n请选择: ");

            RenameDrawingNumber(db,lst,str);
            ed.WriteMessage("\n修改完成! ");
        }
    }
}

