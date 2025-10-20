using System;

namespace TulipAlg.Core.Geometry3D
{
    /// <summary>
    /// 三维空间中的三角形
    /// Represents a triangle in 3D space defined by three vertices.
    /// 
    /// 📘 三角形性质：
    /// - 由三个不共线的顶点定义
    /// - 确定一个唯一平面
    /// - 可用于网格建模、碰撞检测、光线追踪等
    /// </summary>
    public struct Triangle3D
    {
        #region Properties

        /// <summary>
        /// 第一个顶点
        /// </summary>
        public Point3D V0 { get; set; }

        /// <summary>
        /// 第二个顶点
        /// </summary>
        public Point3D V1 { get; set; }

        /// <summary>
        /// 第三个顶点
        /// </summary>
        public Point3D V2 { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 通过三个顶点构造三角形
        /// </summary>
        public Triangle3D(Point3D v0, Point3D v1, Point3D v2)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
        }

        #endregion

        #region Basic Properties

        /// <summary>
        /// 计算三角形法向量（单位向量）
        /// 
        /// 📘 公式：n = (V₁ - V₀) × (V₂ - V₀)
        /// 
        /// 方向遵循右手定则：
        /// 从 V₀ 看向 V₁，再向 V₂ 转，法向量指向观察者
        /// </summary>
        public Vector3 Normal()
        {
            Vector3 edge1 = V1 - V0;
            Vector3 edge2 = V2 - V0;
            return edge1.Cross(edge2).Normalize();
        }

        /// <summary>
        /// 计算三角形面积
        /// 
        /// 📘 公式：A = ½ ||(V₁ - V₀) × (V₂ - V₀)||
        /// 
        /// 叉积的模等于平行四边形面积，三角形是其一半
        /// </summary>
        public double Area()
        {
            Vector3 edge1 = V1 - V0;
            Vector3 edge2 = V2 - V0;
            return 0.5 * edge1.Cross(edge2).Length();
        }

        /// <summary>
        /// 计算三角形重心（质心）
        /// 
        /// 📘 公式：G = (V₀ + V₁ + V₂) / 3
        /// 
        /// 重心是三角形三条中线的交点
        /// </summary>
        public Point3D Centroid()
        {
            return new Point3D(
                (V0.X + V1.X + V2.X) / 3.0,
                (V0.Y + V1.Y + V2.Y) / 3.0,
                (V0.Z + V1.Z + V2.Z) / 3.0
            );
        }

        /// <summary>
        /// 获取三角形所在平面
        /// </summary>
        public Plane3D GetPlane()
        {
            return Plane3D.FromThreePoints(V0, V1, V2);
        }

        #endregion

        #region Barycentric Coordinates

        /// <summary>
        /// 计算点的重心坐标
        /// 
        /// 📘 重心坐标系统：
        /// 
        /// 平面上任意点 P 可表示为：
        /// P = u·V₀ + v·V₁ + w·V₂
        /// 其中 u + v + w = 1
        /// 
        /// 点在三角形内部 ⟺ u,v,w ∈ [0,1]
        /// 
        /// 计算方法（面积法）：
        /// 设三角形总面积为 A
        /// u = Area(P,V₁,V₂) / A
        /// v = Area(V₀,P,V₂) / A
        /// w = Area(V₀,V₁,P) / A
        /// 
        /// 或使用向量法：
        /// 设 v₀ = V₂ - V₀, v₁ = V₁ - V₀, v₂ = P - V₀
        /// 
        /// dot00 = v₀·v₀
        /// dot01 = v₀·v₁
        /// dot02 = v₀·v₂
        /// dot11 = v₁·v₁
        /// dot12 = v₁·v₂
        /// 
        /// invDenom = 1 / (dot00·dot11 - dot01·dot01)
        /// u = (dot11·dot02 - dot01·dot12) · invDenom
        /// v = (dot00·dot12 - dot01·dot02) · invDenom
        /// w = 1 - u - v
        /// </summary>
        public (double u, double v, double w) BarycentricCoordinates(Point3D p)
        {
            Vector3 v0 = V2 - V0;
            Vector3 v1 = V1 - V0;
            Vector3 v2 = p - V0;

            double dot00 = v0.Dot(v0);
            double dot01 = v0.Dot(v1);
            double dot02 = v0.Dot(v2);
            double dot11 = v1.Dot(v1);
            double dot12 = v1.Dot(v2);

            double invDenom = 1.0 / (dot00 * dot11 - dot01 * dot01);
            double u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            double v = (dot00 * dot12 - dot01 * dot02) * invDenom;
            double w = 1.0 - u - v;

            return (w, v, u); // 注意顺序对应 V0, V1, V2
        }

