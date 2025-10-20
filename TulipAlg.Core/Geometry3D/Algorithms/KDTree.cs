using System;
using System.Collections.Generic;
using System.Linq;

namespace TulipAlg.Core.Geometry3D.Algorithms
{
    /// <summary>
    /// KD树（K-Dimensional Tree）数据结构
    /// 
    /// 📘 算法原理：
    /// 
    /// KD树是一种用于组织k维空间中点的数据结构，常用于：
    /// - 最近邻搜索（Nearest Neighbor Search）
    /// - 范围查询（Range Query）
    /// - 点云处理
    /// 
    /// 结构：
    /// - 二叉树
    /// - 每个节点表示一个k维超矩形区域
    /// - 非叶节点包含一个分割轴和分割值
    /// - 左子树包含分割轴上值较小的点
    /// - 右子树包含分割轴上值较大的点
    /// 
    /// 构建算法：
    /// 1. 选择分割轴（通常轮流使用各维度，或选择方差最大的维度）
    /// 2. 找到该轴上的中位数
    /// 3. 递归构建左右子树
    /// 
    /// 时间复杂度：
    /// - 构建：O(n log n)
    /// - 查询：平均 O(log n)，最坏 O(n)
    /// 
    /// 空间复杂度：O(n)
    /// </summary>
    public class KDTree
    {
        #region Node Class

        /// <summary>
        /// KD树节点
        /// </summary>
        private class KDNode
        {
            public Point3D Point { get; set; }
            public int Axis { get; set; }  // 分割轴：0=X, 1=Y, 2=Z
            public KDNode? Left { get; set; }
            public KDNode? Right { get; set; }
        }

        #endregion

        #region Fields

        private KDNode? _root;
        private int _count;

        #endregion

        #region Properties

        /// <summary>
        /// 树中点的数量
        /// </summary>
        public int Count => _count;

        #endregion

        #region Constructor

        /// <summary>
        /// 从点集构建KD树
        /// 
        /// 📘 构建策略：
        /// 轮流使用 X, Y, Z 轴作为分割轴
        /// </summary>
        public KDTree(IEnumerable<Point3D> points)
        {
            var pointList = points.ToList();
            _count = pointList.Count;
            _root = BuildTree(pointList, 0);
        }

        #endregion

        #region Build Tree

        /// <summary>
        /// 递归构建KD树
        /// 
        /// 📘 算法步骤：
        /// 
        /// 1. 如果点集为空，返回 null
        /// 2. 选择分割轴（depth % 3）
        /// 3. 按分割轴排序点集
        /// 4. 选择中位数作为当前节点
        /// 5. 递归构建左子树（小于中位数）
        /// 6. 递归构建右子树（大于中位数）
        /// </summary>
        private KDNode? BuildTree(List<Point3D> points, int depth)
        {
            if (points.Count == 0)
                return null;

            int axis = depth % 3;

            // 按当前轴排序
            points.Sort((p1, p2) => CompareByAxis(p1, p2, axis));

            // 选择中位数
            int medianIndex = points.Count / 2;
            Point3D medianPoint = points[medianIndex];

            // 分割左右子集
            var leftPoints = points.Take(medianIndex).ToList();
            var rightPoints = points.Skip(medianIndex + 1).ToList();

            // 创建节点
            return new KDNode
            {
                Point = medianPoint,
                Axis = axis,
                Left = BuildTree(leftPoints, depth + 1),
                Right = BuildTree(rightPoints, depth + 1)
            };
        }

        /// <summary>
        /// 按指定轴比较两点
        /// </summary>
        private int CompareByAxis(Point3D p1, Point3D p2, int axis)
        {
            double val1 = GetAxisValue(p1, axis);
            double val2 = GetAxisValue(p2, axis);
            return val1.CompareTo(val2);
        }

        /// <summary>
        /// 获取点在指定轴上的坐标值
        /// </summary>
        private double GetAxisValue(Point3D point, int axis)
        {
            return axis switch
            {
                0 => point.X,
                1 => point.Y,
                2 => point.Z,
                _ => throw new ArgumentException("Invalid axis")
            };
        }

        #endregion

        #region Nearest Neighbor Search

        /// <summary>
        /// 查找最近邻点
        /// 
        /// 📘 算法原理：
        /// 
        /// 使用深度优先搜索（DFS）+ 剪枝策略
        /// 
        /// 步骤：
        /// 1. 从根节点开始递归搜索
        /// 2. 每次选择目标点所在的半空间优先搜索
        /// 3. 更新当前最近点和最小距离
        /// 4. 回溯时检查另一半空间是否可能包含更近的点
        ///    - 条件：到分割超平面的距离 < 当前最小距离
        /// 5. 递归搜索另一半空间
        /// 
        /// 📘 剪枝策略：
        /// 
        /// 如果目标点到分割超平面的距离 > 当前最小距离，
        /// 则另一半空间不可能包含更近的点，可以剪枝
        /// 
        /// 伪代码：
        /// ```
        /// function nearestNeighbor(node, target, best):
        ///     if node is null:
        ///         return best
        ///     
        ///     // 更新最近点
        ///     if distance(node.point, target) < distance(best, target):
        ///         best = node.point
        ///     
        ///     // 确定搜索顺序
        ///     if target[axis] < node.point[axis]:
        ///         near, far = node.left, node.right
        ///     else:
        ///         near, far = node.right, node.left
        ///     
        ///     // 搜索近侧
        ///     best = nearestNeighbor(near, target, best)
        ///     
        ///     // 检查是否需要搜索远侧
        ///     if |target[axis] - node.point[axis]| < distance(best, target):
        ///         best = nearestNeighbor(far, target, best)
        ///     
        ///     return best
        /// ```
        /// </summary>
        public Point3D FindNearest(Point3D target)
        {
            if (_root == null)
                throw new InvalidOperationException("KD-Tree is empty");

            var best = new BestPoint { Point = _root.Point, Distance = double.MaxValue };
            FindNearestRecursive(_root, target, best);
            return best.Point;
        }

