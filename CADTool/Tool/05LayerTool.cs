using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAD工具.Tool
{
    public enum AddLayerStatus
    {
        AddLayerOK,
        IllegalLayerName,
        LayerNameExist
    }
    public struct AddLayerResult
    {
        public AddLayerStatus status;
        public string LayerName;
    }
    public static partial class LayerTool
    {
        #region 添加图层
        public static AddLayerResult AddLayer(this Database db, string LayerName)
        {
            AddLayerResult result = new AddLayerResult();
            try
            {
                SymbolUtilityServices.ValidateSymbolName(LayerName, false);//LayerName是否使用特殊字符或空值
            }
            catch
            {
                result.status = AddLayerStatus.IllegalLayerName;
                return result;
            }

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {

                LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead); //图层表               
                if (!lt.Has(LayerName))//判断新建图层是否存在
                {
                    LayerTableRecord ltr = new LayerTableRecord();//新建图层表记录
                    ltr.Name = LayerName;
                    //升级层表打开权限
                    lt.UpgradeOpen();
                    lt.Add(ltr);
                    //降低层表打开权限
                    lt.DowngradeOpen();
                    trans.AddNewlyCreatedDBObject(ltr, true);
                    trans.Commit();
                    result.status = AddLayerStatus.AddLayerOK;
                }
                else
                {
                    result.status = AddLayerStatus.LayerNameExist;
                }
                return result;
            }

        }
        #endregion

        #region 返回所有层表
        /// <summary>
        /// 获取所有所有层表
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <returns></returns>
        public static List<LayerTableRecord> GetAllLayers(this Database db)
        {
            List<LayerTableRecord> layerList = new List<LayerTableRecord>();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
               
                LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead); //打开层表
                lt.GenerateUsageData();//图层上是否有图形对象
                foreach (ObjectId item in lt)
                {
                    LayerTableRecord ltr = (LayerTableRecord)item.GetObject(OpenMode.ForRead);
                    layerList.Add(ltr);
                }
            }
            return layerList;
        }
        #endregion

        #region 返回所有图层名
        /// <summary>
        /// 获取所有图层名
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <returns></returns>
        public static List<string> GetAllLayerNames(this Database db)
        {
            List<string> layerNameList = new List<string>();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //打开层表
                LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                foreach (ObjectId item in lt)
                {
                    LayerTableRecord ltr = (LayerTableRecord)item.GetObject(OpenMode.ForWrite);
                    layerNameList.Add(ltr.Name);
                }
            }
            return layerNameList;
        }
        #endregion

        #region 删除图层
        public static bool DeleteLayer(this Database db, string LayerName)
        {
            if (LayerName == "0" || LayerName == "Defpoints")//图层名不能为0图层或Defpoints图层
            {
                return false;
            }
            bool IsDeleteOK = false;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                lt.GenerateUsageData();//图层内是否有图形数据
                if (lt.Has(LayerName))
                {
                    LayerTableRecord ltr = (LayerTableRecord)lt[LayerName].GetObject(OpenMode.ForWrite);
                    if (!ltr.IsUsed && db.Clayer != lt[LayerName])
                    {
                        ltr.Erase();
                        IsDeleteOK = true;
                    }
                }
                else
                {
                    IsDeleteOK = true;
                }
                trans.Commit();
            }
            return IsDeleteOK;
        }
        #endregion

        #region 强制删除图层
        /// <summary>
        /// 强制删除图层
        /// </summary>
        /// <param name="db"></param>
        /// <param name="LayerName"></param>
        /// <param name="delete"></param>
        /// <returns></returns>
        public static bool DeleteLayer(this Database db, string LayerName,bool delete)
        {
            if (LayerName == "0" || LayerName == "Defpoints")//图层名不能为0图层或Defpoints图层
            {
                return false;
            }
            bool IsDeleteOK = false;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                lt.GenerateUsageData();//图层内是否有图形数据
                if (lt.Has(LayerName))
                {
                    LayerTableRecord ltr = (LayerTableRecord)lt[LayerName].GetObject(OpenMode.ForWrite);
                    if (delete)
                    {
                        if (ltr.IsUsed)
                        {
                            ltr.deleteAllEntityInLayer();
                            
                        }
                        if (db.Clayer == ltr.ObjectId)
                        {
                            db.Clayer = lt["0"];
                        }
                        ltr.Erase();
                        IsDeleteOK = true;
                    }
                    else
                    {
                        if (!ltr.IsUsed && db.Clayer != lt[LayerName])
                        {
                            ltr.Erase();
                            IsDeleteOK = true;
                        }
                    }
                    
                }
                else
                {
                    IsDeleteOK = true;
                }
                trans.Commit();
            }
            return IsDeleteOK;
        }

        public static void deleteAllEntityInLayer(this LayerTableRecord ltr)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            TypedValue[] value = new TypedValue[]
            {
                new TypedValue((int)DxfCode.LayerName,ltr.Name)
            };
            SelectionFilter filter = new SelectionFilter(value);
            PromptSelectionResult psr = ed.SelectAll();
            if (psr.Status == PromptStatus.OK)
            {
                ObjectId[] ids = psr.Value.GetObjectIds();
                using(Transaction trans = db.TransactionManager.StartTransaction())
                {
                    for (int i = 0; i < ids.Length; i++)
                    {
                        Entity ent = (Entity)ids[i].GetObject(OpenMode.ForWrite);
                        ent.Erase();
                    }
                    trans.Commit();
                }
            }
        }

        #endregion

        #region 删除所有未使用的图层
        public static void DeleteNotUsedLayer(this Database db)
        {
            using(Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                lt.GenerateUsageData();
                foreach (ObjectId item in lt)
                {
                    LayerTableRecord ltr = (LayerTableRecord)item.GetObject(OpenMode.ForWrite);
                    if (!ltr.IsUsed)
                    {
                        ltr.Erase();
                    }
                }
                trans.Commit();
            }
        }
        #endregion
    }
}
