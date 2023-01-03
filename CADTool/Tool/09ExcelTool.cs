using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;

namespace CAD工具.Tool
{
    public class ExcelTool
    {
        private struct CircleData//存储的数据
        {
            public string blockName;
            public string layerName;
            public double X;
            public double Y;
            public double Z;
            public double R;
            //public string ZS;
            //public string XS;
        }

        #region 将cad信息导出至Excel文件
        private static string OpenSaveDialog(Database db, Editor ed)//获取文件保存路径及文件名
        {
            string directoryName = Path.GetDirectoryName(db.Filename);
            string fileName = Path.GetFileName(db.Filename);
            fileName = fileName.Substring(0, fileName.IndexOf("."));
            PromptSaveFileOptions opt = new PromptSaveFileOptions("保存Excel文件");
            opt.DialogCaption = "保存Excel文件";
            opt.Filter = "Excel 97-2003 工作簿(*.xls)|*.xls|Excel 工作簿(*.xlsx)|*.xlsx";
            opt.FilterIndex = 1;
            opt.InitialDirectory = directoryName;
            opt.InitialFileName = fileName;
            PromptFileNameResult fileRes = ed.GetFileNameForSave(opt);
            if (fileRes.Status == PromptStatus.OK)
            {
                fileName = fileRes.StringResult;
            }
            else
            {
                fileName = "";
            }
            return fileName;
        }
        private static ObjectId[] Filter(Database db, Editor ed, string BlockTypeName)
        {
            #region 过滤条件
            ObjectId[] ids = new ObjectId[] { };
            TypedValue[] values = new TypedValue[]
            {
                new TypedValue((int)DxfCode.Start,BlockTypeName),
            };
            SelectionFilter filter = new SelectionFilter(values);
            PromptSelectionResult psr = ed.GetSelection(filter);
            if (psr.Status == PromptStatus.OK)
            {
                ids = psr.Value.GetObjectIds();
            }
            else
            {
                return null;
            }
            return ids;
            #endregion
        }