        /// <summary>
        /// 判断点是否在三角形内部（包括边界）
        /// 
        /// 使用重心坐标判定
        /// </summary>
        public bool ContainsPoint(Point3D p, double epsilon = 1e-6)
        {
            // 首先检查点是否在三角形平面上
            Plane3D plane = GetPlane();
            if (plane.DistanceToPoint(p) > epsilon)
                return false;

            var (u, v, w) = BarycentricCoordinates(p);
            
            return u >= -epsilon && v >= -epsilon && w >= -epsilon &&
                   u <= 1 + epsilon && v <= 1 + epsilon && w <= 1 + epsilon;
        }

        #endregion

        #region Distance Calculations

        /// <summary>
        /// 计算点到三角形的最短距离
        /// 
        /// 📘 算法步骤：
        /// 
        /// 1. 将点投影到三角形所在平面
        /// 2. 判断投影点是否在三角形内
        ///    - 若在内部：距离 = 点到平面距离
        ///    - 若在外部：距离 = 点到三角形边界的最短距离
        /// 3. 边界距离 = min(到三条边的距离)
        /// </summary>
        public double DistanceToPoint(Point3D p)
        {
            // 投影到平面
            Plane3D plane = GetPlane();
            Point3D projection = plane.ProjectPoint(p);

            // 检查投影点是否在三角形内
            if (ContainsPoint(projection))
            {
                return plane.DistanceToPoint(p);
            }

            // 投影在外部，计算到三条边的最短距离
            Line3D edge0 = new Line3D(V0, (V1 - V0).Normalize());
            Line3D edge1 = new Line3D(V1, (V2 - V1).Normalize());
            Line3D edge2 = new Line3D(V2, (V0 - V2).Normalize());

            double dist0 = DistanceToEdge(p, V0, V1);
            double dist1 = DistanceToEdge(p, V1, V2);
            double dist2 = DistanceToEdge(p, V2, V0);

            return Math.Min(Math.Min(dist0, dist1), dist2);
        }

        /// <summary>
        /// 计算点到线段的距离（辅助方法）
        /// </summary>
        private double DistanceToEdge(Point3D p, Point3D edgeStart, Point3D edgeEnd)
        {
            Vector3 edge = edgeEnd - edgeStart;
            Vector3 toPoint = p - edgeStart;
            
            double edgeLengthSquared = edge.LengthSquared();
            if (edgeLengthSquared < 1e-10)
                return p.DistanceTo(edgeStart);

            // 参数 t 的范围限制在 [0, 1]，确保在线段上
            double t = Math.Clamp(toPoint.Dot(edge) / edgeLengthSquared, 0.0, 1.0);
            Point3D closestPoint = edgeStart + t * edge;
            
            return p.DistanceTo(closestPoint);
        }

        /// <summary>
        /// 获取点在三角形上的最近点
        /// </summary>
        public Point3D ClosestPointTo(Point3D p)
        {
            // 投影到平面
            Plane3D plane = GetPlane();
            Point3D projection = plane.ProjectPoint(p);

            // 检查投影点是否在三角形内
            if (ContainsPoint(projection))
            {
                return projection;
            }

            // 投影在外部，找到三条边上的最近点
            Point3D closest0 = ClosestPointOnEdge(p, V0, V1);
            Point3D closest1 = ClosestPointOnEdge(p, V1, V2);
            Point3D closest2 = ClosestPointOnEdge(p, V2, V0);

            double dist0 = p.DistanceTo(closest0);
            double dist1 = p.DistanceTo(closest1);
            double dist2 = p.DistanceTo(closest2);

            if (dist0 <= dist1 && dist0 <= dist2)
                return closest0;
            else if (dist1 <= dist2)
                return closest1;
            else
                return closest2;
        }

