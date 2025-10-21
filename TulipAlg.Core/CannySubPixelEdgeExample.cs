using System;

namespace TulipAlg.Core
{
    /// <summary>
    /// CannySubPixelEdge使用示例
    /// </summary>
    public class CannySubPixelEdgeExample
    {
        /// <summary>
        /// 基本使用示例
        /// </summary>
        public static void BasicExample()
        {
            // 创建测试图像 (100x100, 带一个圆形边缘)
            int width = 100;
            int height = 100;
            byte[] image = CreateCircleTestImage(width, height);

            // 创建边缘检测器
            using (var detector = new CannySubPixelEdge())
            {
                // 执行边缘检测
                // sigma: 高斯滤波标准差 (0 = 不滤波)
                // th_h: 高阈值
                // th_l: 低阈值
                var result = detector.DetectEdgesFromBytes(
                    image, width, height,
                    sigma: 1.0,
                    th_h: 20.0,
                    th_l: 10.0);

                if (result != null)
                {
                    Console.WriteLine($"检测到 {result.CurveCount} 条边缘曲线");
                    Console.WriteLine($"总边缘点数: {result.TotalPoints}");

                    // 遍历每条曲线
                    for (int i = 0; i < result.Curves.Count; i++)
                    {
                        var curve = result.Curves[i];
                        Console.WriteLine($"\n曲线 {i + 1}:");
                        Console.WriteLine($"  点数: {curve.Length}");
                        Console.WriteLine($"  长度: {curve.CalculateTotalLength():F2}");
                        Console.WriteLine($"  闭合: {(curve.IsClosed ? "是" : "否")}");

                        // 打印前5个点
                        int pointsToPrint = Math.Min(5, curve.Points.Count);
                        Console.WriteLine($"  前{pointsToPrint}个点:");
                        for (int j = 0; j < pointsToPrint; j++)
                        {
                            Console.WriteLine($"    {curve.Points[j]}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 使用double数组的示例
        /// </summary>
        public static void DoubleArrayExample()
        {
            int width = 100;
            int height = 100;
            
            // 创建double数组图像
            double[] image = new double[width * height];
            
            // 创建一个简单的渐变图像
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    image[x + y * width] = (x + y) / 2.0;
                }
            }

            using (var detector = new CannySubPixelEdge())
            {
                var result = detector.DetectEdges(
                    image, width, height,
                    sigma: 0.0,   // 不进行高斯滤波
                    th_h: 15.0,
                    th_l: 5.0);

                if (result != null)
                {
                    Console.WriteLine($"检测到 {result.CurveCount} 条边缘");
                }
            }
        }

        /// <summary>
        /// 筛选和处理边缘曲线
        /// </summary>
        public static void FilterExample()
        {
            int width = 100;
            int height = 100;
            byte[] image = CreateCircleTestImage(width, height);

            using (var detector = new CannySubPixelEdge())
            {
                var result = detector.DetectEdgesFromBytes(
                    image, width, height,
                    sigma: 1.0,
                    th_h: 20.0,
                    th_l: 10.0);

                if (result != null)
                {
                    // 筛选长度大于10的曲线
                    var longCurves = result.FilterByLength(10);
                    Console.WriteLine($"长度>10的曲线: {longCurves.Count}");

                    // 筛选欧几里得长度大于50的曲线
                    var longTotalLengthCurves = result.FilterByTotalLength(50.0);
                    Console.WriteLine($"总长度>50的曲线: {longTotalLengthCurves.Count}");

                    // 获取所有边缘点
                    var allPoints = result.GetAllPoints();
                    Console.WriteLine($"所有边缘点: {allPoints.Count}");

                    // 只处理闭合曲线
                    var closedCurves = result.Curves.FindAll(c => c.IsClosed);
                    Console.WriteLine($"闭合曲线: {closedCurves.Count}");
                }
            }
        }

        /// <summary>
        /// 参数调优示例
        /// </summary>
        public static void ParameterTuningExample()
        {
            int width = 100;
            int height = 100;
            byte[] image = CreateCircleTestImage(width, height);

            using (var detector = new CannySubPixelEdge())
            {
                // 测试不同的参数组合
                var parameterSets = new[]
                {
                    (sigma: 0.0, th_h: 20.0, th_l: 10.0, name: "无滤波"),
                    (sigma: 1.0, th_h: 20.0, th_l: 10.0, name: "轻度滤波"),
                    (sigma: 2.0, th_h: 20.0, th_l: 10.0, name: "中度滤波"),
                    (sigma: 1.0, th_h: 30.0, th_l: 15.0, name: "高阈值"),
                    (sigma: 1.0, th_h: 10.0, th_l: 5.0, name: "低阈值"),
                };

                foreach (var param in parameterSets)
                {
                    var result = detector.DetectEdgesFromBytes(
                        image, width, height,
                        param.sigma, param.th_h, param.th_l);

                    if (result != null)
                    {
                        Console.WriteLine($"{param.name}: " +
                            $"{result.CurveCount} 曲线, {result.TotalPoints} 点");
                    }
                }
            }
        }

        /// <summary>
        /// 创建一个包含圆形的测试图像
        /// </summary>
        private static byte[] CreateCircleTestImage(int width, int height)
        {
            byte[] image = new byte[width * height];
            int centerX = width / 2;
            int centerY = height / 2;
            int radius = Math.Min(width, height) / 3;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double dx = x - centerX;
                    double dy = y - centerY;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    // 圆内为白色，圆外为黑色
                    if (distance < radius)
                    {
                        image[x + y * width] = 255;
                    }
                    else
                    {
                        image[x + y * width] = 0;
                    }
                }
            }

            return image;
        }

        /// <summary>
        /// 从文件加载图像并检测边缘（需要图像处理库）
        /// </summary>
        public static void ProcessImageFile(string imagePath)
        {
            // 注意：这里需要使用图像处理库（如OpenCvSharp）来加载图像
            // 这只是一个示例框架

            /*
            using (var mat = Cv2.ImRead(imagePath, ImreadModes.Grayscale))
            {
                int width = mat.Width;
                int height = mat.Height;
                byte[] imageData = new byte[width * height];
                Marshal.Copy(mat.Data, imageData, 0, imageData.Length);

                using (var detector = new CannySubPixelEdge())
                {
                    var result = detector.DetectEdgesFromBytes(
                        imageData, width, height,
                        sigma: 1.0,
                        th_h: 50.0,
                        th_l: 20.0);

                    if (result != null)
                    {
                        Console.WriteLine($"检测到 {result.CurveCount} 条边缘");
                        // 进一步处理...
                    }
                }
            }
            */

            Console.WriteLine("此示例需要图像处理库支持");
        }
    }
}
