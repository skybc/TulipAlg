using System.Windows.Controls;
using TulipAlg.Core;
using TulipAlg.Helpers;
using TulipAlg.ViewModels;
using ScottPlot;

namespace TulipAlg.Views
{
    public partial class PointToRegionView : UserControl
    {
        private readonly PointToRegionViewModel _viewModel;

        public PointToRegionView(PointToRegionViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            _viewModel = viewModel;

            // 订阅属性变化事件
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName?.Contains("Result") == true ||
                    e.PropertyName == nameof(_viewModel.PolygonInput))
                {
                    UpdateVisualization();
                }
            };

            // 初始化可视化
            ScottPlotHelper.ClearPlot(WpfPlot1);
            WpfPlot1.Plot.Title("点-区域 几何运算可视化");
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
                var allPoints = new List<PointD> { point };

                // 绘制点
                ScottPlotHelper.DrawPoint(WpfPlot1, point, "P", Colors.Red);

                // 绘制多边形
                if (!string.IsNullOrWhiteSpace(_viewModel.PolygonInput))
                {
                    var polygon = ParsePoints(_viewModel.PolygonInput);
                    if (polygon.Count >= 3)
                    {
                        allPoints.AddRange(polygon);
                        ScottPlotHelper.DrawPolygon(WpfPlot1, polygon, Colors.Purple);
                    }
                }

                // 绘制圆形区域
                if (_viewModel.RegionRadius > 0)
                {
                    var circle = new CircleD(
                        new PointD(_viewModel.RegionCenterX, _viewModel.RegionCenterY),
                        _viewModel.RegionRadius
                    );
                    allPoints.Add(circle.Center);
                    
                    var circles = new List<CircleD> { circle };
                    ScottPlotHelper.DrawCircle(WpfPlot1, circle, Colors.Blue);
                    ScottPlotHelper.AutoScaleWithCircles(WpfPlot1, allPoints, circles);
                }
                else
                {
                    ScottPlotHelper.AutoScale(WpfPlot1, allPoints);
                }

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
