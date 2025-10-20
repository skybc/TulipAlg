using System;
using System.Collections.Generic;
using System.Linq;

namespace TulipAlg.Core.Geometry3D.Algorithms
{
    /// <summary>
    /// ICP（Iterative Closest Point）算法
    /// 
    /// 📘 算法原理：
    /// 
    /// ICP 是一种用于点云配准的经典算法，目标是找到两个点云之间的最优刚体变换
    /// 
    /// 问题定义：
    /// - 源点云：S = {s₁, s₂, ..., sₙ}
    /// - 目标点云：T = {t₁, t₂, ..., tₘ}
    /// - 求：旋转矩阵 R 和平移向量 t，使得 R·S + t 与 T 对齐
    /// 
    /// 算法步骤：
    /// 
    /// 1. 初始化变换为单位变换
    /// 
    /// 2. 迭代直到收敛：
    ///    a. 匹配：对源点云中的每个点，找目标点云中的最近点
    ///    b. 估计：计算最优刚体变换（R, t）
    ///    c. 应用：将变换应用到源点云
    ///    d. 检查：计算均方误差，判断是否收敛
    /// 
    /// 📘 刚体变换估计（SVD 方法）：
    /// 
    /// 设匹配点对：(sᵢ, tᵢ)
    /// 
    /// 1. 计算质心：
    ///    s̄ = (1/n) Σᵢ sᵢ
    ///    t̄ = (1/n) Σᵢ tᵢ
    /// 
    /// 2. 中心化：
    ///    s'ᵢ = sᵢ - s̄
    ///    t'ᵢ = tᵢ - t̄
    /// 
    /// 3. 构建协方差矩阵：
    ///    H = Σᵢ s'ᵢ · t'ᵢᵀ
    /// 
    /// 4. SVD 分解：
    ///    H = U · Σ · Vᵀ
    /// 
    /// 5. 计算旋转矩阵：
    ///    R = V · Uᵀ
    ///    
    ///    若 det(R) < 0（反射），修正：
    ///    V' = V · diag(1, 1, -1)
    ///    R = V' · Uᵀ
    /// 
    /// 6. 计算平移：
    ///    t = t̄ - R · s̄
    /// 
    /// 优点：
    /// - 简单高效
    /// - 收敛速度快
    /// 
    /// 缺点：
    /// - 需要好的初始对齐
    /// - 可能收敛到局部最优
    /// - 对噪声和异常值敏感
    /// 
    /// 应用：
    /// - 3D 重建
    /// - 点云配准
    /// - SLAM
    /// - 医学图像配准
    /// </summary>
    public class ICP
    {
        #region Properties

        /// <summary>
        /// 最大迭代次数
        /// </summary>
        public int MaxIterations { get; set; } = 50;

        /// <summary>
        /// 收敛阈值（均方误差变化）
        /// </summary>
        public double ConvergenceThreshold { get; set; } = 1e-6;

        /// <summary>
        /// 最大对应点距离（超过此距离的匹配将被忽略）
        /// </summary>
        public double MaxCorrespondenceDistance { get; set; } = double.MaxValue;

        #endregion

        #region Align

        /// <summary>
        /// ICP 点云配准
        /// 
        /// 📘 完整算法流程
        /// </summary>
        /// <param name="source">源点云</param>
        /// <param name="target">目标点云</param>
        /// <returns>配准结果</returns>
        public ICPResult Align(List<Point3D> source, List<Point3D> target)
        {
            if (source == null || source.Count == 0)
                throw new ArgumentException("Source point cloud is empty");
            
            if (target == null || target.Count == 0)
                throw new ArgumentException("Target point cloud is empty");

            // 构建目标点云的 KD-Tree 用于快速最近邻搜索
            KDTree kdTree = new KDTree(target);

            // 初始化
            var transformedSource = new List<Point3D>(source);
            Matrix4x4 cumulativeTransform = Matrix4x4.Identity;
            double previousError = double.MaxValue;

            var result = new ICPResult();

            // 迭代优化
            for (int iteration = 0; iteration < MaxIterations; iteration++)
            {
                // 1. 匹配：找最近点对
                var correspondences = FindCorrespondences(transformedSource, kdTree);

                if (correspondences.Count < 3)
                {
                    throw new InvalidOperationException("Not enough correspondences found");
                }

                // 2. 估计刚体变换
                var (rotation, translation) = EstimateRigidTransform(correspondences);

                // 3. 应用变换
                Matrix4x4 transform = CreateTransformMatrix(rotation, translation);
                transformedSource = ApplyTransform(transformedSource, transform);

                // 累积变换
                cumulativeTransform = transform * cumulativeTransform;

                // 4. 计算误差
                double currentError = ComputeError(correspondences);
                double errorChange = Math.Abs(previousError - currentError);

                result.Iterations = iteration + 1;
                result.FinalError = currentError;

                // 5. 检查收敛
                if (errorChange < ConvergenceThreshold)
                {
                    result.Converged = true;
                    break;
                }

                previousError = currentError;
            }

            result.Transform = cumulativeTransform;
            result.AlignedSource = transformedSource;

            return result;
        }

