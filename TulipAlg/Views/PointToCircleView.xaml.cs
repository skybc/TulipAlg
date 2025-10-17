using System.Windows.Controls;
using TulipAlg.Core;
using TulipAlg.Helpers;
using TulipAlg.ViewModels;
using ScottPlot;

namespace TulipAlg.Views
{
    public partial class PointToCircleView : UserControl
    {
        private readonly PointToCircleViewModel _viewModel;

        public PointToCircleView(PointToCircleViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            _viewModel = viewModel;

            // 订阅属性变化事件
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName?.Contains("Result") == true)
                {
                    UpdateVisualization();
                }
            };

            // 初始化可视化
            ScottPlotHelper.ClearPlot(WpfPlot1);
            WpfPlot1.Plot.Title("点-圆 几何运算可视化");
            WpfPlot1.Plot.XLabel("X 轴");
            WpfPlot1.Plot.YLabel("Y 轴");
            WpfPlot1.Refresh();
        }

        private void UpdateVisualization()
        {
            try
            {
                ScottPlotHelper.ClearPlot(WpfPlot1);
                
                var point = new PointD(_viewModel.PointX, _viewModel.PointY);
                var circle = new CircleD(
                    new PointD(_viewModel.CircleCenterX, _viewModel.CircleCenterY),
                    _viewModel.CircleRadius
                );

                var allPoints = new List<PointD> { point, circle.Center };
                var allCircles = new List<CircleD> { circle };

                // 绘制圆
                ScottPlotHelper.DrawCircle(WpfPlot1, circle, Colors.Blue);

                // 绘制点
                ScottPlotHelper.DrawPoint(WpfPlot1, point, "P", Colors.Red);

                // 绘制最近点
                if (!string.IsNullOrEmpty(_viewModel.ClosestPointResult) &&
                    !_viewModel.ClosestPointResult.Contains("错误"))
                {
                    var closest = AlgGeometry.ClosestPointOnCircle(point, circle);
                    allPoints.Add(closest);
                    ScottPlotHelper.DrawPoint(WpfPlot1, closest, "Q", Colors.Green);
                    ScottPlotHelper.DrawDashedLine(WpfPlot1, circle.Center, point, Colors.Gray);
                    ScottPlotHelper.DrawDashedLine(WpfPlot1, point, closest, Colors.Green);
                }

                ScottPlotHelper.AutoScaleWithCircles(WpfPlot1, allPoints, allCircles);
                ScottPlotHelper.Refresh(WpfPlot1);
            }
            catch { }
        }
    }
}
