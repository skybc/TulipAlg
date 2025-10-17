using System;
using System.Collections.Generic;

namespace TulipAlg.Core
{
    /// <summary>
    /// 几何功能使用示例
    /// </summary>
    public class GeometryExamples
    {
        /// <summary>
        /// 演示点与圆的位置关系
        /// </summary>
        public static void DemoPointCircleRelation()
        {
            Console.WriteLine("=== 点与圆的位置关系示例 ===");
            
            CircleD circle = new CircleD(new PointD(0, 0), 5.0);
            
            PointD pointInside = new PointD(2, 2);
            PointD pointOnCircle = new PointD(5, 0);
            PointD pointOutside = new PointD(10, 0);

            Console.WriteLine($"圆: 圆心={circle.Center}, 半径={circle.Radius}");
            Console.WriteLine($"点(2,2) 在圆内: {AlgGeometry.IsPointInsideCircle(pointInside, circle)}");
            Console.WriteLine($"点(5,0) 在圆上: {AlgGeometry.IsPointOnCircle(pointOnCircle, circle)}");
            Console.WriteLine($"点(10,0) 在圆外: {AlgGeometry.IsPointOutsideCircle(pointOutside, circle)}");
            Console.WriteLine();
        }

        /// <summary>
        /// 演示点与圆心的连线
        /// </summary>
        public static void DemoPointToCenter()
        {
            Console.WriteLine("=== 点与圆心的连线示例 ===");
            
            CircleD circle = new CircleD(new PointD(0, 0), 5.0);
            PointD point = new PointD(3, 4);
            
            LineD line = AlgGeometry.LineFromPointToCircleCenter(point, circle);
            Console.WriteLine($"点{point} 到圆心{circle.Center} 的连线: {line.Start} -> {line.End}");
            Console.WriteLine();
        }

        /// <summary>
        /// 演示点到圆的切线
        /// </summary>
        public static void DemoTangentLines()
        {
            Console.WriteLine("=== 点到圆的切线示例 ===");
            
            CircleD circle = new CircleD(new PointD(0, 0), 5.0);
            
            // 圆外的点
            PointD pointOutside = new PointD(10, 0);
            List<LineD> tangents = AlgGeometry.TangentLinesFromPointToCircle(pointOutside, circle);
            Console.WriteLine($"从点{pointOutside}到圆的切线数量: {tangents.Count}");
            for (int i = 0; i < tangents.Count; i++)
            {
                Console.WriteLine($"  切线{i + 1}: {tangents[i].Start} -> {tangents[i].End}");
            }
            
            // 圆上的点
            PointD pointOnCircle = new PointD(5, 0);
            tangents = AlgGeometry.TangentLinesFromPointToCircle(pointOnCircle, circle);
            Console.WriteLine($"从点{pointOnCircle}（圆上）到圆的切线数量: {tangents.Count}");
            
            // 圆内的点
            PointD pointInside = new PointD(2, 0);
            tangents = AlgGeometry.TangentLinesFromPointToCircle(pointInside, circle);
            Console.WriteLine($"从点{pointInside}（圆内）到圆的切线数量: {tangents.Count}");
            Console.WriteLine();
        }

        /// <summary>
        /// 演示点到圆的最近点
        /// </summary>
        public static void DemoClosestPointOnCircle()
        {
            Console.WriteLine("=== 点到圆的最近点示例 ===");
            
            CircleD circle = new CircleD(new PointD(0, 0), 5.0);
            PointD point = new PointD(10, 0);
            
            PointD closest = AlgGeometry.ClosestPointOnCircle(point, circle);
            double distance = AlgGeometry.Distance(point, closest);
            
            Console.WriteLine($"点{point} 到圆{circle.Center}(r={circle.Radius})的最近点: {closest}");
            Console.WriteLine($"距离: {distance:F4}");
            Console.WriteLine();
        }

        /// <summary>
        /// 演示点到圆弧的最近点
        /// </summary>
        public static void DemoClosestPointOnArc()
        {
            Console.WriteLine("=== 点到圆弧的最近点示例 ===");
            
            // 圆弧：圆心(0,0)，半径5，从0度到90度
            ArcD arc = new ArcD(new PointD(0, 0), 5.0, 0, 90);
            
            Console.WriteLine($"圆弧: 圆心={arc.Center}, 半径={arc.Radius}, 角度={arc.StartAngle}°到{arc.EndAngle}°");
            Console.WriteLine($"圆弧起点: {arc.StartPoint}");
            Console.WriteLine($"圆弧终点: {arc.EndPoint}");
            
            // 测试点在圆弧角度范围内
            PointD point1 = new PointD(10, 5);
            PointD closest1 = AlgGeometry.ClosestPointOnArc(point1, arc);
            Console.WriteLine($"点{point1} 到圆弧的最近点: {closest1}");
            
            // 测试点在圆弧角度范围外
            PointD point2 = new PointD(0, -10);
            PointD closest2 = AlgGeometry.ClosestPointOnArc(point2, arc);
            Console.WriteLine($"点{point2} 到圆弧的最近点: {closest2}");
            Console.WriteLine();
        }

