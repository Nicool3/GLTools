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
        /// 测试
        /// </summary>
        [CommandMethod("KSH")]
        public void testKSH()
        {
            // 获取当前文档和数据库
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            PromptEntityOptions peo1 = new PromptEntityOptions("/n请选择: ");
            PromptEntityResult per1 = ed.GetEntity(peo1);
            if (per1.Status != PromptStatus.OK) { return; }
            ObjectId objid1 = per1.ObjectId;

            BlockData data = this.GetBlockData(db, objid1);
            ed.WriteMessage(data.BlockName+"\n");
            ed.WriteMessage(data.X.ToString() + "\n");
            ed.WriteMessage(data.ProjectName + "\n");
            ed.WriteMessage(data.DrawingName + "\n");
            ed.WriteMessage(data.DrawingNumber + "\n");
        }
    }
}

