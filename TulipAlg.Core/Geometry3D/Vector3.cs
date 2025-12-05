using System;

namespace TulipAlg.Core.Geometry3D
{
    /// <summary>
    /// 三维向量
    /// Represents a 3D vector with double precision components.
    /// 
    /// 数学定义：v = (x, y, z) ∈ ℝ³
    /// 
    /// 向量是有方向和大小的量，常用于表示：
    /// - 位移
    /// - 速度
    /// - 法向量
    /// - 方向
    /// </summary>
    public struct Vector3 :  IEquatable<Vector3>
    {
        #region Properties

        /// <summary>
        /// X 分量
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y 分量
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Z 分量
        /// </summary>
        public double Z { get; set; }

        /// <summary>
        /// 零向量 (0, 0, 0)
        /// </summary>
        public static Vector3 Zero => new Vector3(0, 0, 0);

        /// <summary>
        /// X 轴单位向量 (1, 0, 0)
        /// </summary>
        public static Vector3 UnitX => new Vector3(1, 0, 0);

        /// <summary>
        /// Y 轴单位向量 (0, 1, 0)
        /// </summary>
        public static Vector3 UnitY => new Vector3(0, 1, 0);

        /// <summary>
        /// Z 轴单位向量 (0, 0, 1)
        /// </summary>
        public static Vector3 UnitZ => new Vector3(0, 0, 1);

        #endregion

        #region Constructors

        /// <summary>
        /// 构造三维向量
        /// </summary>
        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        #endregion

        #region Length and Normalization

        /// <summary>
        /// 计算向量的模（长度）
        /// 
        /// 公式：||v|| = √(x² + y² + z²)
        /// </summary>
        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        /// <summary>
        /// 计算向量模的平方（避免开方，性能更优）
        /// 
        /// 公式：||v||² = x² + y² + z²
        /// </summary>
        public double LengthSquared()
        {
            return X * X + Y * Y + Z * Z;
        }

        /// <summary>
        /// 归一化向量（返回单位向量）
        /// 
        /// 公式：û = v / ||v||
        /// 
        /// 单位向量保持方向不变，但长度为 1
        /// </summary>
        public Vector3 Normalize()
        {
            double length = Length();
            if (length < 1e-10)
                return Zero;
            
            return new Vector3(X / length, Y / length, Z / length);
        }

        /// <summary>
        /// 原地归一化（修改自身）
        /// </summary>
        public void NormalizeInPlace()
        {
            double length = Length();
            if (length < 1e-10)
            {
                X = Y = Z = 0;
                return;
            }
            
            X /= length;
            Y /= length;
            Z /= length;
        }

        #endregion

        #region Dot and Cross Product

        /// <summary>
        /// 点积（内积）
        /// 
        /// 公式：v₁ · v₂ = x₁x₂ + y₁y₂ + z₁z₂ = ||v₁|| ||v₂|| cos(θ)
        /// 
        /// 几何意义：
        /// - 结果为标量
        /// - 等于 0 时两向量垂直
        /// - 大于 0 时夹角小于 90°
        /// - 小于 0 时夹角大于 90°
        /// </summary>
        public double Dot(Vector3 other)
        {
            return X * other.X + Y * other.Y + Z * other.Z;
        }

        /// <summary>
        /// 叉积（外积）
        /// 
        /// 公式：v₁ × v₂ = |i    j    k  |
        ///                  |x₁   y₁   z₁ |
        ///                  |x₂   y₂   z₂ |
        /// 
        /// = (y₁z₂ - z₁y₂)i - (x₁z₂ - z₁x₂)j + (x₁y₂ - y₁x₂)k
        /// 
        /// 几何意义：
        /// - 结果是同时垂直于 v₁ 和 v₂ 的向量
        /// - 模长 = ||v₁|| ||v₂|| sin(θ)，即两向量张成的平行四边形面积
        /// - 方向遵循右手定则
        /// </summary>
        public Vector3 Cross(Vector3 other)
        {
            return new Vector3(
                Y * other.Z - Z * other.Y,
                Z * other.X - X * other.Z,
                X * other.Y - Y * other.X
            );
        }

        #endregion

        #region Angle and Projection

        /// <summary>
        /// 计算两向量夹角（弧度）
        /// 
        /// 公式：θ = arccos((v₁ · v₂) / (||v₁|| ||v₂||))
        /// 
        /// 返回值范围：[0, π]
        /// </summary>
        public double AngleTo(Vector3 other)
        {
            double dot = Dot(other);
            double lengthProduct = Length() * other.Length();
            
            if (lengthProduct < 1e-10)
                return 0;
            
            // 限制在 [-1, 1] 范围内，避免浮点误差导致 arccos 越界
            double cosAngle = Math.Clamp(dot / lengthProduct, -1.0, 1.0);
            return Math.Acos(cosAngle);
        }

