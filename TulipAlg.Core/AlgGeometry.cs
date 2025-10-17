using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TulipAlg.Core
{
    /// <summary>
    /// 几何算法工具类，包含点、线、圆等基本几何操作的方法。
    /// </summary>
    public class AlgGeometry
    {
        #region 点-点
        /// <summary>
        /// 点平移指定距离（按 dx, dy）
        /// </summary>
        /// <param name="point">要平移的点</param>
        /// <param name="dx">X 方向的平移量</param>
        /// <param name="dy">Y 方向的平移量</param>
        /// <returns>平移后的点</returns>
        public static PointD Translate(PointD point, double dx, double dy)
        {
            return new PointD(point.X + dx, point.Y + dy);
        }
        /// <summary>
        /// 点沿参考线平移指定距离（沿参考线方向移动）
        /// </summary>
        /// <param name="point">要平移的点</param>
        /// <param name="referenceLine">参考线</param>
        /// <param name="distance">沿参考线方向的平移距离（正值同方向，负值反方向）</param>
        /// <returns>平移后的点</returns>
        public static PointD Translate(PointD point, LineD referenceLine, double distance)
        {
            double lineLength = Math.Sqrt(Math.Pow(referenceLine.End.X - referenceLine.Start.X, 2) + Math.Pow(referenceLine.End.Y - referenceLine.Start.Y, 2));
            if (lineLength == 0)
            {
                return point;
            }
            // 避免除以零
            double ux = (referenceLine.End.X - referenceLine.Start.X) / lineLength; // 单位向量的X分量
            double uy = (referenceLine.End.Y - referenceLine.Start.Y) / lineLength; // 单位向量的Y分量
            return new PointD(point.X + ux * distance, point.Y + uy * distance);
        }

        /// <summary>
        /// 点绕指定中心点旋转指定角度（角度单位：度）
        /// </summary>
        /// <param name="point">要旋转的点</param>
        /// <param name="center">旋转中心</param>
        /// <param name="angleDegrees">旋转角度（度）</param>
        /// <returns>旋转后的点</returns>
        public static PointD Rotate(PointD point, PointD center, double angleDegrees)
        {
            double angleRadians = angleDegrees * Math.PI / 180.0;
            double cosTheta = Math.Cos(angleRadians);
            double sinTheta = Math.Sin(angleRadians);
            double translatedX = point.X - center.X;
            double translatedY = point.Y - center.Y;
            double rotatedX = translatedX * cosTheta - translatedY * sinTheta;
            double rotatedY = translatedX * sinTheta + translatedY * cosTheta;
            return new PointD(rotatedX + center.X, rotatedY + center.Y);
        }

        // 垂直点 
        /// <summary>
        /// 计算点到直线的垂足（垂直投影点）
        /// </summary>
        /// <param name="point">需要投影的点</param>
        /// <param name="line">参考直线</param>
        /// <returns>垂足点坐标；若线段长度为零则返回原点</returns>
        public static PointD PerpendicularPoint(PointD point, LineD line)
        {
            double A = line.End.Y - line.Start.Y;
            double B = line.Start.X - line.End.X;
            double C = line.End.X * line.Start.Y - line.Start.X * line.End.Y;
            double denom = A * A + B * B;
            if (denom == 0)
            {
                // 线段长度为零，返回原点
                return point;
            }
            double x = (B * (B * point.X - A * point.Y) - A * C) / denom;
            double y = (A * (-B * point.X + A * point.Y) - B * C) / denom;
            return new PointD(x, y);
        }

        // 距离
        /// <summary>
        /// 计算两点之间的欧氏距离
        /// </summary>
        /// <param name="p1">第一个点</param>
        /// <param name="p2">第二个点</param>
        /// <returns>两点之间的距离</returns>
        public static double Distance(PointD p1, PointD p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }
        /// <summary>
        /// 点在线上的投影点
        /// </summary>
        /// <param name="point">待投影的点</param>
        /// <param name="line">参考直线</param>
        /// <returns>投影点；若线段长度为零则返回线段起点</returns>
        public static PointD ProjectPointOntoLine(PointD point, LineD line)
        {
            double A = line.End.Y - line.Start.Y;
            double B = line.Start.X - line.End.X;
            double C = line.End.X * line.Start.Y - line.Start.X * line.End.Y;
            double denom = A * A + B * B;
            if (denom == 0)
            {
                // 线段长度为零，返回线段起点
                return line.Start;
            }
            double x = (B * (B * point.X - A * point.Y) - A * C) / denom;
            double y = (A * (-B * point.X + A * point.Y) - B * C) / denom;
            return new PointD(x, y);
        }
        /// <summary>
        /// 两点在参考线上的投影距离（沿参考线的投影间距）
        /// </summary>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <param name="referenceLine">参考线</param>
        /// <returns>两点投影之间的距离</returns>
        public static double Distance(PointD point1, PointD point2, LineD referenceLine)
        {
            PointD proj1 = ProjectPointOntoLine(point1, referenceLine);
            PointD proj2 = ProjectPointOntoLine(point2, referenceLine);
            return Distance(proj1, proj2);
        }

        // 两点中心
        /// <summary>
        /// 计算两点的中点
        /// </summary>
        /// <param name="p1">第一个点</param>
        /// <param name="p2">第二个点</param>
        /// <returns>中点坐标</returns>
        public static PointD MidPoint(PointD p1, PointD p2)
        {
            return new PointD((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        }
        #endregion

        #region 线-线
        /// <summary>
        /// 判断两条线是否平行（使用向量叉积判断）
        /// </summary>
        /// <param name="line1">第一条线</param>
        /// <param name="line2">第二条线</param>
        /// <returns>若平行则返回 true，否则返回 false</returns>
        public static bool AreLinesParallel(LineD line1, LineD line2)
        {
            double dx1 = line1.End.X - line1.Start.X;
            double dy1 = line1.End.Y - line1.Start.Y;
            double dx2 = line2.End.X - line2.Start.X;
            double dy2 = line2.End.Y - line2.Start.Y;
            return Math.Abs(dx1 * dy2 - dy1 * dx2) < 1e-10; // 使用一个小的容差值来判断
        }
        /// <summary>
        /// 线按 dx, dy 平移
        /// </summary>
        /// <param name="line">要平移的线</param>
        /// <param name="dx">X 方向平移量</param>
        /// <param name="dy">Y 方向平移量</param>
        /// <returns>平移后的线</returns>
        public static LineD Translate(LineD line, double dx, double dy)
        {
            return new LineD(new PointD(line.Start.X + dx, line.Start.Y + dy), new PointD(line.End.X + dx, line.End.Y + dy));
        }

        /// <summary>
        /// 沿自身方向平移线段（按距离）
        /// </summary>
        /// <param name="line">要平移的线</param>
        /// <param name="distance">沿线方向的平移距离</param>
        /// <returns>平移后的线</returns>
        public static LineD Translate(LineD line, double distance)
        {
            double lineLength = Math.Sqrt(Math.Pow(line.End.X - line.Start.X, 2) + Math.Pow(line.End.Y - line.Start.Y, 2));
            if (lineLength == 0)
            {
                return line;
            }
            // 避免除以零
            double ux = (line.End.X - line.Start.X) / lineLength; // 单位向量的X分量
            double uy = (line.End.Y - line.Start.Y) / lineLength; // 单位向量的Y分量
            return new LineD(new PointD(line.Start.X + ux * distance, line.Start.Y + uy * distance),
                             new PointD(line.End.X + ux * distance, line.End.Y + uy * distance));
        }
        /// <summary>
        /// 使用参考线方向将目标线平移指定距离
        /// </summary>
        /// <param name="line">要平移的线</param>
        /// <param name="referenceLine">参考线，用于确定方向</param>
        /// <param name="distance">平移距离</param>
        /// <returns>平移后的线</returns>
        public static LineD Translate(LineD line, LineD referenceLine, double distance)
        {
            double lineLength = Math.Sqrt(Math.Pow(referenceLine.End.X - referenceLine.Start.X, 2) + Math.Pow(referenceLine.End.Y - referenceLine.Start.Y, 2));
            if (lineLength == 0)
            {
                return line;
            }
            // 避免除以零
            double ux = (referenceLine.End.X - referenceLine.Start.X) / lineLength; // 单位向量的X分量
            double uy = (referenceLine.End.Y - referenceLine.Start.Y) / lineLength; // 单位向量的Y分量
            return new LineD(new PointD(line.Start.X + ux * distance, line.Start.Y + uy * distance),
                             new PointD(line.End.X + ux * distance, line.End.Y + uy * distance));
        }

        // 旋转
        /// <summary>
        /// 将线段绕指定中心点旋转指定角度
        /// </summary>
        /// <param name="line">要旋转的线</param>
        /// <param name="center">旋转中心</param>
        /// <param name="angleDegrees">旋转角度（度）</param>
        /// <returns>旋转后的线段</returns>
        public static LineD Rotate(LineD line, PointD center, double angleDegrees)
        {
            return new LineD(Rotate(line.Start, center, angleDegrees), Rotate(line.End, center, angleDegrees));
        }
        // 交点
        /// <summary>
        /// 计算两条直线的交点（若平行或重合返回 null）
        /// </summary>
        /// <param name="line1">第一条线</param>
        /// <param name="line2">第二条线</param>
        /// <returns>交点坐标或 null</returns>
        public static PointD? Intersection(LineD line1, LineD line2)
        {
            double A1 = line1.End.Y - line1.Start.Y;
            double B1 = line1.Start.X - line1.End.X;
            double C1 = line1.End.X * line1.Start.Y - line1.Start.X * line1.End.Y;
            double A2 = line2.End.Y - line2.Start.Y;
            double B2 = line2.Start.X - line2.End.X;
            double C2 = line2.End.X * line2.Start.Y - line2.Start.X * line2.End.Y;
            double denom = A1 * B2 - A2 * B1;
            if (Math.Abs(denom) < 1e-10)
            {
                // 平行或重合
                return null;
            }
            double x = (B1 * C2 - B2 * C1) / denom;
            double y = (A2 * C1 - A1 * C2) / denom;
            return new PointD(x, y);
        }
        // 夹角
        /// <summary>
        /// 计算两条线段之间的夹角（返回角度，单位：度）
        /// </summary>
        /// <param name="line1">第一条线</param>
        /// <param name="line2">第二条线</param>
        /// <returns>夹角（度）</returns>
        public static double AngleBetweenLines(LineD line1, LineD line2)
        {
            double dx1 = line1.End.X - line1.Start.X;
            double dy1 = line1.End.Y - line1.Start.Y;
            double dx2 = line2.End.X - line2.Start.X;
            double dy2 = line2.End.Y - line2.Start.Y;
            double dotProduct = dx1 * dx2 + dy1 * dy2;
            double magnitude1 = Math.Sqrt(dx1 * dx1 + dy1 * dy1);
            double magnitude2 = Math.Sqrt(dx2 * dx2 + dy2 * dy2);
            if (magnitude1 == 0 || magnitude2 == 0)
            {
                return 0; // 避免除以零
            }
            double cosTheta = dotProduct / (magnitude1 * magnitude2);
            cosTheta = Math.Clamp(cosTheta, -1.0, 1.0); // 确保值在有效范围内
            return Math.Acos(cosTheta) * (180.0 / Math.PI); // 返回角度值
        }

        // 交点和夹角元组输出
        /// <summary>
        /// 同时返回两条线的交点和夹角
        /// </summary>
        /// <param name="line1">第一条线</param>
        /// <param name="line2">第二条线</param>
        /// <returns>包含交点和夹角的元组</returns>
        public static (PointD? intersection, double angle) IntersectionAndAngle(LineD line1, LineD line2)
        {
            return (Intersection(line1, line2), AngleBetweenLines(line1, line2));
        }

        // 垂直
        /// <summary>
        /// 通过给定点和参考线构造一条垂直线（点到参考线的垂线）
        /// </summary>
        /// <param name="point">过该点的垂直线的起点</param>
        /// <param name="line">参考线</param>
        /// <returns>垂直于参考线并经过指定点的线段（起点为 point，终点为垂足）</returns>
        public static LineD PerpendicularLine(PointD point, LineD line)
        {
            PointD foot = PerpendicularPoint(point, line);
            return new LineD(point, foot);
        }

        // 是否垂直
        /// <summary>
        /// 判断两条线是否垂直（使用向量点积判断）
        /// </summary>
        /// <param name="line1">第一条线</param>
        /// <param name="line2">第二条线</param>
        /// <returns>若垂直则返回 true，否则返回 false</returns>
        public static bool AreLinesPerpendicular(LineD line1, LineD line2)
        {
            double dx1 = line1.End.X - line1.Start.X;
            double dy1 = line1.End.Y - line1.Start.Y;
            double dx2 = line2.End.X - line2.Start.X;
            double dy2 = line2.End.Y - line2.Start.Y;
            return Math.Abs(dx1 * dx2 + dy1 * dy2) < 1e-10; // 使用一个小的容差值来判断
        }



        #endregion

        #region 点-线
        // 点到线的距离
        /// <summary>
        /// 计算点到直线的最短距离
        /// </summary>
        /// <param name="point">点</param>
        /// <param name="line">参考直线</param>
        /// <returns>点到直线的距离</returns>
        public static double Distance(PointD point, LineD line)
        {
            // 计算垂足
            PointD foot = PerpendicularPoint(point, line);
            // 计算距离
            return Distance(point, foot);
        }

        // 是否在直线上

        /// <summary>
        /// 判断点是否在给定线段上（使用距离和容差判断）
        /// </summary>
        /// <param name="point">待判断的点</param>
        /// <param name="line">线段</param>
        /// <param name="tolerance">容差，默认为 1e-10</param>
        /// <returns>若点在直线上则返回 true，否则返回 false</returns>
        public static bool IsPointOnLine(PointD point, LineD line, double tolerance = 1e-10)
        {
            double distToStart = Distance(point, line.Start);
            double distToEnd = Distance(point, line.End);
            double lineLength = Distance(line.Start, line.End);
            return Math.Abs((distToStart + distToEnd) - lineLength) < tolerance;
        }

        #endregion

        #region 拟合圆
        /// <summary>
        /// 使用最小二乘法拟合通过给定点集的圆（非最小外接圆）
        /// </summary>
        /// <param name="points">用于拟合的点集合，至少需要 3 个点</param>
        /// <returns>拟合得到的圆（圆心和半径）</returns>
        /// <exception cref="ArgumentException">当点数少于 3 个或输入为空时抛出</exception>
        /// <exception cref="InvalidOperationException">当点近似共线导致无法拟合时抛出</exception>
        public static CircleD FitCircleToPoints(List<PointD> points)
        {
            if (points == null || points.Count < 3)
            {
                throw new ArgumentException("至少需要三个点来拟合圆。");
            }
            int n = points.Count;
            CircleD circle = new CircleD();
            double sumX = 0, sumY = 0, sumX2 = 0, sumY2 = 0, sumXY = 0, sumX3 = 0, sumY3 = 0, sumX2Y = 0, sumXY2 = 0;
            foreach (var p in points)
            {
                double x = p.X;
                double y = p.Y;
                double x2 = x * x;
                double y2 = y * y;
                sumX += x;
                sumY += y;
                sumX2 += x2;
                sumY2 += y2;
                sumXY += x * y;
                sumX3 += x2 * x;
                sumY3 += y2 * y;
                sumX2Y += x2 * y;
                sumXY2 += x * y2;
            }
            double C = n * sumX2 - sumX * sumX;
            double D = n * sumXY - sumX * sumY;
            double E = n * sumY2 - sumY * sumY;
            double G = 0.5 * (n * sumX3 + n * sumXY2 - sumX * sumX2 - sumY * sumXY);
            double H = 0.5 * (n * sumY3 + n * sumX2Y - sumY * sumY2 - sumX * sumXY);
            double denom = C * E - D * D;
            if (Math.Abs(denom) < 1e-10)
            {
                throw new InvalidOperationException("点可能共线，无法拟合圆。");
            }
            double centerX = (G * E - D * H) / denom;
            double centerY = (C * H - D * G) / denom;
            double radius = 0;
            foreach (var p in points)
            {
                radius += Distance(new PointD(centerX, centerY), p);
            }
            radius /= n;
            circle.Center = new PointD(centerX, centerY);
            circle.Radius = radius;
            return circle;
        }

        // 最小外接圆
        /// <summary>
        /// 计算点集的最小外接圆（使用 Welzl 算法）
        /// </summary>
        /// <param name="points">点集</param>
        /// <returns>最小外接圆</returns>
        public static CircleD MinimumEnclosingCircle(List<PointD> points)
        {
            if (points == null || points.Count == 0)
            {
                throw new ArgumentException("点列表不能为空。");
            }
            // 使用Welzl算法
            Random rand = new Random();
            List<PointD> shuffledPoints = points.OrderBy(p => rand.Next()).ToList();
            return Welzl(shuffledPoints, new List<PointD>(), shuffledPoints.Count);
        }

        // 最大内切圆
        /// <summary>
        /// 计算多边形的近似最大内切圆：使用多边形质心作为中心，并取到边界的最小距离作为半径
        /// 注：此方法为近似解，并非精确的最大内切圆算法
        /// </summary>
        /// <param name="polygon">多边形顶点（按顺序）</param>
        /// <returns>近似的最大内切圆</returns>
        public static CircleD MaximumInscribedCircle(List<PointD> polygon)
        {
            // 这里使用一个简单的近似方法：计算多边形的质心，然后找到质心到多边形边界的最小距离作为半径
            if (polygon == null || polygon.Count < 3)
            {
                throw new ArgumentException("多边形至少需要三个顶点。");
            }
            double centroidX = 0, centroidY = 0;
            foreach (var p in polygon)
            {
                centroidX += p.X;
                centroidY += p.Y;
            }
            centroidX /= polygon.Count;
            centroidY /= polygon.Count;
            PointD centroid = new PointD(centroidX, centroidY);
            double minDistance = double.MaxValue;
            for (int i = 0; i < polygon.Count; i++)
            {
                LineD edge = new LineD(polygon[i], polygon[(i + 1) % polygon.Count]);
                double distance = Distance(centroid, edge);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }
            return new CircleD(centroid, minDistance);
        }
        // Welzl算法的递归实现
        private static CircleD Welzl(List<PointD> points, List<PointD> boundary, int n)
        {
            if (n == 0 || boundary.Count == 3)
            {
                return TrivialCircle(boundary);
            }
            PointD p = points[n - 1];
            CircleD circle = Welzl(points, boundary, n - 1);
            if (Distance(p, circle.Center) <= circle.Radius)
            {
                return circle;
            }
            boundary.Add(p);
            CircleD newCircle = Welzl(points, boundary, n - 1);
            boundary.RemoveAt(boundary.Count - 1);
            return newCircle;
        }
        // TrivialCircle
        /// <summary>
        /// 为边界点集合（最多 3 个点）计算显式圆：0 -> 空圆，1 -> 点圆，2 -> 以两点中点为圆心的圆，3 -> 通过三点的外接圆
        /// </summary>
        /// <param name="points">边界点集合（最多 3 个）</param>
        /// <returns>满足约束的圆</returns>
        public static CircleD TrivialCircle(List<PointD> points)
        {
            if (points.Count == 0)
            {
                return new CircleD(new PointD(0, 0), 0);
            }
            else if (points.Count == 1)
            {
                return new CircleD(points[0], 0);
            }
            else if (points.Count == 2)
            {
                PointD center = MidPoint(points[0], points[1]);
                double radius = Distance(points[0], points[1]) / 2;
                return new CircleD(center, radius);
            }
            else if (points.Count == 3)
            {
                return Circumcircle(points[0], points[1], points[2]);
            }
            throw new ArgumentException("TrivialCircle只接受最多三个点。");
        }
        // 计算三角形的外接圆
        /// <summary>
        /// 计算三角形的外接圆（通过三点确定的圆）
        /// </summary>
        /// <param name="A">顶点 A</param>
        /// <param name="B">顶点 B</param>
        /// <param name="C">顶点 C</param>
        /// <returns>通过三点的外接圆</returns>
        public static CircleD Circumcircle(PointD A, PointD B, PointD C)
        {
            double D = 2 * (A.X * (B.Y - C.Y) + B.X * (C.Y - A.Y) + C.X * (A.Y - B.Y));
            if (Math.Abs(D) < 1e-10)
            {
                throw new ArgumentException("点共线，无法计算外接圆。");
            }
            double Ux = ((A.X * A.X + A.Y * A.Y) * (B.Y - C.Y) + (B.X * B.X + B.Y * B.Y) * (C.Y - A.Y) + (C.X * C.X + C.Y * C.Y) * (A.Y - B.Y)) / D;
            double Uy = ((A.X * A.X + A.Y * A.Y) * (C.X - B.X) + (B.X * B.X + B.Y * B.Y) * (A.X - C.X) + (C.X * C.X + C.Y * C.Y) * (B.X - A.X)) / D;
            PointD center = new PointD(Ux, Uy);
            double radius = Distance(center, A); // 半径为任意一个顶点到圆心的距离
            return new CircleD(center, radius);
        }

        // 内切圆
        /// <summary>
        /// 计算三角形的内切圆（圆心为角平分线交点，半径为内接半径）
        /// </summary>
        /// <param name="A">顶点 A</param>
        /// <param name="B">顶点 B</param>
        /// <param name="C">顶点 C</param>
        /// <returns>内切圆</returns>
        public static CircleD Incircle(PointD A, PointD B, PointD C)
        {
            double a = Distance(B, C);
            double b = Distance(A, C);
            double c = Distance(A, B);
            double perimeter = a + b + c;
            if (perimeter == 0)
            {
                throw new ArgumentException("点共线，无法计算内切圆。");
            }
            double centerX = (a * A.X + b * B.X + c * C.X) / perimeter;
            double centerY = (a * A.Y + b * B.Y + c * C.Y) / perimeter;
            PointD center = new PointD(centerX, centerY);
            double s = perimeter / 2;
            double area = Math.Sqrt(s * (s - a) * (s - b) * (s - c));
            double radius = area / s;
            return new CircleD(center, radius);
        }


        #endregion

        #region 点-圆

        /// <summary>
        /// 判断点是否在圆内
        /// </summary>
        /// <param name="point">待判断的点</param>
        /// <param name="circle">圆</param>
        /// <param name="tolerance">容差，默认为 1e-10</param>
        /// <returns>若点在圆内则返回 true，否则返回 false</returns>
        public static bool IsPointInsideCircle(PointD point, CircleD circle, double tolerance = 1e-10)
        {
            double distance = Distance(point, circle.Center);
            return distance < circle.Radius - tolerance;
        }

        /// <summary>
        /// 判断点是否在圆上
        /// </summary>
        /// <param name="point">待判断的点</param>
        /// <param name="circle">圆</param>
        /// <param name="tolerance">容差，默认为 1e-10</param>
        /// <returns>若点在圆上则返回 true，否则返回 false</returns>
        public static bool IsPointOnCircle(PointD point, CircleD circle, double tolerance = 1e-10)
        {
            double distance = Distance(point, circle.Center);
            return Math.Abs(distance - circle.Radius) < tolerance;
        }

        /// <summary>
        /// 判断点是否在圆外
        /// </summary>
        /// <param name="point">待判断的点</param>
        /// <param name="circle">圆</param>
        /// <param name="tolerance">容差，默认为 1e-10</param>
        /// <returns>若点在圆外则返回 true，否则返回 false</returns>
        public static bool IsPointOutsideCircle(PointD point, CircleD circle, double tolerance = 1e-10)
        {
            double distance = Distance(point, circle.Center);
            return distance > circle.Radius + tolerance;
        }

        /// <summary>
        /// 获取点与圆心的连线
        /// </summary>
        /// <param name="point">点</param>
        /// <param name="circle">圆</param>
        /// <returns>从点到圆心的线段</returns>
        public static LineD LineFromPointToCircleCenter(PointD point, CircleD circle)
        {
            return new LineD(point, circle.Center);
        }

        /// <summary>
        /// 计算点到圆的最近点（圆周上距离该点最近的点）
        /// </summary>
        /// <param name="point">点</param>
        /// <param name="circle">圆</param>
        /// <returns>圆周上最近的点</returns>
        public static PointD ClosestPointOnCircle(PointD point, CircleD circle)
        {
            double distance = Distance(point, circle.Center);

            // 如果点在圆心，返回圆上任意一点（这里选择右侧点）
            if (distance < 1e-10)
            {
                return new PointD(circle.Center.X + circle.Radius, circle.Center.Y);
            }

            // 计算从圆心到点的单位向量
            double ux = (point.X - circle.Center.X) / distance;
            double uy = (point.Y - circle.Center.Y) / distance;

            // 最近点 = 圆心 + 半径 * 单位向量
            return new PointD(
                circle.Center.X + circle.Radius * ux,
                circle.Center.Y + circle.Radius * uy
            );
        }

        /// <summary>
        /// 计算从点到圆的切线（最多两条）
        /// </summary>
        /// <param name="point">外部点</param>
        /// <param name="circle">圆</param>
        /// <returns>切线列表（0条、1条或2条）。如果点在圆内则返回空列表，如果点在圆上则返回1条切线，如果点在圆外则返回2条切线</returns>
        public static List<LineD> TangentLinesFromPointToCircle(PointD point, CircleD circle, double tolerance = 1e-10)
        {
            List<LineD> tangents = new List<LineD>();
            double distance = Distance(point, circle.Center);

            // 点在圆内，无切线
            if (distance < circle.Radius - tolerance)
            {
                return tangents;
            }

            // 点在圆上，有一条切线（垂直于半径）
            if (Math.Abs(distance - circle.Radius) < tolerance)
            {
                // 切线方向垂直于从圆心到点的向量
                double dx = point.X - circle.Center.X;
                double dy = point.Y - circle.Center.Y;

                // 垂直向量：(-dy, dx) 归一化后乘以一个长度
                double length = 10.0; // 切线段的长度
                double perpX = -dy / distance * length;
                double perpY = dx / distance * length;

                PointD tangentEnd = new PointD(point.X + perpX, point.Y + perpY);
                tangents.Add(new LineD(point, tangentEnd));
                return tangents;
            }

            // 点在圆外，有两条切线
            // 使用几何方法：切点到圆心的距离是半径，切点到外点的连线垂直于半径

            // 计算切点
            // 设外点为P，圆心为C，切点为T
            // |CT| = r (半径)
            // |CP| = distance
            // |PT|^2 = |CP|^2 - r^2 (勾股定理)

            double tangentLength = Math.Sqrt(distance * distance - circle.Radius * circle.Radius);

            // 从圆心到外点的向量
            double dxOuter = point.X - circle.Center.X;
            double dyOuter = point.Y - circle.Center.Y;

            // 计算切点的角度偏移
            double angleOffset = Math.Asin(circle.Radius / distance);
            double baseAngle = Math.Atan2(dyOuter, dxOuter);

            // 第一条切线的切点
            double angle1 = baseAngle + angleOffset;
            PointD tangentPoint1 = new PointD(
                circle.Center.X + circle.Radius * Math.Cos(angle1),
                circle.Center.Y + circle.Radius * Math.Sin(angle1)
            );
            tangents.Add(new LineD(point, tangentPoint1));

            // 第二条切线的切点
            double angle2 = baseAngle - angleOffset;
            PointD tangentPoint2 = new PointD(
                circle.Center.X + circle.Radius * Math.Cos(angle2),
                circle.Center.Y + circle.Radius * Math.Sin(angle2)
            );
            tangents.Add(new LineD(point, tangentPoint2));

            return tangents;
        }

        /// <summary>
        /// 计算点到圆弧的最近点
        /// </summary>
        /// <param name="point">点</param>
        /// <param name="arc">圆弧</param>
        /// <returns>圆弧上最近的点</returns>
        public static PointD ClosestPointOnArc(PointD point, ArcD arc)
        {
            // 计算点到圆心的向量角度
            double dx = point.X - arc.Center.X;
            double dy = point.Y - arc.Center.Y;
            double angleToPoint = Math.Atan2(dy, dx) * 180.0 / Math.PI;

            // 归一化角度到 [0, 360)
            angleToPoint = NormalizeAngle(angleToPoint);
            double startAngle = NormalizeAngle(arc.StartAngle);
            double endAngle = NormalizeAngle(arc.EndAngle);

            // 判断角度是否在圆弧范围内
            bool isInArc = false;
            if (startAngle <= endAngle)
            {
                isInArc = angleToPoint >= startAngle && angleToPoint <= endAngle;
            }
            else
            {
                // 跨越0度的情况
                isInArc = angleToPoint >= startAngle || angleToPoint <= endAngle;
            }

            if (isInArc)
            {
                // 点的角度在圆弧范围内，最近点在圆弧圆周上
                double distance = Math.Sqrt(dx * dx + dy * dy);
                if (distance < 1e-10)
                {
                    // 点在圆心，返回起点
                    return arc.StartPoint;
                }
                double ux = dx / distance;
                double uy = dy / distance;
                return new PointD(
                    arc.Center.X + arc.Radius * ux,
                    arc.Center.Y + arc.Radius * uy
                );
            }
            else
            {
                // 点的角度不在圆弧范围内，最近点是圆弧的端点之一
                PointD startPoint = arc.StartPoint;
                PointD endPoint = arc.EndPoint;

                double distToStart = Distance(point, startPoint);
                double distToEnd = Distance(point, endPoint);

                return distToStart <= distToEnd ? startPoint : endPoint;
            }
        }

        /// <summary>
        /// 归一化角度到 [0, 360) 范围
        /// </summary>
        /// <param name="angle">角度（度）</param>
        /// <returns>归一化后的角度</returns>
        private static double NormalizeAngle(double angle)
        {
            angle = angle % 360.0;
            if (angle < 0)
            {
                angle += 360.0;
            }
            return angle;
        }

        #endregion

        #region 圆-圆

        /// <summary>
        /// 计算两圆的交点
        /// </summary>
        /// <param name="circle1">第一个圆</param>
        /// <param name="circle2">第二个圆</param>
        /// <param name="tolerance">容差，默认为 1e-10</param>
        /// <returns>交点列表（0个、1个或2个交点）</returns>
        public static List<PointD> CircleIntersectionPoints(CircleD circle1, CircleD circle2, double tolerance = 1e-10)
        {
            List<PointD> intersections = new List<PointD>();

            double distance = Distance(circle1.Center, circle2.Center);
            double r1 = circle1.Radius;
            double r2 = circle2.Radius;

            // 圆心重合
            if (distance < tolerance)
            {
                // 如果半径也相同，则两圆完全重合（无穷多交点，返回空列表）
                // 如果半径不同，则无交点
                return intersections;
            }

            // 两圆相离（距离大于半径之和）
            if (distance > r1 + r2 + tolerance)
            {
                return intersections;
            }

            // 一圆包含另一圆（距离小于半径之差）
            if (distance < Math.Abs(r1 - r2) - tolerance)
            {
                return intersections;
            }

            // 两圆外切或内切（1个交点）
            if (Math.Abs(distance - (r1 + r2)) < tolerance || Math.Abs(distance - Math.Abs(r1 - r2)) < tolerance)
            {
                // 交点在两圆心连线上
                double ratio = r1 / distance;
                PointD intersection = new PointD(
                    circle1.Center.X + ratio * (circle2.Center.X - circle1.Center.X),
                    circle1.Center.Y + ratio * (circle2.Center.Y - circle1.Center.Y)
                );
                intersections.Add(intersection);
                return intersections;
            }

            // 两圆相交（2个交点）
            // 使用解析几何方法
            double a = (r1 * r1 - r2 * r2 + distance * distance) / (2 * distance);
            double h = Math.Sqrt(r1 * r1 - a * a);

            // 从circle1圆心到交点中点的向量
            double cx = circle1.Center.X + a * (circle2.Center.X - circle1.Center.X) / distance;
            double cy = circle1.Center.Y + a * (circle2.Center.Y - circle1.Center.Y) / distance;

            // 垂直于圆心连线的向量
            double offsetX = h * (circle2.Center.Y - circle1.Center.Y) / distance;
            double offsetY = h * (circle2.Center.X - circle1.Center.X) / distance;

            // 两个交点
            intersections.Add(new PointD(cx + offsetX, cy - offsetY));
            intersections.Add(new PointD(cx - offsetX, cy + offsetY));

            return intersections;
        }

        /// <summary>
        /// 计算两圆之间的距离（圆心距离减去两个半径）
        /// </summary>
        /// <param name="circle1">第一个圆</param>
        /// <param name="circle2">第二个圆</param>
        /// <returns>
        /// 两圆之间的距离。
        /// 如果两圆相离，返回正值（圆周之间的最短距离）；
        /// 如果两圆相切，返回0；
        /// 如果两圆相交或包含，返回负值（重叠部分的深度）
        /// </returns>
        public static double DistanceBetweenCircles(CircleD circle1, CircleD circle2)
        {
            double centerDistance = Distance(circle1.Center, circle2.Center);
            return centerDistance - circle1.Radius - circle2.Radius;
        }

        /// <summary>
        /// 计算两圆圆心之间的距离
        /// </summary>
        /// <param name="circle1">第一个圆</param>
        /// <param name="circle2">第二个圆</param>
        /// <returns>圆心距离</returns>
        public static double DistanceBetweenCircleCenters(CircleD circle1, CircleD circle2)
        {
            return Distance(circle1.Center, circle2.Center);
        }

        #endregion

        #region 点-区域

        /// <summary>
        /// 判断点是否在多边形内（使用射线法）
        /// </summary>
        /// <param name="point">待判断的点</param>
        /// <param name="polygon">多边形顶点列表（按顺序）</param>
        /// <returns>若点在多边形内则返回 true，否则返回 false</returns>
        public static bool IsPointInPolygon(PointD point, List<PointD> polygon)
        {
            if (polygon == null || polygon.Count < 3)
            {
                throw new ArgumentException("多边形至少需要三个顶点。");
            }

            int intersectionCount = 0;
            int n = polygon.Count;

            for (int i = 0; i < n; i++)
            {
                PointD p1 = polygon[i];
                PointD p2 = polygon[(i + 1) % n];

                // 检查点是否在边的水平范围内
                if (point.Y > Math.Min(p1.Y, p2.Y) && point.Y <= Math.Max(p1.Y, p2.Y))
                {
                    // 计算射线与边的交点的X坐标
                    if (Math.Abs(p2.Y - p1.Y) > 1e-10)
                    {
                        double xIntersection = (point.Y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X;

                        // 如果交点在点的右侧，计数加1
                        if (xIntersection >= point.X)
                        {
                            intersectionCount++;
                        }
                    }
                }
            }

            // 如果交点数为奇数，则点在多边形内
            return intersectionCount % 2 == 1;
        }

        /// <summary>
        /// 判断点是否在圆形区域内
        /// </summary>
        /// <param name="point">待判断的点</param>
        /// <param name="circle">圆形区域</param>
        /// <param name="tolerance">容差，默认为 1e-10</param>
        /// <returns>若点在圆形区域内（包括圆周）则返回 true，否则返回 false</returns>
        public static bool IsPointInCircleRegion(PointD point, CircleD circle, double tolerance = 1e-10)
        {
            double distance = Distance(point, circle.Center);
            return distance <= circle.Radius + tolerance;
        }

        #endregion
    }
}
