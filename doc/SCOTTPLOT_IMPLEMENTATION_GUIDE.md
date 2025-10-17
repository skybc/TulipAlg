# TulipAlg - ScottPlot 可视化实现指南

## 概述

本项目使用 **ScottPlot.WPF 5.0.47** 实现几何运算的可视化功能。ScottPlot 是一个强大的 .NET 绘图库，专为科学和工程应用设计，提供高性能的2D图表绘制能力。

## 已完成的工作

### 1. 添加 NuGet 包

已在 `TulipAlg.csproj` 中添加：
```xml
<PackageReference Include="ScottPlot.WPF" Version="5.0.47" />
```

### 2. 创建 ScottPlotHelper 辅助类

位置：`TulipAlg/Helpers/ScottPlotHelper.cs`

提供以下方法：
- `ClearPlot(WpfPlot)` - 清空并初始化绘图区
- `DrawPoint(WpfPlot, PointD, label, color, size)` - 绘制带标签的点
- `DrawLine(WpfPlot, start, end, color, width)` - 绘制线段
- `DrawDashedLine(WpfPlot, start, end, color, width)` - 绘制虚线
- `DrawArrow(WpfPlot, start, end, color, width)` - 绘制箭头
- `DrawCircle(WpfPlot, CircleD, color, width)` - 绘制圆
- `DrawPolygon(WpfPlot, points, color, width)` - 绘制多边形
- `AutoScale(WpfPlot, points, margin)` - 自动调整视图范围
- `AutoScaleWithCircles(WpfPlot, points, circles, margin)` - 自动调整视图范围（包含圆）
- `Refresh(WpfPlot)` - 刷新显示

### 3. 更新 PointToPointView (示例)

✅ 已完成 XAML 和代码更新

## 如何为其他 View 添加 ScottPlot 可视化

### 步骤 1: 更新 XAML

在 XAML 文件中：

1. **添加 ScottPlot 命名空间**：
```xaml
xmlns:scottplot="clr-namespace:ScottPlot.WPF;assembly=ScottPlot.WPF"
```

2. **替换 Canvas 为 WpfPlot**：
```xaml
<!-- 原来的 Canvas -->
<Border Grid.Column="1" BorderBrush="Gray" BorderThickness="1" Margin="10,0,0,0">
    <Canvas x:Name="VisualizationCanvas" Background="White">
        <TextBlock Text="可视化区域（未来扩展）" .../>
    </Canvas>
</Border>

<!-- 替换为 WpfPlot -->
<Border Grid.Column="1" BorderBrush="Gray" BorderThickness="1" Margin="10,0,0,0">
    <scottplot:WpfPlot x:Name="WpfPlot1" />
</Border>
```

### 步骤 2: 更新代码后置文件 (.cs)

#### 2.1 添加 using 语句

```csharp
using System.Windows.Controls;
using TulipAlg.Core;
using TulipAlg.Helpers;
using TulipAlg.ViewModels;
using ScottPlot;
```

#### 2.2 修改构造函数

```csharp
public partial class YourView : UserControl
{
    private readonly YourViewModel _viewModel;

    public YourView(YourViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;

        // 订阅属性变化事件
        _viewModel.PropertyChanged += (s, e) =>
        {
            // 根据实际情况监听相关属性
            if (e.PropertyName == nameof(_viewModel.SomeResult))
            {
                UpdateVisualization();
            }
        };

        // 初始化可视化
        ScottPlotHelper.ClearPlot(WpfPlot1);
        WpfPlot1.Plot.Title("您的标题");
        WpfPlot1.Plot.XLabel("X 轴");
        WpfPlot1.Plot.YLabel("Y 轴");
        WpfPlot1.Refresh();
    }

    private void UpdateVisualization()
    {
        try
        {
            ScottPlotHelper.ClearPlot(WpfPlot1);
            
            // 收集所有点用于自动缩放
            var allPoints = new List<PointD>();

            // 根据功能绘制几何对象
            // ... 使用 ScottPlotHelper 的方法绘制

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
```

## 各 View 的可视化实现建议

### LineToLineView (线-线)

**要绘制的内容**：
- 线1（蓝色）+ 端点 A, B
- 线2（绿色）+ 端点 C, D
- 交点 P（红色，如果存在）

```csharp
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
            !_viewModel.IntersectionResult.Contains("无交点"))
        {
            var line1 = new Line(line1Start, line1End);
            var line2 = new Line(line2Start, line2End);
            var intersection = AlgGeometry.LineIntersection(line1, line2);
            if (intersection != null)
            {
                allPoints.Add(intersection.Value);
                ScottPlotHelper.DrawPoint(WpfPlot1, intersection.Value, "P", Colors.Red);
            }
        }

        ScottPlotHelper.AutoScale(WpfPlot1, allPoints);
        ScottPlotHelper.Refresh(WpfPlot1);
    }
    catch { }
}
```

### PointToLineView (点-线)

**要绘制的内容**：
- 线段（蓝色）+ 端点 A, B
- 点 P（红色）
- 垂足 H（绿色）
- 垂线（绿色虚线）

```csharp
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
        if (!string.IsNullOrEmpty(_viewModel.PerpendicularPointResult))
        {
            var line = new Line(lineStart, lineEnd);
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
```

### FitCircleView (拟合圆)

