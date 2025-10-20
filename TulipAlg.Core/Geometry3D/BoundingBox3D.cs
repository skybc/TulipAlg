using System;
using System.Collections.Generic;
using System.Linq;

namespace TulipAlg.Core.Geometry3D
{
    /// <summary>
    /// 轴对齐包围盒（Axis-Aligned Bounding Box, AABB）
    /// 
    /// 📘 定义：
    /// AABB 是边与坐标轴平行的长方体，由最小点和最大点定义
    /// 
    /// 优点：
    /// - 计算简单高效
    /// - 内存占用小
    /// - 相交检测快速
    /// 
    /// 缺点：
    /// - 旋转物体时包围盒会变大
    /// - 不够紧密
    /// 
    /// 应用：
    /// - 碰撞检测粗检阶段
    /// - 空间分割（八叉树等）
    /// - 视锥剔除
    /// </summary>
    public struct BoundingBox3D
    {
        #region Properties

        /// <summary>
        /// 最小点（各坐标的最小值）
        /// </summary>
        public Point3D Min { get; set; }

        /// <summary>
        /// 最大点（各坐标的最大值）
        /// </summary>
        public Point3D Max { get; set; }

        /// <summary>
        /// 中心点
        /// </summary>
        public Point3D Center => new Point3D(
            (Min.X + Max.X) * 0.5,
            (Min.Y + Max.Y) * 0.5,
            (Min.Z + Max.Z) * 0.5
        );

        /// <summary>
        /// 尺寸（长宽高）
        /// </summary>
        public Vector3 Size => new Vector3(
            Max.X - Min.X,
            Max.Y - Min.Y,
            Max.Z - Min.Z
        );

        /// <summary>
        /// 体积
        /// </summary>
        public double Volume
        {
            get
            {
                Vector3 size = Size;
                return size.X * size.Y * size.Z;
            }
        }

