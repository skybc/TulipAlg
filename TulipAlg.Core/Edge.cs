using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;

namespace TulipAlg.Core
{
    /// <summary>
    /// 方向筛选Canny边缘检测结果
    /// </summary>
    public class DirectionalCannyResult
    {
        /// <summary>
        /// 原始Canny边缘图像
        /// </summary>
        public Mat? CannyEdges { get; set; }

        /// <summary>
        /// 方向筛选后的边缘图像
        /// </summary>
        public Mat? FilteredEdges { get; set; }

        /// <summary>
        /// 梯度方向图像（角度，单位：度）
        /// </summary>
        public Mat? GradientDirection { get; set; }

        /// <summary>
        /// 梯度幅值图像
        /// </summary>
        public Mat? GradientMagnitude { get; set; }

        /// <summary>
        /// 亚像素边缘点列表
        /// </summary>
        public List<Point2f> SubPixelEdges { get; set; } = new List<Point2f>();

        /// <summary>
        /// 筛选后的亚像素边缘点列表
        /// </summary>
        public List<Point2f> FilteredSubPixelEdges { get; set; } = new List<Point2f>();

        /// <summary>
        /// 边缘点总数
        /// </summary>
        public int TotalEdgePoints { get; set; }

        /// <summary>
        /// 筛选后的边缘点数
        /// </summary>
        public int FilteredEdgePoints { get; set; }

        /// <summary>
        /// 筛选的中心角度（度）
        /// </summary>
        public double TargetAngle { get; set; }

