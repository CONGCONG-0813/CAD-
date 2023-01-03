using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAD工具.Tool
{
    public static partial class HatchTool
    {
        #region //填充图案（单个填充）
        /// <summary>
        /// 填充图案
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="patternName">图案名称</param>
        /// <param name="entityId">边界图形的ObjectId</param>
        /// <param name="ratio">填充比例</param>
        /// <param name="angle">填充角度</param>
        /// <returns>填充图案的ObjectId</returns>
        public static ObjectId HatchEntity(this Database db, string patternName, ObjectId entityId, double ratio, double angle)
        {
            ObjectId hacthId = ObjectId.Null;
            using (Transaction transaction = db.TransactionManager.StartTransaction())
            {
                //声明图案填充对象
                Hatch hatch = new Hatch();
                //设置填充比例
                hatch.PatternScale = ratio;
                //设置填充类型和图案名
                hatch.SetHatchPattern(HatchPatternType.PreDefined, patternName);
                //加入图形数据库
                BlockTable bt = transaction.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = transaction.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                btr.AppendEntity(hatch);
                transaction.AddNewlyCreatedDBObject(hatch, true);

                //设置填充角度
                hatch.PatternAngle = angle.AngleToRadian();
                //设置关联图形
                hatch.Associative = true;
                //设置边界图形和填充方式
                ObjectIdCollection objectIdCollections = new ObjectIdCollection();
                objectIdCollections.Add(entityId);
                hatch.AppendLoop(HatchLoopTypes.Outermost, objectIdCollections);
                //计算填充并显示
                hatch.EvaluateHatch(true);
                //提交事务
                transaction.Commit();
            }
            return hacthId;
        }
        #endregion

        #region//填充图案名(系统定义)
        public struct HatchPatternName
        {
            public static readonly string solid = "SOLID";
            public static readonly string angle = "ANGLED";
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
            public static readonly string arconc = "AR-CONC";

        }

        #endregion

        #region //填充图案（添加背景色）
        /// <summary>
        /// 填充图案
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="patternName">图案名称</param>
        /// <param name="entityId">边界图形的ObjectId</param>
        /// <param name="ratio">填充比例</param>
        /// <param name="angle">填充角度</param>
        /// <param name="bkColor">背景色</param>
        /// <param name="hatchColorIndex">填充图案颜色</param>
        /// <returns>ObjectId</returns>
        public static ObjectId HatchEntity(this Database db, string patternName, ObjectId entityId, double ratio, double angle, Color bkColor, int hatchColorIndex)
        {
            ObjectId hacthId = ObjectId.Null;
            using (Transaction transaction = db.TransactionManager.StartTransaction())
            {
                //声明图案填充对象
                Hatch hatch = new Hatch();
                //设置填充比例
                hatch.PatternScale = ratio;
                //设置背景色
                hatch.BackgroundColor = bkColor;
                //设置填充图案的颜色
                hatch.ColorIndex = hatchColorIndex;
                //设置填充类型和图案名
                hatch.SetHatchPattern(HatchPatternType.PreDefined, patternName);
                //加入图形数据库
                BlockTable bt = transaction.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = transaction.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                btr.AppendEntity(hatch);
                transaction.AddNewlyCreatedDBObject(hatch, true);

                //设置填充角度
                hatch.PatternAngle = angle.AngleToRadian();
                //设置关联图形
                hatch.Associative = true;
                //设置边界图形和填充方式
                ObjectIdCollection objectIdCollections = new ObjectIdCollection();
                objectIdCollections.Add(entityId);
                hatch.AppendLoop(HatchLoopTypes.Outermost, objectIdCollections);
                //计算填充并显示
                hatch.EvaluateHatch(true);
                //提交事务
                transaction.Commit();
            }
            return hacthId;
        }


        #endregion

        #region //填充图案(bool计算)
        public static ObjectId HatchEntity(this Database db, List<HatchLoopTypes> hatchLoopTypes, string patternName, double ratio, double angle, params ObjectId[] entityIds)
        {
            ObjectId hacthId = ObjectId.Null;
            using (Transaction transaction = db.TransactionManager.StartTransaction())
            {
                //声明图案填充对象
                Hatch hatch = new Hatch();
                //设置填充比例
                hatch.PatternScale = ratio;
                //设置填充类型和图案名
                hatch.SetHatchPattern(HatchPatternType.PreDefined, patternName);
                //加入图形数据库
                BlockTable bt = transaction.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = transaction.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                btr.AppendEntity(hatch);
                transaction.AddNewlyCreatedDBObject(hatch, true);

                //设置填充角度
                hatch.PatternAngle = angle.AngleToRadian();
                //设置关联图形
                hatch.Associative = true;
                //设置边界图形和填充方式
                ObjectIdCollection obIds = new ObjectIdCollection();
                for (int i = 0; i < entityIds.Length; i++)
                {
                    obIds.Clear();
                    obIds.Add(entityIds[i]);
                    hatch.AppendLoop(hatchLoopTypes[i], obIds);
                }

                //计算填充并显示
                hatch.EvaluateHatch(true);
                //提交事务
                transaction.Commit();
            }
            return hacthId;
        }


        #endregion
    }
}
