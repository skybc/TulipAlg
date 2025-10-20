using System;

namespace TulipAlg.Core.Geometry3D
{
    /// <summary>
    /// 三维空间中的平面
    /// Represents a plane in 3D space using implicit form.
    /// 
    /// 📘 平面方程：
    /// 
    /// 隐式方程：n·(P - P₀) = 0
    /// 展开后：n·P = d
    /// 或：ax + by + cz + d = 0
    /// 
    /// 其中：
    /// - n = (a, b, c) 是法向量
    /// - P₀ 是平面上的一点
    /// - d = -n·P₀
    /// </summary>
    public struct Plane3D
    {
        #region Properties

        /// <summary>
        /// 平面法向量（单位向量）
        /// </summary>
        public Vector3 Normal { get; set; }

        /// <summary>
        /// 平面方程常数项 D
        /// 
        /// 满足：n·P + D = 0 对于平面上任意点 P
        /// 或：D = -n·P₀（P₀ 是平面上任意点）
        /// </summary>
        public double D { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 通过法向量和常数项构造平面
        /// </summary>
        public Plane3D(Vector3 normal, double d)
        {
            Normal = normal.Normalize();
            D = d;
        }

        /// <summary>
        /// 通过法向量和平面上一点构造平面
        /// </summary>
        public Plane3D(Vector3 normal, Point3D point)
        {
            Normal = normal.Normalize();
            D = -Normal.Dot(point.ToVector());
        }

        /// <summary>
        /// 通过三个不共线的点构造平面
        /// 
        /// 📘 原理：
        /// 两向量 v₁ = P₁ - P₀, v₂ = P₂ - P₀
        /// 法向量 n = v₁ × v₂
        /// </summary>
        public static Plane3D FromThreePoints(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3 v1 = p1 - p0;
            Vector3 v2 = p2 - p0;
            Vector3 normal = v1.Cross(v2).Normalize();
            
            return new Plane3D(normal, p0);
        }

        /// <summary>
        /// 通过点和两个方向向量构造平面
        /// </summary>
        public static Plane3D FromPointAndVectors(Point3D point, Vector3 v1, Vector3 v2)
        {
            Vector3 normal = v1.Cross(v2).Normalize();
            return new Plane3D(normal, point);
        }

        #endregion

        #region Point Operations

        /// <summary>
        /// 计算点到平面的有符号距离
        /// 
        /// 📘 公式：d = n·P + D
        /// 
        /// 结果意义：
        /// - d > 0：点在平面法向量指向的一侧
        /// - d = 0：点在平面上
        /// - d < 0：点在平面法向量相反的一侧
        /// </summary>
        public double SignedDistanceToPoint(Point3D point)
        {
            return Normal.Dot(point.ToVector()) + D;
        }

        /// <summary>
        /// 计算点到平面的距离（绝对值）
        /// 
        /// 📘 公式：distance = |n·P + D| / ||n||
        /// 
        /// 由于 n 已归一化，||n|| = 1，故：
        /// distance = |n·P + D|
        /// </summary>
        public double DistanceToPoint(Point3D point)
        {
            return Math.Abs(SignedDistanceToPoint(point));
        }

        /// <summary>
        /// 判断点是否在平面上
        /// </summary>
        public bool ContainsPoint(Point3D point, double epsilon = 1e-6)
        {
            return Math.Abs(SignedDistanceToPoint(point)) < epsilon;
        }

        /// <summary>
        /// 获取点在平面上的投影
        /// 
        /// 📘 公式：P' = P - d·n
        /// 
        /// 其中 d 是点到平面的有符号距离
        /// </summary>
        public Point3D ProjectPoint(Point3D point)
        {
            double distance = SignedDistanceToPoint(point);
            return point - distance * Normal;
        }

        /// <summary>
        /// 判断点在平面的哪一侧
        /// 
        /// 返回值：
        ///  1：法向量方向侧
        ///  0：平面上
        /// -1：法向量反方向侧
        /// </summary>
        public int GetSide(Point3D point, double epsilon = 1e-6)
        {
            double distance = SignedDistanceToPoint(point);
            if (Math.Abs(distance) < epsilon)
                return 0;
            return distance > 0 ? 1 : -1;
        }

        #endregion

        #region Line Intersection

        /// <summary>
        /// 计算直线与平面的交点
        /// 
        /// 📘 数学原理：
        /// 
        /// 设直线：L(t) = P + t·d
        /// 设平面：n·X + D = 0
        /// 
        /// 代入直线方程到平面方程：
        /// n·(P + t·d) + D = 0
        /// n·P + t(n·d) + D = 0
        /// 
        /// 解得：t = -(n·P + D) / (n·d)
        /// 
        /// 特殊情况：
        /// - n·d = 0：直线平行于平面
        ///   - 若 n·P + D = 0：直线在平面内
        ///   - 否则：直线与平面无交点
        /// </summary>
        public bool IntersectLine(Line3D line, out Point3D intersection, out double t)
        {
            double denominator = Normal.Dot(line.Direction);
            
            // 平行情况
            if (Math.Abs(denominator) < 1e-10)
            {
                intersection = Point3D.Origin;
                t = 0;
                return false;
            }

            double numerator = -(Normal.Dot(line.Point.ToVector()) + D);
            t = numerator / denominator;
            intersection = line.GetPoint(t);
            
            return true;
        }

        /// <summary>
        /// 判断直线是否与平面平行
        /// </summary>
        public bool IsParallelToLine(Line3D line, double epsilon = 1e-6)
        {
            return Math.Abs(Normal.Dot(line.Direction)) < epsilon;
        }

        /// <summary>
        /// 判断直线是否在平面内
        /// </summary>
        public bool ContainsLine(Line3D line, double epsilon = 1e-6)
        {
            return IsParallelToLine(line, epsilon) && ContainsPoint(line.Point, epsilon);
        }

        #endregion

        #region Plane Intersection

        /// <summary>
        /// 计算两平面的夹角（0 到 π/2）
        /// 
        /// 📘 公式：θ = arccos(|n₁·n₂|)
        /// 
        /// 取绝对值是为了得到锐角
        /// </summary>
        public double AngleTo(Plane3D other)
        {
            double cosAngle = Math.Abs(Normal.Dot(other.Normal));
            cosAngle = Math.Clamp(cosAngle, 0.0, 1.0);
            return Math.Acos(cosAngle);
        }

        /// <summary>
        /// 判断两平面是否平行
        /// </summary>
        public bool IsParallelTo(Plane3D other, double epsilon = 1e-6)
        {
            // 法向量平行 <=> 叉积接近零
            return Normal.Cross(other.Normal).Length() < epsilon;
        }

        /// <summary>
        /// 计算两平面的交线
        /// 
        /// 📘 数学原理：
        /// 
        /// 设两平面：
        /// π₁: n₁·X + d₁ = 0
        /// π₂: n₂·X + d₂ = 0
        /// 
        /// 交线方向：d = n₁ × n₂
        /// 
        /// 交线上一点的求解：
        /// 选择使某个坐标为 0 的方法，例如令 z = 0，求解：
        /// n₁·(x,y,0) + d₁ = 0
        /// n₂·(x,y,0) + d₂ = 0
        /// 
        /// 这是一个二元线性方程组
        /// </summary>
        public bool IntersectPlane(Plane3D other, out Line3D intersection)
        {
            Vector3 direction = Normal.Cross(other.Normal);
            
            // 平行情况
            if (direction.Length() < 1e-10)
            {
                intersection = default;
                return false;
            }

            direction = direction.Normalize();

            // 找交线上的一点
            // 策略：选择法向量叉积中绝对值最大的分量对应的坐标设为 0
            Point3D point;
            double ax = Math.Abs(direction.X);
            double ay = Math.Abs(direction.Y);
            double az = Math.Abs(direction.Z);

            if (ax >= ay && ax >= az)
            {
                // 设 x = 0，求解 y 和 z
                double det = Normal.Y * other.Normal.Z - Normal.Z * other.Normal.Y;
                double y = (Normal.Z * other.D - other.Normal.Z * D) / det;
                double z = (other.Normal.Y * D - Normal.Y * other.D) / det;
                point = new Point3D(0, y, z);
            }
            else if (ay >= az)
            {
                // 设 y = 0，求解 x 和 z
                double det = Normal.X * other.Normal.Z - Normal.Z * other.Normal.X;
                double x = (Normal.Z * other.D - other.Normal.Z * D) / det;
                double z = (other.Normal.X * D - Normal.X * other.D) / det;
                point = new Point3D(x, 0, z);
            }
            else
            {
                // 设 z = 0，求解 x 和 y
                double det = Normal.X * other.Normal.Y - Normal.Y * other.Normal.X;
                double x = (Normal.Y * other.D - other.Normal.Y * D) / det;
                double y = (other.Normal.X * D - Normal.X * other.D) / det;
                point = new Point3D(x, y, 0);
            }

            intersection = new Line3D(point, direction);
            return true;
        }

        #endregion

        #region Flip and Transform

        /// <summary>
        /// 翻转平面（法向量反向）
        /// </summary>
        public Plane3D Flip()
        {
            return new Plane3D(-Normal, -D);
        }

        #endregion

        #region String Representation

        public override string ToString()
        {
            return $"Plane3D({Normal.X:F3}x + {Normal.Y:F3}y + {Normal.Z:F3}z + {D:F3} = 0)";
        }

        #endregion
    }
}
