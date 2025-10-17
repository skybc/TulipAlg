using ScottPlot;
using ScottPlot.WPF;
using TulipAlg.Core;

namespace TulipAlg.Helpers
{
    /// <summary>
    /// ScottPlot 可视化辅助类
    /// </summary>
    public static class ScottPlotHelper
    {
        /// <summary>
        /// 清空并初始化 Plot
        /// </summary>
        public static void ClearPlot(WpfPlot wpfPlot)
        {
            wpfPlot.Plot.Clear();
            wpfPlot.Plot.Axes.SetLimitsX(-20, 20);
            wpfPlot.Plot.Axes.SetLimitsY(-20, 20);
            wpfPlot.Plot.Grid.MajorLineColor = Color.FromHex("#E0E0E0");
            wpfPlot.Plot.Grid.MinorLineColor = Color.FromHex("#F5F5F5");
            wpfPlot.Plot.Axes.Color(Color.FromHex("#333333"));
        }

        /// <summary>
        /// 绘制点
        /// </summary>
        public static void DrawPoint(WpfPlot wpfPlot, PointD point, string label, Color color, float markerSize = 10)
        {
            var marker = wpfPlot.Plot.Add.Marker(point.X, point.Y);
            marker.Color = color;
            marker.Size = markerSize;
            marker.Shape = MarkerShape.FilledCircle;

            if (!string.IsNullOrEmpty(label))
            {
                var text = wpfPlot.Plot.Add.Text(label, point.X, point.Y);
                text.LabelFontColor = color;
                text.LabelFontSize = 12;
                text.LabelBold = true;
                text.OffsetX = 10;
                text.OffsetY = 10;
            }
        }

        /// <summary>
        /// 绘制线段
        /// </summary>
        public static void DrawLine(WpfPlot wpfPlot, PointD start, PointD end, Color color, float lineWidth = 2)
        {
            var line = wpfPlot.Plot.Add.Line(start.X, start.Y, end.X, end.Y);
            line.Color = color;
            line.LineWidth = lineWidth;
        }

        /// <summary>
        /// 绘制虚线
        /// </summary>
        public static void DrawDashedLine(WpfPlot wpfPlot, PointD start, PointD end, Color color, float lineWidth = 1)
        {
            var line = wpfPlot.Plot.Add.Line(start.X, start.Y, end.X, end.Y);
            line.Color = color;
            line.LineWidth = lineWidth;
            line.LinePattern = LinePattern.Dashed;
        }

        /// <summary>
        /// 绘制箭头
        /// </summary>
        public static void DrawArrow(WpfPlot wpfPlot, PointD start, PointD end, Color color, float lineWidth = 2)
        {
            var arrow = wpfPlot.Plot.Add.Arrow(start.X, start.Y, end.X, end.Y);
            arrow.ArrowLineColor = color;
            arrow.ArrowLineWidth = lineWidth;
        }

        /// <summary>
        /// 绘制圆
        /// </summary>
        public static void DrawCircle(WpfPlot wpfPlot, CircleD circle, Color color, float lineWidth = 2)
        {
            var ellipse = wpfPlot.Plot.Add.Circle(circle.Center.X, circle.Center.Y, circle.Radius);
            ellipse.LineColor = color;
            ellipse.LineWidth = lineWidth;
            ellipse.FillColor = Colors.Transparent;

            // 绘制圆心
            DrawPoint(wpfPlot, circle.Center, "C", color, 6);
        }

        /// <summary>
        /// 绘制多边形
        /// </summary>
        public static void DrawPolygon(WpfPlot wpfPlot, List<PointD> points, Color color, float lineWidth = 2)
        {
            if (points == null || points.Count < 2) return;

            // 绘制边
            for (int i = 0; i < points.Count; i++)
            {
                var start = points[i];
                var end = points[(i + 1) % points.Count];
                DrawLine(wpfPlot, start, end, color, lineWidth);
            }

            // 绘制顶点
            for (int i = 0; i < points.Count; i++)
            {
                DrawPoint(wpfPlot, points[i], $"P{i + 1}", color, 8);
            }
        }

        /// <summary>
        /// 自动调整视图范围
        /// </summary>
        public static void AutoScale(WpfPlot wpfPlot, List<PointD> points, double margin = 2.0)
        {
            if (points == null || points.Count == 0)
            {
                ClearPlot(wpfPlot);
                return;
            }

            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);

            double rangeX = maxX - minX;
            double rangeY = maxY - minY;

            // 确保最小范围
            if (rangeX < 1) rangeX = 10;
            if (rangeY < 1) rangeY = 10;

            wpfPlot.Plot.Axes.SetLimitsX(minX - margin, maxX + rangeX * 0.1 + margin);
            wpfPlot.Plot.Axes.SetLimitsY(minY - margin, maxY + rangeY * 0.1 + margin);
        }

        /// <summary>
        /// 自动调整视图范围（包含圆）
        /// </summary>
        public static void AutoScaleWithCircles(WpfPlot wpfPlot, List<PointD> points, List<CircleD> circles, double margin = 2.0)
        {
            var allPoints = new List<PointD>(points ?? new List<PointD>());

            // 添加圆的边界点
            if (circles != null)
            {
                foreach (var circle in circles)
                {
                    allPoints.Add(new PointD(circle.Center.X - circle.Radius, circle.Center.Y));
                    allPoints.Add(new PointD(circle.Center.X + circle.Radius, circle.Center.Y));
                    allPoints.Add(new PointD(circle.Center.X, circle.Center.Y - circle.Radius));
                    allPoints.Add(new PointD(circle.Center.X, circle.Center.Y + circle.Radius));
                }
            }

            AutoScale(wpfPlot, allPoints, margin);
        }

        /// <summary>
        /// 刷新显示
        /// </summary>
        public static void Refresh(WpfPlot wpfPlot)
        {
            wpfPlot.Refresh();
        }
    }
}
