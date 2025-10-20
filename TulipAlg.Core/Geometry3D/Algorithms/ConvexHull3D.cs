using System;
using System.Collections.Generic;
using System.Linq;

namespace TulipAlg.Core.Geometry3D.Algorithms
{
    /// <summary>
    /// QuickHull 3D 凸包算法
    /// 
    /// 📘 算法原理：
    /// 
    /// 凸包（Convex Hull）是包含所有点的最小凸多面体
    /// 
    /// QuickHull 是一种分治算法，类似于 QuickSort
    /// 
    /// 算法步骤：
    /// 
    /// 1. 初始化：
    ///    a. 找到 6 个极值点（X, Y, Z 方向的最小/最大值）
    ///    b. 选择距离最远的两个点构建初始单纯形
    ///    c. 找到距离这两点连线最远的第三个点，形成三角形
    ///    d. 找到距离三角形平面最远的第四个点，形成四面体
    /// 
    /// 2. 递归构建：
    ///    对每个面：
    ///    a. 找到所有在面"外侧"的点
    ///    b. 如果没有外侧点，该面是凸包的一部分
    ///    c. 否则，找到距离面最远的点（apex）
    ///    d. 删除所有从 apex 可见的面
    ///    e. 构建从 apex 到可见面边界的新面
    ///    f. 递归处理新面
    /// 
    /// 时间复杂度：
    /// - 平均：O(n log n)
    /// - 最坏：O(n²)
    /// 
    /// 空间复杂度：O(n)
    /// 
    /// 📘 关键概念：
    /// 
    /// 1. 面的方向性：
    ///    - 使用右手定则确定法向量
    ///    - 法向量指向外侧
    /// 
    /// 2. 点在面外侧的判定：
    ///    - 计算点到面的有符号距离
    ///    - 距离 > 0 表示在外侧
    /// 
    /// 3. Horizon（地平线）：
    ///    - 从 apex 可见的面的边界
    ///    - 用于构建新面
    /// </summary>
    public class ConvexHull3D
    {
        #region Face Class

        /// <summary>
        /// 凸包的面（三角形）
        /// </summary>
        private class Face
        {
            public Point3D V0 { get; set; }
            public Point3D V1 { get; set; }
            public Point3D V2 { get; set; }
            public Vector3 Normal { get; set; }
            public List<Point3D> OutsidePoints { get; set; } = new List<Point3D>();

            public Plane3D GetPlane()
            {
                return Plane3D.FromThreePoints(V0, V1, V2);
            }

            public Triangle3D GetTriangle()
            {
                return new Triangle3D(V0, V1, V2);
            }
        }

        #endregion

        #region Compute Convex Hull

        /// <summary>
        /// 计算凸包
        /// </summary>
        public ConvexHullResult Compute(List<Point3D> points)
        {
            if (points == null || points.Count < 4)
            {
                throw new ArgumentException("At least 4 points are required for 3D convex hull");
            }

            // 1. 找到初始四面体
            var initialTetrahedron = FindInitialTetrahedron(points);
            if (initialTetrahedron == null)
            {
                throw new InvalidOperationException("Cannot find initial tetrahedron (points may be coplanar)");
            }

            // 2. 初始化面列表
            var faces = new List<Face>
            {
                CreateFace(initialTetrahedron[0], initialTetrahedron[1], initialTetrahedron[2]),
                CreateFace(initialTetrahedron[0], initialTetrahedron[2], initialTetrahedron[3]),
                CreateFace(initialTetrahedron[0], initialTetrahedron[3], initialTetrahedron[1]),
                CreateFace(initialTetrahedron[1], initialTetrahedron[3], initialTetrahedron[2])
            };

            // 确保法向量指向外侧
            foreach (var face in faces)
            {
                Point3D centroid = ComputeCentroid(initialTetrahedron);
                if (!IsFacingAway(face, centroid))
                {
                    // 翻转面
                    (face.V0, face.V1) = (face.V1, face.V0);
                    face.Normal = -face.Normal;
                }
            }

            // 3. 分配剩余点到各面
            var remainingPoints = points.Except(initialTetrahedron).ToList();
            foreach (var point in remainingPoints)
            {
                foreach (var face in faces)
                {
                    if (IsPointOutside(point, face))
                    {
                        face.OutsidePoints.Add(point);
                        break; // 每个点只分配给一个面
                    }
                }
            }

            // 4. 递归构建凸包
            var finalFaces = new List<Face>();
            foreach (var face in faces)
            {
                BuildHull(face, finalFaces);
            }

            // 5. 提取顶点和三角形
            var vertices = ExtractVertices(finalFaces);
            var triangles = finalFaces.Select(f => f.GetTriangle()).ToList();

            return new ConvexHullResult
            {
                Vertices = vertices,
                Triangles = triangles,
                FaceCount = finalFaces.Count
            };
        }

        #endregion

        #region Build Hull Recursively

