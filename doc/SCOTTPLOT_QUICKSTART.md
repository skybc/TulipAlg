# TulipAlg - ScottPlot 可视化快速开始

## ✅ 已完成的工作

1. **ScottPlot.WPF 5.0.47** NuGet 包已添加到项目
2. **ScottPlotHelper.cs** 辅助类已创建（提供所有绘图方法）
3. **PointToPointView** 已更新为 ScottPlot 实现（作为示例）

## 📋 实现状态

| View | XAML 更新 | 代码更新 | 状态 |
|------|-----------|----------|------|
| PointToPointView | ✅ | ✅ | **完成** |
| LineToLineView | ❌ | ❌ | 待完成 |
| PointToLineView | ❌ | ❌ | 待完成 |
| FitCircleView | ❌ | ❌ | 待完成 |
| PointToCircleView | ❌ | ❌ | 待完成 |
| CircleToCircleView | ❌ | ❌ | 待完成 |
| PointToRegionView | ❌ | ❌ | 待完成 |

## 🚀 如何运行项目

### 方法1: 使用 Visual Studio 2022（推荐）

1. **打开解决方案**
   ```
   用 Visual Studio 2022 打开: TulipAlg.slnx
   ```

2. **解决 C++ 工具集问题（如果遇到）**
   - 右键点击解决方案 → "重定目标解决方案"
   - 选择 v143 工具集
   - 点击确定

3. **设置启动项目**
   - 右键点击 `TulipAlg` 项目 → "设为启动项目"

4. **运行**
   - 按 `F5` 或点击"开始调试"按钮
   - 应用程序将启动，可以看到左侧菜单

5. **测试 ScottPlot 可视化**
   - 点击菜单"通用" → "点-点"
   - 输入参数并点击计算按钮
   - 右侧将显示 ScottPlot 可视化图形

### 方法2: 跳过 C++ 项目（临时方案）

如果不需要 C++ 项目功能：

1. 打开 `TulipAlg\TulipAlg.csproj`
2. 注释掉 C++相关的项目引用：
   ```xml
   <!-- 临时注释
   <ProjectReference Include="..\TulipAlg.Core3D\TulipAlg.Core3D.csproj" />
   -->
   ```
3. 然后使用命令行：
   ```powershell
   cd c:\Users\baochun\source\repos\TulipAlg\TulipAlg
   dotnet run
   ```

## 📝 完成剩余 View 的步骤

请参考 `SCOTTPLOT_IMPLEMENTATION_GUIDE.md` 文件，其中包含：

1. **详细的步骤说明**
2. **每个 View 的代码示例**
3. **ScottPlot 特性说明**
4. **常见问题解决**

### 快速步骤摘要

对于每个 View（以 LineToLineView 为例）：

#### 步骤 1: 更新 XAML

在 `Views/LineToLineView.xaml`：

```xaml
<!-- 1. 添加命名空间（第一行附近） -->
xmlns:scottplot="clr-namespace:ScottPlot.WPF;assembly=ScottPlot.WPF"

<!-- 2. 替换 Canvas 为 WpfPlot -->
<Border Grid.Column="1" BorderBrush="Gray" BorderThickness="1" Margin="10,0,0,0">
    <scottplot:WpfPlot x:Name="WpfPlot1" />
</Border>
```

#### 步骤 2: 更新代码

在 `Views/LineToLineView.xaml.cs`：

```csharp
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

            // 订阅属性变化
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName?.Contains("Result") == true)
                {
                    UpdateVisualization();
                }
            };

            // 初始化
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

                // 绘制交点（如果存在）
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
    }
}
```

## 🎨 ScottPlot 特性

### 内置交互功能

ScottPlot 提供强大的交互功能：

- **鼠标左键拖动** - 平移视图
- **鼠标滚轮** - 缩放视图
- **鼠标右键** - 显示上下文菜单
  - 自动缩放
  - 复制图像到剪贴板
  - 保存图像
- **双击** - 自动适应所有内容到视图

### 专业的绘图能力

- 高性能渲染
- 抗锯齿效果
- 专业的坐标轴
- 网格线
- 标题和标签
- 多种标记形状
- 多种线型

## 📚 更多资源

- **完整实现指南**: `SCOTTPLOT_IMPLEMENTATION_GUIDE.md`
- **项目总结**: `PROJECT_SUMMARY.md`
- **ScottPlot 官方文档**: https://scottplot.net/

## ❓ 常见问题

### Q: 为什么选择 ScottPlot？

**A:** ScottPlot 相比传统 Canvas 的优势：
- ✅ 专业的科学绘图库
- ✅ 内置交互功能（缩放、平移）
- ✅ 更好的性能
- ✅ 更少的代码
- ✅ 自动坐标轴和网格
- ✅ 丰富的绘图元素

### Q: 为什么不用之前的 Canvas+VisualizationHelper？

**A:** 
- Canvas 需要手动实现所有交互
- Canvas 需要复杂的坐标转换
- ScottPlot 是行业标准的科学绘图解决方案
- ScottPlot 提供更好的用户体验

### Q: 编译错误怎么办？

**A:** 常见的编译错误都是预构建的 IDE 警告：
- `WpfPlot1 不存在` - 需要构建项目后 XAML 生成代码
- `PointD 找不到` - 需要构建 TulipAlg.Core 项目
- 使用 Visual Studio 2022 构建可以避免这些问题

### Q: C++ 项目错误？

**A:** 
1. 在 Visual Studio 中右键解决方案 → "重定目标解决方案"
2. 或者临时注释掉 C++ 项目引用（如果不需要3D功能）

## ✨ 下一步

1. **在 Visual Studio 2022 中打开项目**
2. **运行并测试 PointToPointView 的 ScottPlot 可视化**
3. **参考 SCOTTPLOT_IMPLEMENTATION_GUIDE.md 完成其他 6 个 View**
4. **享受专业的几何可视化效果！**

---

💡 **提示**: ScottPlot 是一个成熟的专业绘图库，被广泛应用于科学和工程领域。您现在拥有了一个强大的几何可视化工具！