        /// <summary>
        /// 角度容差范围（±度）
        /// </summary>
        public double AngleTolerance { get; set; }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            CannyEdges?.Dispose();
            FilteredEdges?.Dispose();
            GradientDirection?.Dispose();
            GradientMagnitude?.Dispose();
        }
    }

    /// <summary>
    /// 方向筛选Canny边缘检测器
    /// 支持按指定角度范围筛选边缘，并提供亚像素精度定位
    /// 角度定义：0°为水平向右，逆时针增加（标准数学定义）
    /// </summary>
    public class DirectionalCannyEdge
    {
        /// <summary>
        /// Canny边缘检测（带方向筛选）
        /// </summary>
        /// <param name="image">输入图像（灰度图）</param>
        /// <param name="lowThreshold">Canny低阈值</param>
        /// <param name="highThreshold">Canny高阈值</param>
        /// <param name="targetAngle">目标角度（度，0°为水平向右，逆时针增加）</param>
        /// <param name="angleTolerance">角度容差（±度）</param>
        /// <param name="apertureSize">Sobel算子孔径大小（3, 5, 7）</param>
        /// <param name="useSubPixel">是否使用亚像素精度定位</param>
        /// <param name="subPixelWinSize">亚像素搜索窗口大小</param>
        /// <returns>检测结果</returns>
        public DirectionalCannyResult DetectEdges(
            Mat image,
            double lowThreshold,
            double highThreshold,
            double targetAngle,
            double angleTolerance,
            int apertureSize = 3,
            bool useSubPixel = true,
            int subPixelWinSize = 5)
        {
            if (image == null || image.Empty())
            {
                throw new ArgumentException("输入图像无效", nameof(image));
            }

            if (image.Channels() != 1)
            {
                throw new ArgumentException("输入图像必须是灰度图", nameof(image));
            }

            var result = new DirectionalCannyResult
            {
                TargetAngle = targetAngle,
                AngleTolerance = angleTolerance
            };

            // 1. 计算梯度
            using var gradX = new Mat();
            using var gradY = new Mat();
            Cv2.Sobel(image, gradX, MatType.CV_32F, 1, 0, apertureSize);
            Cv2.Sobel(image, gradY, MatType.CV_32F, 0, 1, apertureSize);

            // 2. 计算梯度幅值和方向
            result.GradientMagnitude = new Mat();
            result.GradientDirection = new Mat();
            Cv2.CartToPolar(gradX, gradY, result.GradientMagnitude, result.GradientDirection, true);

            // 3. 执行标准Canny边缘检测
            result.CannyEdges = new Mat();
            Cv2.Canny(image, result.CannyEdges, lowThreshold, highThreshold, apertureSize);

            // 4. 根据方向筛选边缘
            result.FilteredEdges = FilterEdgesByDirection(
                result.CannyEdges,
                result.GradientDirection,
                targetAngle,
                angleTolerance);

            // 5. 统计边缘点数
            result.TotalEdgePoints = Cv2.CountNonZero(result.CannyEdges);
            result.FilteredEdgePoints = Cv2.CountNonZero(result.FilteredEdges);

            // 6. 提取边缘点坐标
            var allEdgePoints = ExtractEdgePoints(result.CannyEdges);
            var filteredEdgePoints = ExtractEdgePoints(result.FilteredEdges);

            // 7. 亚像素精度定位
            if (useSubPixel && allEdgePoints.Count > 0)
            {
                result.SubPixelEdges = RefineEdgesSubPixel(image, allEdgePoints, subPixelWinSize);
                result.FilteredSubPixelEdges = RefineEdgesSubPixel(image, filteredEdgePoints, subPixelWinSize);
            }
            else
            {
                result.SubPixelEdges = allEdgePoints;
                result.FilteredSubPixelEdges = filteredEdgePoints;
            }

            return result;
        }

        /// <summary>
        /// 根据梯度方向筛选边缘
        /// </summary>
        private Mat FilterEdgesByDirection(Mat edges, Mat direction, double targetAngle, double tolerance)
        {
            var filtered = new Mat(edges.Size(), MatType.CV_8UC1, Scalar.Black);

            // 将目标角度归一化到[0, 360)
            targetAngle = NormalizeAngle(targetAngle);
            double minAngle = NormalizeAngle(targetAngle - tolerance);
            double maxAngle = NormalizeAngle(targetAngle + tolerance);

            unsafe
            {
                var edgePtr = (byte*)edges.Data;
                var dirPtr = (float*)direction.Data;
                var filteredPtr = (byte*)filtered.Data;

                int width = edges.Width;
                int height = edges.Height;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int idx = y * width + x;

                        // 如果不是边缘点，跳过
                        if (edgePtr[idx] == 0)
                            continue;

                        // 获取梯度方向（OpenCV返回的是0-360度）
                        double angle = dirPtr[idx];
                        angle = NormalizeAngle(angle);

                        // 检查是否在目标角度范围内
                        if (IsAngleInRange(angle, minAngle, maxAngle))
                        {
                            filteredPtr[idx] = 255;
                        }
                    }
                }
            }

            return filtered;
        }

        /// <summary>
        /// 将角度归一化到[0, 360)范围
        /// </summary>
        private double NormalizeAngle(double angle)
        {
            angle = angle % 360.0;
            if (angle < 0)
                angle += 360.0;
            return angle;
        }

        /// <summary>
        /// 检查角度是否在范围内（处理跨越0°的情况）
        /// </summary>
        private bool IsAngleInRange(double angle, double minAngle, double maxAngle)
        {
            if (minAngle <= maxAngle)
            {
                // 正常范围，如 [30, 60]
                return angle >= minAngle && angle <= maxAngle;
            }
            else
            {
                // 跨越0°的范围，如 [350, 10]
                return angle >= minAngle || angle <= maxAngle;
            }
        }

        /// <summary>
        /// 提取边缘点坐标
        /// </summary>
        private List<Point2f> ExtractEdgePoints(Mat edges)
        {
            var points = new List<Point2f>();

            using var locations = new Mat();
            Cv2.FindNonZero(edges, locations);

            if (locations.Empty())
                return points;

            for (int i = 0; i < locations.Total(); i++)
            {
                var pt = locations.At<Point>(i);
                points.Add(new Point2f(pt.X, pt.Y));
            }

            return points;
        }

        /// <summary>
        /// 使用CornerSubPix进行亚像素精度定位
        /// </summary>
        private List<Point2f> RefineEdgesSubPixel(Mat image, List<Point2f> points, int winSize)
        {
            if (points.Count == 0)
                return new List<Point2f>();

            // 复制点列表，因为CornerSubPix会修改输入
            var refinedPoints = new List<Point2f>(points);

            try
            {
                // CornerSubPix参数
                var criteria = new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.MaxIter, 30, 0.01);
                var winSizeParam = new Size(winSize, winSize);
                var zeroZone = new Size(-1, -1);

                // 执行亚像素精度优化
                Cv2.CornerSubPix(image, refinedPoints, winSizeParam, zeroZone, criteria);
            }
            catch (Exception ex)
            {
                // 如果亚像素优化失败，返回原始点
                Console.WriteLine($"亚像素优化失败: {ex.Message}");
                return points;
            }

            return refinedPoints;
        }

        /// <summary>
        /// 将OpenCV的Point2f转换为PointD
        /// </summary>
        public static List<PointD> ConvertToPointD(List<Point2f> points)
        {
            return points.Select(p => new PointD(p.X, p.Y)).ToList();
        }

        /// <summary>
        /// 创建可视化图像（叠加原图和边缘）
        /// </summary>
        public static Mat CreateVisualization(Mat originalImage, Mat edges, Scalar edgeColor)
        {
            Mat visualization;

            // 如果原图是灰度图，转换为彩色
            if (originalImage.Channels() == 1)
            {
                visualization = new Mat();
                Cv2.CvtColor(originalImage, visualization, ColorConversionCodes.GRAY2BGR);
            }
            else
            {
                visualization = originalImage.Clone();
            }

            // 在边缘位置绘制彩色标记
            unsafe
            {
                var edgePtr = (byte*)edges.Data;
                var visPtr = (byte*)visualization.Data;

                int width = edges.Width;
                int height = edges.Height;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (edgePtr[y * width + x] > 0)
                        {
                            int visIdx = (y * width + x) * 3;
                            visPtr[visIdx] = (byte)edgeColor.Val0;     // B
                            visPtr[visIdx + 1] = (byte)edgeColor.Val1; // G
                            visPtr[visIdx + 2] = (byte)edgeColor.Val2; // R
                        }
                    }
                }
            }

            return visualization;
        }

        /// <summary>
        /// 创建方向可视化图像（用颜色表示梯度方向）
        /// </summary>
        public static Mat CreateDirectionVisualization(Mat direction, Mat magnitude, double magnitudeThreshold = 10.0)
        {
            var visualization = new Mat(direction.Size(), MatType.CV_8UC3);

            unsafe
            {
                var dirPtr = (float*)direction.Data;
                var magPtr = (float*)magnitude.Data;
                var visPtr = (byte*)visualization.Data;

                int width = direction.Width;
                int height = direction.Height;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int idx = y * width + x;

                        // 只显示梯度幅值足够大的点
                        if (magPtr[idx] < magnitudeThreshold)
                        {
                            visPtr[idx * 3] = 0;
                            visPtr[idx * 3 + 1] = 0;
                            visPtr[idx * 3 + 2] = 0;
                            continue;
                        }

                        // 将角度映射到HSV颜色空间的H通道（0-180）
                        float hue = (float)(dirPtr[idx] / 2.0); // OpenCV HSV中H的范围是0-180

                        // 创建HSV颜色
                        byte h = (byte)Math.Min(179, Math.Max(0, hue));
                        byte s = 255;
                        byte v = (byte)Math.Min(255, magPtr[idx]);

                        // HSV转BGR
                        var bgr = HsvToBgr(h, s, v);
                        visPtr[idx * 3] = bgr.Item1;     // B
                        visPtr[idx * 3 + 1] = bgr.Item2; // G
                        visPtr[idx * 3 + 2] = bgr.Item3; // R
                    }
                }
            }

            return visualization;
        }

        /// <summary>
        /// HSV转BGR
        /// </summary>
        private static (byte, byte, byte) HsvToBgr(byte h, byte s, byte v)
        {
            using var hsv = new Mat(1, 1, MatType.CV_8UC3, new Scalar(h, s, v));
            using var bgr = new Mat();
            Cv2.CvtColor(hsv, bgr, ColorConversionCodes.HSV2BGR);

            unsafe
            {
                var ptr = (byte*)bgr.Data;
                return (ptr[0], ptr[1], ptr[2]);
            }
        }
    }
}
