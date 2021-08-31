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
using Autodesk.AutoCAD.GraphicsInterface;

namespace GLTools
{
    public static class LayerStyleTools
    {
        /// <summary>
        /// 新建文字样式
        /// </summary>
        public static ObjectId AddTextStyle(this Database db, string TextStyleName, string FontFilename, string BigFontFilename, double WidthFactor)
        {
            ObjectId tsId = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 打开文字样式表
                TextStyleTable tst = (TextStyleTable)trans.GetObject(db.TextStyleTableId, OpenMode.ForRead);
                // 如果不存在名为styleName的文字样式，则新建一个文字样式
                if (!tst.Has(TextStyleName))
                {
                    //定义一个新的的文字样式表记录
                    TextStyleTableRecord tsr = new TextStyleTableRecord();
                    //设置的文字样式名
                    tsr.Name = TextStyleName;
                    //设置文字样式的字体
                    tsr.FileName = FontFilename;
                    tsr.BigFontFileName = BigFontFilename;
                    // 切换文字样式表的状态为写以添加新的文字样式
                    tsr.XScale = WidthFactor;
                    tst.UpgradeOpen();
                    // 更新数据信息
                    tst.Add(tsr);
                    trans.AddNewlyCreatedDBObject(tsr, true);
                    // 为了安全，将文字样式表的状态切换为读
                    tst.DowngradeOpen();

                    tsId = tst[TextStyleName];
                }
                // 提交事务
                trans.Commit();
            }
            return tsId;
        }

        /// <summary>
        /// 将指定文字样式设为当前
        /// </summary>
        public static bool SetTextStyleCurrent(this Database db, string TextStyleName,
            string FontFilename = "smsim.shx", string BigFontFilename = "smfs.shx", double WidthFactor = 0.75)
        {
            // 开启事务处理
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 打开文字样式表
                TextStyleTable tst = (TextStyleTable)trans.GetObject(db.TextStyleTableId, OpenMode.ForRead);
                //如果不存在指定文字样式，则创建
                if (!tst.Has(TextStyleName))
                {
                    try { db.AddTextStyle(TextStyleName, FontFilename, BigFontFilename, WidthFactor); }
                    catch { return false; }
                }

                //获取名为TextStyleName的的文字样式表记录的Id
                ObjectId tsId = tst[TextStyleName];
                //指定当前文字样式
                db.Textstyle = tsId;

                // 提交事务
                trans.Commit();
            }
            return true;
        }

        /// <summary>
        /// 获取文字样式Id
        /// </summary>
        public static ObjectId GetTextStyleId(this Database db, string TextStyleName)
        {
            ObjectId TextStyleId = ObjectId.Null;
            // 开启事务处理
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 打开文字样式表
                TextStyleTable tst = (TextStyleTable)trans.GetObject(db.TextStyleTableId, OpenMode.ForRead);
                //如果存在名为TextStyleName的文字样式，则获取对应的文字样式表记录的Id
                if (tst.Has(TextStyleName)) TextStyleId = tst[TextStyleName];
                trans.Commit();
            }
            return TextStyleId;
        }

        /// <summary>
        /// 新建图层
        /// </summary>
        public static ObjectId AddLayer(this Database db, string LayerName, short ColorIndex)
        {
            ObjectId layerId = ObjectId.Null;
            // 开启事务处理
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //打开层表
                LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                //新建层表记录
                if (!lt.Has(LayerName))
                {
                    LayerTableRecord ltr = new LayerTableRecord();
                    ltr.Name = LayerName;
                    ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, ColorIndex);
                    //升级层表打开权限
                    lt.UpgradeOpen();
                    lt.Add(ltr);
                    trans.AddNewlyCreatedDBObject(ltr, true);
                    //降低层表打开权限
                    lt.DowngradeOpen();

                    layerId = lt[LayerName];
                }
                // 提交事务
                trans.Commit();
            }
            return layerId;
        }

        /// <summary>
        /// 将指定图层设为当前
        /// </summary>
        public static bool SetLayerCurrent(this Database db, string LayerName, short ColorIndex)
        {
            // 开启事务处理
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //打开层表
                LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                //如果不存在指定层，则创建
                if (!lt.Has(LayerName))
                {
                    try { db.AddLayer(LayerName, ColorIndex); }
                    catch { return false; }
                }

                //获取名为LayerName的图层的Id
                ObjectId layerId = lt[LayerName];
                //指定当前图层
                db.Clayer = layerId;

                // 提交事务
                trans.Commit();
            }
            return true;
        }

        /// <summary>
        /// 加载指定线型
        /// </summary>
        public static void LoadLineType(this Database db, string LineTypeName)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 以读模式打开线型表
                LinetypeTable ltt = (LinetypeTable)trans.GetObject(db.LinetypeTableId, OpenMode.ForRead);
                if (ltt.Has(LineTypeName) == false)
                {
                    try { db.LoadLineTypeFile(LineTypeName, "acad.lin"); }
                    catch { }
                }
                trans.Commit();
            }
        }
    }
}