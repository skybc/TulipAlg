using System;
using System.Collections.Generic;
using TulipAlg.Core.Geometry3D;
using TulipAlg.Core.Geometry3D.Algorithms;

namespace TulipAlg.Core.Geometry3D.Examples
{
    /// <summary>
    /// 3D几何库使用示例
    /// 
    /// 本类包含所有主要功能的演示代码
    /// </summary>
    public class Geometry3DExamples
    {
        /// <summary>
        /// 示例 1：计算点到平面的距离
        /// 
        /// 📘 应用场景：
        /// - 碰撞检测
        /// - 点云分割
        /// - 平面拟合评估
        /// </summary>
        public static void Example1_PointToPlaneDistance()
        {
            Console.WriteLine("=== 示例 1：点到平面距离 ===\n");

            // 定义平面：z = 5（即 0x + 0y + 1z - 5 = 0）
            Vector3 normal = new Vector3(0, 0, 1);
            Point3D pointOnPlane = new Point3D(0, 0, 5);
            Plane3D plane = new Plane3D(normal, pointOnPlane);

            // 测试点
            Point3D testPoint = new Point3D(3, 4, 10);

            // 计算距离
            double distance = plane.DistanceToPoint(testPoint);
            double signedDistance = plane.SignedDistanceToPoint(testPoint);

            Console.WriteLine($"平面方程：{plane}");
            Console.WriteLine($"测试点：{testPoint}");
            Console.WriteLine($"距离：{distance:F3}");
            Console.WriteLine($"有符号距离：{signedDistance:F3}");
            Console.WriteLine($"点在平面{(signedDistance > 0 ? "上方" : signedDistance < 0 ? "下方" : "上")}");

            // 投影点
            Point3D projection = plane.ProjectPoint(testPoint);
            Console.WriteLine($"投影点：{projection}");
            Console.WriteLine();
        }

        /// <summary>
        /// 示例 2：计算两条直线的最短距离
        /// 
        /// 📘 应用场景：
        /// - 机器人路径规划
        /// - 骨架提取
        /// - 线缆距离计算
        /// </summary>
        public static void Example2_LineToLineDistance()
        {
            Console.WriteLine("=== 示例 2：两直线最短距离 ===\n");

            // 第一条直线：通过原点，方向为 (1,0,0)
            Line3D line1 = new Line3D(
                Point3D.Origin,
                Vector3.UnitX
            );

            // 第二条直线：通过 (0,2,3)，方向为 (0,1,0)
            Line3D line2 = new Line3D(
                new Point3D(0, 2, 3),
                Vector3.UnitY
            );

            // 计算最短距离
            double distance = line1.DistanceToLine(line2);
            Console.WriteLine($"直线1：{line1}");
            Console.WriteLine($"直线2：{line2}");
            Console.WriteLine($"最短距离：{distance:F3}");

            // 计算最近点对
            var (p1, p2) = line1.ClosestPointsTo(line2);
            Console.WriteLine($"直线1上最近点：{p1}");
            Console.WriteLine($"直线2上最近点：{p2}");
            Console.WriteLine($"验证距离：{p1.DistanceTo(p2):F3}");
            Console.WriteLine();
        }

        /// <summary>
        /// 示例 3：光线与三角形相交检测（Möller-Trumbore）
        /// 
        /// 📘 应用场景：
        /// - 光线追踪
        /// - 拾取（Picking）
        /// - 可见性判定
        /// </summary>
        public static void Example3_RayTriangleIntersection()
        {
            Console.WriteLine("=== 示例 3：光线与三角形相交 ===\n");

            // 定义三角形（在 XY 平面上）
            Triangle3D triangle = new Triangle3D(
                new Point3D(0, 0, 0),
                new Point3D(5, 0, 0),
                new Point3D(0, 5, 0)
            );

            // 光线1：从上方垂直向下（应相交）
            Point3D rayOrigin1 = new Point3D(1, 1, 10);
            Vector3 rayDirection1 = new Vector3(0, 0, -1);

            bool intersects1 = triangle.RayIntersection(
                rayOrigin1,
                rayDirection1,
                out Point3D intersection1,
                out double t1,
                out double u1,
                out double v1
            );

            Console.WriteLine($"三角形：{triangle}");
            Console.WriteLine($"\n光线1起点：{rayOrigin1}");
            Console.WriteLine($"光线1方向：{rayDirection1}");
            Console.WriteLine($"是否相交：{intersects1}");
            if (intersects1)
            {
                Console.WriteLine($"交点：{intersection1}");
                Console.WriteLine($"距离参数 t：{t1:F3}");
                Console.WriteLine($"重心坐标：u={u1:F3}, v={v1:F3}, w={1 - u1 - v1:F3}");
            }

            // 光线2：平行于三角形（不相交）
            Point3D rayOrigin2 = new Point3D(10, 10, 5);
            Vector3 rayDirection2 = new Vector3(1, 0, 0);

            bool intersects2 = triangle.RayIntersection(
                rayOrigin2,
                rayDirection2,
                out Point3D intersection2,
                out double t2,
                out double u2,
                out double v2
            );

            Console.WriteLine($"\n光线2起点：{rayOrigin2}");
            Console.WriteLine($"光线2方向：{rayDirection2}");
            Console.WriteLine($"是否相交：{intersects2}");
            Console.WriteLine();
        }

