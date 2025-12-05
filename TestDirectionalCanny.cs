using System;
using OpenCvSharp;
using TulipAlg.Core;

namespace TestDirectionalCanny
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== 方向筛选Canny边缘检测测试 ===\n");

            // 创建测试图像（包含不同方向的边缘）
            Console.WriteLine("1. 创建测试图像...");
            int width = 400;
            int height = 400;
            using var testImage = new Mat(height, width, MatType.CV_8UC1, Scalar.White);

            // 绘制水平线（0°）
            Cv2.Line(testImage, new Point(50, 100), new Point(350, 100), Scalar.Black, 2);
            
            // 绘制垂直线（90°）
            Cv2.Line(testImage, new Point(200, 50), new Point(200, 350), Scalar.Black, 2);
            
            // 绘制45°斜线
            Cv2.Line(testImage, new Point(50, 200), new Point(250, 350), Scalar.Black, 2);
            
            // 绘制圆形
            Cv2.Circle(testImage, new Point(300, 200), 60, Scalar.Black, 2);

            Console.WriteLine($"   图像尺寸: {width} x {height}");

            // 创建边缘检测器
            Console.WriteLine("\n2. 创建DirectionalCannyEdge检测器...");
            var detector = new DirectionalCannyEdge();

            // 测试不同角度的筛选
            var testCases = new[]
            {
                (angle: 0.0, tolerance: 15.0, name: "水平方向"),
                (angle: 90.0, tolerance: 15.0, name: "垂直方向"),
                (angle: 45.0, tolerance: 15.0, name: "45度方向"),
            };

            foreach (var testCase in testCases)
            {
                Console.WriteLine($"\n3. 测试 {testCase.name} ({testCase.angle}° ±{testCase.tolerance}°)...");

                try
                {
                    var result = detector.DetectEdges(
                        testImage,
                        lowThreshold: 50.0,
                        highThreshold: 150.0,
                        targetAngle: testCase.angle,
                        angleTolerance: testCase.tolerance,
                        apertureSize: 3,
                        useSubPixel: true,
                        subPixelWinSize: 5
                    );

                    if (result != null)
                    {
                        Console.WriteLine($"   ✓ 检测成功!");
                        Console.WriteLine($"   - 原始Canny边缘点: {result.TotalEdgePoints:N0}");
                        Console.WriteLine($"   - 筛选后边缘点: {result.FilteredEdgePoints:N0}");
                        Console.WriteLine($"   - 保留比例: {(result.TotalEdgePoints > 0 ? (double)result.FilteredEdgePoints / result.TotalEdgePoints * 100.0 : 0.0):F2}%");
                        Console.WriteLine($"   - 亚像素边缘点: {result.SubPixelEdges.Count:N0}");
                        Console.WriteLine($"   - 筛选后亚像素点: {result.FilteredSubPixelEdges.Count:N0}");

                        // 保存结果图像
                        string filename = $"test_result_{testCase.angle}deg.png";
                        if (result.FilteredEdges != null)
                        {
                            Cv2.ImWrite(filename, result.FilteredEdges);
                            Console.WriteLine($"   - 已保存结果: {filename}");
                        }

                        result.Dispose();
                    }
                    else
                    {
                        Console.WriteLine($"   ✗ 检测失败：返回结果为null");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ✗ 检测失败：{ex.Message}");
                    Console.WriteLine($"   堆栈跟踪：{ex.StackTrace}");
                }
            }

            Console.WriteLine("\n=== 测试完成 ===");
            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }
    }
}
