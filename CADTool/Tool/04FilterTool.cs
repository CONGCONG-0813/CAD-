using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAD工具.Tool
{
    public class FilterTool
    {
        public static void FilterAll(Database db, Editor ed)
        {           
            PromptSelectionResult psr = ed.SelectAll();//全选图面内的内容

            TypedValue[] values = new TypedValue[]
            {
                //dxfcode.start是一种枚举类型，用于过滤器的使用
                //Start表示名称
                //LineTypeName表示线型
                new TypedValue((int)DxfCode.Start ,"circle" )

            };
            SelectionFilter filter = new SelectionFilter(values);
            //PromptSelectionResult psr = ed.GetSelection(filter);

            if (psr.Status == PromptStatus.OK)
            {
                //SelectionSet selectionSet = psr.Value;
                //this.ChangeColor();
            }
        }

        public void FilterCircle()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            TypedValue[] values = new TypedValue[]
            {

                new TypedValue((int)DxfCode.Start,"circle")

            };
            SelectionFilter filter = new SelectionFilter(values);
            PromptSelectionResult psr01 = ed.GetSelection(filter);
            List<ObjectId> objectIds = new List<ObjectId>();

            if (psr01.Status == PromptStatus.OK)
            {
                SelectionSet selectionSet = psr01.Value;
                List<Point3d> points = this.GetPoint(selectionSet);
                for (int i = 0; i < points.Count; i++)
                {
                    PromptSelectionResult psr03 = ed.SelectCrossingWindow(points.ElementAt(i), points.ElementAt(i));
                    objectIds.AddRange(psr03.Value.GetObjectIds());
                }
            }
        }

        public void Filter03()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptSelectionResult psr = ed.SelectImplied();

            ed.WriteMessage("选取完成");
        }

        public List<Point3d> GetPoint(SelectionSet selectionSet)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            List<Point3d> points = new List<Point3d>();
            ObjectId[] ids = selectionSet.GetObjectIds();

            using (Transaction transaction = db.TransactionManager.StartTransaction())
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    Entity entity = ids[i].GetObject(OpenMode.ForRead) as Entity;
                    Point3d center = (entity as Circle).Center;
                    double radius = (entity as Circle).Radius;
                    points.Add(new Point3d(center.X + radius, center.Y, center.Z));


                }
                transaction.Commit();
            }
            return points;
        }
        
    }
}
