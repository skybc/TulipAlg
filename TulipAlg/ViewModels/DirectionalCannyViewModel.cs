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
using Point = OpenCvSharp.Point;
using Rect = OpenCvSharp.Rect;

namespace TulipAlg.ViewModels
{
    /// <summary>
    /// 方向筛选Canny边缘检测视图模型
    /// </summary>
    public partial class DirectionalCannyViewModel : ObservableObject
    {
        private Mat? _currentImage;
        private DirectionalCannyResult? _detectionResult;

        #region 参数属性

        [ObservableProperty]
        private double _lowThreshold = 50.0;

        [ObservableProperty]
        private double _highThreshold = 150.0;

        [ObservableProperty]
        private double _targetAngle = 0.0;

        [ObservableProperty]
        private double _angleTolerance = 15.0;

        [ObservableProperty]
        private int _apertureSize = 3;

        [ObservableProperty]
        private bool _useSubPixel = true;

        [ObservableProperty]
        private int _subPixelWinSize = 5;

        #endregion

        #region 显示属性

        [ObservableProperty]
        private System.Windows.Media.ImageSource? _originalImage;

        [ObservableProperty]
        private System.Windows.Media.ImageSource? _cannyEdgesImage;

        [ObservableProperty]
        private System.Windows.Media.ImageSource? _filteredEdgesImage;

        [ObservableProperty]
        private System.Windows.Media.ImageSource? _directionVisImage;

        [ObservableProperty]
        private string _imageInfo = "未加载图像";

        [ObservableProperty]
        private string _detectionResultText = string.Empty;

        [ObservableProperty]
        private bool _isImageLoaded;

        [ObservableProperty]
        private bool _isDetectionComplete;

        #endregion

        #region 统计信息

        [ObservableProperty]
        private int _totalEdgePoints;

        [ObservableProperty]
        private int _filteredEdgePoints;

        [ObservableProperty]
        private double _filterRatio;

        #endregion

        public DirectionalCannyResult? DetectionResult => _detectionResult;
        public DirectionalCannyViewModel()
        {

        }
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
                    _currentImage?.Dispose();
                    _currentImage = Cv2.ImRead(dialog.FileName, ImreadModes.Grayscale);

