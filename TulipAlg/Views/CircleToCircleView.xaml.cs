using System.Windows.Controls;
using TulipAlg.Core;
using TulipAlg.Helpers;
using TulipAlg.ViewModels;
using ScottPlot;

namespace TulipAlg.Views
{
    public partial class CircleToCircleView : UserControl
    {
        private readonly CircleToCircleViewModel _viewModel;

        public CircleToCircleView(CircleToCircleViewModel viewModel)
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
            WpfPlot1.Plot.Title("圆-圆 几何运算可视化");
            WpfPlot1.Plot.XLabel("X 轴");
            WpfPlot1.Plot.YLabel("Y 轴");
            WpfPlot1.Refresh();
        }

        private void UpdateVisualization()
        {
            try
            {
                ScottPlotHelper.ClearPlot(WpfPlot1);
                
                var circle1 = new CircleD(
                    new PointD(_viewModel.Circle1CenterX, _viewModel.Circle1CenterY),
                    _viewModel.Circle1Radius
                );
                var circle2 = new CircleD(
                    new PointD(_viewModel.Circle2CenterX, _viewModel.Circle2CenterY),
                    _viewModel.Circle2Radius
                );

                var allPoints = new List<PointD> { circle1.Center, circle2.Center };
                var allCircles = new List<CircleD> { circle1, circle2 };

                // 绘制圆1
                ScottPlotHelper.DrawCircle(WpfPlot1, circle1, Colors.Blue);

                // 绘制圆2
                ScottPlotHelper.DrawCircle(WpfPlot1, circle2, Colors.Green);

                // 绘制圆心连线
                ScottPlotHelper.DrawDashedLine(WpfPlot1, circle1.Center, circle2.Center, Colors.Gray);

                // 绘制交点
                if (!string.IsNullOrEmpty(_viewModel.IntersectionPointsResult) &&
                    !_viewModel.IntersectionPointsResult.Contains("错误"))
                {
                    var intersections = AlgGeometry.CircleIntersectionPoints(circle1, circle2);
                    if (intersections.Count > 0)
                    {
                        for (int i = 0; i < intersections.Count; i++)
                        {
                            allPoints.Add(intersections[i]);
                            ScottPlotHelper.DrawPoint(WpfPlot1, intersections[i], $"I{i + 1}", Colors.Red, 12);
                        }
                    }
                }

                ScottPlotHelper.AutoScaleWithCircles(WpfPlot1, allPoints, allCircles);
                ScottPlotHelper.Refresh(WpfPlot1);
            }
            catch { }
        }
    }
}
