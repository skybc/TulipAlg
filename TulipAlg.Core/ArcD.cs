using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TulipAlg.Core
{
    /// <summary>
    /// 表示圆弧的结构体
    /// 圆弧由圆心、半径、起始角度和结束角度定义
    /// 角度以度为单位，从X轴正方向开始逆时针测量
    /// </summary>
    public struct ArcD
    {
        /// <summary>
        /// 圆心
        /// </summary>
        public PointD Center { get; set; }

        /// <summary>
        /// 半径
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// 起始角度（度），从X轴正方向开始逆时针测量
        /// </summary>
        public double StartAngle { get; set; }

        /// <summary>
        /// 结束角度（度），从X轴正方向开始逆时针测量
        /// </summary>
        public double EndAngle { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="center">圆心</param>
        /// <param name="radius">半径</param>
        /// <param name="startAngle">起始角度（度）</param>
        /// <param name="endAngle">结束角度（度）</param>
        public ArcD(PointD center, double radius, double startAngle, double endAngle)
        {
            Center = center;
            Radius = radius;
            StartAngle = startAngle;
            EndAngle = endAngle;
        }

        /// <summary>
        /// 获取圆弧的起点
        /// </summary>
        public PointD StartPoint
        {
            get
            {
                double radians = StartAngle * Math.PI / 180.0;
                return new PointD(
                    Center.X + Radius * Math.Cos(radians),
                    Center.Y + Radius * Math.Sin(radians)
                );
            }
        }

        /// <summary>
        /// 获取圆弧的终点
        /// </summary>
        public PointD EndPoint
        {
            get
            {
                double radians = EndAngle * Math.PI / 180.0;
                return new PointD(
                    Center.X + Radius * Math.Cos(radians),
                    Center.Y + Radius * Math.Sin(radians)
                );
            }
        }

        /// <summary>
        /// 返回圆弧的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"Arc[Center={Center}, Radius={Radius}, StartAngle={StartAngle}°, EndAngle={EndAngle}°]";
        }
    }
}
