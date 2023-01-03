using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAD工具.Tool
{
    public static partial class TableTool
    {
        public static void AddTable(Database db,int Rows,int Cols)
        {
            Table table = new Table();
            table.SetSize(Rows,Cols);
            using(Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                btr.AppendEntity(table);
                trans.AddNewlyCreatedDBObject(table, true);
                trans.Commit();
            }
        }

        #region CAD中获取图元信息
        //在对话框输入(setq ent(entset))，回车
        //选择对象
        //在对话框输入(setq ent(car ent))，回车
        //在对话框输入(setq ent data(entget ent))，回车
        #endregion

        #region 将图形信息存入表格

        public struct BlockData//存储的数据
        {
            public string blockName;
            public string layerName;
            public string X;
            public string Y;
            public string Z;
            //public string ZS;
            //public string XS;
        }
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="EntityType">数据类型</param>
        /// <param name="LayerName">数据图层名</param>
        public static void DataToTable(this Database db, Editor ed, string EntityType, string LayerName)
        {
            //Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            TypedValue[] values = new TypedValue[]
            {
                new TypedValue((int)DxfCode.Start,EntityType),
                new TypedValue((int)DxfCode.LayerName,LayerName)
            };

            SelectionFilter filter = new SelectionFilter(values);
            PromptSelectionResult psr = ed.GetSelection(filter);
            if (psr.Status == PromptStatus.OK)
            {
                ObjectId[] ids = psr.Value.GetObjectIds();
                PromptPointResult ppr = ed.GetPoint("选择表格插入点");
                if (ppr.Status == PromptStatus.OK)
                {
                    Point3d point = ppr.Value;
                    BlockData[] datas = GetBlockRefDate(db,ids);
                    SetDataToTable(db, datas, point, "数据提取");
                }              
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="ids">拾取到的ObjectId</param>
        /// <returns></returns>
        public static BlockData[] GetBlockRefDate(Database db, ObjectId[] ids)
        {
            BlockData[] datas = new BlockData[ids.Length];
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                for (int i = 0; i < ids.Length; i++)//获取块名 图层 X Y Z ZS XS
                {
                    BlockReference br = (BlockReference)ids[i].GetObject(OpenMode.ForRead);
                    datas[i].blockName = br.Name;
                    datas[i].layerName = br.Layer;
                    datas[i].X = br.Position.X.ToString();
                    datas[i].Y = br.Position.Y.ToString();
                    datas[i].Z = br.Position.Z.ToString();
                    //foreach (ObjectId item in br.AttributeCollection)
                    //{
                    //    AttributeReference attRef = (AttributeReference)item.GetObject(OpenMode.ForRead);
                    //    if (attRef.Tag.ToString() == "ZS")
                    //    {
                    //        datas[i].ZS = attRef.TextString;
                    //    }
                    //    else if(attRef.Tag.ToString() == "XS")
                    //    {
                    //        datas[i].XS = attRef.TextString;
                    //    }
                    //}
                }
            }
            return datas;
        }

        public static void SetDataToTable(Database db, BlockData[] datas,Point3d point,string header)
        {
            Table table = new Table();
            table.SetSize(datas.Length + 1, 5);
            table.Position = point;
            table.Cells[0, 0].TextString = header;
            table.SetColumnWidth(80);
            for (int i = 0; i < datas.Length + 1; i++)
            {
                table.Cells[i, 0].TextString = datas[i - 1].blockName;
                table.Cells[i, 1].TextString = datas[i - 1].layerName;
                table.Cells[i, 2].TextString = datas[i - 1].X.ToString();
                table.Cells[i, 3].TextString = datas[i - 1].Y.ToString();
                table.Cells[i, 4].TextString = datas[i - 1].Z.ToString();
                //table.Cells[i, 5].TextString = datas[i - 1].ZS.ToString();
                //table.Cells[i, 6].TextString = datas[i - 1].XS.ToString();
            }
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                btr.AppendEntity(table);
                trans.AddNewlyCreatedDBObject(table, true);
                trans.Commit();
            }
        }
        #endregion

    }
}