        /// <summary>
        /// 示例 4：RANSAC 平面拟合
        /// 
        /// 📘 应用场景：
        /// - 点云分割
        /// - 地面检测
        /// - 建筑物重建
        /// </summary>
        public static void Example4_RANSACPlaneFitting()
        {
            Console.WriteLine("=== 示例 4：RANSAC 平面拟合 ===\n");

            // 生成测试点云：主要在平面 z = 5 上，加少量噪声和异常值
            Random random = new Random(42);
            List<Point3D> pointCloud = new List<Point3D>();

            // 内点：在平面附近
            for (int i = 0; i < 100; i++)
            {
                double x = random.NextDouble() * 10 - 5;
                double y = random.NextDouble() * 10 - 5;
                double z = 5 + (random.NextDouble() - 0.5) * 0.2; // 小噪声
                pointCloud.Add(new Point3D(x, y, z));
            }

            // 异常值
            for (int i = 0; i < 20; i++)
            {
                double x = random.NextDouble() * 10 - 5;
                double y = random.NextDouble() * 10 - 5;
                double z = random.NextDouble() * 20; // 随机高度
                pointCloud.Add(new Point3D(x, y, z));
            }

            Console.WriteLine($"点云总数：{pointCloud.Count}");
            Console.WriteLine($"内点：100，异常值：20");

            // RANSAC 拟合
            RansacPlaneFitting ransac = new RansacPlaneFitting(42)
            {
                MaxIterations = 500,
                DistanceThreshold = 0.3,
                MinInlierRatio = 0.8
            };

            RansacPlaneResult result = ransac.FitPlane(pointCloud);

            Console.WriteLine($"\n拟合结果：{result}");
            Console.WriteLine($"检测到的内点：{result.InlierCount}/{pointCloud.Count}");
            Console.WriteLine($"拟合平面：{result.Plane}");
            Console.WriteLine($"理论平面：Plane3D(0.000x + 0.000y + 1.000z + -5.000 = 0)");
            Console.WriteLine();
        }

        /// <summary>
        /// 示例 5：ICP 点云配准
        /// 
        /// 📘 应用场景：
        /// - 3D 扫描配准
        /// - SLAM
        /// - 姿态估计
        /// </summary>
        public static void Example5_ICPPointCloudAlignment()
        {
            Console.WriteLine("=== 示例 5：ICP 点云配准 ===\n");

            // 生成源点云（立方体顶点）
            List<Point3D> sourceCloud = new List<Point3D>
            {
                new Point3D(0, 0, 0),
                new Point3D(1, 0, 0),
                new Point3D(0, 1, 0),
                new Point3D(1, 1, 0),
                new Point3D(0, 0, 1),
                new Point3D(1, 0, 1),
                new Point3D(0, 1, 1),
                new Point3D(1, 1, 1)
            };

            // 创建目标点云：平移和轻微旋转
            Matrix4x4 transform = Matrix4x4.CreateTranslation(5, 3, 2) *
                                  Matrix4x4.CreateRotationZ(Math.PI / 12);

            List<Point3D> targetCloud = sourceCloud
                .Select(p => transform.Transform(p))
                .ToList();

            Console.WriteLine($"源点云数量：{sourceCloud.Count}");
            Console.WriteLine($"目标点云数量：{targetCloud.Count}");
            Console.WriteLine($"真实变换：平移(5,3,2) + 绕Z轴旋转15°");

            // ICP 配准
            ICP icp = new ICP
            {
                MaxIterations = 50,
                ConvergenceThreshold = 1e-6
            };

            ICPResult result = icp.Align(sourceCloud, targetCloud);

            Console.WriteLine($"\n配准结果：{result}");
            Console.WriteLine($"变换矩阵：");
            Console.WriteLine(result.Transform);
            Console.WriteLine();
        }

