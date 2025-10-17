using System.Windows.Controls;
using TulipAlg.Core;
using TulipAlg.Helpers;
using TulipAlg.ViewModels;
using ScottPlot;

namespace TulipAlg.Views
{
    public partial class PointToLineView : UserControl
    {
        private readonly PointToLineViewModel _viewModel;

        public PointToLineView(PointToLineViewModel viewModel)
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
            WpfPlot1.Plot.Title("点-线 几何运算可视化");
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

                var point = new PointD(_viewModel.PointX, _viewModel.PointY);
                var lineStart = new PointD(_viewModel.LineStartX, _viewModel.LineStartY);
                var lineEnd = new PointD(_viewModel.LineEndX, _viewModel.LineEndY);

                allPoints.AddRange(new[] { point, lineStart, lineEnd });

                // 绘制线段
                ScottPlotHelper.DrawLine(WpfPlot1, lineStart, lineEnd, Colors.Blue, 2);
                ScottPlotHelper.DrawPoint(WpfPlot1, lineStart, "A", Colors.Blue);
                ScottPlotHelper.DrawPoint(WpfPlot1, lineEnd, "B", Colors.Blue);

                // 绘制点
                ScottPlotHelper.DrawPoint(WpfPlot1, point, "P", Colors.Red);

                // 绘制垂足
                if (!string.IsNullOrEmpty(_viewModel.PerpendicularPointResult) &&
                    !_viewModel.PerpendicularPointResult.Contains("错误"))
                {
                    var line = new LineD(lineStart, lineEnd);
                    var foot = AlgGeometry.PerpendicularPoint(point, line);
                    allPoints.Add(foot);
                    ScottPlotHelper.DrawPoint(WpfPlot1, foot, "H", Colors.Green);
                    ScottPlotHelper.DrawDashedLine(WpfPlot1, point, foot, Colors.Green);
                }

                ScottPlotHelper.AutoScale(WpfPlot1, allPoints);
                ScottPlotHelper.Refresh(WpfPlot1);
            }
            catch { }
        }
    }
}
