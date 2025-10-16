using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TulipAlg.Core
{
    public class AlgGeometry
    {
        #region 点-点
        /// <summary>
        /// 点平移指定距离
        /// </summary>
        /// <param name="point"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <returns></returns>
        public static PointD Translate(PointD point, double dx, double dy)
        {
            return new PointD(point.X + dx, point.Y + dy);
        }
        /// <summary>
        /// 点沿参考线平移指定距离
        /// </summary>
        /// <param name="point"></param>
        /// <param name="referenceLine"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
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
        /// 点绕指定中心点旋转指定角度（度数）
        /// </summary>
        /// <param name="point"></param>
        /// <param name="center"></param>
        /// <param name="angleDegrees"></param>
        /// <returns></returns>
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
        public static double Distance(PointD p1, PointD p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }
        /// <summary>
        /// 点在线上的投影点
        /// </summary>
        /// <param name="point"></param>
        /// <param name="line"></param>
        /// <returns></returns>
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
        /// 两点在参考线上的投影距离
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="referenceLine"></param>
        /// <returns></returns>
        public static double Distance(PointD point1, PointD point2, LineD referenceLine)
        {
            PointD proj1 = ProjectPointOntoLine(point1, referenceLine);
            PointD proj2 = ProjectPointOntoLine(point2, referenceLine);
            return Distance(proj1, proj2);
        }

        // 两点中心
        public static PointD MidPoint(PointD p1, PointD p2)
        {
            return new PointD((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        }
        #endregion

        #region 线-线
        /// <summary>
        /// 是否平行
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public static bool AreLinesParallel(LineD line1, LineD line2)
        {
            double dx1 = line1.End.X - line1.Start.X;
            double dy1 = line1.End.Y - line1.Start.Y;
            double dx2 = line2.End.X - line2.Start.X;
            double dy2 = line2.End.Y - line2.Start.Y;
            return Math.Abs(dx1 * dy2 - dy1 * dx2) < 1e-10; // 使用一个小的容差值来判断
        }
        /// <summary>
        /// 线平移指定距离
        /// </summary>
        public static LineD Translate(LineD line, double dx, double dy)
        {
            return new LineD(new PointD(line.Start.X + dx, line.Start.Y + dy), new PointD(line.End.X + dx, line.End.Y + dy));
        }

        /// <summary>
        /// 线平移指定距离
        /// </summary>
        /// <param name="line"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
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
        /// 线平移指定距离，有参考线
        /// </summary>
        /// <param name="line"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
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
        public static LineD Rotate(LineD line, PointD center, double angleDegrees)
        {
            return new LineD(Rotate(line.Start, center, angleDegrees), Rotate(line.End, center, angleDegrees));
        }
        // 交点
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
        public static (PointD? intersection, double angle) IntersectionAndAngle(LineD line1, LineD line2)
        {
            return (Intersection(line1, line2), AngleBetweenLines(line1, line2));
        }

        // 垂直
        public static LineD PerpendicularLine(PointD point, LineD line)
        {
            PointD foot = PerpendicularPoint(point, line);
            return new LineD(point, foot);
        }

        // 是否垂直
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
        public static double Distance(PointD point, LineD line)
        {
            // 计算垂足
            PointD foot = PerpendicularPoint(point, line);
            // 计算距离
            return Distance(point, foot);
        }

        // 是否在直线上

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
        /// 拟合圆
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
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
    }
}
