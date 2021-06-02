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

    public partial class BlockTools
    {
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
        [CommandMethod("THCP")]
        public void SortDrawingNumber()
        {
            // 获取当前文档和数据库
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            List<BlockData> lst = new List<BlockData>();

            string blocktype = "INSERT"; 
            SelectionFilter selFtrBlock = blocktype.GetSingleTypeFilter();
            SelectionSet ss = doc.GetSelectionSet("请选择图号需要重排的图框", selFtrBlock);
            if (ss != null)
            {
                foreach (SelectedObject obj in ss)
                {
                    BlockData data = db.GetBlockData(obj.ObjectId);
                    if (data.ProjectName!=null) lst.Add(data);
                }

                string method = ed.GetStringKeywordOnScreen("共找到"+ lst.Count.ToString() +"个图框, 请选择重排方式: ", "RowFirst", "按行(R)", "ColumnFirst", "按列(C)");

                if (method != null)
                {
                    lst = SortBlockDataList(lst, method);
                    string str = ed.GetStringOnScreen("\n请输入起始的完整图号: ");

                    RenameDrawingNumber(db, lst, str);
                    ed.WriteMessage("\n修改完成! ");
                }
            }
        }
    }
}

