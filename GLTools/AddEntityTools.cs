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
    public static class AddEntityTools
    {
        /// <summary>
        /// 将图形对象添加到图形文件中
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="ent">图形对象</param>
        /// <returns>图形的ObjectId</returns>
        public static ObjectId AddEntityToModeSpace(this Database db, Entity ent)
        {
            // 声明ObjectId 用于返回
            ObjectId entId = ObjectId.Null;
            // 开启事务处理
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 打开块表
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                // 打开块表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                // 添加图形到块表记录
                entId = btr.AppendEntity(ent);
                // 更新数据信息
                trans.AddNewlyCreatedDBObject(ent, true);
                // 提交事务
                trans.Commit();
            }
            return entId;
        }


        /// <summary>
        /// 添加多个图形对象到图形文件中
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="ent">图形对象 可变参数</param>
        /// <returns>图形的ObjectId数组</returns>
        public static ObjectId[] AddEntityToModeSpace(this Database db, params Entity[] ent)
        {
            // 声明ObjectId 用于返回
            ObjectId[] entId = new ObjectId[ent.Length];
            // 开启事务处理
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 打开块表
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                // 打开块表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                for (int i = 0; i < ent.Length; i++)
                {
                    // 将图形添加到块表记录
                    entId[i] = btr.AppendEntity(ent[i]);
                    // 更新数据信息
                    trans.AddNewlyCreatedDBObject(ent[i], true);

                }
                // 提交事务
                trans.Commit();
            }
            return entId;
        }

        /// <summary>
        /// 绘制直线
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="startPoint">起点坐标</param>
        /// <param name="endPoint">终点坐标</param>
        /// <returns></returns>
        public static ObjectId AddLineToModeSpace(this Database db, Point3d startPoint, Point3d endPoint)
        {
            return db.AddEntityToModeSpace(new Line(startPoint, endPoint));
        }

        /// <summary>
        /// 绘制折线多段线
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="isClosed">是否闭合</param>
        /// <param name="contantWidth">线宽</param>
        /// <param name="vertices">多线段的顶点 可变参数</param>
        /// <returns></returns>
        public static ObjectId AddPolyLineToModeSpace(this Database db, bool isClosed, double contantWidth, params Point2d[] vertices)
        {
            if (vertices.Length < 2)  // 顶点个数小于2 无法绘制
            {
                return ObjectId.Null;
            }
            // 声明一个多段线对象
            Polyline pline = new Polyline();
            // 添加多段线顶点
            for (int i = 0; i < vertices.Length; i++)
            {
                pline.AddVertexAt(i, vertices[i], 0, 0, 0);
            }
            if (isClosed)
            {
                pline.Closed = true;
            }
            // 设置多段线的线宽
            pline.ConstantWidth = contantWidth;
            return db.AddEntityToModeSpace(pline);
        }

        /// <summary>
        /// 添加文字
        /// </summary>
        /// <param name="db"></param>
        /// <param name="point0">文字起始点</param>
        /// <param name="height0">文字高度</param>
        /// <param name="str0">文字内容</param>
        /// <returns>图形的ObjectId</returns>
        public static ObjectId AddTextToModeSpace(this Database db, string content, Point3d position, 
            double height=3.5, double rotation=0, double widthfactor=0.7, string textstylename="SMEDI",
            TextHorizontalMode thmode=TextHorizontalMode.TextCenter, TextVerticalMode tvmode = TextVerticalMode.TextVerticalMid)
        {
            DBText text = new DBText();
            text.TextString = content;
            text.Position = position;
            text.Height = height;
            text.Rotation = rotation;
            text.WidthFactor = widthfactor;
            text.TextStyleId = db.GetTextStyleId(textstylename);
            text.HorizontalMode = thmode;
            text.VerticalMode = tvmode;
            text.AlignmentPoint = text.Position;
            return db.AddEntityToModeSpace(text);
        }

        /// <summary>
        /// 存储自定义数据
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dataName">数据名称</param>
        /// <param name="dataValue">数据值</param>
        /// <returns></returns>
        public static void WriteNumberToNOD(this Database db, string dataName, double dataValue)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 命名对象字典
                DBDictionary nod = trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite) as DBDictionary;
                // 自定义数据
                Xrecord myXrecord = new Xrecord();
                myXrecord.Data = new ResultBuffer(new TypedValue((int)DxfCode.Real, dataValue));
                // 向命名对象字典中存储自定义数据
                nod.SetAt(dataName, myXrecord);
                trans.AddNewlyCreatedDBObject(myXrecord, true);
                trans.Commit();
            }
        }

        /// <summary>
        /// 读取自定义数据
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dataName">数据名称</param>
        /// <returns>读取得到的数据值</returns>
        public static double? ReadNumberFromNOD(this Database db, string dataName)
        {
            double? result = null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 命名对象字典
                DBDictionary nod = trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite) as DBDictionary;
                // 查找自定义数据
                if (nod.Contains(dataName))
                {
                    ObjectId myDataId = nod.GetAt(dataName);
                    Xrecord myXrecord = trans.GetObject(myDataId, OpenMode.ForRead) as Xrecord;
                    result = (double?)myXrecord.Data.AsArray()[0].Value;
                }
                trans.Commit();
            }
            return result;
        }

        /// <summary>
        /// 管廊纵断面工具-标高及桩号初始化
        /// </summary>
        public static void WriteDataToNOD(this Database db, Editor ed)
        {
            string[] messages = { "选择标高基准点1" , "输入标高基准点1标高: " , "选择标高基准点2", "输入标高基准点2标高: " ,
                                  "选择桩号基准点1", "输入桩号基准点1桩号(不含字母): ", "选择桩号基准点2", "输入桩号基准点2桩号(不含字母): "};
            Point3d[] points = new Point3d[4];
            double[] numbers = new double[4];

            Point3d pBG1, pBG2, pZH1, pZH2;
            double BG1, BG2, ZH1, ZH2;

            bool status = true;
            for (int i = 0; i< 8; i = i + 2)
            {
                Point3d? p = ed.GetPointOnScreen(messages[i]);
                double? num = ed.GetNumberOnScreen(messages[i+1]);
                if (p != null && num != null)
                {
                    try
                    {
                        points[i / 2] = (Point3d)p;
                        numbers[i / 2] = (double)num;
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception e)
                    {
                        status = false;
                        throw e;
                    }
                }
                else
                {
                    status = false;
                    break;
                }
            };

            if(status == true)
            {
                pBG1 = points[0];
                BG1 = numbers[0];
                pBG2 = points[1];
                BG2 = numbers[1];
                pZH1 = points[2];
                ZH1 = numbers[2];
                pZH2 = points[3];
                ZH2 = numbers[3];

                // 储存上述结果
                db.WriteNumberToNOD("X1", pZH1.X);
                db.WriteNumberToNOD("Y1", pBG1.Y);
                db.WriteNumberToNOD("BG1", BG1);
                db.WriteNumberToNOD("ZH1", ZH1);
                db.WriteNumberToNOD("SC_BG", Math.Abs((BG2 - BG1) / (pBG2.Y - pBG1.Y)));
                db.WriteNumberToNOD("SC_ZH", Math.Abs((ZH2 - ZH1) / (pZH2.X - pZH1.X)));

                if (db.initialized()) ed.WriteMessage("初始化成功! ");
            }
            else
            {
                ed.WriteMessage("出现错误, 请重新输入! ");
            }
        }

        /// <summary>
        /// 判断DB中是否已存在经过初始化的数据
        /// </summary>
        public static bool initialized(this Database db)
        {
            string[] DBNameArray = { "X1", "Y1", "SC_BG", "SC_ZH" };
            double?[] DBNumberArray = new double?[4];

            for (int i = 0; i < 4; i++)
            {
                DBNumberArray[i] = db.ReadNumberFromNOD(DBNameArray[i]);
            }
            bool emptyFlag = DBNumberArray.Any(x => string.IsNullOrEmpty(x.ToString()));
            return !emptyFlag;
         }
    }
}