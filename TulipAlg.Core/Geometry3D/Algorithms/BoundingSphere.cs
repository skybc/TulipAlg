using System;
using System.Collections.Generic;
using System.Linq;

namespace TulipAlg.Core.Geometry3D.Algorithms
{
    /// <summary>
    /// 最小包围球（Bounding Sphere）算法
    /// 
    /// 📘 算法原理：
    /// 
    /// 最小包围球是包含所有点的最小球体
    /// 
    /// Welzl 算法：
    /// - 随机增量算法
    /// - 基于递归和随机化
    /// - 期望时间复杂度 O(n)
    /// 
    /// 算法思想：
    /// 
    /// MiniSphere(P, R):
    ///   输入：点集 P，边界点集 R（|R| ≤ 4）
    ///   输出：最小包围球
    ///   
    ///   if P = ∅ or |R| = 4:
    ///       return Sphere(R)
    ///   
    ///   随机选择 p ∈ P
    ///   D = MiniSphere(P \ {p}, R)
    ///   
    ///   if p ∈ D:
    ///       return D
    ///   else:
    ///       return MiniSphere(P \ {p}, R ∪ {p})
    /// 
    /// 📘 几何基础：
    /// 
    /// 1个点：球心 = 点，半径 = 0
    /// 2个点：球心 = 中点，半径 = 距离/2
    /// 3个点：外接圆
    /// 4个点：外接球
    /// 
    /// 应用：
    /// - 碰撞检测
    /// - 视锥剔除
    /// - LOD 选择
    /// </summary>
    public class BoundingSphere
    {
        #region Properties

        /// <summary>
        /// 球心
        /// </summary>
        public Point3D Center { get; set; }

        /// <summary>
        /// 半径
        /// </summary>
        public double Radius { get; set; }

        #endregion

        #region Constructors

        public BoundingSphere(Point3D center, double radius)
        {
            Center = center;
            Radius = radius;
        }

        #endregion

        #region Welzl Algorithm

        /// <summary>
        /// Welzl 算法计算最小包围球
        /// 
        /// 📘 完整递归实现
        /// </summary>
        public static BoundingSphere ComputeMinimalSphere(List<Point3D> points)
        {
            if (points == null || points.Count == 0)
            {
                throw new ArgumentException("Points collection cannot be empty");
            }

            // 随机化点序（重要！）
            var shuffled = points.OrderBy(x => Guid.NewGuid()).ToList();

            return WelzlAlgorithm(shuffled, new List<Point3D>(), shuffled.Count);
        }

        /// <summary>
        /// Welzl 递归算法
        /// </summary>
        private static BoundingSphere WelzlAlgorithm(List<Point3D> points, List<Point3D> boundary, int n)
        {
            // 基本情况：所有点已处理或边界点足够
            if (n == 0 || boundary.Count == 4)
            {
                return SphereFromBoundary(boundary);
            }

            // 随机选择一个点（使用最后一个点）
            int idx = n - 1;
            Point3D p = points[idx];

            // 递归计算不包含 p 的最小包围球
            BoundingSphere sphere = WelzlAlgorithm(points, boundary, n - 1);

            // 如果 p 在球内，直接返回
            if (sphere.Contains(p))
            {
                return sphere;
            }

            // 否则，p 必须在边界上
            boundary.Add(p);
            sphere = WelzlAlgorithm(points, boundary, n - 1);
            boundary.RemoveAt(boundary.Count - 1); // 回溯

            return sphere;
        }

        /// <summary>
        /// 从边界点构造最小包围球
        /// 
        /// 📘 根据点数分类处理
        /// </summary>
        private static BoundingSphere SphereFromBoundary(List<Point3D> points)
        {
            return points.Count switch
            {
                0 => new BoundingSphere(Point3D.Origin, 0),
                1 => new BoundingSphere(points[0], 0),
                2 => SphereFrom2Points(points[0], points[1]),
                3 => SphereFrom3Points(points[0], points[1], points[2]),
                4 => SphereFrom4Points(points[0], points[1], points[2], points[3]),
                _ => throw new ArgumentException("Too many boundary points")
            };
        }

        #endregion

        #region Sphere Construction

        /// <summary>
        /// 通过2点构造球（直径）
        /// 
        /// 📘 球心 = 中点，半径 = 距离/2
        /// </summary>
        private static BoundingSphere SphereFrom2Points(Point3D p1, Point3D p2)
        {
            Point3D center = new Point3D(
                (p1.X + p2.X) * 0.5,
                (p1.Y + p2.Y) * 0.5,
                (p1.Z + p2.Z) * 0.5
            );

            double radius = p1.DistanceTo(p2) * 0.5;

            return new BoundingSphere(center, radius);
        }

