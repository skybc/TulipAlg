using System;

namespace TulipAlg.Core.Geometry3D
{
    /// <summary>
    /// 三维空间中的点
    /// Represents a point in 3D space with double precision coordinates.
    /// 
    /// 数学定义：P = (x, y, z) ∈ ℝ³
    /// </summary>
    public struct Point3D : IEquatable<Point3D>
    {
        #region Properties

        /// <summary>
        /// X 坐标
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y 坐标
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Z 坐标
        /// </summary>
        public double Z { get; set; }

        /// <summary>
        /// 原点 (0, 0, 0)
        /// </summary>
        public static Point3D Origin => new Point3D(0, 0, 0);

        #endregion

        #region Constructors

        /// <summary>
        /// 构造三维点
        /// </summary>
        /// <param name="x">X 坐标</param>
        /// <param name="y">Y 坐标</param>
        /// <param name="z">Z 坐标</param>
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        #endregion

        #region Distance Methods

        /// <summary>
        /// 计算到另一点的欧氏距离
        /// 
        /// 公式：d(P₁, P₂) = √[(x₂-x₁)² + (y₂-y₁)² + (z₂-z₁)²]
        /// </summary>
        /// <param name="other">目标点</param>
        /// <returns>欧氏距离</returns>
        public double DistanceTo(Point3D other)
        {
            double dx = other.X - X;
            double dy = other.Y - Y;
            double dz = other.Z - Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// 计算到另一点的平方距离（避免开方运算，用于性能优化）
        /// 
        /// 公式：d²(P₁, P₂) = (x₂-x₁)² + (y₂-y₁)² + (z₂-z₁)²
        /// </summary>
        public double DistanceSquaredTo(Point3D other)
        {
            double dx = other.X - X;
            double dy = other.Y - Y;
            double dz = other.Z - Z;
            return dx * dx + dy * dy + dz * dz;
        }

        #endregion

        #region Operators

        /// <summary>
        /// 点加向量 → 点平移
        /// P + v = (x + vₓ, y + vᵧ, z + v_z)
        /// </summary>
        public static Point3D operator +(Point3D point, Vector3 vector)
        {
            return new Point3D(point.X + vector.X, point.Y + vector.Y, point.Z + vector.Z);
        }

        /// <summary>
        /// 点减向量 → 点平移
        /// P - v = (x - vₓ, y - vᵧ, z - v_z)
        /// </summary>
        public static Point3D operator -(Point3D point, Vector3 vector)
        {
            return new Point3D(point.X - vector.X, point.Y - vector.Y, point.Z - vector.Z);
        }

        /// <summary>
        /// 点减点 → 向量
        /// P₂ - P₁ = (x₂-x₁, y₂-y₁, z₂-z₁)
        /// </summary>
        public static Vector3 operator -(Point3D p1, Point3D p2)
        {
            return new Vector3(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        /// <summary>
        /// 点的标量乘法（用于重心坐标等场景）
        /// </summary>
        public static Point3D operator *(Point3D point, double scalar)
        {
            return new Point3D(point.X * scalar, point.Y * scalar, point.Z * scalar);
        }

        /// <summary>
        /// 点的标量乘法（交换律）
        /// </summary>
        public static Point3D operator *(double scalar, Point3D point)
        {
            return point * scalar;
        }

        /// <summary>
        /// 相等比较
        /// </summary>
        public static bool operator ==(Point3D p1, Point3D p2)
        {
            return p1.Equals(p2);
        }

        /// <summary>
        /// 不等比较
        /// </summary>
        public static bool operator !=(Point3D p1, Point3D p2)
        {
            return !p1.Equals(p2);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 线性插值
        /// 
        /// 公式：P(t) = P₁ + t(P₂ - P₁) = (1-t)P₁ + tP₂
        /// 
        /// 当 t ∈ [0,1] 时，结果在线段 P₁P₂ 上
        /// </summary>
        /// <param name="p1">起点</param>
        /// <param name="p2">终点</param>
        /// <param name="t">插值参数</param>
        public static Point3D Lerp(Point3D p1, Point3D p2, double t)
        {
            return new Point3D(
                p1.X + t * (p2.X - p1.X),
                p1.Y + t * (p2.Y - p1.Y),
                p1.Z + t * (p2.Z - p1.Z)
            );
        }

        /// <summary>
        /// 转换为向量（从原点到该点的位置向量）
        /// </summary>
        public Vector3 ToVector()
        {
            return new Vector3(X, Y, Z);
        }

        #endregion

        #region IEquatable Implementation

        public bool Equals(Point3D other)
        {
            const double epsilon = 1e-10;
            return Math.Abs(X - other.X) < epsilon &&
                   Math.Abs(Y - other.Y) < epsilon &&
                   Math.Abs(Z - other.Z) < epsilon;
        }

        public override bool Equals(object? obj)
        {
            return obj is Point3D point && Equals(point);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        #endregion

        #region String Representation

        public override string ToString()
        {
            return $"Point3D({X:F3}, {Y:F3}, {Z:F3})";
        }

        #endregion
    }
}