        /// <summary>
        /// 获取点在线段上的最近点（辅助方法）
        /// </summary>
        private Point3D ClosestPointOnEdge(Point3D p, Point3D edgeStart, Point3D edgeEnd)
        {
            Vector3 edge = edgeEnd - edgeStart;
            Vector3 toPoint = p - edgeStart;
            
            double edgeLengthSquared = edge.LengthSquared();
            if (edgeLengthSquared < 1e-10)
                return edgeStart;

            double t = Math.Clamp(toPoint.Dot(edge) / edgeLengthSquared, 0.0, 1.0);
            return edgeStart + t * edge;
        }

        #endregion

        #region Ray Intersection (Möller-Trumbore Algorithm)

        /// <summary>
        /// 光线与三角形相交检测（Möller-Trumbore 算法）
        /// 
        /// 📘 数学原理：
        /// 
        /// 光线：R(t) = O + t·D  (t ≥ 0)
        /// 三角形：P(u,v) = (1-u-v)V₀ + uV₁ + vV₂
        /// 
        /// 相交条件：R(t) = P(u,v)
        /// 即：O + t·D = (1-u-v)V₀ + uV₁ + vV₂
        /// 
        /// 整理成矩阵形式：
        /// [-D, V₁-V₀, V₂-V₀] [t]   [O - V₀]
        ///                     [u] = 
        ///                     [v]
        /// 
        /// 使用 Cramer 法则求解：
        /// 
        /// E₁ = V₁ - V₀
        /// E₂ = V₂ - V₀
        /// T = O - V₀
        /// P = D × E₂
        /// Q = T × E₁
        /// 
        /// det = E₁ · P
        /// 
        /// t = (E₂ · Q) / det
        /// u = (T · P) / det
        /// v = (D · Q) / det
        /// 
        /// 相交条件：
        /// 1. u ≥ 0, v ≥ 0, u + v ≤ 1  (在三角形内)
        /// 2. t ≥ 0  (光线正方向)
        /// </summary>
        /// <param name="rayOrigin">光线起点</param>
        /// <param name="rayDirection">光线方向（应为单位向量）</param>
        /// <param name="intersection">交点</param>
        /// <param name="t">光线参数 t</param>
        /// <param name="u">重心坐标 u</param>
        /// <param name="v">重心坐标 v</param>
        /// <returns>是否相交</returns>
        public bool RayIntersection(
            Point3D rayOrigin,
            Vector3 rayDirection,
            out Point3D intersection,
            out double t,
            out double u,
            out double v)
        {
            const double epsilon = 1e-10;

            Vector3 edge1 = V1 - V0;
            Vector3 edge2 = V2 - V0;

            Vector3 h = rayDirection.Cross(edge2);
            double det = edge1.Dot(h);

            // 光线平行于三角形
            if (Math.Abs(det) < epsilon)
            {
                intersection = Point3D.Origin;
                t = u = v = 0;
                return false;
            }

            double invDet = 1.0 / det;
            Vector3 s = rayOrigin - V0;
            u = s.Dot(h) * invDet;

            // u 在 [0, 1] 范围外
            if (u < 0.0 || u > 1.0)
            {
                intersection = Point3D.Origin;
                t = v = 0;
                return false;
            }

            Vector3 q = s.Cross(edge1);
            v = rayDirection.Dot(q) * invDet;

            // v 或 u+v 在有效范围外
            if (v < 0.0 || u + v > 1.0)
            {
                intersection = Point3D.Origin;
                t = 0;
                return false;
            }

            t = edge2.Dot(q) * invDet;

            // t < 0 表示交点在光线反方向
            if (t < epsilon)
            {
                intersection = Point3D.Origin;
                return false;
            }

            // 计算交点
            intersection = rayOrigin + t * rayDirection;
            return true;
        }

        #endregion

        #region String Representation

        public override string ToString()
        {
            return $"Triangle3D(V0: {V0}, V1: {V1}, V2: {V2})";
        }

        #endregion
    }
}
