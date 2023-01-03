using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAD工具.Tool
{
    public static partial class AddEntityTool
    {
        #region //将图形添加到图形文件中
        /// <summary>
        /// 将图形添加到图形文件中
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="entity">图形对象</param>
        /// <returns>图形的ObjectId</returns>
        /// 
        public static ObjectId AddEntityToModelSpace(this Database db, Entity entity)
        {
            //声明ObjectId，用于返回
            ObjectId entityId = ObjectId.Null;
            //开启事务处理
            using (Transaction transaction = db.TransactionManager.StartTransaction())
            {
                //打开块表
                BlockTable bt = transaction.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                //打开块表记录
                BlockTableRecord btr = transaction.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                //添加图形到块表记录
                entityId = btr.AppendEntity(entity);
                //更新数据信息
                transaction.AddNewlyCreatedDBObject(entity, true);
                //提交事务
                transaction.Commit();
            }
            return entityId;

        }
        #endregion

        #region //将图形的归降添加到图形文件中(添加多个对象)
        /// <summary>
        /// 将图形的归降添加到图形文件中(添加多个对象)
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="entity[]">图形对象,可变参数</param>
        /// <returns>图形的ObjectId[]，返回数组</returns>
        /// 
        public static ObjectId[] AddEntityToModelSpace(this Database db, params Entity[] entity)
        {
            //声明ObjectId，用于返回
            ObjectId[] entityId = new ObjectId[entity.Length];
            //开启事务处理
            using (Transaction transaction = db.TransactionManager.StartTransaction())
            {
                //打开块表
                BlockTable bt = transaction.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                //打开块表记录
                BlockTableRecord btr = transaction.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                for (int i = 0; i < entity.Length; i++)
                {
                    //添加图形到块表记录
                    entityId[i] = btr.AppendEntity(entity[i]);
                    //更新数据信息
                    transaction.AddNewlyCreatedDBObject(entity[i], true);

                }
                //提交事务
                transaction.Commit();
            }
            return entityId;
        }
        #endregion

        #region //将图形的归降添加到图形文件中(直接输入点添加直线)
        /// <summary>
        /// 创建直线
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="startpoint">起点坐标</param>
        /// <param name="endpoint">终点坐标</param>
        /// <returns>ObjectId</returns>

        public static ObjectId AddLineToModelSpace(this Database db, Point3d startpoint, Point3d endpoint)
        {
            return db.AddEntityToModelSpace(new Line(startpoint, endpoint));

        }
        #endregion

        #region //绘制直线     
        /// <summary>
        /// 绘制直线
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="startpoint">起点坐标</param>
        /// <param name="length">直线长度</param>
        /// <param name="angle">与X轴正方形的角度</param>
        /// <returns></returns>
        public static ObjectId AddLineToModelSpace(this Database db, Point3d startPoint, Double length, double angle)
        {
            Point3d endpoint = new Point3d();
            //计算终点坐标
            double X = startPoint.X + length * Math.Cos(angle.AngleToRadian());
            double Y = startPoint.Y + length * Math.Sin(angle.AngleToRadian());
            Point3d endPoint = new Point3d(X, Y, 0);
            return db.AddEntityToModelSpace(new Line(startPoint, endPoint));


        }
        #endregion

        #region// 绘制圆弧(中心，半径，角度起点，角度终点)
        /// <summary>
        /// 绘制圆弧(中心，半径，角度起点，角度终点)
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="center">圆弧所在圆的圆心点</param>
        /// <param name="radius">圆弧的半径</param>
        /// <param name="startDegree">起始角度</param>
        /// <param name="endDegree">终止角度</param>
        /// <returns></returns>
        public static ObjectId AddArcToModelSpace(this Database db, Point3d center, double radius, double startDegree, double endDegree)
        {
            return db.AddEntityToModelSpace(new Arc(center, radius, startDegree.RadianToAngle(), endDegree.RadianToAngle()));
        }
        #endregion

        #region // 三点绘制圆弧(起点，圆弧点，终点)
        /// <summary>
        /// 绘制圆弧(起点，圆弧点，终点)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="startPoint">起点</param>
        /// <param name="pointOnArcPoint">圆弧点</param>
        /// <param name="endPoint">终点</param>
        /// <returns></returns>
        public static ObjectId AddArcToModelSpace(this Database db, Point3d startPoint, Point3d pointOnArcPoint, Point3d endPoint)
        {
            //先判断三点是否在同一条直线上
            if (startPoint.IsOnOneLine(pointOnArcPoint, endPoint))
            {
                return ObjectId.Null;
            }
            //创建几何类对象
            CircularArc3d cArc3D = new CircularArc3d(startPoint, pointOnArcPoint, endPoint);
            //通过几何类对象获取其属性
            double radius = cArc3D.Radius;//半径
            #region //原始
            //Point3d center = cArc3D.Center;//圆心
            ////获取圆心到起点向量
            //Vector3d cs = center.GetVectorTo(startPoint);
            ////获取圆心到终点向量
            //Vector3d ce = center.GetVectorTo(endPoint);
            ////X正方向向量
            //Vector3d xvector = new Vector3d(1, 0, 0);
            ////圆弧的起始角度，判断向量的方向，并取正方向
            //double startAngle = cs.Y > 0 ? xvector.GetAngleTo(cs) : -xvector.GetAngleTo(cs);
            ////圆弧的终止角度，判断向量的方向，并取正方向
            //double endAngle = ce.Y > 0 ? xvector.GetAngleTo(ce) : -xvector.GetAngleTo(ce);
            ////创建圆弧对象
            //Arc arc = new Arc(center, radius, startAngle, endAngle);
            #endregion
            Arc arc = new Arc(cArc3D.Center, cArc3D.Radius, cArc3D.Center.GetVectorToXAxis(startPoint), cArc3D.Center.GetVectorToXAxis(endPoint));

            //加入图形数据库
            return db.AddEntityToModelSpace(arc);


        }
        #endregion

        #region// 通过圆心、起点、夹角绘制圆弧
        /// <summary>
        /// 通过圆心、起点、夹角绘制圆弧
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="startPoint">圆心</param>
        /// <param name="pointOnArcPoint">起点</param>
        /// <param name="degree">夹角,角度值（0-360）</param>
        /// <returns>ObjectId</returns>
        public static ObjectId AddArcToModelSpace(this Database db, Point3d center, Point3d startPoint, double degree)
        {
            //获取半径
            double radius = center.GetDistance(startPoint);
            //获取起点角度，弧度值
            double startAngle = center.GetVectorToXAxis(startPoint);
            //声明掩护对象
            Arc arc = new Arc(center, radius, startAngle, startAngle + degree.AngleToRadian());

            return db.AddEntityToModelSpace(arc);


        }
        #endregion

        #region // 绘制圆(圆心，半径)
        /// <summary>
        /// 绘制圆
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="center">圆心</param>
        /// <param name="radius">半径</param>
        /// <returns></returns>
        public static ObjectId AddCircleModelSpace(this Database db, Point3d center, double radius)
        {
            return db.AddEntityToModelSpace(new Circle(center, new Vector3d(0, 0, 1), radius));
        }
        #endregion

        #region // 绘制圆（两点）
        /// <summary>
        /// 绘制圆（两点）
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="point01">第一个点</param>
        /// <param name="point02">第二个点</param>
        /// <returns>圆</returns>
        public static ObjectId AddCircleModelSpace(this Database db, Point3d point01, Point3d point02)
        {
            Point3d center = point01.GetCenterPoint(point02);
            double radius = center.GetDistance(point01);
            return db.AddCircleModelSpace(center, radius);

        }
        #endregion

        #region //绘制圆（三点）
        /// <summary>
        /// 绘制圆（三点）
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="point01">第一个点</param>
        /// <param name="point02">第二个点</param>
        /// <param name="point03">第三个点</param>
        /// <returns></returns>
        public static ObjectId AddCircleModelSpace(this Database db, Point3d point01, Point3d point02, Point3d point03)
        {
            //先判断三点是否在同一条直线上
            if (point01.IsOnOneLine(point02, point03))
            {
                return ObjectId.Null;
            }
            //声明几何类的CircularArc3d对象
            CircularArc3d circularArc3D = new CircularArc3d(point01, point02, point03);
            return db.AddCircleModelSpace(circularArc3D.Center, circularArc3D.Radius);

        }
        #endregion

        #region //绘制多段线
        /// <summary>
        /// 绘制折线多段线（不含圆弧）
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="isClosed">是否闭合</param>
        /// <param name="contantWidth">线宽</param>
        /// <param name="vertices">多段线的顶点，可变参数</param>
        /// <returns> ObjectId</returns>
        public static ObjectId AddPolyLineToModelSpace(this Database db, bool isClosed, double contantWidth, params Point2d[] vertices)
        {
            //判断数组数据个数
            if (vertices.Length < 2)
            {
                return ObjectId.Null;
            }
            //声明一个多段线对象
            Polyline polyline = new Polyline();
            //添加多线段的顶点
            for (int i = 0; i < vertices.Length; i++)
            {
                polyline.AddVertexAt(i, vertices[i], 0, 0, 0);
            }
            //判断是否闭合
            if (isClosed)
            {
                polyline.Closed = true;
            }
            //设置多线段的线宽
            polyline.ConstantWidth = contantWidth;
            return db.AddEntityToModelSpace(polyline);
        }
        #endregion

        #region //绘制矩形
        /// <summary>
        /// 绘制矩形
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="point01">第一个点</param>
        /// <param name="point02">对角点</param>
        /// <returns></returns>
        public static ObjectId AddRectToModelSpace(this Database db, Point2d point01, Point2d point02)
        {
            //判断两个点的X,Y均不相等

            //声明多段线
            Polyline polyline = new Polyline();
            //计算矩形的四个顶点坐标
            Point2d p1 = new Point2d(Math.Min(point01.X, point02.X), Math.Min(point01.Y, point02.Y));
            Point2d p2 = new Point2d(Math.Min(point01.X, point02.X), Math.Max(point01.Y, point02.Y));
            Point2d p3 = new Point2d(Math.Max(point01.X, point02.X), Math.Max(point01.Y, point02.Y));
            Point2d p4 = new Point2d(Math.Max(point01.X, point02.X), Math.Min(point01.Y, point02.Y));
            //添加多段线的顶点
            polyline.AddVertexAt(0, p1, 0, 0, 0);
            polyline.AddVertexAt(1, p2, 0, 0, 0);
            polyline.AddVertexAt(2, p3, 0, 0, 0);
            polyline.AddVertexAt(3, p4, 0, 0, 0);
            //闭合
            polyline.Closed = true;
            return db.AddEntityToModelSpace(polyline);
        }
        #endregion

        #region //绘制正多边形
        /// <summary>
        /// 绘制正多边形
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="center">多边形所在元的内接圆心</param>
        /// <param name="radius">所在圆的半径</param>
        /// <param name="sideNum">边数</param>
        /// <param name="startDegree">起始角度</param>
        /// <returns>ObjectId</returns>
        public static ObjectId AddPolygonToModelSpace(this Database db, Point2d center, double radius, int sideNum, double startDegree)
        {
            //声明多段线
            Polyline polyline = new Polyline();
            //判断边数sideNum是否满足要求
            if (sideNum < 3)
            {
                return ObjectId.Null;
            }
            Point2d[] points = new Point2d[sideNum];
            double angle = startDegree.AngleToRadian();
            //计算每个顶点的坐标
            for (int i = 0; i < sideNum; i++)
            {
                points[i] = new Point2d(center.X + radius * Math.Cos(angle), center.Y + radius * Math.Sin(angle));
                polyline.AddVertexAt(i, points[i], 0, 0, 0);
                angle += Math.PI * 2 / sideNum;
            }
            //闭合多段线
            polyline.Closed = true;

            return db.AddEntityToModelSpace(polyline);
        }
        #endregion

        #region //绘制椭圆
        /// <summary>
        /// 绘制椭圆
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="center">椭圆中心</param>
        /// <param name="majorRadius">长轴长度</param>
        /// <param name="shortRadius">短轴长度</param>
        /// <param name="angle">长轴与X轴夹角，角度值</param>
        /// <param name="startAngle">起始角度</param>
        /// <param name="endAngle">终止角度</param>
        /// <returns>ObjectId</returns>

        public static ObjectId AddEllipseToModelSpace(this Database db, Point3d center, double majorRadius, double shortRadius, double angle, double startAngle, double endAngle)
        {
            //计算相关参数
            double ratio = shortRadius / majorRadius;
            Vector3d majorAxis = new Vector3d(majorRadius * Math.Cos(angle.AngleToRadian()), majorRadius * Math.Sin(angle.AngleToRadian()), 0);

            //声明椭圆对象
            Ellipse ellipse = new Ellipse(center, Vector3d.ZAxis, majorAxis, ratio, startAngle.AngleToRadian(), endAngle.AngleToRadian());
            return db.AddEntityToModelSpace(ellipse);

        }
        #endregion

        #region //绘制椭圆()
        /// <summary>
        /// 绘制椭圆
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="majorPoint01">长轴端点1</param>
        /// <param name="majorPoint02">长轴端点2</param>
        /// <param name="shortRadius">短轴的长度(小于长轴的长度)</param>
        /// <returns>ObjectId</returns>
        public static ObjectId AddEllipseToModelSpace(this Database db, Point3d majorPoint01, Point3d majorPoint02, double shortRadius)
        {

            //椭圆的圆心
            Point3d center = majorPoint01.GetCenterPoint(majorPoint02);
            //计算相关参数(长短轴比例)
            double ratio = 2 * shortRadius / majorPoint01.GetDistance(majorPoint02);
            //长轴的向量
            Vector3d majorAxis = majorPoint02.GetVectorTo(center);

            Ellipse ellipse = new Ellipse(center, Vector3d.ZAxis, majorAxis, ratio, 0, 2 * Math.PI);
            return db.AddEntityToModelSpace(ellipse);

        }
        #endregion

        #region //绘制椭圆()
        /// <summary>
        /// 绘制椭圆
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="point01">所在矩形的顶点1</param>
        /// <param name="point02">所在矩形的顶点2</param>
        /// <returns>ObjectId</returns>
        public static ObjectId AddEllipseToModelSpace(this Database db, Point3d point01, Point3d point02)
        {
            //椭圆的圆心
            Point3d center = point01.GetCenterPoint(point02);
            //计算相关参数(长短轴比例<1)
            double ratio = Math.Abs((point01.X - point02.X) / (point01.Y - point02.Y));
            //长轴的向量
            Vector3d majorVector = new Vector3d(Math.Abs((point01.X - point02.X)) / 2, 0, 0);

            Ellipse ellipse = new Ellipse(center, Vector3d.ZAxis, majorVector, ratio, 0, 2 * Math.PI);
            return db.AddEntityToModelSpace(ellipse);


        }
        #endregion

        #region //图案填充

        #endregion
    }
}
