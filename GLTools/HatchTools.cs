using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLTools
{
    public static partial class HatchTools
    {
        /// <summary>
        /// 填充图案名称
        /// </summary>
        public struct HatchPatternName
        {
            public static readonly string solid = "SOLID";
            public static readonly string angle = "ANGLE";
            public static readonly string ansi31 = "ANSI31";
            public static readonly string ansi32 = "ANSI32";
            public static readonly string ansi33 = "ANSI33";
            public static readonly string ansi34 = "ANSI34";
            public static readonly string ansi35 = "ANSI35";
            public static readonly string ansi36 = "ANSI36";
            public static readonly string ansi37 = "ANSI37";
            public static readonly string ansi38 = "ANSI38";
            public static readonly string arb816 = "AR-B816";
            public static readonly string arb816C = "AR-B816C";
            public static readonly string arb88 = "AR-B88";
            public static readonly string arbrelm = "AR-BRELM";
            public static readonly string arbrstd = "AR-BRSTD";
            public static readonly string arbconc = "AR-CONC";
        }

        /// <summary>
        /// 渐变填充名称
        /// </summary>
        public struct HatchGradientName
        {
            public static readonly string gr_linear = "Linear";
            public static readonly string gr_cylinear = "Cylinear";
            public static readonly string gr_invcylinear = "Invcylinear";
            public static readonly string gr_spherical = "Spherical";
            public static readonly string gr_hemisperical = "Hemisperical";
            public static readonly string gr_curved = "Curved";
            public static readonly string gr_invsperical = "Inveperical";
            public static readonly string gr_invhemisperical = "Invhemisperical";
            public static readonly string gr_invcurved = "Invcurved";

        }

        /// <summary>
        /// 图案填充 无颜色
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="patternName">图案名称</param>
        /// <param name="scale">填充比例</param>
        /// <param name="degree">旋转角度</param>
        /// <param name="entid">边界图形的ObjectId</param>
        /// <returns></returns>
        public static ObjectId HatchEnity(this Database db, string patternName, double scale, double degree, ObjectId entid)
        {
            ObjectId hatchId = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 声明一个图案填充对象
                Hatch hatch = new Hatch();
                // 设置填充比例
                hatch.PatternScale = scale;
                // 设置填充类型和图案名称
                hatch.SetHatchPattern(HatchPatternType.PreDefined, "ANGLE");
                // 加入图形数据库
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                hatchId = btr.AppendEntity(hatch);
                trans.AddNewlyCreatedDBObject(hatch, true);

                // 设置填充角度
                hatch.PatternAngle = degree;
                // 设置关联
                hatch.Associative = true;
                // 设置边界图形和填充方式

                ObjectIdCollection obIds = new ObjectIdCollection();
                obIds.Add(entid);
                hatch.AppendLoop(HatchLoopTypes.Outermost, obIds);
                // 计算填充并显示
                hatch.EvaluateHatch(true);
                // 提交事务
                trans.Commit();
            }
            return hatchId;
        }


        /// <summary>
        /// 图案填充 有填充颜色
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="patternName">图案名称</param>
        /// <param name="scale">填充比例</param>
        /// <param name="degree">旋转角度</param>
        /// <param name="bkColor">背景色</param>
        /// <param name="hatchColorIndex">填充图案的颜色</param>
        /// <param name="entid">边界图形的ObjectId</param>
        /// <returns></returns>

        public static ObjectId HatchEnity(this Database db, string patternName, double scale, double degree, Color bkColor, int hatchColorIndex, ObjectId entid)
        {
            ObjectId hatchId = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 声明一个图案填充对象
                Hatch hatch = new Hatch();
                // 设置填充比例
                hatch.PatternScale = scale;
                // 设置背景色
                hatch.BackgroundColor = bkColor;
                // 设置填充图案颜色
                hatch.ColorIndex = hatchColorIndex;
                // 设置填充类型和图案名称
                hatch.SetHatchPattern(HatchPatternType.PreDefined, "ANGLE");
                // 加入图形数据库
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                hatchId = btr.AppendEntity(hatch);
                trans.AddNewlyCreatedDBObject(hatch, true);

                // 设置填充角度
                hatch.PatternAngle = degree;
                // 设置关联
                hatch.Associative = true;
                // 设置边界图形和填充方式


                ObjectIdCollection obIds = new ObjectIdCollection();
                obIds.Add(entid);
                hatch.AppendLoop(HatchLoopTypes.Outermost, obIds);
                // 计算填充并显示
                hatch.EvaluateHatch(true);
                // 提交事务
                trans.Commit();
            }
            return hatchId;
        }


        /// <summary>
        /// 图案填充
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="loopTypes"></param>
        /// <param name="patternName">图案名称</param>
        /// <param name="scale">填充比例</param>
        /// <param name="degree">旋转角度</param>
        /// <param name="entid">边界图形的ObjectId</param>
        /// <returns></returns>
        public static ObjectId HatchEnity(this Database db, List<HatchLoopTypes> loopTypes, string patternName, double scale, double degree, params ObjectId[] entid) // 一个方法只能传递一个可变参数 且需要放在最后
        {
            ObjectId hatchId = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 声明一个图案填充对象
                Hatch hatch = new Hatch();
                // 设置填充比例
                hatch.PatternScale = scale;
                // 设置填充类型和图案名称
                hatch.SetHatchPattern(HatchPatternType.PreDefined, "ANGLE");
                // 加入图形数据库
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                hatchId = btr.AppendEntity(hatch);
                trans.AddNewlyCreatedDBObject(hatch, true);

                // 设置填充角度
                hatch.PatternAngle = degree;
                // 设置关联
                hatch.Associative = true;
                // 设置边界图形和填充方式


                ObjectIdCollection obIds = new ObjectIdCollection();
                // 依次添加图形填充样式
                for (int i = 0; i < entid.Length; i++)
                {
                    obIds.Clear();
                    obIds.Add(entid[i]);
                    hatch.AppendLoop(loopTypes[i], obIds);
                }


                // 计算填充并显示
                hatch.EvaluateHatch(true);
                // 提交事务
                trans.Commit();
            }
            return hatchId;
        }


        /// <summary>
        /// 渐变填充
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="colorIndex1">颜色索引1</param>
        /// <param name="colorIndex2">颜色索引2</param>
        /// <param name="hatchGradientName">渐变图案</param>
        /// <param name="entId">边界图形的ObjectId</param>
        /// <returns>ObjectId</returns>
        public static ObjectId HatchGradient(this Database db, short colorIndex1, short colorIndex2, string hatchGradientName, ObjectId entId)
        {
            // 声明ObjectId, 用于返回
            ObjectId hatchId = ObjectId.Null;
            ObjectIdCollection objIds = new ObjectIdCollection();
            objIds.Add(entId);
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 声明填充对象
                Hatch hatch = new Hatch();
                // 设置填充类型为渐变类型填充
                hatch.HatchObjectType = HatchObjectType.GradientObject;
                // 设置渐变填充的类型和渐变填充的图案名称
                hatch.SetGradient(GradientPatternType.PreDefinedGradient, hatchGradientName);
                // 设置填充颜色
                Color color1 = Color.FromColorIndex(ColorMethod.ByColor, colorIndex1);
                Color color2 = Color.FromColorIndex(ColorMethod.ByColor, colorIndex2);
                GradientColor gColor1 = new GradientColor(color1, 0);
                GradientColor gColor2 = new GradientColor(color2, 1);
                hatch.SetGradientColors(new GradientColor[] { gColor1, gColor2 });

                // 将填充对象加入图形数据库
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                hatchId = btr.AppendEntity(hatch);
                trans.AddNewlyCreatedDBObject(hatch, true);
                // 添加关联
                hatch.Associative = true;
                hatch.AppendLoop(HatchLoopTypes.Outermost, objIds);
                // 计算并显示填充
                hatch.EvaluateHatch(true);
                // 提交事务处理
                trans.Commit();
            }
            return hatchId;
        }
    }
}