        /// <summary>
        /// 演示两圆交点
        /// </summary>
        public static void DemoCircleIntersection()
        {
            Console.WriteLine("=== 两圆交点示例 ===");
            
            // 两圆相交
            CircleD circle1 = new CircleD(new PointD(0, 0), 5.0);
            CircleD circle2 = new CircleD(new PointD(6, 0), 5.0);
            
            List<PointD> intersections = AlgGeometry.CircleIntersectionPoints(circle1, circle2);
            Console.WriteLine($"圆1: {circle1.Center}, r={circle1.Radius}");
            Console.WriteLine($"圆2: {circle2.Center}, r={circle2.Radius}");
            Console.WriteLine($"交点数量: {intersections.Count}");
            for (int i = 0; i < intersections.Count; i++)
            {
                Console.WriteLine($"  交点{i + 1}: {intersections[i]}");
            }
            
            // 两圆相切
            CircleD circle3 = new CircleD(new PointD(10, 0), 5.0);
            intersections = AlgGeometry.CircleIntersectionPoints(circle1, circle3);
            Console.WriteLine($"\n圆1与圆3（外切）交点数量: {intersections.Count}");
            if (intersections.Count > 0)
            {
                Console.WriteLine($"  切点: {intersections[0]}");
            }
            
            // 两圆相离
            CircleD circle4 = new CircleD(new PointD(20, 0), 5.0);
            intersections = AlgGeometry.CircleIntersectionPoints(circle1, circle4);
            Console.WriteLine($"圆1与圆4（相离）交点数量: {intersections.Count}");
            Console.WriteLine();
        }

        /// <summary>
        /// 演示两圆距离
        /// </summary>
        public static void DemoCircleDistance()
        {
            Console.WriteLine("=== 两圆距离示例 ===");
            
            CircleD circle1 = new CircleD(new PointD(0, 0), 5.0);
            
            // 相离的圆
            CircleD circle2 = new CircleD(new PointD(20, 0), 5.0);
            double distance1 = AlgGeometry.DistanceBetweenCircles(circle1, circle2);
            Console.WriteLine($"相离: 圆1与圆2的距离 = {distance1} (正值表示相离)");
            
            // 外切的圆
            CircleD circle3 = new CircleD(new PointD(10, 0), 5.0);
            double distance2 = AlgGeometry.DistanceBetweenCircles(circle1, circle3);
            Console.WriteLine($"外切: 圆1与圆3的距离 = {distance2} (0表示相切)");
            
            // 相交的圆
            CircleD circle4 = new CircleD(new PointD(6, 0), 5.0);
            double distance3 = AlgGeometry.DistanceBetweenCircles(circle1, circle4);
            Console.WriteLine($"相交: 圆1与圆4的距离 = {distance3} (负值表示相交)");
            
            // 圆心距离
            double centerDistance = AlgGeometry.DistanceBetweenCircleCenters(circle1, circle2);
            Console.WriteLine($"圆1与圆2的圆心距离 = {centerDistance}");
            Console.WriteLine();
        }

        /// <summary>
        /// 演示点是否在多边形内
        /// </summary>
        public static void DemoPointInPolygon()
        {
            Console.WriteLine("=== 点是否在多边形内示例 ===");
            
            // 定义一个正方形
            List<PointD> square = new List<PointD>
            {
                new PointD(0, 0),
                new PointD(10, 0),
                new PointD(10, 10),
                new PointD(0, 10)
            };
            
            PointD pointInside = new PointD(5, 5);
            PointD pointOutside = new PointD(15, 5);
            PointD pointOnEdge = new PointD(10, 5);
            
            Console.WriteLine("正方形顶点: (0,0), (10,0), (10,10), (0,10)");
            Console.WriteLine($"点(5,5) 在多边形内: {AlgGeometry.IsPointInPolygon(pointInside, square)}");
            Console.WriteLine($"点(15,5) 在多边形内: {AlgGeometry.IsPointInPolygon(pointOutside, square)}");
            Console.WriteLine($"点(10,5) 在多边形内: {AlgGeometry.IsPointInPolygon(pointOnEdge, square)}");
            
            // 定义一个三角形
            List<PointD> triangle = new List<PointD>
            {
                new PointD(0, 0),
                new PointD(10, 0),
                new PointD(5, 10)
            };
            
            PointD trianglePoint = new PointD(5, 3);
            Console.WriteLine($"\n三角形顶点: (0,0), (10,0), (5,10)");
            Console.WriteLine($"点(5,3) 在三角形内: {AlgGeometry.IsPointInPolygon(trianglePoint, triangle)}");
            Console.WriteLine();
        }

        /// <summary>
        /// 演示点是否在圆形区域内
        /// </summary>
        public static void DemoPointInCircleRegion()
        {
            Console.WriteLine("=== 点是否在圆形区域内示例 ===");
            
            CircleD circle = new CircleD(new PointD(0, 0), 5.0);
            
            PointD pointInside = new PointD(2, 2);
            PointD pointOnCircle = new PointD(5, 0);
            PointD pointOutside = new PointD(10, 0);
            
            Console.WriteLine($"圆形区域: 圆心={circle.Center}, 半径={circle.Radius}");
            Console.WriteLine($"点(2,2) 在区域内: {AlgGeometry.IsPointInCircleRegion(pointInside, circle)}");
            Console.WriteLine($"点(5,0) 在区域内（圆周上）: {AlgGeometry.IsPointInCircleRegion(pointOnCircle, circle)}");
            Console.WriteLine($"点(10,0) 在区域内: {AlgGeometry.IsPointInCircleRegion(pointOutside, circle)}");
            Console.WriteLine();
        }

        /// <summary>
        /// 运行所有示例
        /// </summary>
        public static void RunAllExamples()
        {
            DemoPointCircleRelation();
            DemoPointToCenter();
            DemoTangentLines();
            DemoClosestPointOnCircle();
            DemoClosestPointOnArc();
            DemoCircleIntersection();
            DemoCircleDistance();
            DemoPointInPolygon();
            DemoPointInCircleRegion();
        }
    }
}
