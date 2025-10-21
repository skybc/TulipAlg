using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace TulipAlg.ViewModels
{
    /// <summary>
    /// OpenCvSharp Canny边缘检测视图模型
    /// </summary>
    public partial class OpenCvCannyViewModel : ObservableObject
    {
        private Mat? _originalImage;
        private Mat? _grayImage;

        [ObservableProperty]
        private BitmapSource? _originalImageSource;

        [ObservableProperty]
        private BitmapSource? _edgeImageSource;

        [ObservableProperty]
        private double _threshold1 = 50;

        [ObservableProperty]
        private double _threshold2 = 150;

        [ObservableProperty]
        private int _apertureSize = 3;

        [ObservableProperty]
        private bool _useL2Gradient = false;

        [ObservableProperty]
        private string _imageInfo = "未加载图像";

        [ObservableProperty]
        private string _detectionInfo = "";

        [ObservableProperty]
        private bool _isImageLoaded = false;

        [ObservableProperty]
        private bool _autoDetect = true;

        public OpenCvCannyViewModel()
        {
        }

        [RelayCommand]
        private void LoadImage()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "图像文件|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff|所有文件|*.*",
                Title = "选择图像"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // 释放之前的图像
                    _originalImage?.Dispose();
                    _grayImage?.Dispose();

                    // 加载图像
                    _originalImage = Cv2.ImRead(dialog.FileName, ImreadModes.Color);
                    _grayImage = new Mat();
                    Cv2.CvtColor(_originalImage, _grayImage, ColorConversionCodes.BGR2GRAY);

                    // 转换为WPF显示
                    OriginalImageSource = _originalImage.ToBitmapSource();

                    ImageInfo = $"图像大小: {_originalImage.Width} × {_originalImage.Height}\n" +
                               $"通道数: {_originalImage.Channels()}\n" +
                               $"深度: {_originalImage.Depth()}\n" +
                               $"路径: {dialog.FileName}";

                    IsImageLoaded = true;

                    // 自动执行边缘检测
                    if (AutoDetect)
                    {
                        DetectEdges();
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"加载图像失败: {ex.Message}", "错误",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void GenerateTestImage()
        {
            try
            {
                // 释放之前的图像
                _originalImage?.Dispose();
                _grayImage?.Dispose();

                // 创建测试图像 (500x500)
                _originalImage = new Mat(500, 500, MatType.CV_8UC3, new Scalar(255, 255, 255));

                // 绘制一些几何形状
                // 圆形
                Cv2.Circle(_originalImage, new OpenCvSharp.Point(150, 150), 80, new Scalar(0, 0, 255), 3);
                
                // 矩形
                Cv2.Rectangle(_originalImage, new OpenCvSharp.Point(300, 50), new OpenCvSharp.Point(450, 200), 
                    new Scalar(0, 255, 0), 3);
                
                // 直线
                Cv2.Line(_originalImage, new OpenCvSharp.Point(50, 300), new OpenCvSharp.Point(450, 350), 
                    new Scalar(255, 0, 0), 3);
                
                // 椭圆
                Cv2.Ellipse(_originalImage, new OpenCvSharp.Point(250, 400), new OpenCvSharp.Size(100, 50), 
                    45, 0, 360, new Scalar(255, 165, 0), 3);

                // 多边形
                var points = new[]
                {
                    new OpenCvSharp.Point(50, 400),
                    new OpenCvSharp.Point(100, 450),
                    new OpenCvSharp.Point(150, 420),
                    new OpenCvSharp.Point(120, 380)
                };
                Cv2.Polylines(_originalImage, new[] { points }, true, new Scalar(128, 0, 128), 3);

                // 转换为灰度
                _grayImage = new Mat();
                Cv2.CvtColor(_originalImage, _grayImage, ColorConversionCodes.BGR2GRAY);

                // 显示
                OriginalImageSource = _originalImage.ToBitmapSource();

                ImageInfo = $"测试图像生成成功\n大小: {_originalImage.Width} × {_originalImage.Height}\n" +
                           $"包含: 圆形、矩形、直线、椭圆、多边形";

                IsImageLoaded = true;

                // 自动执行边缘检测
                if (AutoDetect)
                {
                    DetectEdges();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"生成测试图像失败: {ex.Message}", "错误",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void DetectEdges()
        {
            if (_grayImage == null || _grayImage.Empty())
            {
                System.Windows.MessageBox.Show("请先加载图像", "提示",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            try
            {
                var startTime = DateTime.Now;

                // 执行Canny边缘检测
                using var edges = new Mat();
                Cv2.Canny(_grayImage, edges, Threshold1, Threshold2, ApertureSize, UseL2Gradient);

                // 转换为彩色图像以便显示
                using var colorEdges = new Mat();
                Cv2.CvtColor(edges, colorEdges, ColorConversionCodes.GRAY2BGR);

                // 显示
                EdgeImageSource = colorEdges.ToBitmapSource();

                // 统计边缘点数
                int edgePoints = Cv2.CountNonZero(edges);
                int totalPixels = edges.Width * edges.Height;
                double edgeRatio = (double)edgePoints / totalPixels * 100;

                var elapsedTime = (DateTime.Now - startTime).TotalMilliseconds;

                DetectionInfo = $"边缘检测完成\n" +
                               $"边缘点数: {edgePoints:N0}\n" +
                               $"总像素数: {totalPixels:N0}\n" +
                               $"边缘比例: {edgeRatio:F2}%\n" +
                               $"处理时间: {elapsedTime:F2} ms\n\n" +
                               $"参数:\n" +
                               $"  阈值1: {Threshold1}\n" +
                               $"  阈值2: {Threshold2}\n" +
                               $"  孔径大小: {ApertureSize}\n" +
                               $"  L2梯度: {(UseL2Gradient ? "是" : "否")}";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"边缘检测失败: {ex.Message}", "错误",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ClearResults()
        {
            EdgeImageSource = null;
            DetectionInfo = "";
        }

        [RelayCommand]
        private void ApplyPreset(string presetName)
        {
            switch (presetName)
            {
                case "Sensitive":
                    Threshold1 = 30;
                    Threshold2 = 90;
                    ApertureSize = 3;
                    UseL2Gradient = false;
                    break;

                case "Balanced":
                    Threshold1 = 50;
                    Threshold2 = 150;
                    ApertureSize = 3;
                    UseL2Gradient = false;
                    break;

                case "Strong":
                    Threshold1 = 100;
                    Threshold2 = 200;
                    ApertureSize = 3;
                    UseL2Gradient = false;
                    break;

                case "HighQuality":
                    Threshold1 = 50;
                    Threshold2 = 150;
                    ApertureSize = 5;
                    UseL2Gradient = true;
                    break;

                case "Fast":
                    Threshold1 = 80;
                    Threshold2 = 160;
                    ApertureSize = 3;
                    UseL2Gradient = false;
                    break;
            }

            if (AutoDetect && IsImageLoaded)
            {
                DetectEdges();
            }
        }

        // 监听参数变化，自动执行检测
        partial void OnThreshold1Changed(double value)
        {
            if (AutoDetect && IsImageLoaded)
            {
                DetectEdges();
            }
        }

        partial void OnThreshold2Changed(double value)
        {
            if (AutoDetect && IsImageLoaded)
            {
                DetectEdges();
            }
        }

        partial void OnApertureSizeChanged(int value)
        {
            // 确保是3, 5, 7
            if (value % 2 == 0)
            {
                ApertureSize = value + 1;
            }

            if (AutoDetect && IsImageLoaded)
            {
                DetectEdges();
            }
        }

        partial void OnUseL2GradientChanged(bool value)
        {
            if (AutoDetect && IsImageLoaded)
            {
                DetectEdges();
            }
        }

        ~OpenCvCannyViewModel()
        {
            _originalImage?.Dispose();
            _grayImage?.Dispose();
        }
    }
}
