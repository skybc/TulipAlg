using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TulipAlg.ViewModels;
using TulipAlg.Core;

namespace TulipAlg.Views
{
    /// <summary>
    /// CannyEdgeDetectorView.xaml 的交互逻辑
    /// </summary>
    public partial class CannyEdgeDetectorView : UserControl
    {
        private readonly CannyEdgeDetectorViewModel _viewModel;

        public CannyEdgeDetectorView(CannyEdgeDetectorViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            // 订阅属性变化事件以更新图表
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.EdgeResult))
            {
                UpdatePlot();
            }
        }

        private void UpdatePlot()
        {
            try
            {
                var edgeResult = _viewModel.EdgeResult;
                if (edgeResult == null || edgeResult.Curves.Count == 0)
                {
                    ResultImage.Source = null;
                    PlaceholderText.Visibility = Visibility.Visible;
                    return;
                }

                PlaceholderText.Visibility = Visibility.Collapsed;

                // 创建绘图表面
                var visual = new DrawingVisual();
                using (var dc = visual.RenderOpen())
                {
                    // 绘制白色背景
                    dc.DrawRectangle(Brushes.White, null, 
                        new Rect(0, 0, edgeResult.ImageWidth, edgeResult.ImageHeight));

                    // 定义颜色列表
                    var colors = new[]
                    {
                        Colors.Red, Colors.Blue, Colors.Green, Colors.Orange,
                        Colors.Purple, Colors.Brown, Colors.Pink, Colors.Cyan
                    };

                    int colorIndex = 0;
                    int curveCount = 0;

                    // 筛选长度大于MinCurveLength的曲线
                    var filteredCurves = edgeResult.FilterByLength(_viewModel.MinCurveLength);

                    foreach (var curve in filteredCurves)
                    {
                        if (curve.Points.Count < 2)
                            continue;

                        // 选择颜色
                        var color = colors[colorIndex % colors.Length];
                        var pen = new Pen(new SolidColorBrush(color), 2);

                        // 绘制曲线的线段
                        for (int i = 0; i < curve.Points.Count - 1; i++)
                        {
                            var p1 = curve.Points[i];
                            var p2 = curve.Points[i + 1];

                            // Y轴反转以匹配图像坐标系（图像坐标原点在左上角）
                            dc.DrawLine(pen,
                                new System.Windows.Point(p1.X,   p1.Y),
                                new System.Windows.Point(p2.X,   p2.Y));
                        }

                        colorIndex++;
                        curveCount++;

                        // 限制显示的曲线数量以避免图表过于拥挤
                        if (curveCount >= 50)
                            break;
                    }
                }

                // 渲染到位图
                var bitmap = new RenderTargetBitmap(
                    edgeResult.ImageWidth,
                    edgeResult.ImageHeight,
                    96, 96,
                    PixelFormats.Pbgra32);
                bitmap.Render(visual);

                // 显示在Image控件上
                ResultImage.Source = bitmap;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"更新图表失败: {ex.Message}", "错误",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