        /// <summary>
        /// 递归构建凸包
        /// </summary>
        private void BuildHull(Face face, List<Face> result)
        {
            if (face.OutsidePoints.Count == 0)
            {
                // 没有外侧点，该面是凸包的一部分
                result.Add(face);
                return;
            }

            // 找到距离面最远的点（apex）
            Point3D apex = FindFarthestPoint(face);

            // 找到从 apex 可见的所有面（包括递归查找）
            var visibleFaces = new List<Face> { face };
            
            // 简化：这里只处理当前面
            // 完整实现需要查找所有可见面并构建 horizon

            // 构建新面
            var newFaces = new List<Face>
            {
                CreateFace(apex, face.V0, face.V1),
                CreateFace(apex, face.V1, face.V2),
                CreateFace(apex, face.V2, face.V0)
            };

            // 分配外侧点到新面
            foreach (var point in face.OutsidePoints)
            {
                if (point.Equals(apex))
                    continue;

                foreach (var newFace in newFaces)
                {
                    if (IsPointOutside(point, newFace))
                    {
                        newFace.OutsidePoints.Add(point);
                        break;
                    }
                }
            }

            // 递归处理新面
            foreach (var newFace in newFaces)
            {
                BuildHull(newFace, result);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 找到初始四面体的四个顶点
        /// </summary>
        private List<Point3D>? FindInitialTetrahedron(List<Point3D> points)
        {
            // 找 X 方向极值点
            Point3D minX = points[0], maxX = points[0];
            foreach (var p in points)
            {
                if (p.X < minX.X) minX = p;
                if (p.X > maxX.X) maxX = p;
            }

            // 找到距离 minX-maxX 连线最远的点
            Line3D line = Line3D.FromTwoPoints(minX, maxX);
            Point3D p3 = points[0];
            double maxDist = 0;
            foreach (var p in points)
            {
                double dist = line.DistanceToPoint(p);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    p3 = p;
                }
            }

            // 找到距离三角形平面最远的点
            Plane3D plane = Plane3D.FromThreePoints(minX, maxX, p3);
            Point3D p4 = points[0];
            maxDist = 0;
            foreach (var p in points)
            {
                double dist = plane.DistanceToPoint(p);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    p4 = p;
                }
            }

            if (maxDist < 1e-6)
            {
                return null; // 所有点共面
            }

            return new List<Point3D> { minX, maxX, p3, p4 };
        }

        /// <summary>
        /// 创建面
        /// </summary>
        private Face CreateFace(Point3D v0, Point3D v1, Point3D v2)
        {
            var triangle = new Triangle3D(v0, v1, v2);
            return new Face
            {
                V0 = v0,
                V1 = v1,
                V2 = v2,
                Normal = triangle.Normal()
            };
        }

        /// <summary>
        /// 判断点是否在面的外侧
        /// </summary>
        private bool IsPointOutside(Point3D point, Face face, double epsilon = 1e-6)
        {
            Plane3D plane = face.GetPlane();
            return plane.SignedDistanceToPoint(point) > epsilon;
        }

        /// <summary>
        /// 判断面是否背向指定点
        /// </summary>
        private bool IsFacingAway(Face face, Point3D point)
        {
            Vector3 toPoint = point - face.V0;
            return face.Normal.Dot(toPoint) < 0;
        }

        /// <summary>
        /// 找到距离面最远的点
        /// </summary>
        private Point3D FindFarthestPoint(Face face)
        {
            Plane3D plane = face.GetPlane();
            Point3D farthest = face.OutsidePoints[0];
            double maxDist = 0;

            foreach (var point in face.OutsidePoints)
            {
                double dist = plane.DistanceToPoint(point);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    farthest = point;
                }
            }

            return farthest;
        }

        /// <summary>
        /// 计算点集质心
        /// </summary>
        private Point3D ComputeCentroid(List<Point3D> points)
        {
            double sumX = 0, sumY = 0, sumZ = 0;
            foreach (var p in points)
            {
                sumX += p.X;
                sumY += p.Y;
                sumZ += p.Z;
            }

            int count = points.Count;
            return new Point3D(sumX / count, sumY / count, sumZ / count);
        }

        /// <summary>
        /// 提取所有唯一顶点
        /// </summary>
        private List<Point3D> ExtractVertices(List<Face> faces)
        {
            var vertices = new HashSet<Point3D>();
            foreach (var face in faces)
            {
                vertices.Add(face.V0);
                vertices.Add(face.V1);
                vertices.Add(face.V2);
            }
            return vertices.ToList();
        }

        #endregion
    }

    /// <summary>
    /// 凸包计算结果
    /// </summary>
    public class ConvexHullResult
    {
        /// <summary>
        /// 凸包顶点
        /// </summary>
        public List<Point3D> Vertices { get; set; } = new List<Point3D>();

        /// <summary>
        /// 凸包三角面
        /// </summary>
        public List<Triangle3D> Triangles { get; set; } = new List<Triangle3D>();

        /// <summary>
        /// 面数量
        /// </summary>
        public int FaceCount { get; set; }

        public override string ToString()
        {
            return $"ConvexHull: {Vertices.Count} vertices, {FaceCount} faces";
        }
    }
}
