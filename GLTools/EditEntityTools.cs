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
    public static partial class EditEntityTools
    {
        /// <summary>
        /// 改变图形颜色
        /// </summary>
        /// <param name="c1Id">图形的ObjectId</param>
        /// <param name="colorIndex">颜色索引</param>
        /// <returns>图形的ObjectId</returns> 图形已经添加图形数据库

        public static ObjectId ChangeEntityColor(this ObjectId c1Id, short colorIndex)
        {
            // 图形数据库
            Database db = HostApplicationServices.WorkingDatabase;
            // 开启事务处理
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 打开块表
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                // 打开块表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                // 获取图形对象
                Entity ent1 = (Entity)c1Id.GetObject(OpenMode.ForWrite);
                // 设置颜色
                ent1.ColorIndex = colorIndex;
                trans.Commit();
            }
            return c1Id;
        }


        /// <summary>
        /// 改变图形颜色  图形没有添加到图形数据库
        /// </summary>
        /// <param name="ent">图形对象</param>
        /// <param name="colorIndex">颜色索引</param>
        /// <returns></returns>
        public static void ChangeEntityColor(this Entity ent, short colorIndex)
        {
            // 判断图形的IsNewlyObject
            if (ent.IsNewObject)
            {
                ent.ColorIndex = colorIndex;
            }
            // 不是新图形就调用上面的方法
            else
            {
                ent.ObjectId.ChangeEntityColor(colorIndex);
            }
        }



        /// <summary>
        ///  移动图形 图形已经加入到图形数据库中
        /// </summary>
        /// <param name="entId">图形对象的ObjectId</param>
        /// <param name="sourcePoint">参考原点</param>
        /// <param name="targetPoint">参考目标点</param>
        public static void MoveEntity(this ObjectId entId, Point3d sourcePoint, Point3d targetPoint)
        {
            // 打开当前图形数据库
            Database db = HostApplicationServices.WorkingDatabase;
            // 开启事务处理
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 打开块表
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                // 打开块表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                //Entity ent = (Entity)trans.GetObject(entId, OpenMode.ForWrite);
                // 打开图形
                Entity ent = (Entity)entId.GetObject(OpenMode.ForWrite);
                // 计算变换矩阵
                Vector3d vectoc = sourcePoint.GetVectorTo(targetPoint);
                Matrix3d mt = Matrix3d.Displacement(vectoc);
                ent.TransformBy(mt);
                // 提交事务处理
                trans.Commit();
            }
        }


        /// <summary>
        ///  移动图形 图形没有加到图形数据库中
        /// </summary>
        /// <param name="entId">图形对象的ObjectId</param>
        /// <param name="sourcePoint">参考原点</param>
        /// <param name="targetPoint">参考目标点</param>
        public static void MoveEntity(this Entity ent, Point3d sourcePoint, Point3d targetPoint)
        {
            // 判断图形对象的IsNewlyObject属性
            if (ent.IsNewObject)
            {
                // 计算变换矩阵
                Vector3d vector = sourcePoint.GetVectorTo(targetPoint);
                Matrix3d mt = Matrix3d.Displacement(vector);
                ent.TransformBy(mt);
            }
            else
            {
                ent.ObjectId.MoveEntity(sourcePoint, targetPoint);
            }
        }



        /// <summary>
        ///  复制图形 图形已经加入到图形数据库中
        /// </summary>
        /// <param name="entId">图形对象的ObjectId</param>
        /// <param name="sourcePoint">参考原点</param>
        /// <param name="targetPoint">参考目标点</param>
        public static Entity CopyEntity(this ObjectId entId, Point3d sourcePoint, Point3d targetPoint)
        {
            // 声明一个图形对象
            Entity entR;
            // 当前图形数据库
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 打开块表
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                // 打开块表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                // Entity ent = (Entity)trans.GetObject(entId, OpenMode.ForWrite);
                // 打开图形
                Entity ent = (Entity)entId.GetObject(OpenMode.ForWrite);
                // 计算变换矩阵
                Vector3d vectoc = sourcePoint.GetVectorTo(targetPoint);
                Matrix3d mt = Matrix3d.Displacement(vectoc);
                entR = ent.GetTransformedCopy(mt);
                // 提交事务处理
                trans.Commit();
            }
            return entR;
        }



        /// <summary>
        ///  复制图形 图形没有加到图形数据库中
        /// </summary>
        /// <param name="ent">图形对象</param>
        /// <param name="sourcePoint">参考原点</param>
        /// <param name="targetPoint">参考目标点</param>
        public static Entity CopyEntity(this Entity ent, Point3d sourcePoint, Point3d targetPoint)
        {
            //声明一个图形对象
            Entity entR;
            // 判断图形对象的IsNewlyObject属性
            if (ent.IsNewObject)
            {
                // 计算变换矩阵
                Vector3d vector = sourcePoint.GetVectorTo(targetPoint);
                Matrix3d mt = Matrix3d.Displacement(vector);
                entR = ent.GetTransformedCopy(mt);
            }
            else
            {
                entR = ent.ObjectId.CopyEntity(sourcePoint, targetPoint);
            }
            return entR;
        }
        

        /// <summary>
        /// 旋转图形 图形在数据库中
        /// </summary>
        /// <param name="ent">图形对象</param>
        /// <param name="center">旋转中心</param>
        /// <param name="degree">旋转角度</param>
        public static void RotateEntity(this ObjectId entId, Point3d center, double degree)
        {
            // 当前图形数据库
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 打开块表
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                // 打开块表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                //Entity ent = (Entity)trans.GetObject(entId, OpenMode.ForWrite);
                // 打开图形
                Entity ent = (Entity)entId.GetObject(OpenMode.ForWrite);
                // 计算变换矩阵
                Matrix3d mt = Matrix3d.Rotation(degree.DegreeToAngle(), Vector3d.ZAxis, center);
                ent.TransformBy(mt);
                // 提交事务处理
                trans.Commit();
            }
        }


        /// <summary>
        /// 旋转图形 图形不在数据库中
        /// </summary>
        /// <param name="ent">图形对象</param>
        /// <param name="center">旋转中心</param>
        /// <param name="degree">旋转角度</param>
        public static void RotateEntity(this Entity ent, Point3d center, double degree)
        {
            // 判断图形对象的IsNewlyObject属性
            if (ent.IsNewObject)
            {
                // 计算变换矩阵

                Matrix3d mt = Matrix3d.Rotation(degree.DegreeToAngle(), Vector3d.ZAxis, center);
                ent.TransformBy(mt);
            }
            else
            {
                ent.ObjectId.RotateEntity(center, degree);
            }
        }


        /// <summary>
        /// 镜像图形
        /// </summary>
        /// <param name="ent">图形对象的ObjectId</param>
        /// <param name="point1">第一个镜像点</param>
        /// <param name="point2">第二个镜像点</param>
        /// <param name="isEraseSource">是否删除原图形</param>
        /// <returns>返回新的图形对象  加入图形数据库的情况</returns>
        public static Entity MirrorEntity(this ObjectId entId, Point3d point1, Point3d point2, bool isEraseSource)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;

            // 打开当前图形数据库
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = doc.Editor;
            // 声明一个图形对象用于返回
            Entity entR;
            // 计算镜像的变换矩阵
            Matrix3d mt = Matrix3d.Mirroring(new Line3d(point1, point2));
            // 打开事务处理
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 打开块表
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                // 打开块表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                // 判断是否删除原对象
                if (isEraseSource)
                {
                    // 打开原对象
                    Entity ent = (Entity)entId.GetObject(OpenMode.ForWrite);
                    ed.WriteMessage("\n" + Application.GetSystemVariable("MIRRTEXT").ToString());
                    // 执行变换
                    ent.TransformBy(mt);
                    entR = ent;
                }
                else
                {

                    // 打开原对象
                    Entity ent = (Entity)entId.GetObject(OpenMode.ForRead);
                    entR = ent.GetTransformedCopy(mt);
                }
                trans.Commit();
            }
            return entR;
        }


        /// <summary>
        /// 镜像图形
        /// </summary>
        /// <param name="ent">图形对象的ObjectId</param>
        /// <param name="point1">第一个镜像点</param>
        /// <param name="point2">第二个镜像点</param>
        /// <param name="isEraseSource">是否删除原图形</param>
        /// <returns>返回新的图形对象  没有加入图形数据库的情况</returns>
        public static Entity MirrorEntity(this Entity ent, Point3d point1, Point3d point2, bool isEraseSource)
        {
            
            // 声明一个图形对象用于返回
            Entity entR;

            if (ent.IsNewObject == true)
            {
                // 计算镜像的变换矩阵
                Matrix3d mt = Matrix3d.Mirroring(new Line3d(point1, point2));
                entR = ent.GetTransformedCopy(mt);
            }
            else
            {
                entR = ent.ObjectId.MirrorEntity(point1, point2, isEraseSource);
            }

            return entR;
        }

        /// <summary>
        /// 镜像文字-关于自身镜像
        /// </summary>
        public static Entity MirrorText(this ObjectId entId)
        {
            // 打开当前图形数据库
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = doc.Editor;

            // 声明一个图形对象用于返回
            Entity entR;
            
            // 打开事务处理
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 打开块表
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                // 打开块表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                
                // 打开原对象
                Entity ent = (Entity)entId.GetObject(OpenMode.ForWrite);
                Point2d[] ps = db.GetGeometricExtents(ent);
                TextData tdata = db.GetTextData(entId);
                double ro = tdata.Rotation;

                // 计算镜像的变换矩阵
                Point3d p1 = new Point3d((ps[0].X + ps[1].X) / 2, (ps[0].Y + ps[1].Y) / 2, 0);
                Point3d p2 = new Point3d((ps[0].X + ps[1].X) / 2 + 10 * Math.Cos(ro), (ps[0].Y + ps[1].Y) / 2 + 10 * Math.Sin(ro), 0);
                Matrix3d mt = Matrix3d.Mirroring(new Line3d(p1, p2));
                // 执行变换
                ent.TransformBy(mt);
                entR = ent;
                
                trans.Commit();
            }
            return entR;
        }


        /// <summary>
        /// 缩放图形 图形已经加到图形数据库中
        /// </summary>
        /// <param name="entId">图形对象的ObjectId</param>
        /// <param name="basePoint">缩放的基点</param>
        /// <param name="facter">缩放比例</param>
        public static void ScaleEntity(this ObjectId entId, Point3d basePoint, double facter)
        {
            // 计算缩放矩阵
            Matrix3d mt = Matrix3d.Scaling(facter, basePoint);
            // 启动事务处理
            using (Transaction trans = entId.Database.TransactionManager.StartTransaction())
            {
                // 打开要缩放的图形对象
                Entity ent = (Entity)entId.GetObject(OpenMode.ForWrite);
                ent.TransformBy(mt);
                trans.Commit();
            }
        }


        /// <summary>
        /// 缩放图形 图形没有加到数据库中情况
        /// </summary>
        /// <param name="ent">图形对象</param>
        /// <param name="basePoint">缩放基点</param>
        /// <param name="facter">缩放比例</param>
        public static void ScaleEntity(this Entity ent, Point3d basePoint, double facter)
        {
            // 判断图形对象的IsNewlyObject属性
            if (ent.IsNewObject == true)
            {
                // 计算缩放矩阵
                Matrix3d mt = Matrix3d.Scaling(facter, basePoint);
                ent.TransformBy(mt);
            }
            else
            {
                ent.ObjectId.ScaleEntity(basePoint, facter);
            }
        }


        /// <summary>
        /// 删除图形对象
        /// </summary>
        /// <param name="entId">图形对象的ObjectId</param>
        public static void EraseEntity(this ObjectId entId)
        {
            // 打开事务处理
            using (Transaction trans = entId.Database.TransactionManager.StartTransaction())
            {
                // 打开要删除的图形对象
                Entity ent = (Entity)entId.GetObject(OpenMode.ForWrite);
                ent.Erase();
                trans.Commit();
            }
        }


        /// <summary>
        /// 矩形阵列
        /// </summary>
        /// <param name="entId">图形对象的ObjectId</param>
        /// <param name="rowNum">行数</param>
        /// <param name="columnNum">列数</param>
        /// <param name="disRow">行间距</param>
        /// <param name="disColumn">列间距</param>
        /// <returns>List</returns>  已加入图形数据库
        public static List<Entity> ArrayRectEntity(this ObjectId entId, int rowNum, int columnNum, double disRow, double disColumn)
        {
            // 声明一个Entity类型集合 用于返回
            List<Entity> entList = new List<Entity>();

            // 当前图形数据库
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 打开块表
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                // 打开块表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                //Entity ent = (Entity)trans.GetObject(entId, OpenMode.ForWrite);
                // 打开图形
                Entity ent = (Entity)entId.GetObject(OpenMode.ForWrite);

                // 计算变换矩阵
                for (int i = 0; i < rowNum; i++)
                {
                    for (int j = 0; j < columnNum; j++)
                    {
                        Matrix3d mt = Matrix3d.Displacement(new Vector3d(j * disColumn, i * disRow, 0));
                        Entity entA = ent.GetTransformedCopy(mt);
                        btr.AppendEntity(entA);
                        trans.AddNewlyCreatedDBObject(entA, true);
                        entList.Add(entA);
                    }
                }
                ent.Erase(); // 删除多余的图形
                // 提交事务处理
                trans.Commit();
            }
            return entList;
        }


        /// <summary>
        /// 矩形阵列
        /// </summary>
        /// <param name="entS">图形对象</param>
        /// <param name="rowNum">行数</param>
        /// <param name="columnNum">列数</param>
        /// <param name="disRow">行间距</param>
        /// <param name="disColumn">列间距</param>
        /// <returns>List</returns>  没有加入图形数据库
        public static List<Entity> ArrayRectEntity(this Entity entS, int rowNum, int columnNum, double disRow, double disColumn)
        {
            if (entS.IsNewObject == true)
            {
                // 声明一个Entity类型集合 用于返回
                List<Entity> entList = new List<Entity>();
                Database db = HostApplicationServices.WorkingDatabase;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    // 打开块表
                    BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                    // 打开块表记录
                    BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    for (int i = 0; i < rowNum; i++)
                    {
                        for (int j = 0; j < columnNum; j++)
                        {
                            Matrix3d mt = Matrix3d.Displacement(new Vector3d(j * disColumn, i * disRow, 0));
                            Entity entA = entS.GetTransformedCopy(mt);
                            btr.AppendEntity(entA);
                            trans.AddNewlyCreatedDBObject(entA, true);
                            entList.Add(entA);
                        }
                    }
                    trans.Commit();
                }
                return entList;
            }
            else
            {
                return entS.ArrayRectEntity(rowNum, columnNum, disRow, disColumn);
            }

        }


        /// <summary>
        /// 环形阵列
        /// </summary>
        /// <param name="entId">图形对象的ObjectId</param>
        /// <param name="num">图形数量</param>
        /// <param name="degree">中心点到各个图形的夹角</param>
        /// <param name="center">中心点</param>
        /// <returns>List</returns>  已经加入图形数据库
        public static List<Entity> ArrayPolarEntity(this ObjectId entId, int num, double degree, Point3d center)
        {
            // 声明一个List集合 用于返回
            List<Entity> entList = new List<Entity>();
            // 打开事务处理
            using (Transaction trans = entId.Database.TransactionManager.StartTransaction())
            {
                // 打开块表
                BlockTable bt = (BlockTable)trans.GetObject(entId.Database.BlockTableId, OpenMode.ForRead);
                // 打开块表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                Entity ent = (Entity)entId.GetObject(OpenMode.ForWrite);
                // 限定阵列角度大小
                degree = degree > 360 ? 360 : degree;
                degree = degree < -360 ? 360 : degree;
                int divAngnum = num - 1;
                if (degree == 360 || degree == -360)
                {
                    divAngnum = num;
                }
                for (int i = 0; i < num; i++)
                {
                    Matrix3d mt = Matrix3d.Rotation((i * degree / divAngnum).DegreeToAngle(), Vector3d.ZAxis, center);
                    Entity entA = ent.GetTransformedCopy(mt);
                    btr.AppendEntity(entA);
                    trans.AddNewlyCreatedDBObject(entA, true);
                    entList.Add(entA);
                }
                ent.Erase();
                trans.Commit();
            }
            return entList;
        }



        /// <summary>
        /// 环形阵列
        /// </summary>
        /// <param name="ent">图形对象</param>
        /// <param name="num">图形数量</param>
        /// <param name="degree">中心点到各个图形的夹角</param>
        /// <param name="center">中心点</param>
        /// <returns>List</returns>
        public static List<Entity> ArrayPolarEntity(this Entity ent, int num, double degree, Point3d center)
        {
            if (ent.IsNewObject == true)
            {
                // 声明一个List集合 用于返回
                List<Entity> entList = new List<Entity>();
                Database db = HostApplicationServices.WorkingDatabase;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    // 打开块表
                    BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                    // 打开块表记录
                    BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    degree = degree > 360 ? 360 : degree;
                    degree = degree < -360 ? -360 : degree;
                    int divAngnum = num - 1;
                    if (degree == 360 || degree == -360)
                    {
                        divAngnum = num;
                    }
                    for (int i = 0; i < num; i++)
                    {
                        Matrix3d mt = Matrix3d.Rotation((i * degree / divAngnum).DegreeToAngle(), Vector3d.ZAxis, center);
                        Entity entA = ent.GetTransformedCopy(mt);
                        btr.AppendEntity(entA);
                        trans.AddNewlyCreatedDBObject(entA, true);
                        entList.Add(entA);
                    }
                    trans.Commit();
                }
                return entList;
            }
            else
            {
                return ent.ObjectId.ArrayPolarEntity(num, degree, center);
            }

        }

    }
}