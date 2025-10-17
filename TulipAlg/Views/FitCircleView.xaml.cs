using System.Windows.Controls;
using TulipAlg.Core;
using TulipAlg.Helpers;
using TulipAlg.ViewModels;
using ScottPlot;

namespace TulipAlg.Views
{
    public partial class FitCircleView : UserControl
    {
        private readonly FitCircleViewModel _viewModel;

        public FitCircleView(FitCircleViewModel viewModel)
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
            WpfPlot1.Plot.Title("拟合圆可视化");
            WpfPlot1.Plot.XLabel("X 轴");
            WpfPlot1.Plot.YLabel("Y 轴");
            WpfPlot1.Refresh();
        }

        private void UpdateVisualization()
        {
            try
            {
                ScottPlotHelper.ClearPlot(WpfPlot1);
                var allPoints = ParsePoints(_viewModel.PointsInput);
                var allCircles = new List<CircleD>();

                // 绘制输入点
                for (int i = 0; i < allPoints.Count; i++)
                {
                    ScottPlotHelper.DrawPoint(WpfPlot1, allPoints[i], $"P{i + 1}", Colors.Blue, 8);
                }

                // 绘制拟合圆
                if (!string.IsNullOrEmpty(_viewModel.FitCircleResult) &&
                    !_viewModel.FitCircleResult.Contains("错误") &&
                    allPoints.Count >= 3)
                {
                    try
                    {
                        var fitCircle = AlgGeometry.FitCircleToPoints(allPoints);
                        allCircles.Add(fitCircle);
                        ScottPlotHelper.DrawCircle(WpfPlot1, fitCircle, Colors.Green);
                    }
                    catch { }
                }

                // 绘制最小外接圆
                if (!string.IsNullOrEmpty(_viewModel.MinEnclosingCircleResult) &&
                    !_viewModel.MinEnclosingCircleResult.Contains("错误") &&
                    allPoints.Count >= 2)
                {
                    try
                    {
                        var enclosingCircle = AlgGeometry.MinimumEnclosingCircle(allPoints);
                        allCircles.Add(enclosingCircle);
                        ScottPlotHelper.DrawCircle(WpfPlot1, enclosingCircle, Colors.Orange);
                    }
                    catch { }
                }

                ScottPlotHelper.AutoScaleWithCircles(WpfPlot1, allPoints, allCircles);
                ScottPlotHelper.Refresh(WpfPlot1);
            }
            catch { }
        }

        private List<PointD> ParsePoints(string input)
        {
            var points = new List<PointD>();
            if (string.IsNullOrWhiteSpace(input)) return points;

            var pairs = input.Split(';');
            foreach (var pair in pairs)
            {
                var coords = pair.Trim().Split(',');
                if (coords.Length == 2 &&
                    double.TryParse(coords[0], out double x) &&
                    double.TryParse(coords[1], out double y))
                {
                    points.Add(new PointD(x, y));
                }
            }
            return points;
        }
    }
}