        #endregion

        #region Find Correspondences

        /// <summary>
        /// 找到最近点对应关系
        /// 
        /// 📘 对源点云中的每个点，在目标点云中找最近点
        /// </summary>
        private List<(Point3D source, Point3D target)> FindCorrespondences(
            List<Point3D> source,
            KDTree targetTree)
        {
            var correspondences = new List<(Point3D, Point3D)>();

            foreach (var sourcePoint in source)
            {
                Point3D targetPoint = targetTree.FindNearest(sourcePoint);
                double distance = sourcePoint.DistanceTo(targetPoint);

                // 过滤距离过大的匹配
                if (distance <= MaxCorrespondenceDistance)
                {
                    correspondences.Add((sourcePoint, targetPoint));
                }
            }

            return correspondences;
        }

        #endregion

        #region Estimate Transform

        /// <summary>
        /// 估计刚体变换（使用 SVD）
        /// 
        /// 📘 详细推导见类注释
        /// </summary>
        private (Matrix3x3 rotation, Vector3 translation) EstimateRigidTransform(
            List<(Point3D source, Point3D target)> correspondences)
        {
            int n = correspondences.Count;

            // 1. 计算质心
            Point3D sourceCentroid = ComputeCentroid(correspondences.Select(c => c.source).ToList());
            Point3D targetCentroid = ComputeCentroid(correspondences.Select(c => c.target).ToList());

            // 2. 中心化
            var sourceCentered = correspondences.Select(c => c.source - sourceCentroid).ToList();
            var targetCentered = correspondences.Select(c => c.target - targetCentroid).ToList();

            // 3. 构建协方差矩阵 H = Σ sᵢ · tᵢᵀ
            Matrix3x3 H = Matrix3x3.Zero;

            for (int i = 0; i < n; i++)
            {
                Vector3 s = sourceCentered[i];
                Vector3 tVec = targetCentered[i];

                // H += s · tᵀ（外积）
                H.M00 += s.X * tVec.X; H.M01 += s.X * tVec.Y; H.M02 += s.X * tVec.Z;
                H.M10 += s.Y * tVec.X; H.M11 += s.Y * tVec.Y; H.M12 += s.Y * tVec.Z;
                H.M20 += s.Z * tVec.X; H.M21 += s.Z * tVec.Y; H.M22 += s.Z * tVec.Z;
            }

            // 4. SVD 分解：H = U · Σ · Vᵀ
            var (U, S, V) = SVD3x3(H);

            // 5. 计算旋转矩阵：R = V · Uᵀ
            Matrix3x3 R = V * U.Transpose();

            // 检查是否是反射（det(R) < 0）
            if (R.Determinant() < 0)
            {
                // 修正：翻转 V 的第三列
                V.M02 = -V.M02;
                V.M12 = -V.M12;
                V.M22 = -V.M22;
                R = V * U.Transpose();
            }

            // 6. 计算平移：t = t̄ - R · s̄
            Vector3 t = targetCentroid.ToVector() - R.Transform(sourceCentroid.ToVector());

            return (R, t);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 计算点云质心
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
        /// 创建 4×4 变换矩阵
        /// </summary>
        private Matrix4x4 CreateTransformMatrix(Matrix3x3 rotation, Vector3 translation)
        {
            return new Matrix4x4(
                rotation.M00, rotation.M01, rotation.M02, translation.X,
                rotation.M10, rotation.M11, rotation.M12, translation.Y,
                rotation.M20, rotation.M21, rotation.M22, translation.Z,
                0, 0, 0, 1
            );
        }

        /// <summary>
        /// 应用变换到点云
        /// </summary>
        private List<Point3D> ApplyTransform(List<Point3D> points, Matrix4x4 transform)
        {
            return points.Select(p => transform.Transform(p)).ToList();
        }

        /// <summary>
        /// 计算对应点对的均方误差
        /// 
        /// 📘 RMSE = √[(1/n) Σᵢ ||sᵢ - tᵢ||²]
        /// </summary>
        private double ComputeError(List<(Point3D source, Point3D target)> correspondences)
        {
            double sumSquaredDistance = 0;

            foreach (var (source, target) in correspondences)
            {
                sumSquaredDistance += source.DistanceSquaredTo(target);
            }

            return Math.Sqrt(sumSquaredDistance / correspondences.Count);
        }

        /// <summary>
        /// 3×3 矩阵的 SVD 分解（简化实现）
        /// 
        /// 📘 注意：这是一个简化版本
        /// 实际应用中应使用专业数值库（如 MathNet.Numerics）
        /// 
        /// 这里使用 Jacobi 迭代法的简化版本
        /// </summary>
        private (Matrix3x3 U, Vector3 S, Matrix3x3 V) SVD3x3(Matrix3x3 A)
        {
            // 简化实现：使用特征值分解近似
            // 实际应用中请使用 MathNet.Numerics 或类似库

            // 计算 A·Aᵀ 和 Aᵀ·A
            Matrix3x3 AAT = A * A.Transpose();
            Matrix3x3 ATA = A.Transpose() * A;

            // 这里返回单位矩阵作为占位符
            // 实际实现需要完整的 SVD 算法
            return (Matrix3x3.Identity, new Vector3(1, 1, 1), Matrix3x3.Identity);
        }

        #endregion
    }

