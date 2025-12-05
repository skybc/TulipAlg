using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TulipAlg.Core
{
    ///// <summary>
    ///// 边缘点结构
    ///// </summary>
    //public struct EdgePoint
    //{
    //    /// <summary>
    //    /// X坐标（亚像素精度）
    //    /// </summary>
    //    public double X { get; set; }

    //    /// <summary>
    //    /// Y坐标（亚像素精度）
    //    /// </summary>
    //    public double Y { get; set; }

    //    public EdgePoint(double x, double y)
    //    {
    //        X = x;
    //        Y = y;
    //    }

    //    public override string ToString()
    //    {
    //        return $"({X:F3}, {Y:F3})";
    //    }
    //}

    /// <summary>
    /// 边缘曲线结构
    /// </summary>
    public class EdgeCurve
    {
        /// <summary>
        /// 边缘点列表
        /// </summary>
        public List<PointD> Points { get; set; } = new List<PointD>();

        /// <summary>
        /// 是否为闭合曲线
        /// </summary>
        public bool IsClosed { get; set; }

        /// <summary>
        /// 曲线长度（点数）
        /// </summary>
        public int Length => Points.Count;

        /// <summary>
        /// 计算曲线的总长度（欧几里得距离）
        /// </summary>
        public double CalculateTotalLength()
        {
            if (Points.Count < 2) return 0.0;

            double totalLength = 0.0;
            for (int i = 0; i < Points.Count - 1; i++)
            {
                double dx = Points[i + 1].X - Points[i].X;
                double dy = Points[i + 1].Y - Points[i].Y;
                totalLength += Math.Sqrt(dx * dx + dy * dy);
            }
            return totalLength;
        }
    }

    /// <summary>
    /// Canny亚像素边缘检测结果
    /// </summary>
    public class CannyEdgeResult
    {
        /// <summary>
        /// 检测到的边缘曲线列表
        /// </summary>
        public List<EdgeCurve> Curves { get; set; } = new List<EdgeCurve>();

        /// <summary>
        /// 总边缘点数
        /// </summary>
        public int TotalPoints { get; set; }

        /// <summary>
        /// 图像宽度
        /// </summary>
        public int ImageWidth { get; set; }

        /// <summary>
        /// 图像高度
        /// </summary>
        public int ImageHeight { get; set; }

        /// <summary>
        /// 曲线数量
        /// </summary>
        public int CurveCount => Curves.Count;

        /// <summary>
        /// 获取所有边缘点（展平的列表）
        /// </summary>
        public List<PointD> GetAllPoints()
        {
            var allPoints = new List<PointD>();
            foreach (var curve in Curves)
            {
                allPoints.AddRange(curve.Points);
            }
            return allPoints;
        }

        /// <summary>
        /// 筛选长度大于指定阈值的曲线
        /// </summary>
        public List<EdgeCurve> FilterByLength(int minLength)
        {
            return Curves.FindAll(c => c.Length >= minLength);
        }

        /// <summary>
        /// 筛选欧几里得长度大于指定阈值的曲线
        /// </summary>
        public List<EdgeCurve> FilterByTotalLength(double minTotalLength)
        {
            return Curves.FindAll(c => c.CalculateTotalLength() >= minTotalLength);
        }
    }

    /// <summary>
    /// Canny/Devernay亚像素边缘检测器
    /// 基于Rafael Grompone von Gioi和Gregory Randall的实现
    /// </summary>
    public class CannySubPixelEdge : IDisposable
    {
        private IntPtr _nativeInstance;
        private bool _disposed = false;

        #region Native Methods

        [DllImport("TulipAlg.CoreExtern.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CannySubPixelEdge_Create();

        [DllImport("TulipAlg.CoreExtern.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CannySubPixelEdge_Destroy(IntPtr instance);

        [DllImport("TulipAlg.CoreExtern.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool CannySubPixelEdge_DetectEdges(
            IntPtr instance,
            double[] image,
            IntPtr mask,
            int width,
            int height,
            double sigma,
            double th_h,
            double th_l,
            out IntPtr x_ptr,
            out IntPtr y_ptr,
            out IntPtr curve_limits_ptr,
            out int totalPoints,
            out int curveCount);

        [DllImport("TulipAlg.CoreExtern.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool CannySubPixelEdge_DetectEdgesFromBytes(
            IntPtr instance,
            IntPtr image,
            IntPtr mask,
            int width,
            int height,
            double sigma,
            double th_h,
            double th_l,
            out IntPtr x_ptr,
            out IntPtr y_ptr,
            out IntPtr curve_limits_ptr,
            out int totalPoints,
            out int curveCount);

        [DllImport("TulipAlg.CoreExtern.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CannySubPixelEdge_FreeResult(
            IntPtr x_ptr,
            IntPtr y_ptr,
            IntPtr curve_limits_ptr);

        [DllImport("TulipAlg.CoreExtern.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CannySubPixelEdge_GetLastError(IntPtr instance);

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public CannySubPixelEdge()
        {
            _nativeInstance = CannySubPixelEdge_Create();
            if (_nativeInstance == IntPtr.Zero)
            {
                throw new Exception("创建 CannySubPixelEdge 本地实例失败");
            }
        }

        /// <summary>
        /// 从double数组检测边缘
        /// </summary>
        /// <param name="image">输入图像数据（double数组，行优先存储：image[x+y*width]）</param>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <param name="sigma">高斯滤波标准差（0表示不滤波）</param>
        /// <param name="th_h">Canny高阈值</param>
        /// <param name="th_l">Canny低阈值</param>
        /// <returns>边缘检测结果</returns>
        public CannyEdgeResult? DetectEdges(double[] image, byte[]? mask, int width, int height,
                            double sigma, double th_h, double th_l)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(CannySubPixelEdge));

            if (image == null || image.Length != width * height)
                throw new ArgumentException("图像数据无效");

            IntPtr x_ptr = IntPtr.Zero;
            IntPtr y_ptr = IntPtr.Zero;
            IntPtr curve_limits_ptr = IntPtr.Zero;

            try
            {
                if (mask == null)
                {
                    bool success = CannySubPixelEdge_DetectEdges(
                        _nativeInstance, image, IntPtr.Zero, width, height, sigma, th_h, th_l,
                        out x_ptr, out y_ptr, out curve_limits_ptr,
                        out int totalPoints, out int curveCount);

                    if (!success)
                    {
                        string? error = GetLastError();
                        throw new Exception($"边缘检测失败：{error}");
                    }

                    return ParseResult(x_ptr, y_ptr, curve_limits_ptr, totalPoints, curveCount, width, height);
                }
                else
                {
                    unsafe
                    {
                        fixed (byte* maskPtr = mask)
                        {
                            IntPtr mptr = (IntPtr)maskPtr;
                            bool success = CannySubPixelEdge_DetectEdges(
                                _nativeInstance, image, mptr, width, height, sigma, th_h, th_l,
                                out x_ptr, out y_ptr, out curve_limits_ptr,
                                out int totalPoints, out int curveCount);

                            if (!success)
                            {
                                string? error = GetLastError();
                                throw new Exception($"边缘检测失败：{error}");
                            }

                            return ParseResult(x_ptr, y_ptr, curve_limits_ptr, totalPoints, curveCount, width, height);
                        }
                    }
                }
            }
            finally
            {
                if (x_ptr != IntPtr.Zero || y_ptr != IntPtr.Zero || curve_limits_ptr != IntPtr.Zero)
                {
                    CannySubPixelEdge_FreeResult(x_ptr, y_ptr, curve_limits_ptr);
                }
            }
        }

        /// <summary>
        /// 从字节数组检测边缘
        /// </summary>
        /// <param name="image">输入图像数据（byte数组，灰度图像）</param>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <param name="sigma">高斯滤波标准差（0表示不滤波）</param>
        /// <param name="th_h">Canny高阈值</param>
        /// <param name="th_l">Canny低阈值</param>
        /// <returns>边缘检测结果</returns>
        public CannyEdgeResult? DetectEdgesFromBytes(Span<byte> image, Span<byte> mask, int width, int height,
                                 double sigma, double th_h, double th_l)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(CannySubPixelEdge));

            if (image == null || image.Length != width * height)
                throw new ArgumentException("图像数据无效");

            IntPtr x_ptr = IntPtr.Zero;
            IntPtr y_ptr = IntPtr.Zero;
            IntPtr curve_limits_ptr = IntPtr.Zero;

            try
            {
                unsafe
                {
                    fixed (byte* imagePtr = image)
                    {
                        if (mask.IsEmpty)
                        {
                            bool success = CannySubPixelEdge_DetectEdgesFromBytes(
                                _nativeInstance, (IntPtr)imagePtr, IntPtr.Zero, width, height, sigma, th_h, th_l,
                                out x_ptr, out y_ptr, out curve_limits_ptr,
                                out int totalPoints, out int curveCount);

                            if (!success)
                            {
                                string? error = GetLastError();
                                throw new Exception($"边缘检测失败：{error}");
                            }

                            return ParseResult(x_ptr, y_ptr, curve_limits_ptr, totalPoints, curveCount, width, height);
                        }
                        else
                        {
                            unsafe
                            {
                                fixed (byte* maskPtr = mask)
                                {
                                    IntPtr mptr = (IntPtr)maskPtr;
                                    bool success = CannySubPixelEdge_DetectEdgesFromBytes(
                                        _nativeInstance, (IntPtr)imagePtr, mptr, width, height, sigma, th_h, th_l,
                                        out x_ptr, out y_ptr, out curve_limits_ptr,
                                        out int totalPoints, out int curveCount);

                                    if (!success)
                                    {
                                        string? error = GetLastError();
                                        throw new Exception($"边缘检测失败：{error}");
                                    }

                                    return ParseResult(x_ptr, y_ptr, curve_limits_ptr, totalPoints, curveCount, width, height);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                if (x_ptr != IntPtr.Zero || y_ptr != IntPtr.Zero || curve_limits_ptr != IntPtr.Zero)
                {
                    CannySubPixelEdge_FreeResult(x_ptr, y_ptr, curve_limits_ptr);
                }
            }
        }

        /// <summary>
        /// 获取最后的错误信息
        /// </summary>
        private string? GetLastError()
        {
            if (_disposed) return null;

            IntPtr errorPtr = CannySubPixelEdge_GetLastError(_nativeInstance);
            if (errorPtr == IntPtr.Zero)
                return null;

            return Marshal.PtrToStringAnsi(errorPtr);
        }

        /// <summary>
        /// 解析Native返回的结果
        /// </summary>
        private CannyEdgeResult ParseResult(IntPtr x_ptr, IntPtr y_ptr, IntPtr curve_limits_ptr,
                                           int totalPoints, int curveCount, int width, int height)
        {
            var result = new CannyEdgeResult
            {
                TotalPoints = totalPoints,
                ImageWidth = width,
                ImageHeight = height
            };

            if (totalPoints == 0 || curveCount == 0)
                return result;

            // 复制坐标数据
            double[] x_array = new double[totalPoints];
            double[] y_array = new double[totalPoints];
            int[] curve_limits = new int[curveCount + 1];

            Marshal.Copy(x_ptr, x_array, 0, totalPoints);
            Marshal.Copy(y_ptr, y_array, 0, totalPoints);
            Marshal.Copy(curve_limits_ptr, curve_limits, 0, curveCount + 1);

            // 构建曲线
            for (int k = 0; k < curveCount; k++)
            {
                var curve = new EdgeCurve();
                int start = curve_limits[k];
                int end = curve_limits[k + 1];

                for (int i = start; i < end; i++)
                {
                    curve.Points.Add(new PointD(x_array[i], y_array[i]));
                }

                // 检查是否为闭合曲线
                if (end > start)
                {
                    var firstPoint = curve.Points[0];
                    var lastPoint = curve.Points[^1];
                    if (Math.Abs(firstPoint.X - lastPoint.X) < 1e-10 &&
                        Math.Abs(firstPoint.Y - lastPoint.Y) < 1e-10)
                    {
                        curve.IsClosed = true;
                    }
                }

                result.Curves.Add(curve);
            }

            return result;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_nativeInstance != IntPtr.Zero)
                {
                    CannySubPixelEdge_Destroy(_nativeInstance);
                    _nativeInstance = IntPtr.Zero;
                }
                _disposed = true;
            }
        }

        ~CannySubPixelEdge()
        {
            Dispose(false);
        }
    }
}