        /// <summary>
        /// 查找 K 个最近邻点
        /// 
        /// 使用优先队列维护 K 个最近点
        /// </summary>
        public List<Point3D> FindKNearest(Point3D target, int k)
        {
            if (_root == null)
                throw new InvalidOperationException("KD-Tree is empty");

            if (k <= 0)
                throw new ArgumentException("k must be positive");

            var kBest = new SortedSet<(double distance, Point3D point)>(
                Comparer<(double, Point3D)>.Create((a, b) =>
                {
                    int cmp = a.Item1.CompareTo(b.Item1);
                    if (cmp == 0)
                    {
                        // 距离相同时，比较坐标确保唯一性
                        cmp = a.Item2.X.CompareTo(b.Item2.X);
                        if (cmp == 0)
                        {
                            cmp = a.Item2.Y.CompareTo(b.Item2.Y);
                            if (cmp == 0)
                            {
                                cmp = a.Item2.Z.CompareTo(b.Item2.Z);
                            }
                        }
                    }
                    return cmp;
                })
            );

            FindKNearestRecursive(_root, target, k, kBest);

            return kBest.Select(x => x.point).ToList();
        }

        /// <summary>
        /// 最近邻搜索的辅助类
        /// </summary>
        private class BestPoint
        {
            public Point3D Point { get; set; }
            public double Distance { get; set; }
        }

        /// <summary>
        /// 递归查找最近邻
        /// </summary>
        private void FindNearestRecursive(KDNode? node, Point3D target, BestPoint best)
        {
            if (node == null)
                return;

            // 计算当前节点到目标点的距离
            double distance = node.Point.DistanceTo(target);

            // 更新最佳点
            if (distance < best.Distance)
            {
                best.Point = node.Point;
                best.Distance = distance;
            }

            // 确定搜索顺序
            double targetAxisValue = GetAxisValue(target, node.Axis);
            double nodeAxisValue = GetAxisValue(node.Point, node.Axis);
            double axisDiff = targetAxisValue - nodeAxisValue;

            KDNode? nearNode = axisDiff < 0 ? node.Left : node.Right;
            KDNode? farNode = axisDiff < 0 ? node.Right : node.Left;

            // 先搜索近侧
            FindNearestRecursive(nearNode, target, best);

            // 检查是否需要搜索远侧（剪枝条件）
            if (Math.Abs(axisDiff) < best.Distance)
            {
                FindNearestRecursive(farNode, target, best);
            }
        }

        /// <summary>
        /// 递归查找 K 个最近邻
        /// </summary>
        private void FindKNearestRecursive(
            KDNode? node,
            Point3D target,
            int k,
            SortedSet<(double distance, Point3D point)> kBest)
        {
            if (node == null)
                return;

            // 计算距离
            double distance = node.Point.DistanceTo(target);

            // 更新 K 最近点集合
            if (kBest.Count < k)
            {
                kBest.Add((distance, node.Point));
            }
            else if (distance < kBest.Max.distance)
            {
                kBest.Remove(kBest.Max);
                kBest.Add((distance, node.Point));
            }

            // 确定搜索顺序
            double targetAxisValue = GetAxisValue(target, node.Axis);
            double nodeAxisValue = GetAxisValue(node.Point, node.Axis);
            double axisDiff = targetAxisValue - nodeAxisValue;

            KDNode? nearNode = axisDiff < 0 ? node.Left : node.Right;
            KDNode? farNode = axisDiff < 0 ? node.Right : node.Left;

            // 先搜索近侧
            FindKNearestRecursive(nearNode, target, k, kBest);

            // 检查是否需要搜索远侧
            double maxDistance = kBest.Count < k ? double.MaxValue : kBest.Max.distance;
            if (Math.Abs(axisDiff) < maxDistance)
            {
                FindKNearestRecursive(farNode, target, k, kBest);
            }
        }

        #endregion

        #region Range Query

        /// <summary>
        /// 范围查询：查找包围盒内的所有点
        /// 
        /// 📘 算法：递归遍历，剪枝不相交的子树
        /// </summary>
        public List<Point3D> RangeQuery(BoundingBox3D bounds)
        {
            var result = new List<Point3D>();
            RangeQueryRecursive(_root, bounds, result);
            return result;
        }

        /// <summary>
        /// 递归范围查询
        /// </summary>
        private void RangeQueryRecursive(KDNode? node, BoundingBox3D bounds, List<Point3D> result)
        {
            if (node == null)
                return;

            // 检查当前点是否在范围内
            if (bounds.Contains(node.Point))
            {
                result.Add(node.Point);
            }

            // 检查左子树
            double minValue = GetAxisValue(bounds.Min, node.Axis);
            double nodeValue = GetAxisValue(node.Point, node.Axis);
            if (minValue <= nodeValue)
            {
                RangeQueryRecursive(node.Left, bounds, result);
            }

            // 检查右子树
            double maxValue = GetAxisValue(bounds.Max, node.Axis);
            if (maxValue >= nodeValue)
            {
                RangeQueryRecursive(node.Right, bounds, result);
            }
        }

        #endregion
    }
}
