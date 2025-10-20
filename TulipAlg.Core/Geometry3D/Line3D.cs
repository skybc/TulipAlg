using System;

namespace TulipAlg.Core.Geometry3D
{
    /// <summary>
    /// 三维空间中的直线
    /// Represents a line in 3D space using parametric form.
    /// 
    /// 参数方程：L(t) = P + t·d
    /// 其中：
    /// - P 是直线上的一点（Point）
    /// - d 是方向向量（Direction）
    /// - t ∈ ℝ 是参数
    /// </summary>
    public struct Line3D
    {
        #region Properties

        /// <summary>
        /// 直线上的一点
        /// </summary>
        public Point3D Point { get; set; }

        /// <summary>
        /// 方向向量（通常为单位向量）
        /// </summary>
        public Vector3 Direction { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 通过点和方向向量构造直线
        /// </summary>
        public Line3D(Point3D point, Vector3 direction)
        {
            Point = point;
            Direction = direction.Normalize(); // 归一化方向向量
        }

        /// <summary>
        /// 通过两点构造直线
        /// </summary>
        public static Line3D FromTwoPoints(Point3D p1, Point3D p2)
        {
            Vector3 direction = (p2 - p1).Normalize();
            return new Line3D(p1, direction);
        }

        #endregion

        #region Point on Line

        /// <summary>
        /// 获取直线上参数为 t 的点
        /// 
        /// 公式：P(t) = P₀ + t·d
        /// </summary>
        public Point3D GetPoint(double t)
        {
            return Point + t * Direction;
        }

        /// <summary>
        /// 判断点是否在直线上
        /// 
        /// 方法：计算点到直线的距离，若距离接近 0 则在直线上
        /// </summary>
        public bool ContainsPoint(Point3D p, double epsilon = 1e-6)
        {
            return DistanceToPoint(p) < epsilon;
        }

        /// <summary>
        /// 获取点在直线上的投影参数 t
        /// 
        /// 公式：t = (P - P₀) · d
        /// </summary>
        public double GetParameterForPoint(Point3D p)
        {
            Vector3 v = p - Point;
            return v.Dot(Direction);
        }

        #endregion

        #region Distance Calculations

        /// <summary>
        /// 计算点到直线的距离
        /// 
        /// 📘 数学原理：
        /// 设直线为 L: P₀ + t·d，点为 Q
        /// 
        /// 方法 1（向量法）：
        /// 距离 = ||(Q - P₀) × d|| / ||d||
        /// 
        /// 方法 2（投影法）：
        /// 1. 计算 Q 在直线上的投影点 P'
        /// 2. 距离 = ||Q - P'||
        /// 
        /// 这里使用方法 1，因为不需要计算投影点
        /// </summary>
        public double DistanceToPoint(Point3D p)
        {
            Vector3 v = p - Point;
            Vector3 cross = v.Cross(Direction);
            return cross.Length();
        }

        /// <summary>
        /// 获取点在直线上的最近点
        /// 
        /// 📘 数学原理：
        /// 投影公式：P' = P₀ + [(Q - P₀) · d] d
        /// </summary>
        public Point3D ClosestPointTo(Point3D p)
        {
            double t = GetParameterForPoint(p);
            return GetPoint(t);
        }

        /// <summary>
        /// 计算两条直线的最短距离
        /// 
        /// 📘 数学原理：
        /// 设两直线：
        /// L₁: P₁ + s·d₁
        /// L₂: P₂ + t·d₂
        /// 
        /// 情况 1：平行或重合
        /// 若 d₁ × d₂ = 0，则平行
        /// 距离 = ||(P₂ - P₁) × d₁|| / ||d₁||
        /// 
        /// 情况 2：异面直线
        /// 距离 = |(P₂ - P₁) · (d₁ × d₂)| / ||d₁ × d₂||
        /// </summary>
        public double DistanceToLine(Line3D other)
        {
            Vector3 cross = Direction.Cross(other.Direction);
            double crossLength = cross.Length();

            // 平行或重合的情况
            if (crossLength < 1e-10)
            {
                return DistanceToPoint(other.Point);
            }

            // 异面直线
            Vector3 w = Point - other.Point;
            return Math.Abs(w.Dot(cross)) / crossLength;
        }

        /// <summary>
        /// 计算两条直线的最近点对
        /// 
        /// 📘 数学原理：
        /// 设：
        /// L₁: P₁ + s·d₁
        /// L₂: P₂ + t·d₂
        /// w = P₁ - P₂
        /// 
        /// 最近点参数：
        /// s = [(d₁·d₂)(d₂·w) - (d₂·d₂)(d₁·w)] / [(d₁·d₁)(d₂·d₂) - (d₁·d₂)²]
        /// t = [(d₁·d₁)(d₂·w) - (d₁·d₂)(d₁·w)] / [(d₁·d₁)(d₂·d₂) - (d₁·d₂)²]
        /// </summary>
        public (Point3D pointOnThis, Point3D pointOnOther) ClosestPointsTo(Line3D other)
        {
            Vector3 w = Point - other.Point;
            double a = Direction.Dot(Direction);
            double b = Direction.Dot(other.Direction);
            double c = other.Direction.Dot(other.Direction);
            double d = Direction.Dot(w);
            double e = other.Direction.Dot(w);

            double denominator = a * c - b * b;

            // 平行情况
            if (Math.Abs(denominator) < 1e-10)
            {
                double t1 = 0;
                double t2 = GetParameterForPoint(other.Point);
                return (GetPoint(t1), other.GetPoint(t2));
            }

            // 一般情况
            double s = (b * e - c * d) / denominator;
            double t = (a * e - b * d) / denominator;

            return (GetPoint(s), other.GetPoint(t));
        }

        #endregion

        #region Intersection

        /// <summary>
        /// 判断两条直线是否相交
        /// </summary>
        public bool Intersects(Line3D other, out Point3D intersection, double epsilon = 1e-6)
        {
            var (p1, p2) = ClosestPointsTo(other);
            
            if (p1.DistanceTo(p2) < epsilon)
            {
                intersection = Point3D.Lerp(p1, p2, 0.5);
                return true;
            }

            intersection = Point3D.Origin;
            return false;
        }

        #endregion

        #region String Representation

        public override string ToString()
        {
            return $"Line3D(Point: {Point}, Direction: {Direction})";
        }

        #endregion
    }
}
