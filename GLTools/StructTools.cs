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
    /// 定义多段线属性结构体
    /// </summary>
    public struct PLineData
    {
        public ObjectId PLineId;
        public Point3d StartPoint;
        public Point3d EndPoint;
        public double Length;
        public bool IsClosed;
        public int VertexCount;
        public Point3d[] VertexPoints;
        public Vector3d[] Vectors;
    }

    /// <summary>
    /// 定义矩形属性结构体
    /// </summary>
    public struct RectangleData
    {
        public ObjectId RectangleId;
        public Point3d BasePoint;
        public double Width;
        public double Height;
        public int BasePointIndex;
        public bool IsClockWise;
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
        /// 获取文字及多行文字通用属性
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
        /// 获取直线及多段线通用属性
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
        /// 获取多段线属性
        /// </summary>
        public static PLineData GetPLineData(this Database db, ObjectId Id)
        {
            PLineData data = new PLineData();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                Entity ent = trans.GetObject(Id, OpenMode.ForRead) as Entity;
                data.PLineId = Id;

                try
                {
                    Polyline pline = ent as Polyline;
                    data.StartPoint = pline.StartPoint;
                    data.EndPoint = pline.EndPoint;
                    data.Length = pline.Length;
                    data.VertexCount = pline.NumberOfVertices;
                    data.IsClosed = pline.Closed;
                    data.VertexPoints = new Point3d[data.VertexCount];
                    data.Vectors = new Vector3d[data.VertexCount];
                    for (int i = 0; i < data.VertexCount; i++)
                    {
                        data.VertexPoints[i] = pline.GetPoint3dAt(i);
                        data.Vectors[i] = pline.GetFirstDerivative(pline.GetPoint3dAt(i)).GetNormal();
                    }
                }
                catch(Autodesk.AutoCAD.Runtime.Exception e)
                {
                    throw e;
                }
                trans.Commit();
            }
            return data;
        }

        /// <summary>
        /// 判断多段线是否为矩形
        /// </summary>
        public static bool IsRectangle(this Database db, ObjectId id)
        {
            bool flag = false;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 获取图形对象
                Entity ent = trans.GetObject(id, OpenMode.ForWrite) as Entity;
                if (ent.GetType() == typeof(Polyline))
                {
                    db.PLinePurge(id);
                    PLineData plinedata = db.GetPLineData(id);
                    if (plinedata.VertexCount == 4)
                    {
                        if (plinedata.Vectors[0].DotProduct(plinedata.Vectors[1]).ToString()=="0" &&
                            plinedata.Vectors[1].DotProduct(plinedata.Vectors[2]).ToString() == "0" &&
                            plinedata.Vectors[2].DotProduct(plinedata.Vectors[3]).ToString() == "0") flag = true;
                    }
                }
                trans.Commit();
            }
            return flag;
        }

        /// <summary>
        /// 删除多段线中的多余点并将应闭合的多段线闭合
        /// </summary>
        public static void PLinePurge(this Database db, ObjectId id)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 打开块表
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                // 打开块表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                // 获取图形对象
                Entity ent = trans.GetObject(id, OpenMode.ForWrite) as Entity;
                if (ent.GetType() == typeof(Polyline))
                {
                    Polyline pline = ent as Polyline;
                    PLineData plinedata = db.GetPLineData(id);
                    /// 删除多段线中的多余点
                    if (plinedata.VertexCount > 2)
                    {
                        // 为避免下标出现问题, 从下标最大处开始遍历
                        for (int i = plinedata.VertexCount-2; i > 0; i--)
                        {
                            if(plinedata.Vectors[i-1]== plinedata.Vectors[i])
                            {
                                btr.UpgradeOpen();
                                pline.RemoveVertexAt(i);
                                btr.DowngradeOpen();
                            }
                        }
                    }
                    // 重新读取
                   plinedata = db.GetPLineData(id);
                   if (plinedata.VertexPoints[0] == plinedata.VertexPoints[plinedata.VertexCount-1])
                   {
                        btr.UpgradeOpen();
                        pline.Closed = true;
                        pline.RemoveVertexAt(plinedata.VertexCount - 1);
                        btr.DowngradeOpen();
                   }
                }
                trans.Commit();
            }
        }

        /// <summary>
        /// 获取矩形属性
        /// </summary>
        public static RectangleData GetRectangleData(this Database db, ObjectId id)
        {
            RectangleData data = new RectangleData();
            if (db.IsRectangle(id))
            {
                PLineData plinedata = db.GetPLineData(id);
                for (int i=0; i<4; i++)
                {
                    Vector3d vec1 = plinedata.Vectors[i];
                    Vector3d vec2 = plinedata.Vectors[(i+1)%4];
                    if (vec1 == new Vector3d(1,0,0)&& vec2 == new Vector3d(0, 1, 0))
                    {
                        data.IsClockWise = false;
                        data.BasePointIndex = i;
                        data.BasePoint = plinedata.VertexPoints[i];
                    }
                    else if (vec1 == new Vector3d(0, 1, 0) && vec2 == new Vector3d(1, 0, 0))
                    {
                        data.IsClockWise = true;
                        data.BasePointIndex = i;
                        data.BasePoint = plinedata.VertexPoints[i];
                    }
                }
                Point3d p0 = plinedata.VertexPoints[0];
                Point3d p2 = plinedata.VertexPoints[2];
                data.Width = Math.Abs(p2.X - p0.X);
                data.Height = Math.Abs(p2.Y - p0.Y);
            }
            return data;
        }

        /// <summary>
        /// 修改矩形宽度
        /// </summary>
        public static void SetRectangleWidth(this Database db, ObjectId id, double newWidth)
        {
            if (db.IsRectangle(id))
            {
                RectangleData data = db.GetRectangleData(id);
                int baseIndex = data.BasePointIndex;
                Point3d basePoint = data.BasePoint;
                bool clockWise = data.IsClockWise;
                double width = data.Width;
                double height = data.Height;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    // 打开块表
                    BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                    // 打开块表记录
                    BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                    // 获取图形对象
                    Entity ent = trans.GetObject(id, OpenMode.ForWrite) as Entity;
                    if (ent.GetType() == typeof(Polyline))
                    {
                        Polyline pline = ent as Polyline;
                        btr.UpgradeOpen();
                        pline.SetPointAt(baseIndex, new Point2d(basePoint.X- newWidth/2, basePoint.Y));
                        if (clockWise)
                        {
                            pline.SetPointAt((baseIndex+1)%4, new Point2d(basePoint.X - newWidth / 2, basePoint.Y+height));
                            pline.SetPointAt((baseIndex + 2) % 4, new Point2d(basePoint.X +width+ newWidth / 2, basePoint.Y + height));
                            pline.SetPointAt((baseIndex + 3) % 4, new Point2d(basePoint.X + width + newWidth / 2, basePoint.Y));
                        }
                        else
                        {
                            pline.SetPointAt((baseIndex + 1) % 4, new Point2d(basePoint.X + width + newWidth / 2, basePoint.Y));
                            pline.SetPointAt((baseIndex + 2) % 4, new Point2d(basePoint.X + width + newWidth / 2, basePoint.Y + height));
                            pline.SetPointAt((baseIndex + 3) % 4, new Point2d(basePoint.X - newWidth / 2, basePoint.Y + height));
                        }
                        btr.DowngradeOpen();
                    }
                    trans.Commit();
                }
            }
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