        /// <summary>
        /// 表面积
        /// </summary>
        public double SurfaceArea
        {
            get
            {
                Vector3 size = Size;
                return 2 * (size.X * size.Y + size.Y * size.Z + size.Z * size.X);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 通过最小点和最大点构造包围盒
        /// </summary>
        public BoundingBox3D(Point3D min, Point3D max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// 从点集构造包围盒
        /// 
        /// 📘 算法：遍历所有点，找到每个坐标的最小和最大值
        /// 时间复杂度：O(n)
        /// </summary>
        public static BoundingBox3D FromPoints(IEnumerable<Point3D> points)
        {
            if (!points.Any())
                throw new ArgumentException("Points collection cannot be empty");

            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double minZ = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;
            double maxZ = double.MinValue;

            foreach (var point in points)
            {
                if (point.X < minX) minX = point.X;
                if (point.Y < minY) minY = point.Y;
                if (point.Z < minZ) minZ = point.Z;
                if (point.X > maxX) maxX = point.X;
                if (point.Y > maxY) maxY = point.Y;
                if (point.Z > maxZ) maxZ = point.Z;
            }

            return new BoundingBox3D(
                new Point3D(minX, minY, minZ),
                new Point3D(maxX, maxY, maxZ)
            );
        }

        /// <summary>
        /// 从中心点和尺寸构造包围盒
        /// </summary>
        public static BoundingBox3D FromCenterAndSize(Point3D center, Vector3 size)
        {
            Vector3 halfSize = size * 0.5;
            return new BoundingBox3D(
                center - halfSize,
                center + halfSize
            );
        }

        #endregion

        #region Point Operations

        /// <summary>
        /// 判断点是否在包围盒内（包括边界）
        /// 
        /// 📘 条件：min.x ≤ p.x ≤ max.x 且类似地对 y, z
        /// </summary>
        public bool Contains(Point3D point)
        {
            return point.X >= Min.X && point.X <= Max.X &&
                   point.Y >= Min.Y && point.Y <= Max.Y &&
                   point.Z >= Min.Z && point.Z <= Max.Z;
        }

        /// <summary>
        /// 计算点到包围盒的最短距离
        /// 
        /// 📘 算法：
        /// - 若点在盒内，距离为 0
        /// - 若点在盒外，计算到盒表面的最短距离
        /// </summary>
        public double DistanceToPoint(Point3D point)
        {
            double dx = Math.Max(Math.Max(Min.X - point.X, 0), point.X - Max.X);
            double dy = Math.Max(Math.Max(Min.Y - point.Y, 0), point.Y - Max.Y);
            double dz = Math.Max(Math.Max(Min.Z - point.Z, 0), point.Z - Max.Z);

            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// 获取点在包围盒表面的最近点
        /// </summary>
        public Point3D ClosestPoint(Point3D point)
        {
            return new Point3D(
                Math.Clamp(point.X, Min.X, Max.X),
                Math.Clamp(point.Y, Min.Y, Max.Y),
                Math.Clamp(point.Z, Min.Z, Max.Z)
            );
        }

        #endregion

        #region Intersection Tests

        /// <summary>
        /// 判断两个 AABB 是否相交
        /// 
        /// 📘 分离轴定理（SAT）的应用：
        /// 
        /// 对于 AABB，只需检查三个坐标轴
        /// 
        /// 相交条件：在每个轴上的投影都有重叠
        /// 
        /// 即：
        /// box1.max.x ≥ box2.min.x && box1.min.x ≤ box2.max.x &&
        /// box1.max.y ≥ box2.min.y && box1.min.y ≤ box2.max.y &&
        /// box1.max.z ≥ box2.min.z && box1.min.z ≤ box2.max.z
        /// </summary>
        public bool Intersects(BoundingBox3D other)
        {
            return Max.X >= other.Min.X && Min.X <= other.Max.X &&
                   Max.Y >= other.Min.Y && Min.Y <= other.Max.Y &&
                   Max.Z >= other.Min.Z && Min.Z <= other.Max.Z;
        }

        /// <summary>
        /// 判断包围盒是否完全包含另一个包围盒
        /// </summary>
        public bool Contains(BoundingBox3D other)
        {
            return Min.X <= other.Min.X && Max.X >= other.Max.X &&
                   Min.Y <= other.Min.Y && Max.Y >= other.Max.Y &&
                   Min.Z <= other.Min.Z && Max.Z >= other.Max.Z;
        }

        /// <summary>
        /// 光线与 AABB 相交检测
        /// 
        /// 📘 Slab 方法：
        /// 
        /// 将盒子看作三对平行平面（slabs）的交集
        /// 对每对平面计算光线进入和离开的 t 值
        /// 
        /// 对于 x 轴：
        /// t_min_x = (min.x - origin.x) / direction.x
        /// t_max_x = (max.x - origin.x) / direction.x
        /// 
        /// 确保 t_min < t_max（如果方向为负则交换）
        /// 
        /// 最终：
        /// t_enter = max(t_min_x, t_min_y, t_min_z)
        /// t_exit = min(t_max_x, t_max_y, t_max_z)
        /// 
        /// 相交条件：t_enter ≤ t_exit && t_exit ≥ 0
        /// </summary>
        public bool RayIntersection(Point3D origin, Vector3 direction, out double tMin, out double tMax)
        {
            tMin = 0;
            tMax = double.MaxValue;

            for (int i = 0; i < 3; i++)
            {
                double invD = 1.0 / GetComponent(direction, i);
                double t0 = (GetComponent(Min, i) - GetComponent(origin, i)) * invD;
                double t1 = (GetComponent(Max, i) - GetComponent(origin, i)) * invD;

                if (invD < 0)
                {
                    (t0, t1) = (t1, t0);
                }

                tMin = Math.Max(tMin, t0);
                tMax = Math.Min(tMax, t1);

                if (tMax < tMin)
                    return false;
            }

            return tMax >= 0;
        }

        private double GetComponent(Point3D p, int index)
        {
            return index switch
            {
                0 => p.X,
                1 => p.Y,
                2 => p.Z,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private double GetComponent(Vector3 v, int index)
        {
            return index switch
            {
                0 => v.X,
                1 => v.Y,
                2 => v.Z,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        #endregion

        #region Expansion and Merging

        /// <summary>
        /// 扩展包围盒以包含指定点
        /// </summary>
        public BoundingBox3D Expand(Point3D point)
        {
            return new BoundingBox3D(
                new Point3D(
                    Math.Min(Min.X, point.X),
                    Math.Min(Min.Y, point.Y),
                    Math.Min(Min.Z, point.Z)
                ),
                new Point3D(
                    Math.Max(Max.X, point.X),
                    Math.Max(Max.Y, point.Y),
                    Math.Max(Max.Z, point.Z)
                )
            );
        }

        /// <summary>
        /// 扩展包围盒以包含另一个包围盒
        /// </summary>
        public BoundingBox3D Merge(BoundingBox3D other)
        {
            return new BoundingBox3D(
                new Point3D(
                    Math.Min(Min.X, other.Min.X),
                    Math.Min(Min.Y, other.Min.Y),
                    Math.Min(Min.Z, other.Min.Z)
                ),
                new Point3D(
                    Math.Max(Max.X, other.Max.X),
                    Math.Max(Max.Y, other.Max.Y),
                    Math.Max(Max.Z, other.Max.Z)
                )
            );
        }

        /// <summary>
        /// 在各方向上扩展包围盒
        /// </summary>
        public BoundingBox3D Inflate(double amount)
        {
            Vector3 offset = new Vector3(amount, amount, amount);
            return new BoundingBox3D(Min - offset, Max + offset);
        }

        #endregion

        #region Corner Points

        /// <summary>
        /// 获取包围盒的 8 个顶点
        /// 
        /// 顺序（二进制编码）：
        /// 0: (min.x, min.y, min.z)
        /// 1: (max.x, min.y, min.z)
        /// 2: (min.x, max.y, min.z)
        /// 3: (max.x, max.y, min.z)
        /// 4: (min.x, min.y, max.z)
        /// 5: (max.x, min.y, max.z)
        /// 6: (min.x, max.y, max.z)
        /// 7: (max.x, max.y, max.z)
        /// </summary>
        public Point3D[] GetCorners()
        {
            return new Point3D[]
            {
                new Point3D(Min.X, Min.Y, Min.Z),
                new Point3D(Max.X, Min.Y, Min.Z),
                new Point3D(Min.X, Max.Y, Min.Z),
                new Point3D(Max.X, Max.Y, Min.Z),
                new Point3D(Min.X, Min.Y, Max.Z),
                new Point3D(Max.X, Min.Y, Max.Z),
                new Point3D(Min.X, Max.Y, Max.Z),
                new Point3D(Max.X, Max.Y, Max.Z)
            };
        }

        #endregion

        #region String Representation

        public override string ToString()
        {
            return $"BoundingBox3D(Min: {Min}, Max: {Max})";
        }

        #endregion
    }
}
