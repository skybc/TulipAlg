using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using TulipAlg.Core;

namespace TulipAlg.ViewModels
{
    /// <summary>
    /// Canny亚像素边缘检测视图模型
    /// </summary>
    public partial class CannyEdgeDetectorViewModel : ObservableObject
    {
        private byte[]? _currentImageData;
        private int _imageWidth;
        private int _imageHeight;
        private CannyEdgeResult? _edgeDetectionResult;

        // 参数输入
        [ObservableProperty]
        private double _sigma = 1.0;

        [ObservableProperty]
        private double _highThreshold = 20.0;

        [ObservableProperty]
        private double _lowThreshold = 10.0;

        [ObservableProperty]
        private int _minCurveLength = 10;

        // 图像和结果
        [ObservableProperty]
        private System.Windows.Media.ImageSource? _originalImage;

        [ObservableProperty]
        private string _imageInfo = "未加载图像";

        [ObservableProperty]
        private string _detectionResultText = string.Empty;

        [ObservableProperty]
        private string _statisticsInfo = string.Empty;

        [ObservableProperty]
        private bool _isImageLoaded;

        [ObservableProperty]
        private bool _isDetectionComplete;

        // 用于传递给View进行绘图的数据
        public CannyEdgeResult? EdgeResult => _edgeDetectionResult;

        [RelayCommand]
        private void LoadImage()
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Title = "选择图像文件",
                    Filter = "图像文件|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff|所有文件|*.*"
                };