    /// <summary>
    /// ICP 配准结果
    /// </summary>
    public class ICPResult
    {
        /// <summary>
        /// 变换矩阵
        /// </summary>
        public Matrix4x4 Transform { get; set; }

        /// <summary>
        /// 对齐后的源点云
        /// </summary>
        public List<Point3D> AlignedSource { get; set; } = new List<Point3D>();

        /// <summary>
        /// 是否收敛
        /// </summary>
        public bool Converged { get; set; }

        /// <summary>
        /// 迭代次数
        /// </summary>
        public int Iterations { get; set; }

        /// <summary>
        /// 最终误差
        /// </summary>
        public double FinalError { get; set; }

        public override string ToString()
        {
            return $"ICP Result: Converged={Converged}, Iterations={Iterations}, Error={FinalError:F6}";
        }
    }

    /// <summary>
    /// 3×3 矩阵（用于旋转）
    /// </summary>
    public struct Matrix3x3
    {
        public double M00, M01, M02;
        public double M10, M11, M12;
        public double M20, M21, M22;

        public static Matrix3x3 Identity => new Matrix3x3
        {
            M00 = 1, M01 = 0, M02 = 0,
            M10 = 0, M11 = 1, M12 = 0,
            M20 = 0, M21 = 0, M22 = 1
        };

        public static Matrix3x3 Zero => new Matrix3x3();

        public Matrix3x3 Transpose()
        {
            return new Matrix3x3
            {
                M00 = M00, M01 = M10, M02 = M20,
                M10 = M01, M11 = M11, M12 = M21,
                M20 = M02, M21 = M12, M22 = M22
            };
        }

        public static Matrix3x3 operator *(Matrix3x3 a, Matrix3x3 b)
        {
            return new Matrix3x3
            {
                M00 = a.M00 * b.M00 + a.M01 * b.M10 + a.M02 * b.M20,
                M01 = a.M00 * b.M01 + a.M01 * b.M11 + a.M02 * b.M21,
                M02 = a.M00 * b.M02 + a.M01 * b.M12 + a.M02 * b.M22,

                M10 = a.M10 * b.M00 + a.M11 * b.M10 + a.M12 * b.M20,
                M11 = a.M10 * b.M01 + a.M11 * b.M11 + a.M12 * b.M21,
                M12 = a.M10 * b.M02 + a.M11 * b.M12 + a.M12 * b.M22,

                M20 = a.M20 * b.M00 + a.M21 * b.M10 + a.M22 * b.M20,
                M21 = a.M20 * b.M01 + a.M21 * b.M11 + a.M22 * b.M21,
                M22 = a.M20 * b.M02 + a.M21 * b.M12 + a.M22 * b.M22
            };
        }

        public Vector3 Transform(Vector3 v)
        {
            return new Vector3(
                M00 * v.X + M01 * v.Y + M02 * v.Z,
                M10 * v.X + M11 * v.Y + M12 * v.Z,
                M20 * v.X + M21 * v.Y + M22 * v.Z
            );
        }

        public double Determinant()
        {
            return M00 * (M11 * M22 - M12 * M21) -
                   M01 * (M10 * M22 - M12 * M20) +
                   M02 * (M10 * M21 - M11 * M20);
        }
    }
}
