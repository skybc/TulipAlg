using System.Windows.Controls;
using TulipAlg.Core;
using TulipAlg.Helpers;
using TulipAlg.ViewModels;
using ScottPlot;

namespace TulipAlg.Views
{
    /// <summary>
    /// PointToPointView.xaml 的交互逻辑
    /// </summary>
    public partial class PointToPointView : UserControl
    {
        private readonly PointToPointViewModel _viewModel;

        public PointToPointView(PointToPointViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            _viewModel = viewModel;

            // 订阅属性变化事件
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.TranslateResult) ||
                    e.PropertyName == nameof(_viewModel.RotateResult) ||
                    e.PropertyName == nameof(_viewModel.DistanceResult) ||
                    e.PropertyName == nameof(_viewModel.MidPointResult))
                {
                    UpdateVisualization();
                }
            };

            // 初始化可视化
            ScottPlotHelper.ClearPlot(WpfPlot1);
            WpfPlot1.Plot.Title("点-点 几何运算可视化");
            WpfPlot1.Plot.XLabel("X 轴");
            WpfPlot1.Plot.YLabel("Y 轴");
            WpfPlot1.Refresh();
        }

        private void UpdateVisualization()
        {
            try
            {
                ScottPlotHelper.ClearPlot(WpfPlot1);

                var allPoints = new List<PointD>();
                var point1 = new PointD(_viewModel.PointX, _viewModel.PointY);
                allPoints.Add(point1);

                // 绘制原始点 P1
                ScottPlotHelper.DrawPoint(WpfPlot1, point1, "P1", Colors.Blue);

                // 绘制平移
                if (!string.IsNullOrEmpty(_viewModel.TranslateResult) && !_viewModel.TranslateResult.Contains("错误"))
                {
                    var translatedPoint = new PointD(_viewModel.PointX + _viewModel.Dx, _viewModel.PointY + _viewModel.Dy);
                    allPoints.Add(translatedPoint);
                    ScottPlotHelper.DrawPoint(WpfPlot1, translatedPoint, "P1'", Colors.Orange);
                    ScottPlotHelper.DrawArrow(WpfPlot1, point1, translatedPoint, Colors.OrangeRed);
                }

                // 绘制旋转
                if (!string.IsNullOrEmpty(_viewModel.RotateResult) && !_viewModel.RotateResult.Contains("错误"))
                {
                    var center = new PointD(_viewModel.CenterX, _viewModel.CenterY);
                    allPoints.Add(center);
                    ScottPlotHelper.DrawPoint(WpfPlot1, center, "C", Colors.Purple);

                    var rotatedPoint = AlgGeometry.Rotate(point1, center, _viewModel.AngleDegrees);
                    allPoints.Add(rotatedPoint);
                    ScottPlotHelper.DrawPoint(WpfPlot1, rotatedPoint, "P1\"", Colors.Red);
                    ScottPlotHelper.DrawDashedLine(WpfPlot1, center, point1, Colors.Purple);
                    ScottPlotHelper.DrawDashedLine(WpfPlot1, center, rotatedPoint, Colors.Red);
                }

                // 绘制第二个点和距离/中点
                if (_viewModel.Point2X != 0 || _viewModel.Point2Y != 0)
                {
                    var point2 = new PointD(_viewModel.Point2X, _viewModel.Point2Y);
                    allPoints.Add(point2);
                    ScottPlotHelper.DrawPoint(WpfPlot1, point2, "P2", Colors.Green);

                    if (!string.IsNullOrEmpty(_viewModel.DistanceResult))
                    {
                        ScottPlotHelper.DrawLine(WpfPlot1, point1, point2, Colors.Green);
                    }

                    if (!string.IsNullOrEmpty(_viewModel.MidPointResult) && !_viewModel.MidPointResult.Contains("错误"))
                    {
                        var midPoint = AlgGeometry.MidPoint(point1, point2);
                        allPoints.Add(midPoint);
                        ScottPlotHelper.DrawPoint(WpfPlot1, midPoint, "M", Colors.DarkViolet, 8);
                    }
                }

                // 自动调整视图范围
                ScottPlotHelper.AutoScale(WpfPlot1, allPoints);
                ScottPlotHelper.Refresh(WpfPlot1);
            }
            catch
            {
                // 忽略可视化错误
            }
        }
    }
}