                    if (_currentImage.Empty())
                    {
                        MessageBox.Show("无法加载图像文件", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // 显示原始图像
                    OriginalImage = _currentImage.ToWriteableBitmap();

                    // 更新状态
                    IsImageLoaded = true;
                    IsDetectionComplete = false;
                    ImageInfo = $"图像尺寸: {_currentImage.Width} × {_currentImage.Height}";
                    DetectionResultText = string.Empty;

                    // 清空之前的结果
                    ClearResults();
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
            if (_currentImage == null || _currentImage.Empty())
            {
                MessageBox.Show("请先加载图像", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                DetectionResultText = "正在检测边缘...";

                // 释放之前的结果
                _detectionResult?.Dispose();

                // 创建边缘检测器
                var detector = new DirectionalCannyEdge();

                // 执行检测
                _detectionResult = detector.DetectEdges(
                    _currentImage,
                    LowThreshold,
                    HighThreshold,
                    TargetAngle,
                    AngleTolerance,
                    ApertureSize,
                    UseSubPixel,
                    SubPixelWinSize);

                if (_detectionResult != null)
                {
                    // 更新统计信息
                    TotalEdgePoints = _detectionResult.TotalEdgePoints;
                    FilteredEdgePoints = _detectionResult.FilteredEdgePoints;
                    FilterRatio = TotalEdgePoints > 0
                        ? (double)FilteredEdgePoints / TotalEdgePoints * 100.0
                        : 0.0;

                    // 显示结果图像
                    if (_detectionResult.CannyEdges != null)
                    {
                        CannyEdgesImage = _detectionResult.CannyEdges.ToWriteableBitmap();
                    }

                    if (_detectionResult.FilteredEdges != null)
                    {
                        FilteredEdgesImage = _detectionResult.FilteredEdges.ToWriteableBitmap();
                    }

                    // 创建方向可视化图像
                    if (_detectionResult.GradientDirection != null && _detectionResult.GradientMagnitude != null)
                    {
                        using var dirVis = DirectionalCannyEdge.CreateDirectionVisualization(
                            _detectionResult.GradientDirection,
                            _detectionResult.GradientMagnitude,
                            magnitudeThreshold: 10.0);
                        DirectionVisImage = dirVis.ToWriteableBitmap();
                    }

                    // 显示检测结果文本
                    DetectionResultText = $"检测完成！\n\n" +
                        $"原始Canny边缘点: {TotalEdgePoints:N0}\n" +
                        $"筛选后边缘点: {FilteredEdgePoints:N0}\n" +
                        $"保留比例: {FilterRatio:F2}%\n\n" +
                        $"目标角度: {TargetAngle:F1}°\n" +
                        $"容差范围: ±{AngleTolerance:F1}°\n" +
                        $"角度范围: [{(TargetAngle - AngleTolerance):F1}°, {(TargetAngle + AngleTolerance):F1}°]";

                    if (UseSubPixel)
                    {
                        DetectionResultText += $"\n\n亚像素边缘点:\n" +
                            $"  全部: {_detectionResult.SubPixelEdges.Count:N0}\n" +
                            $"  筛选后: {_detectionResult.FilteredSubPixelEdges.Count:N0}";
                    }

                    IsDetectionComplete = true;
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
                MessageBox.Show($"边缘检测失败: {ex.Message}\n\n{ex.StackTrace}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                IsDetectionComplete = false;
            }
        }

        [RelayCommand]
        private void GenerateTestImage()
        {
            try
            {
                // 生成测试图像（包含不同方向的边缘）
                int width = 512;
                int height = 512;

                _currentImage?.Dispose();
                _currentImage = new Mat(height, width, MatType.CV_8UC1, Scalar.White);

                // 绘制水平线（0°）
                Cv2.Line(_currentImage, new Point(50, 100), new Point(450, 100), Scalar.Black, 2);
                Cv2.PutText(_currentImage, "0deg", new Point(460, 105),
                    HersheyFonts.HersheySimplex, 0.5, Scalar.Black, 1);

                // 绘制45°斜线
                Cv2.Line(_currentImage, new Point(50, 200), new Point(350, 500), Scalar.Black, 2);
                Cv2.PutText(_currentImage, "45deg", new Point(360, 505),
                    HersheyFonts.HersheySimplex, 0.5, Scalar.Black, 1);

                // 绘制垂直线（90°）
                Cv2.Line(_currentImage, new Point(250, 50), new Point(250, 450), Scalar.Black, 2);
                Cv2.PutText(_currentImage, "90deg", new Point(255, 60),
                    HersheyFonts.HersheySimplex, 0.5, Scalar.Black, 1);

                // 绘制135°斜线
                Cv2.Line(_currentImage, new Point(450, 200), new Point(150, 500), Scalar.Black, 2);
                Cv2.PutText(_currentImage, "135deg", new Point(100, 505),
                    HersheyFonts.HersheySimplex, 0.5, Scalar.Black, 1);

                // 绘制圆形（所有方向）
                Cv2.Circle(_currentImage, new Point(400, 150), 60, Scalar.Black, 2);
                Cv2.PutText(_currentImage, "Circle", new Point(380, 220),
                    HersheyFonts.HersheySimplex, 0.5, Scalar.Black, 1);

                // 绘制矩形
                Cv2.Rectangle(_currentImage, new Rect(50, 300, 150, 100), Scalar.Black, 2);
                Cv2.PutText(_currentImage, "Rectangle", new Point(55, 410),
                    HersheyFonts.HersheySimplex, 0.5, Scalar.Black, 1);

                // 显示图像
                OriginalImage = _currentImage.ToWriteableBitmap();

                // 更新状态
                IsImageLoaded = true;
                IsDetectionComplete = false;
                ImageInfo = $"测试图像: {width} × {height}";
                DetectionResultText = "测试图像包含以下方向的边缘：\n" +
                    "- 0° (水平)\n" +
                    "- 45° (左下到右上)\n" +
                    "- 90° (垂直)\n" +
                    "- 135° (左上到右下)\n" +
                    "- 圆形(所有方向)\n" +
                    "- 矩形(0°和90°)";

                // 清空之前的结果
                ClearResults();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成测试图像失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ApplyAnglePreset(string preset)
        {
            switch (preset)
            {
                case "Horizontal":
                    TargetAngle = 0.0;
                    AngleTolerance = 15.0;
                    break;
                case "Vertical":
                    TargetAngle = 90.0;
                    AngleTolerance = 15.0;
                    break;
                case "Diagonal45":
                    TargetAngle = 45.0;
                    AngleTolerance = 15.0;
                    break;
                case "Diagonal135":
                    TargetAngle = 135.0;
                    AngleTolerance = 15.0;
                    break;
                case "AllDirections":
                    TargetAngle = 0.0;
                    AngleTolerance = 180.0;
                    break;
            }
        }

        [RelayCommand]
        private void ApplyThresholdPreset(string preset)
        {
            switch (preset)
            {
                case "Low":
                    LowThreshold = 30.0;
                    HighThreshold = 90.0;
                    break;
                case "Medium":
                    LowThreshold = 50.0;
                    HighThreshold = 150.0;
                    break;
                case "High":
                    LowThreshold = 100.0;
                    HighThreshold = 200.0;
                    break;
            }
        }

        [RelayCommand]
        private void SaveResults()
        {
            if (_detectionResult == null)
            {
                MessageBox.Show("没有可保存的结果", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var dialog = new SaveFileDialog
                {
                    Title = "保存检测结果",
                    Filter = "PNG图像|*.png|JPEG图像|*.jpg|BMP图像|*.bmp",
                    FileName = $"DirectionalCanny_{TargetAngle}deg_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (dialog.ShowDialog() == true)
                {
                    // 创建组合图像显示结果
                    using var combined = CreateCombinedResultImage();
                    if (combined != null)
                    {
                        Cv2.ImWrite(dialog.FileName, combined);
                        MessageBox.Show("结果已保存", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Mat? CreateCombinedResultImage()
        {
            if (_currentImage == null || _detectionResult == null)
                return null;

            int width = _currentImage.Width;
            int height = _currentImage.Height;

            // 创建2x2网格图像
            var combined = new Mat(height * 2, width * 2, MatType.CV_8UC3, Scalar.White);

            try
            {
                // 左上：原图
                using var colorImg = new Mat();
                Cv2.CvtColor(_currentImage, colorImg, ColorConversionCodes.GRAY2BGR);
                var roi1 = new Mat(combined, new Rect(0, 0, width, height));
                colorImg.CopyTo(roi1);

                // 右上：Canny边缘
                if (_detectionResult.CannyEdges != null)
                {
                    using var cannyColor = new Mat();
                    Cv2.CvtColor(_detectionResult.CannyEdges, cannyColor, ColorConversionCodes.GRAY2BGR);
                    var roi2 = new Mat(combined, new Rect(width, 0, width, height));
                    cannyColor.CopyTo(roi2);
                }

                // 左下：筛选后的边缘
                if (_detectionResult.FilteredEdges != null)
                {
                    using var filteredColor = new Mat();
                    Cv2.CvtColor(_detectionResult.FilteredEdges, filteredColor, ColorConversionCodes.GRAY2BGR);
                    var roi3 = new Mat(combined, new Rect(0, height, width, height));
                    filteredColor.CopyTo(roi3);
                }

                // 右下：方向可视化
                if (_detectionResult.GradientDirection != null && _detectionResult.GradientMagnitude != null)
                {
                    using var dirVis = DirectionalCannyEdge.CreateDirectionVisualization(
                        _detectionResult.GradientDirection,
                        _detectionResult.GradientMagnitude);
                    var roi4 = new Mat(combined, new Rect(width, height, width, height));
                    dirVis.CopyTo(roi4);
                }

                // 添加文字标签
                Cv2.PutText(combined, "Original", new Point(10, 30),
                    HersheyFonts.HersheySimplex, 1, Scalar.Red, 2);
                Cv2.PutText(combined, "Canny Edges", new Point(width + 10, 30),
                    HersheyFonts.HersheySimplex, 1, Scalar.Red, 2);
                Cv2.PutText(combined, $"Filtered ({TargetAngle}deg +/-{AngleTolerance}deg)",
                    new Point(10, height + 30), HersheyFonts.HersheySimplex, 1, Scalar.Red, 2);
                Cv2.PutText(combined, "Gradient Direction", new Point(width + 10, height + 30),
                    HersheyFonts.HersheySimplex, 1, Scalar.Red, 2);

                return combined;
            }
            catch
            {
                combined.Dispose();
                throw;
            }
        }

        [RelayCommand]
        private void ClearAll()
        {
            _currentImage?.Dispose();
            _currentImage = null;

            ClearResults();

            OriginalImage = null;
            ImageInfo = "未加载图像";
            DetectionResultText = string.Empty;
            IsImageLoaded = false;
            IsDetectionComplete = false;
        }

        private void ClearResults()
        {
            _detectionResult?.Dispose();
            _detectionResult = null;

            CannyEdgesImage = null;
            FilteredEdgesImage = null;
            DirectionVisImage = null;

            TotalEdgePoints = 0;
            FilteredEdgePoints = 0;
            FilterRatio = 0.0;
        }

        public void Cleanup()
        {
            _currentImage?.Dispose();
            _detectionResult?.Dispose();
        }
    }
}
