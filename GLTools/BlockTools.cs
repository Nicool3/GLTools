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
        /// 通用图素列表排序
        /// </summary>
        public List<BasicEntityData> SortEntityDataList(List<BasicEntityData> EntityList, string method = "Y")
        {
            List<BasicEntityData> lst = new List<BasicEntityData>(EntityList);
            var result = lst;
            switch (method)
            {
                case "Y":
                    result = lst.OrderBy(s => s.Position.X).ToList();
                    break;

                case "X":
                    result = lst.OrderByDescending(s => s.Position.Y).ToList();
                    break;

                default:
                    result = lst;
                    break;
            }
            return result;
        }

        /// <summary>
        /// 块参照列表排序
        /// </summary>
        public List<BlockData> SortBlockDataList(List<BlockData> BlockList, string method = "RowFirst")
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
                                AttRef.TextString = NameHead + "-" + i.ToString("00");
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
        /// 图号重排
        /// </summary>
        [CommandMethod("THCP")]
        public void SortDrawingNumber()
        {
            // 获取当前文档和数据库
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            string method = ed.GetStringKeywordOnScreen("请选择重排方式: ", "RowFirst", "按行(R)", "ColumnFirst", "按列(C)");

            List<BlockData> lst = new List<BlockData>();

            string blocktype = "INSERT";
            SelectionFilter selFtrBlock = blocktype.GetSingleTypeFilter();
            SelectionSet ss = doc.GetSelectionSet("请选择图号需要重排的图框", selFtrBlock);
            if (ss != null)
            {
                foreach (SelectedObject obj in ss)
                {
                    BlockData data = db.GetBlockData(obj.ObjectId);
                    if (data.ProjectName != null) lst.Add(data);
                }

                if (method != null && lst.Count > 0)
                {
                    lst = SortBlockDataList(lst, method);
                    string str = ed.GetStringOnScreen("\n共找到" + lst.Count.ToString() + "个图框, 请输入起始的完整图号: ");

                    RenameDrawingNumber(db, lst, str);
                    ed.WriteMessage("\n修改完成! ");
                }
            }
        }

        /// <summary>
        /// 生成图纸目录
        /// </summary>
        [CommandMethod("TZML")]
        public void CreateMenu()
        {
            // 获取当前文档和数据库
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            List<BlockData> lst = new List<BlockData>();

            string blocktype = "INSERT";
            SelectionFilter selFtrBlock = blocktype.GetSingleTypeFilter();
            SelectionSet ss = doc.GetSelectionSet("请选择图框", selFtrBlock);
            if (ss != null)
            {
                foreach (SelectedObject obj in ss)
                {
                    BlockData data = db.GetBlockData(obj.ObjectId);
                    if (data.ProjectName != null) lst.Add(data);
                }

                if (lst.Count > 0)
                {
                    var orderlst = lst.OrderBy(s => s.DrawingNumber).ToList();
                    Point3d? inp = ed.GetPointOnScreen("\n共找到" + lst.Count.ToString() + "个图框, 请点击图纸目录左上角点: ");
                    if (inp != null)
                    {
                        Point3d p = (Point3d)inp;
                        double h = 3.5, r = 0, f = 0.75;
                        string t = "SMEDI";
                        TextHorizontalMode thmode = TextHorizontalMode.TextLeft;
                        TextVerticalMode tvmode = TextVerticalMode.TextBottom;

                        db.AddTextToModeSpace("1", p + new Vector3d(29.78, -69.09, 0), h, r, f, t, thmode, tvmode);
                        db.AddTextToModeSpace((orderlst[0].DrawingNumber.Substring(0, orderlst[0].DrawingNumber.Length - 2) + "00"), p + new Vector3d(41.48, -69.09, 0), h, r, f, t, thmode, tvmode);
                        db.AddTextToModeSpace("图纸目录", p + new Vector3d(78.36, -69.09, 0), h, r, f, t, thmode, tvmode);
                        for (int i = 0; i < orderlst.Count(); i++)
                        {
                            db.AddTextToModeSpace((i + 2).ToString(), p + new Vector3d(29.78, -69.09 - 8 * (i + 1), 0), h, r, f, t, thmode, tvmode);
                            db.AddTextToModeSpace(orderlst[i].DrawingNumber, p + new Vector3d(41.48, -69.09 - 8 * (i + 1), 0), h, r, f, t, thmode, tvmode);
                            db.AddTextToModeSpace(orderlst[i].DrawingName, p + new Vector3d(78.36, -69.09 - 8 * (i + 1), 0), h, r, f, t, thmode, tvmode);
                        }
                        ed.WriteMessage("\n生成完成! ");
                    }
                }
            }
        }

        /// <summary>
        /// 读图号和图名
        /// </summary>
        [CommandMethod("THTM")]
        public void ReadNumName()
        {
            // 获取当前文档和数据库
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            
        }
    }
}

