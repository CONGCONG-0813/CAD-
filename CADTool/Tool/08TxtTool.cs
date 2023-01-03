using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Windows;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using System.IO;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace CAD工具.Tool
{
    public static partial class TxtTool
    {
        
        #region 将cad信息导出至txt文件
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
        public static BlockData[] GetBlockRefDate(Database db, ObjectId[] ids)//获取数据
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
        public static void CADDataToTxt(Database db, Editor ed, string BlockName, string LayerName)
        {
            #region 过滤条件
            TypedValue[] values = new TypedValue[]
            {
                new TypedValue((int)DxfCode.Start,BlockName),
                new TypedValue((int)DxfCode.LayerName,LayerName)
            };

            SelectionFilter filter = new SelectionFilter(values);
            PromptSelectionResult psr = ed.GetSelection(filter);
            if (psr.Status == PromptStatus.OK)
            {
                ObjectId[] ids = psr.Value.GetObjectIds();
                BlockData[] datas = GetBlockRefDate(db, ids);

                #region 保存文件
                System.Windows.Forms.SaveFileDialog saveDlg = new System.Windows.Forms.SaveFileDialog();
                saveDlg.Title = "保存图形数据";
                saveDlg.Filter = "文本文件(*.txt)|*.txt";
                saveDlg.InitialDirectory = Path.GetDirectoryName(db.Filename);
                string fileName = Path.GetFileName(db.Filename);
                saveDlg.FileName = fileName.Substring(0, fileName.IndexOf('.'));
                System.Windows.Forms.DialogResult saveDlgRes = saveDlg.ShowDialog();
                if (saveDlgRes == System.Windows.Forms.DialogResult.OK)
                {
                    string[] contents = new string[datas.Length];
                    for (int i = 0; i < contents.Length; i++)
                    {
                        contents[i] = datas[i].blockName + "," + datas[i].layerName + "," + datas[i].X + "," + datas[i].Y + "," + datas[i].Z;
                        //contents[i] = datas[i].blockName + "," + datas[i].layerName + "," + datas[i].X + "," + datas[i].Y + "," + datas[i].Z+ "," + datas[i].ZS+ "," + datas[i].XZ;
                    }
                    File.WriteAllLines(saveDlg.FileName, contents);
                }
                #endregion
            }
            #endregion
        }
        #endregion

        #region 清空信息
        public static void DeleteTxtInfo(Database db, Editor ed, string BlockName, string LayerName)
        {
            #region 过滤条件
            TypedValue[] values = new TypedValue[]
            {
                new TypedValue((int)DxfCode.Start,BlockName),
                new TypedValue((int)DxfCode.LayerName,LayerName)
            };

            SelectionFilter filter = new SelectionFilter(values);
            PromptSelectionResult psr = ed.GetSelection(filter);
            if (psr.Status == PromptStatus.OK)
            {
                ObjectId[] ids = psr.Value.GetObjectIds();
                using(Transaction trans = db.TransactionManager.StartTransaction())
                {
                    for (int i = 0; i < ids.Length; i++)
                    {
                        DBObject obj = ids[i].GetObject(OpenMode.ForWrite);
                        obj.Erase();
                    }
                    trans.Commit();
                }
               
            }
            #endregion
        }
        #endregion

        #region 将txt文件信息导入CAD

        public struct TxtData
        {
            public string blockName;
            public string layerName;
            public Point3d position;
            //Dictionary<string, string> attrs;
        }
        public static int TransData(string[] contents,out List<TxtData> datas)
        {
            datas = new List<TxtData>();
            int row = -1;
            TxtData data = new TxtData();
            for (int i = 0; i < contents.Length; i++)
            {
                string[] con = contents[i].Split(new char[] { ',' });
                data.blockName = con[0];
                data.layerName = con[1];
                double X,Y,Z;
                if (!Double.TryParse(con[2],out X))
                {
                    row = i;
                    break;
                }
                if (!Double.TryParse(con[3], out Y))
                {
                    row = i;
                    break;
                }
                if (!Double.TryParse(con[4], out Z))
                {
                    row = i;
                    break;
                }
                data.position = new Point3d(X,Y,Z);
                //data.attrs = new Dictionary<string, string>();
                //data.attrs.Add("ZS", con[5].ToString());
                //data.attrs.Add("XS", con[6].ToString());
                datas.Add(data);
            }
            return row;
        }
        
        /// <summary>
        /// 将数据转换为图形加入模型空间
        /// </summary>
        /// <param name="db"></param>
        /// <param name="data"></param>
        public static void InsertAttrBlockReference(Database db,List<TxtData> data)
        {
            using(Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                for (int i = 0; i < data.Count; i++)
                {     
                    if (bt.Has(data[i].blockName))
                    {
                        ObjectId blockId = bt[data[i].blockName];//获取块的ObjectId
                        //声明块参照
                        BlockReference br = new BlockReference(data[i].position, blockId);
                        //br.Layer = layerName;//声明图层
                        btr.AppendEntity(br);
                        //添加属性定义
                        #region 添加属性定义
                        //BlockTableRecord blockRecord = (BlockTableRecord)blockId.GetObject(OpenMode.ForRead);//添加属性定义
                        //if (blockRecord.HasAttributeDefinitions)
                        //{
                        //    foreach (ObjectId item in blockRecord)
                        //    {
                        //        DBObject obj = item.GetObject(OpenMode.ForRead);
                        //        if (obj is AttributeDefinition)
                        //        {
                        //            AttributeReference attrRef = new AttributeReference();
                        //            attrRef.SetAttributeFromBlock((AttributeDefinition)obj, br.BlockTransform);
                        //            if (data[i].attrs.ContainsKey(attrRef.Tag.ToString()))
                        //            {
                        //                attrRef.TextString = data[i].attrs[attrRef.Tag.ToString()];
                        //            }
                        //            br.AttributeCollection.AppendAttribute(attrRef);
                        //            trans.AddNewlyCreatedDBObject(attrRef, true);
                        //        }
                        //    }
                        //}
                        //trans.AddNewlyCreatedDBObject(br, true);
                        #endregion
                    }
                }
            }
        }
        public static void TxtDataToCAD(Database db,Editor ed)
        {
            System.Windows.Forms.OpenFileDialog openDlg = new System.Windows.Forms.OpenFileDialog();
            openDlg.Title = "打开数据文件";
            openDlg.Filter = "文本文件(*.txt)|*.txt";
            System.Windows.Forms.DialogResult openRes = openDlg.ShowDialog();
            if (openRes == System.Windows.Forms.DialogResult.OK)
            {
                string[] contents = File.ReadAllLines(openDlg.FileName);
                List<TxtData> datas;
                int row = TransData(contents, out datas);
                if (row < 0)
                {
                    InsertAttrBlockReference(db, datas);
                }
                else
                {
                    ed.WriteMessage("外部数据文件在第{0}出错",row);
                }
            }
        }
        #endregion
    }

}