                if (dialog.ShowDialog() == true)
                {
                    // 使用OpenCV加载图像
                    using var colorMat = Cv2.ImRead(dialog.FileName, ImreadModes.Color);
                    if (colorMat.Empty())
                    {
                        MessageBox.Show("无法加载图像文件", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // 转换为灰度图像
                    using var grayMat = new Mat();
                    Cv2.CvtColor(colorMat, grayMat, ColorConversionCodes.BGR2GRAY);

                    // 保存图像数据
                    _imageWidth = grayMat.Width;
                    _imageHeight = grayMat.Height;
                    _currentImageData = new byte[_imageWidth * _imageHeight];
                    
                    // 复制数据
                    System.Runtime.InteropServices.Marshal.Copy(
                        grayMat.Data, _currentImageData, 0, _currentImageData.Length);

                    // 显示原始图像
                    OriginalImage = grayMat.ToWriteableBitmap();

                    // 更新状态
                    IsImageLoaded = true;
                    IsDetectionComplete = false;
                    ImageInfo = $"图像尺寸: {_imageWidth} × {_imageHeight}";
                    DetectionResultText = string.Empty;
                    StatisticsInfo = string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载图像失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void DetectEdges()
        {
            if (_currentImageData == null)
            {
                MessageBox.Show("请先加载图像", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                DetectionResultText = "正在检测边缘...";

                // 创建边缘检测器
                using var detector = new CannySubPixelEdge();

                // 执行检测
                _edgeDetectionResult = detector.DetectEdgesFromBytes(
                    _currentImageData,
                    _imageWidth,
                    _imageHeight,
                    Sigma,
                    HighThreshold,
                    LowThreshold);

                if (_edgeDetectionResult != null)
                {
                    // 筛选曲线
                    var filteredCurves = _edgeDetectionResult.FilterByLength(MinCurveLength);

                    // 显示结果
                    DetectionResultText = $"检测完成！\n" +
                        $"检测到 {_edgeDetectionResult.CurveCount} 条边缘曲线\n" +
                        $"总边缘点数: {_edgeDetectionResult.TotalPoints}\n" +
                        $"筛选后 (长度>{MinCurveLength}): {filteredCurves.Count} 条曲线";

                    // 统计信息
                    var closedCurves = _edgeDetectionResult.Curves.Count(c => c.IsClosed);
                    var avgLength = _edgeDetectionResult.Curves.Average(c => c.Length);
                    var avgTotalLength = _edgeDetectionResult.Curves.Average(c => c.CalculateTotalLength());
                    var maxLength = _edgeDetectionResult.Curves.Max(c => c.Length);
                    var minLength = _edgeDetectionResult.Curves.Min(c => c.Length);

                    StatisticsInfo = $"曲线统计:\n" +
                        $"  闭合曲线: {closedCurves} 条\n" +
                        $"  开放曲线: {_edgeDetectionResult.CurveCount - closedCurves} 条\n" +
                        $"  平均点数: {avgLength:F1}\n" +
                        $"  平均长度: {avgTotalLength:F2}\n" +
                        $"  最长曲线: {maxLength} 点\n" +
                        $"  最短曲线: {minLength} 点";

                    IsDetectionComplete = true;

                    // 触发属性更改以通知View更新图表
                    OnPropertyChanged(nameof(EdgeResult));
                }
                else
                {
                    DetectionResultText = "检测失败！";
                    IsDetectionComplete = false;
                }
            }
            catch (Exception ex)
            {
                DetectionResultText = $"错误: {ex.Message}";
                MessageBox.Show($"边缘检测失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                IsDetectionComplete = false;
            }
        }

        [RelayCommand]
        private void GenerateTestImage()
        {
            try
            {
                // 生成一个测试图像（包含圆形和方形）
                int width = 400;
                int height = 400;
                using var mat = new Mat(height, width, MatType.CV_8UC1, Scalar.White);

                // 绘制圆形
                Cv2.Circle(mat, new OpenCvSharp.Point(150, 150), 80, Scalar.Black, 2);

                // 绘制矩形
                Cv2.Rectangle(mat, new OpenCvSharp.Rect(220, 220, 120, 100), Scalar.Black, 2);

                // 绘制三角形
                var trianglePoints = new[]
                {
                    new OpenCvSharp.Point(100, 300),
                    new OpenCvSharp.Point(180, 300),
                    new OpenCvSharp.Point(140, 350)
                };
                Cv2.Polylines(mat, new[] { trianglePoints }, true, Scalar.Black, 2);

                // 保存图像数据
                _imageWidth = width;
                _imageHeight = height;
                _currentImageData = new byte[width * height];

                System.Runtime.InteropServices.Marshal.Copy(
                    mat.Data, _currentImageData, 0, _currentImageData.Length);

                // 转换为彩色以显示
                using var colorMat = new Mat();
                Cv2.CvtColor(mat, colorMat, ColorConversionCodes.GRAY2BGR);
                OriginalImage = colorMat.ToWriteableBitmap();

                // 更新状态
                IsImageLoaded = true;
                IsDetectionComplete = false;
                ImageInfo = $"测试图像: {width} × {height}";
                DetectionResultText = string.Empty;
                StatisticsInfo = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成测试图像失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ClearResults()
        {
            _currentImageData = null;
            _edgeDetectionResult = null;
            OriginalImage = null;
            ImageInfo = "未加载图像";
            DetectionResultText = string.Empty;
            StatisticsInfo = string.Empty;
            IsImageLoaded = false;
            IsDetectionComplete = false;
            OnPropertyChanged(nameof(EdgeResult));
        }

        [RelayCommand]
        private void ApplyPreset(string preset)
        {
            switch (preset)
            {
                case "Default":
                    Sigma = 1.0;
                    HighThreshold = 20.0;
                    LowThreshold = 10.0;
                    MinCurveLength = 10;
                    break;
                case "Noisy":
                    Sigma = 1.5;
                    HighThreshold = 30.0;
                    LowThreshold = 15.0;
                    MinCurveLength = 15;
                    break;
                case "Clear":
                    Sigma = 0.5;
                    HighThreshold = 15.0;
                    LowThreshold = 7.0;
                    MinCurveLength = 5;
                    break;
                case "Detailed":
                    Sigma = 0.0;
                    HighThreshold = 10.0;
                    LowThreshold = 5.0;
                    MinCurveLength = 3;
                    break;
                case "HighQuality":
                    Sigma = 1.0;
                    HighThreshold = 40.0;
                    LowThreshold = 20.0;
                    MinCurveLength = 20;
                    break;
            }
        }
    }
}