        /// <summary>
        /// 通过3点构造球（外接圆）
        /// 
        /// 📘 三点外接圆公式：
        /// 
        /// 设三点 A, B, C 不共线
        /// 
        /// 1. 计算两条边的中垂面：
        ///    - AB 的中点 M₁，方向 AB
        ///    - AC 的中点 M₂，方向 AC
        /// 
        /// 2. 球心在两个中垂面的交线上
        /// 
        /// 3. 球心也在三角形所在平面上
        /// 
        /// 使用向量方法：
        /// O = A + s·(B-A) + t·(C-A)
        /// 
        /// 约束：|O-A| = |O-B| = |O-C|
        /// </summary>
        private static BoundingSphere SphereFrom3Points(Point3D p1, Point3D p2, Point3D p3)
        {
            Vector3 a = p2 - p1;
            Vector3 b = p3 - p1;

            double aCrossB_LengthSq = a.Cross(b).LengthSquared();

            // 检查共线
            if (aCrossB_LengthSq < 1e-10)
            {
                // 退化为2点情况
                double d12 = p1.DistanceTo(p2);
                double d13 = p1.DistanceTo(p3);
                double d23 = p2.DistanceTo(p3);

                if (d12 >= d13 && d12 >= d23)
                    return SphereFrom2Points(p1, p2);
                else if (d13 >= d23)
                    return SphereFrom2Points(p1, p3);
                else
                    return SphereFrom2Points(p2, p3);
            }

            // 外接圆公式
            double alpha = b.LengthSquared() * a.Dot(a.Cross(b)) / (2 * aCrossB_LengthSq);
            double beta = -a.LengthSquared() * b.Dot(a.Cross(b)) / (2 * aCrossB_LengthSq);

            Vector3 centerVec = p1.ToVector() + alpha * a + beta * b;
            Point3D center = new Point3D(centerVec.X, centerVec.Y, centerVec.Z);

            double radius = center.DistanceTo(p1);

            return new BoundingSphere(center, radius);
        }

        /// <summary>
        /// 通过4点构造球（外接球）
        /// 
        /// 📘 四点外接球公式：
        /// 
        /// 设四点 A, B, C, D 不共面
        /// 
        /// 球心 O 满足：
        /// |O-A|² = |O-B|² = |O-C|² = |O-D|²
        /// 
        /// 展开：
        /// (O-A)·(O-A) = (O-B)·(O-B)
        /// O·O - 2O·A + A·A = O·O - 2O·B + B·B
        /// 2O·(B-A) = B·B - A·A
        /// 
        /// 构建线性方程组：
        /// [2(B-A)]   [B·B - A·A]
        /// [2(C-A)] · O = [C·C - A·A]
        /// [2(D-A)]   [D·D - A·A]
        /// 
        /// 使用 Cramer 法则或高斯消元求解
        /// </summary>
        private static BoundingSphere SphereFrom4Points(Point3D p1, Point3D p2, Point3D p3, Point3D p4)
        {
            // 构建方程组
            Vector3 v1 = p2 - p1;
            Vector3 v2 = p3 - p1;
            Vector3 v3 = p4 - p1;

            double b1 = v1.LengthSquared();
            double b2 = v2.LengthSquared();
            double b3 = v3.LengthSquared();

            // 使用 Cramer 法则求解 3×3 线性方程组
            Matrix3x3 A = new Matrix3x3
            {
                M00 = 2 * v1.X, M01 = 2 * v1.Y, M02 = 2 * v1.Z,
                M10 = 2 * v2.X, M11 = 2 * v2.Y, M12 = 2 * v2.Z,
                M20 = 2 * v3.X, M21 = 2 * v3.Y, M22 = 2 * v3.Z
            };

            double det = A.Determinant();

            // 检查共面
            if (Math.Abs(det) < 1e-10)
            {
                // 退化为3点情况
                return SphereFrom3Points(p1, p2, p3);
            }

            // Cramer 法则
            Matrix3x3 Ax = new Matrix3x3
            {
                M00 = b1, M01 = 2 * v1.Y, M02 = 2 * v1.Z,
                M10 = b2, M11 = 2 * v2.Y, M12 = 2 * v2.Z,
                M20 = b3, M21 = 2 * v3.Y, M22 = 2 * v3.Z
            };

            Matrix3x3 Ay = new Matrix3x3
            {
                M00 = 2 * v1.X, M01 = b1, M02 = 2 * v1.Z,
                M10 = 2 * v2.X, M11 = b2, M12 = 2 * v2.Z,
                M20 = 2 * v3.X, M21 = b3, M22 = 2 * v3.Z
            };

            Matrix3x3 Az = new Matrix3x3
            {
                M00 = 2 * v1.X, M01 = 2 * v1.Y, M02 = b1,
                M10 = 2 * v2.X, M11 = 2 * v2.Y, M12 = b2,
                M20 = 2 * v3.X, M21 = 2 * v3.Y, M22 = b3
            };

            double x = Ax.Determinant() / det;
            double y = Ay.Determinant() / det;
            double z = Az.Determinant() / det;

            Point3D center = p1 + new Vector3(x, y, z);
            double radius = center.DistanceTo(p1);

            return new BoundingSphere(center, radius);
        }

        #endregion

        #region Queries

        /// <summary>
        /// 判断点是否在球内
        /// </summary>
        public bool Contains(Point3D point, double epsilon = 1e-6)
        {
            return Center.DistanceTo(point) <= Radius + epsilon;
        }

        /// <summary>
        /// 判断两球是否相交
        /// </summary>
        public bool Intersects(BoundingSphere other)
        {
            double distance = Center.DistanceTo(other.Center);
            return distance <= Radius + other.Radius;
        }

        #endregion

        #region String Representation

        public override string ToString()
        {
            return $"BoundingSphere(Center: {Center}, Radius: {Radius:F3})";
        }

        #endregion
    }
}
