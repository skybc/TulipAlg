using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TulipAlg.Core
{
    /// <summary>
    /// 表示二维点的结构体（双精度）。
    /// 提供基本的算术运算符重载和与 `System.Drawing.PointF` 的隐式转换。
    /// </summary>
    public struct  PointD
    {
        /// <summary>
        /// X 坐标（双精度）。
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y 坐标（双精度）。
        /// </summary>
        public double Y { get; set; }
        // 构造函数
        /// <summary>
        /// 使用指定的 X 和 Y 值初始化一个新的 <see cref="PointD"/> 实例。
        /// </summary>
        /// <param name="x">X 坐标。</param>
        /// <param name="y">Y 坐标。</param>
        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }

        // 重载加法运算符
        /// <summary>
        /// 返回两个点的向量和（坐标逐项相加）。
        /// </summary>
        public static PointD operator +(PointD a, PointD b)
        {
            return new PointD(a.X + b.X, a.Y + b.Y);
        }
        // 重载减法运算符
        /// <summary>
        /// 返回两个点的向量差（坐标逐项相减）。
        /// </summary>
        public static PointD operator -(PointD a, PointD b)
        {
            return new PointD(a.X - b.X, a.Y - b.Y);
        }

        // 转换为PontF
        /// <summary>
        /// 将 <see cref="PointD"/> 隐式转换为 <see cref="System.Drawing.PointF"/>（可能有精度损失）。
        /// </summary>
        public static implicit operator System.Drawing.PointF(PointD p)
        {
            return new System.Drawing.PointF((float)p.X, (float)p.Y);
        }

        // PointF 转换为 PointD
        /// <summary>
        /// 将 <see cref="System.Drawing.PointF"/> 隐式转换为 <see cref="PointD"/>。
        /// </summary>
        public static implicit operator PointD(System.Drawing.PointF p)
        {
            return new PointD(p.X, p.Y);
        }

        // 重载ToString方法
        /// <summary>
        /// 返回点的字符串表示，格式为 "(X, Y)"。
        /// </summary>
        public override string ToString()
        {
            return $"({X}, {Y})";
        } 
    }


}
