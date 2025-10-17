using System.Windows.Controls;
using TulipAlg.Core;
using TulipAlg.Helpers;
using TulipAlg.ViewModels;
using ScottPlot;

namespace TulipAlg.Views
{
    public partial class LineToLineView : UserControl
    {
        private readonly LineToLineViewModel _viewModel;

        public LineToLineView(LineToLineViewModel viewModel)
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
            WpfPlot1.Plot.Title("线-线 几何运算可视化");
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

                var line1Start = new PointD(_viewModel.Line1StartX, _viewModel.Line1StartY);
                var line1End = new PointD(_viewModel.Line1EndX, _viewModel.Line1EndY);
                var line2Start = new PointD(_viewModel.Line2StartX, _viewModel.Line2StartY);
                var line2End = new PointD(_viewModel.Line2EndX, _viewModel.Line2EndY);

                allPoints.AddRange(new[] { line1Start, line1End, line2Start, line2End });

                // 绘制线1
                ScottPlotHelper.DrawLine(WpfPlot1, line1Start, line1End, Colors.Blue, 2);
                ScottPlotHelper.DrawPoint(WpfPlot1, line1Start, "A", Colors.Blue);
                ScottPlotHelper.DrawPoint(WpfPlot1, line1End, "B", Colors.Blue);

                // 绘制线2
                ScottPlotHelper.DrawLine(WpfPlot1, line2Start, line2End, Colors.Green, 2);
                ScottPlotHelper.DrawPoint(WpfPlot1, line2Start, "C", Colors.Green);
                ScottPlotHelper.DrawPoint(WpfPlot1, line2End, "D", Colors.Green);

                // 绘制交点
                if (!string.IsNullOrEmpty(_viewModel.IntersectionResult) && 
                    !_viewModel.IntersectionResult.Contains("无交点") &&
                    !_viewModel.IntersectionResult.Contains("错误"))
                {
                    var line1 = new LineD(line1Start, line1End);
                    var line2 = new LineD(line2Start, line2End);
                    var intersection = AlgGeometry.Intersection(line1, line2);
                    if (intersection != null)
                    {
                        allPoints.Add(intersection.Value);
                        ScottPlotHelper.DrawPoint(WpfPlot1, intersection.Value, "P", Colors.Red, 12);
                    }
                }

                ScottPlotHelper.AutoScale(WpfPlot1, allPoints);
                ScottPlotHelper.Refresh(WpfPlot1);
            }
            catch { }
        }
    }
}