**要绘制的内容**：
- 所有输入点 P1, P2, P3...（蓝色）
- 拟合圆（绿色）
- 最小外接圆（橙色）

```csharp
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
            ScottPlotHelper.DrawPoint(WpfPlot1, allPoints[i], $"P{i + 1}", Colors.Blue);
        }

        // 绘制拟合圆
        if (!string.IsNullOrEmpty(_viewModel.FitCircleResult))
        {
            try
            {
                var fitCircle = AlgGeometry.FitCircle(allPoints);
                allCircles.Add(fitCircle);
                ScottPlotHelper.DrawCircle(WpfPlot1, fitCircle, Colors.Green);
            }
            catch { }
        }

        // 绘制最小外接圆
        if (!string.IsNullOrEmpty(_viewModel.MinEnclosingCircleResult))
        {
            try
            {
                var enclosingCircle = AlgGeometry.MinEnclosingCircle(allPoints);
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
```

### PointToCircleView (点-圆)

**要绘制的内容**：
- 圆（蓝色）+ 圆心 C
- 点 P（红色）
- 最近点 Q（绿色）
- 连线

```csharp
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
        if (!string.IsNullOrEmpty(_viewModel.ClosestPointResult))
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
```

### CircleToCircleView (圆-圆)

**要绘制的内容**：
- 圆1（蓝色）+ 圆心 C1
- 圆2（绿色）+ 圆心 C2
- 圆心连线（灰色虚线）
- 交点 I1, I2（红色）

```csharp
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
        if (!string.IsNullOrEmpty(_viewModel.IntersectionPointsResult))
        {
            var intersections = AlgGeometry.CircleIntersectionPoints(circle1, circle2);
            if (intersections.Count > 0)
            {
                for (int i = 0; i < intersections.Count; i++)
                {
                    allPoints.Add(intersections[i]);
                    ScottPlotHelper.DrawPoint(WpfPlot1, intersections[i], $"I{i + 1}", Colors.Red);
                }
            }
        }

        ScottPlotHelper.AutoScaleWithCircles(WpfPlot1, allPoints, allCircles);
        ScottPlotHelper.Refresh(WpfPlot1);
    }
    catch { }
}
```

### PointToRegionView (点-区域)

**要绘制的内容**：
- 点 P（红色）
- 多边形（紫色）+ 顶点
- 或圆形区域（蓝色）

```csharp
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
            ScottPlotHelper.DrawCircle(WpfPlot1, circle, Colors.Blue);
        }

        ScottPlotHelper.AutoScale(WpfPlot1, allPoints);
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
```

## ScottPlot 特性说明

### 1. 交互功能

ScottPlot 提供开箱即用的交互功能：
- **鼠标拖动** - 平移视图
- **滚轮缩放** - 缩放视图
- **右键菜单** - 自动缩放、复制图像等
- **双击** - 自动适应视图

### 2. 颜色系统

使用 `ScottPlot.Color` 或 `ScottPlot.Colors`：
```csharp
Colors.Blue      // 蓝色
Colors.Green     // 绿色
Colors.Red       // 红色
Colors.Orange    // 橙色
Colors.Purple    // 紫色
Colors.Gray      // 灰色
Color.FromHex("#FF5733")  // 自定义颜色
```

### 3. 标记形状

```csharp
MarkerShape.FilledCircle    // 实心圆
MarkerShape.OpenCircle      // 空心圆
MarkerShape.FilledSquare    // 实心方块
MarkerShape.OpenSquare      // 空心方块
MarkerShape.FilledTriangleUp  // 实心三角形
```

### 4. 线型

```csharp
LinePattern.Solid    // 实线
LinePattern.Dashed   // 虚线
LinePattern.Dotted   // 点线
LinePattern.DashDot  // 点划线
```

## 性能优化建议

1. **批量更新**：使用 `Plot.Clear()` 然后添加所有元素，最后 `Refresh()`
2. **条件绘制**：只绘制需要显示的元素
3. **异常处理**：用 try-catch 包裹可视化代码，防止影响主功能
4. **延迟更新**：只在计算完成后更新可视化

## 下一步

请按照本指南更新剩余的 6 个 View：
- [ ] LineToLineView
- [ ] PointToLineView
- [ ] FitCircleView
- [ ] PointToCircleView
- [ ] CircleToCircleView
- [ ] PointToRegionView

每个 View 需要更新两个文件：
1. `.xaml` - 添加命名空间和 WpfPlot 控件
2. `.xaml.cs` - 添加可视化逻辑

## 故障排除

### 问题：WpfPlot1 不存在
**解决**：确保 XAML 中正确添加了 `x:Name="WpfPlot1"`

### 问题：PointD/CircleD 等类型找不到
**解决**：
1. 确保已添加 `using TulipAlg.Core;`
2. 确保项目引用了 TulipAlg.Core 项目
3. 尝试重新构建解决方案

### 问题：Arrow.Color 过时警告
**解决**：使用新的属性名：
```csharp
arrow.ArrowLineColor = color;
arrow.ArrowFillColor = color;
arrow.ArrowLineWidth = lineWidth;
```

## 参考资源

- ScottPlot 官方文档: https://scottplot.net/
- ScottPlot 5.0 快速入门: https://scottplot.net/quickstart/
- ScottPlot WPF 示例: https://scottplot.net/cookbook/5.0/WPF/