        /// <summary>
        /// 计算当前向量在目标向量上的投影
        /// 
        /// 公式：proj_v₂(v₁) = ((v₁ · v₂) / ||v₂||²) v₂
        /// 
        /// 几何意义：v₁ 在 v₂ 方向上的分量
        /// </summary>
        public Vector3 ProjectOnto(Vector3 target)
        {
            double targetLengthSquared = target.LengthSquared();
            if (targetLengthSquared < 1e-10)
                return Zero;
            
            double scalar = Dot(target) / targetLengthSquared;
            return target * scalar;
        }

        /// <summary>
        /// 计算投影的标量值（投影长度，带符号）
        /// 
        /// 公式：proj_scalar = (v₁ · v₂) / ||v₂||
        /// </summary>
        public double ProjectScalarOnto(Vector3 target)
        {
            double targetLength = target.Length();
            if (targetLength < 1e-10)
                return 0;
            
            return Dot(target) / targetLength;
        }

        /// <summary>
        /// 向量反射
        /// 
        /// 公式：r = v - 2(v · n)n
        /// 
        /// 其中 n 是归一化的法向量
        /// 
        /// 用于光线反射、碰撞反弹等场景
        /// </summary>
        /// <param name="normal">反射面法向量（应为单位向量）</param>
        public Vector3 Reflect(Vector3 normal)
        {
            return this - 2 * Dot(normal) * normal;
        }

        #endregion

        #region Operators

        /// <summary>
        /// 向量加法
        /// v₁ + v₂ = (x₁+x₂, y₁+y₂, z₁+z₂)
        /// </summary>
        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        /// <summary>
        /// 向量减法
        /// v₁ - v₂ = (x₁-x₂, y₁-y₂, z₁-z₂)
        /// </summary>
        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        /// <summary>
        /// 向量取负
        /// -v = (-x, -y, -z)
        /// </summary>
        public static Vector3 operator -(Vector3 v)
        {
            return new Vector3(-v.X, -v.Y, -v.Z);
        }

        /// <summary>
        /// 标量乘法
        /// sv = (sx, sy, sz)
        /// </summary>
        public static Vector3 operator *(double scalar, Vector3 v)
        {
            return new Vector3(scalar * v.X, scalar * v.Y, scalar * v.Z);
        }

        /// <summary>
        /// 标量乘法（交换律）
        /// </summary>
        public static Vector3 operator *(Vector3 v, double scalar)
        {
            return scalar * v;
        }

        /// <summary>
        /// 标量除法
        /// v/s = (x/s, y/s, z/s)
        /// </summary>
        public static Vector3 operator /(Vector3 v, double scalar)
        {
            return new Vector3(v.X / scalar, v.Y / scalar, v.Z / scalar);
        }

        /// <summary>
        /// 相等比较
        /// </summary>
        public static bool operator ==(Vector3 v1, Vector3 v2)
        {
            return v1.Equals(v2);
        }

        /// <summary>
        /// 不等比较
        /// </summary>
        public static bool operator !=(Vector3 v1, Vector3 v2)
        {
            return !v1.Equals(v2);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 计算到另一向量的距离
        /// </summary>
        public double DistanceTo(Vector3 other)
        {
            return (this - other).Length();
        }

        /// <summary>
        /// 线性插值
        /// 
        /// 公式：v(t) = (1-t)v₁ + tv₂
        /// </summary>
        public static Vector3 Lerp(Vector3 v1, Vector3 v2, double t)
        {
            return new Vector3(
                v1.X + t * (v2.X - v1.X),
                v1.Y + t * (v2.Y - v1.Y),
                v1.Z + t * (v2.Z - v1.Z)
            );
        }

        /// <summary>
        /// 判断向量是否接近零向量
        /// </summary>
        public bool IsZero(double epsilon = 1e-10)
        {
            return LengthSquared() < epsilon * epsilon;
        }

        #endregion

        #region IEquatable Implementation

        public bool Equals(Vector3 other)
        {
            const double epsilon = 1e-10;
            return Math.Abs(X - other.X) < epsilon &&
                   Math.Abs(Y - other.Y) < epsilon &&
                   Math.Abs(Z - other.Z) < epsilon;
        }

        public override bool Equals(object? obj)
        {
            return obj is Vector3 vector && Equals(vector);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        #endregion

        #region String Representation

        public override string ToString()
        {
            return $"Vector3({X:F3}, {Y:F3}, {Z:F3})";
        }

        #endregion
    }
}