        /// <summary>
        /// 示例 6：KD-Tree 最近邻搜索
        /// 
        /// 📘 应用场景：
        /// - 点云处理
        /// - 最近邻分类
        /// - 碰撞检测
        /// </summary>
        public static void Example6_KDTreeNearestNeighbor()
        {
            Console.WriteLine("=== 示例 6：KD-Tree 最近邻搜索 ===\n");

            // 生成随机点云
            Random random = new Random(42);
            List<Point3D> points = new List<Point3D>();

            for (int i = 0; i < 1000; i++)
            {
                points.Add(new Point3D(
                    random.NextDouble() * 100,
                    random.NextDouble() * 100,
                    random.NextDouble() * 100
                ));
            }

            // 构建 KD-Tree
            Console.WriteLine($"构建 KD-Tree，点数：{points.Count}");
            KDTree kdTree = new KDTree(points);

            // 查询点
            Point3D queryPoint = new Point3D(50, 50, 50);
            Console.WriteLine($"查询点：{queryPoint}");

            // 最近邻
            Point3D nearest = kdTree.FindNearest(queryPoint);
            Console.WriteLine($"最近邻点：{nearest}");
            Console.WriteLine($"距离：{queryPoint.DistanceTo(nearest):F3}");

            // K 近邻
            int k = 5;
            List<Point3D> kNearest = kdTree.FindKNearest(queryPoint, k);
            Console.WriteLine($"\n{k} 个最近邻：");
            for (int i = 0; i < kNearest.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {kNearest[i]}, 距离: {queryPoint.DistanceTo(kNearest[i]):F3}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// 示例 7：凸包计算
        /// 
        /// 📘 应用场景：
        /// - 碰撞检测
        /// - 形状简化
        /// - 可见性判定
        /// </summary>
        public static void Example7_ConvexHull()
        {
            Console.WriteLine("=== 示例 7：3D 凸包计算 ===\n");

            // 生成随机点云
            Random random = new Random(42);
            List<Point3D> points = new List<Point3D>();

            for (int i = 0; i < 50; i++)
            {
                points.Add(new Point3D(
                    random.NextDouble() * 10 - 5,
                    random.NextDouble() * 10 - 5,
                    random.NextDouble() * 10 - 5
                ));
            }

            Console.WriteLine($"输入点数：{points.Count}");

            // 计算凸包
            ConvexHull3D convexHull = new ConvexHull3D();
            ConvexHullResult result = convexHull.Compute(points);

            Console.WriteLine($"\n凸包结果：{result}");
            Console.WriteLine($"凸包顶点数：{result.Vertices.Count}");
            Console.WriteLine($"凸包面数：{result.FaceCount}");
            Console.WriteLine();
        }

        /// <summary>
        /// 示例 8：最小包围球
        /// 
        /// 📘 应用场景：
        /// - LOD 选择
        /// - 视锥剔除
        /// - 碰撞检测
        /// </summary>
        public static void Example8_BoundingSphere()
        {
            Console.WriteLine("=== 示例 8：最小包围球 ===\n");

            // 测试点集（立方体顶点）
            List<Point3D> points = new List<Point3D>
            {
                new Point3D(-1, -1, -1),
                new Point3D(1, -1, -1),
                new Point3D(-1, 1, -1),
                new Point3D(1, 1, -1),
                new Point3D(-1, -1, 1),
                new Point3D(1, -1, 1),
                new Point3D(-1, 1, 1),
                new Point3D(1, 1, 1)
            };

            Console.WriteLine($"输入点数：{points.Count}");
            Console.WriteLine("点集：边长为2的立方体顶点");

            // 计算最小包围球
            BoundingSphere sphere = BoundingSphere.ComputeMinimalSphere(points);

            Console.WriteLine($"\n包围球：{sphere}");
            Console.WriteLine($"理论球心：(0, 0, 0)");
            Console.WriteLine($"理论半径：√3 ≈ {Math.Sqrt(3):F3}");

            // 验证所有点在球内
            bool allInside = points.All(p => sphere.Contains(p));
            Console.WriteLine($"所有点在球内：{allInside}");
            Console.WriteLine();
        }

        /// <summary>
        /// 示例 9：矩阵变换
        /// 
        /// 📘 应用场景：
        /// - 坐标系变换
        /// - 物体变换
        /// - 相机投影
        /// </summary>
        public static void Example9_MatrixTransformations()
        {
            Console.WriteLine("=== 示例 9：矩阵变换 ===\n");

            // 原始点
            Point3D point = new Point3D(1, 0, 0);
            Console.WriteLine($"原始点：{point}");

            // 平移
            Matrix4x4 translation = Matrix4x4.CreateTranslation(5, 3, 2);
            Point3D translated = translation.Transform(point);
            Console.WriteLine($"平移后：{translated}");

            // 旋转（绕 Z 轴 90°）
            Matrix4x4 rotation = Matrix4x4.CreateRotationZ(Math.PI / 2);
            Point3D rotated = rotation.Transform(point);
            Console.WriteLine($"旋转后：{rotated}");

            // 缩放
            Matrix4x4 scaling = Matrix4x4.CreateScale(2, 2, 2);
            Point3D scaled = scaling.Transform(point);
            Console.WriteLine($"缩放后：{scaled}");

            // 组合变换：先旋转，再平移
            Matrix4x4 combined = translation * rotation;
            Point3D transformed = combined.Transform(point);
            Console.WriteLine($"组合变换后：{transformed}");

            // Rodrigues 旋转（绕任意轴）
            Vector3 axis = new Vector3(1, 1, 1).Normalize();
            Matrix4x4 rodrigues = Matrix4x4.CreateRotation(axis, Math.PI / 3);
            Point3D rodriguesRotated = rodrigues.Transform(point);
            Console.WriteLine($"Rodrigues旋转后：{rodriguesRotated}");
            Console.WriteLine();
        }

        /// <summary>
        /// 示例 10：包围盒操作
        /// 
        /// 📘 应用场景：
        /// - 空间分割
        /// - 粗检测
        /// - 八叉树构建
        /// </summary>
        public static void Example10_BoundingBox()
        {
            Console.WriteLine("=== 示例 10：AABB 包围盒 ===\n");

            // 生成点云
            List<Point3D> points = new List<Point3D>
            {
                new Point3D(0, 0, 0),
                new Point3D(5, 3, 2),
                new Point3D(-2, 4, 1),
                new Point3D(3, -1, 4)
            };

            // 构建 AABB
            BoundingBox3D aabb = BoundingBox3D.FromPoints(points);
            Console.WriteLine($"点数：{points.Count}");
            Console.WriteLine($"AABB：{aabb}");
            Console.WriteLine($"中心：{aabb.Center}");
            Console.WriteLine($"尺寸：{aabb.Size}");
            Console.WriteLine($"体积：{aabb.Volume:F3}");

            // 点包含测试
            Point3D testPoint = new Point3D(1, 1, 1);
            bool contains = aabb.Contains(testPoint);
            Console.WriteLine($"\n测试点 {testPoint} 在AABB内：{contains}");

            // 包围盒相交测试
            BoundingBox3D aabb2 = new BoundingBox3D(
                new Point3D(2, 2, 2),
                new Point3D(6, 6, 6)
            );
            bool intersects = aabb.Intersects(aabb2);
            Console.WriteLine($"AABB 相交测试：{intersects}");

            // 光线相交测试
            Point3D rayOrigin = new Point3D(-10, 0, 0);
            Vector3 rayDirection = Vector3.UnitX;
            bool rayHit = aabb.RayIntersection(rayOrigin, rayDirection, out double tMin, out double tMax);
            Console.WriteLine($"\n光线相交：{rayHit}");
            if (rayHit)
            {
                Console.WriteLine($"进入参数：{tMin:F3}, 离开参数：{tMax:F3}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// 运行所有示例
        /// </summary>
        public static void RunAllExamples()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         3D Geometry Library - 完整示例演示                    ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

            Example1_PointToPlaneDistance();
            Example2_LineToLineDistance();
            Example3_RayTriangleIntersection();
            Example4_RANSACPlaneFitting();
            Example5_ICPPointCloudAlignment();
            Example6_KDTreeNearestNeighbor();
            Example7_ConvexHull();
            Example8_BoundingSphere();
            Example9_MatrixTransformations();
            Example10_BoundingBox();

            Console.WriteLine("所有示例执行完毕！");
        }
    }
}