        private static CircleData[] GetCircleData(Database db, ObjectId[] ids)//获取数据
        {
            CircleData[] datas = new CircleData[ids.Length];
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    Circle c= (Circle)ids[i].GetObject(OpenMode.ForRead);
                    datas[i].blockName = c.BlockName;
                    datas[i].layerName = c.Layer;
                    datas[i].X = c.Center.X;
                    datas[i].Y = c.Center.Y;
                    datas[i].Z = c.Center.Z;
                    datas[i].R = c.Radius;  
                }
            }
            return datas;
        }
        private static void SaveDataToExcel(string fileName, CircleData[] datas)
        {

            NetOffice.ExcelApi.Application execlApp = new NetOffice.ExcelApi.Application();//声明Excel程序
            NetOffice.ExcelApi.Workbook book = execlApp.Workbooks.Add();//Excel工作簿
            NetOffice.ExcelApi.Worksheet sheet = (NetOffice.ExcelApi.Worksheet)book.Worksheets[1];//获取第一张工作表
            sheet.Cells[1, 1].Value = "序号";
            sheet.Cells[1, 2].Value = "图块";
            sheet.Cells[1, 3].Value = "图层";
            sheet.Cells[1, 4].Value = "X坐标";
            sheet.Cells[1, 5].Value = "Y坐标";
            sheet.Cells[1, 6].Value = "Z坐标";
            sheet.Cells[1, 7].Value = "半径";
            for (int i = 0; i < datas.Length; i++)
            {
                sheet.Cells[i + 2, 1].Value = i + 1;
                sheet.Cells[i + 2, 2].Value = datas[i].blockName;
                sheet.Cells[i + 2, 3].Value = datas[i].layerName;
                sheet.Cells[i + 2, 4].Value = datas[i].X;
                sheet.Cells[i + 2, 5].Value = datas[i].Y;
                sheet.Cells[i + 2, 6].Value = datas[i].Z;
                sheet.Cells[i + 2, 7].Value = datas[i].R;
            }
            book.SaveAs(fileName);//保存工作簿
            execlApp.Quit();//退出Excel程序
            execlApp.Dispose();//销毁Excel程序
        }
        public static void CreateExcel(Database db, Editor ed)
        {
            //db = HostApplicationServices.WorkingDatabase;
            //ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            string fileName = OpenSaveDialog(db, ed);
            if (fileName == "") { return; }
            ObjectId[] ids = Filter(db, ed, "circle");
            CircleData[] datas = GetCircleData(db, ids);
            SaveDataToExcel(fileName, datas);
        }

        #endregion

        #region 将Excel文件信息导入CAD

        private static string OpenExcelDialog(Database db,Editor ed)//获取打开文件的全路径
        {
            string directoryName = Path.GetDirectoryName(db.Filename);
            string fileName = Path.GetFileName(db.Filename);
            fileName = fileName.Substring(0, fileName.IndexOf("."));
            PromptOpenFileOptions opt = new PromptOpenFileOptions("读取Excel文件");
            opt.DialogCaption = "读取Excel文件";
            opt.Filter = "Excel 97-2003 工作簿(*.xls)|*.xls|Excel 工作簿(*.xlsx)|*.xlsx";
            opt.FilterIndex = 1;
            opt.InitialDirectory = directoryName;
            opt.InitialFileName = fileName;
            PromptFileNameResult fileRes = ed.GetFileNameForOpen(opt);
            if (fileRes.Status == PromptStatus.OK)
            {
                fileName = fileRes.StringResult;
            }
            else
            {
                fileName = "";
            }
            return fileName;
        }

        private static List<CircleData> GetDataFromExcel(string fileName)
        {
            List<CircleData> datas = new List<CircleData>();
            NetOffice.ExcelApi.Application execlApp = new NetOffice.ExcelApi.Application();//声明Excel程序
            NetOffice.ExcelApi.Workbook book = execlApp.Workbooks.Open(fileName);//Excel工作簿
            NetOffice.ExcelApi.Worksheet sheet = (NetOffice.ExcelApi.Worksheet)book.Worksheets[1];//获取第一张工作表
            int i = 2, row = -1;
            while (sheet.Cells[i, 7].Value != null&& sheet.Cells[i, 7].Value.ToString().Trim() != "")
            {
                CircleData data = new CircleData();
                data.blockName = sheet.Cells[i, 2].Value.ToString();
                data.layerName = sheet.Cells[i, 3].Value.ToString(); 
                data.X = (double)sheet.Cells[i, 4].Value;
                data.Y = (double)sheet.Cells[i, 5].Value;
                data.Z = (double)sheet.Cells[i, 6].Value;
                data.R = (double)sheet.Cells[i, 7].Value;
                #region 判断
                //double X, Y, Z, R;
                //if (!Double.TryParse(sheet.Cells[i, 4].Value.ToString(), out X))
                //{
                //    row = i;
                //    break;
                //}
                //if (!Double.TryParse(sheet.Cells[i, 5].Value.ToString(), out Y))
                //{
                //    row = i;
                //    break;
                //}
                //if (!Double.TryParse(sheet.Cells[i, 6].Value.ToString(), out Z))
                //{
                //    row = i;
                //    break;
                //}
                //if (!Double.TryParse(sheet.Cells[i, 7].Value.ToString(), out R))
                //{
                //    row = i;
                //    break;
                //}
                #endregion

                datas.Add(data);
                i++;
            }           
            execlApp.Quit();//退出Excel程序
            execlApp.Dispose();//销毁Excel程序
            return datas;
        }

        private static void DrawCircle(Database db,List<CircleData> datas)
        {
            using(Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                for (int i = 0; i < datas.Count; i++)
                {    
                    Point3d center = new Point3d(datas[i].X, datas[i].Y, datas[i].Z);
                    double radius = datas[i].R;
                    Circle circle = new Circle(center,Vector3d.ZAxis,radius);
                    circle.Layer = datas[i].layerName;
                    btr.AppendEntity(circle);
                    trans.AddNewlyCreatedDBObject(circle, true);
                }
                trans.Commit();
            }
        }
        public static void ExcelDataToCAD(Database db, Editor ed)
        {

            string fileName = OpenExcelDialog(db, ed);
            if (fileName == "") { return; }
            List<CircleData> datas = GetDataFromExcel(fileName);
            if (datas.Count == 0 ){ return; }
            DrawCircle(db, datas);

        }

        #endregion
    }
}
