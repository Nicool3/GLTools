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

    /// <summary>
    /// 定义文字属性结构体
    /// </summary>
    public struct TextData
    {
        public ObjectId TextId;
        public string Content;
        public Point3d Position;
        public double X;
        public double Y;
        public double Rotation;
    }

    /// <summary>
    /// 定义线属性结构体
    /// </summary>
    public struct LineData
    {
        public ObjectId LineId;
        public Point3d StartPoint;
        public Point3d EndPoint;
        public double Length;
        public double Orientation;
    }

    /// <summary>
    /// 定义通用图素基础属性结构体
    /// </summary>
    public struct BasicEntityData
    {
        public ObjectId Id;
        public string Type;
        public Point3d Position;
        public int Orientation;
    }

    /// <summary>
    /// 定义管廊初始化内容结构体
    /// </summary>
    public struct GLInitialData
    {
        public double X1, Y1, BG1, ZH1, SC_BG, SC_ZH;
    }


    public static partial class StructTools
    {
        /// <summary>
        /// 获取图框块参照的属性
        /// </summary>
        public static BlockData GetBlockData(this Database db, ObjectId Id)
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
        /// 获取文字基础属性
        /// </summary>
        public static TextData GetTextData(this Database db, ObjectId Id)
        {
            TextData data = new TextData();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                Entity ent = trans.GetObject(Id, OpenMode.ForRead) as Entity;
                data.TextId = Id;

                if (ent != null && ent.GetType() == typeof(DBText))
                {
                    DBText text = ent as DBText;
                    data.Content = text.TextString;
                    data.Position = text.Position;
                    data.X = text.Position.X;
                    data.Y = text.Position.Y;
                    data.Rotation = text.Rotation;
                }
                else if (ent != null && ent.GetType() == typeof(MText))
                {
                    MText mtext = ent as MText;
                    data.Content = mtext.Text;
                    data.Position = mtext.Location;
                    data.X = mtext.Location.X;
                    data.Y = mtext.Location.Y;
                    data.Rotation = mtext.Rotation;
                }
                trans.Commit();
            }
            return data;
        }

        /// <summary>
        /// 获取直线及多段线属性
        /// </summary>
        public static LineData GetLineData(this Database db, ObjectId Id)
        {
            LineData data = new LineData();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                Entity ent = trans.GetObject(Id, OpenMode.ForRead) as Entity;
                data.LineId = Id;

                if (ent != null && ent.GetType() == typeof(Line))
                {
                    Line line = ent as Line;
                    data.StartPoint = line.StartPoint;
                    data.EndPoint = line.EndPoint;
                    data.Length = line.Length;
                    data.Orientation = Math.Asin(Math.Sin(line.Angle));
                    //data.Orientation = Math.Asin((data.EndPoint.Y - data.StartPoint.Y) / data.Length);
                }
                else if (ent != null && ent.GetType() == typeof(Polyline))
                {
                    Polyline pline = ent as Polyline;
                    data.StartPoint = pline.StartPoint;
                    data.EndPoint = pline.EndPoint;
                    data.Length = pline.Length;
                    data.Orientation = Math.Asin((data.EndPoint.Y - data.StartPoint.Y) / data.Length);
                }
                trans.Commit();
            }
            return data;
        }

        /// <summary>
        /// 获取通用图素的属性
        /// </summary>
        public static BasicEntityData GetBasicEntityData(this Database db, ObjectId Id)
        {
            BasicEntityData data = new BasicEntityData();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                Entity ent = trans.GetObject(Id, OpenMode.ForRead) as Entity;
                data.Id = Id;
                if (ent != null && ent.GetType() == typeof(Line))
                {
                    Line line = ent as Line;
                    data.Type = "LINE";
                    data.Position = line.StartPoint;
                    data.Orientation = (int)Math.Abs(Math.Sin(line.Angle)); // Only 0 or 1
                }
                else if (ent != null && ent.GetType() == typeof(DBText))
                {
                    DBText text = ent as DBText;
                    data.Type = "TEXT";
                    data.Position = text.Position;
                    data.Orientation = (int)Math.Sin(text.Rotation);
                }
                else
                {
                    data.Type = "UNSUPPORTED";
                }
            }
            return data;
        }
    }
}
