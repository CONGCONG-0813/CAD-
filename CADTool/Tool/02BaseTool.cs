using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAD工具.Tool
{
    public static partial class BaseTool
    {
        #region //角度转弧度
        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="angle">角度值</param>
        /// <returns>弧度</returns>
        public static double AngleToRadian(this Double angle)
        {
            return angle * Math.PI / 180;
        }

        #endregion

        #region // 弧度转角度
        /// <summary>
        /// 弧度转角度
        /// </summary>
        /// <param name="radian">弧度制</param>
        /// <returns>角度</returns>
        public static double RadianToAngle(this double radian)
        {
            return radian * 180 / Math.PI;
        }

        #endregion

        #region //判断三点不在同一条直线上
        /// <summary>
        /// 判断三点不在同一条直线上
        /// </summary>
        /// <param name="firstPoint">第一个点</param>
        /// <param name="secondPoint">第二个点</param>
        /// <param name="thirdPoint">第三个点</param>
        /// <returns></returns>
        public static bool IsOnOneLine(this Point3d firstPoint, Point3d secondPoint, Point3d thirdPoint)
        {
            Vector3d v21 = secondPoint.GetVectorTo(firstPoint);
            Vector3d v23 = secondPoint.GetVectorTo(thirdPoint);
            if (v21.GetAngleTo(v23) == 0 || v21.GetAngleTo(v23) == Math.PI)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        #endregion

        #region // 两点之间的向量
        /// <summary>
        /// 两点之间的向量
        /// </summary>
        /// <param name="startPoint">起点</param>
        /// <param name="endPoint">终点</param>
        /// <returns></returns>
        public static double GetVectorToXAxis(this Point3d startPoint, Point3d endPoint)
        {
            //声明一个与X轴平行的向量
            Vector3d vx = new Vector3d(1, 0, 0);
            //获取起点到终点的向量
            Vector3d vstartpointToendpoint = startPoint.GetVectorTo(endPoint);
            //判断
            return vstartpointToendpoint.Y > 0 ? vx.GetAngleTo(vstartpointToendpoint) : -vx.GetAngleTo(vstartpointToendpoint);

        }
        #endregion

        #region //两点之间的距离
        /// <summary>
        /// 获取两点之间的距离
        /// </summary>
        /// <param name="startPoint">起点</param>
        /// <param name="endPoint">终点</param>
        /// <returns></returns>
        public static double GetDistance(this Point3d startPoint, Point3d endPoint)
        {
            return Math.Sqrt((startPoint.X - endPoint.X) * (startPoint.X - endPoint.X) + (startPoint.Y - endPoint.Y) * (startPoint.Y - endPoint.Y) + (startPoint.Z - endPoint.Z) * (startPoint.Z - endPoint.Z));

        }
        #endregion

        #region //获取中心点，两点之间
        /// <summary>
        /// 获取两点的中心点
        /// </summary>
        /// <param name="point01">第一个点</param>
        /// <param name="point02">第二个点</param>
        /// <returns>中心点</returns>
        public static Point3d GetCenterPoint(this Point3d point01, Point3d point02)
        {
            return new Point3d((point01.X + point02.X) / 2, (point01.Y + point02.Y) / 2, (point01.Z + point02.Z) / 2);
        }
        #endregion
    }
}
