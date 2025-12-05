# 方向筛选Canny边缘检测 - 使用指南

## 概述

**DirectionalCannyEdge** 是一个支持按指定角度范围筛选边缘的Canny边缘检测器，并提供亚像素精度定位功能。该实现使用纯 OpenCvSharp4，无需额外的 C++ 依赖。

## 核心特性

1. ✅ **标准Canny边缘检测** - 基于OpenCvSharp的标准实现
2. ✅ **梯度方向计算** - 自动计算每个像素的梯度方向
3. ✅ **角度范围筛选** - 只保留指定方向范围内的边缘
4. ✅ **亚像素定位** - 使用 `Cv2.CornerSubPix` 实现亚像素精度
5. ✅ **可视化支持** - 提供多种结果可视化方法
6. ✅ **灵活的参数控制** - 支持自定义各种检测参数

## 角度定义

**重要**: 本实现采用标准数学定义的角度系统：
- **0°** = 水平向右
- **90°** = 垂直向上
- **角度逆时针增加**

示例：
- 水平边缘：0° 或 180°
- 垂直边缘：90° 或 270°
- 左下到右上对角线：45°
- 左上到右下对角线：135°

## 快速开始

### 1. 基本用法

```csharp
using OpenCvSharp;
using TulipAlg.Core;

// 加载图像（灰度图）
using var image = Cv2.ImRead("input.jpg", ImreadModes.Grayscale);

// 创建检测器
var detector = new DirectionalCannyEdge();

// 执行检测（只保留水平方向的边缘，±15°）
var result = detector.DetectEdges(
    image,
    lowThreshold: 50.0,      // Canny低阈值
    highThreshold: 150.0,     // Canny高阈值
    targetAngle: 0.0,         // 目标角度（水平）
    angleTolerance: 15.0,     // 角度容差（±15°）
    apertureSize: 3,          // Sobel算子孔径
    useSubPixel: true,        // 启用亚像素
    subPixelWinSize: 5        // 亚像素搜索窗口
);

// 显示结果
Console.WriteLine($"总边缘点: {result.TotalEdgePoints}");
Console.WriteLine($"筛选后: {result.FilteredEdgePoints}");

// 保存结果
Cv2.ImWrite("filtered_edges.png", result.FilteredEdges);

// 清理资源
result.Dispose();
```

### 2. 检测不同方向的边缘

```csharp
// 水平边缘 (0° ±15°)
var horizontal = detector.DetectEdges(image, 50, 150, 0.0, 15.0);

// 垂直边缘 (90° ±15°)
var vertical = detector.DetectEdges(image, 50, 150, 90.0, 15.0);

// 45度对角线 (45° ±15°)
var diagonal45 = detector.DetectEdges(image, 50, 150, 45.0, 15.0);

// 135度对角线 (135° ±15°)
var diagonal135 = detector.DetectEdges(image, 50, 150, 135.0, 15.0);

// 所有方向 (0° ±180°)
var allDirections = detector.DetectEdges(image, 50, 150, 0.0, 180.0);
```

### 3. 使用亚像素边缘点

```csharp
var result = detector.DetectEdges(image, 50, 150, 0.0, 15.0, 
    useSubPixel: true);

// 获取亚像素精度的边缘点
foreach (var point in result.FilteredSubPixelEdges)
{
    Console.WriteLine($"边缘点: ({point.X:F3}, {point.Y:F3})");
}

// 转换为 PointD 格式
var pointDList = DirectionalCannyEdge.ConvertToPointD(
    result.FilteredSubPixelEdges);
```

### 4. 结果可视化

```csharp
var result = detector.DetectEdges(image, 50, 150, 45.0, 15.0);

// 方法1：直接使用边缘图像
Cv2.ImShow("Canny边缘", result.CannyEdges);
Cv2.ImShow("筛选后的边缘", result.FilteredEdges);

// 方法2：创建彩色叠加图像
using var visualization = DirectionalCannyEdge.CreateVisualization(
    image, 
    result.FilteredEdges, 
    new Scalar(0, 255, 0) // 绿色边缘
);
Cv2.ImShow("叠加结果", visualization);

// 方法3：梯度方向可视化（颜色表示方向）
using var dirVis = DirectionalCannyEdge.CreateDirectionVisualization(
    result.GradientDirection,
    result.GradientMagnitude,
    magnitudeThreshold: 10.0
);
Cv2.ImShow("梯度方向", dirVis);

Cv2.WaitKey();
```

## 参数说明

### DetectEdges 方法参数

| 参数 | 类型 | 说明 | 推荐值 |
|------|------|------|--------|
| `image` | Mat | 输入灰度图像 | - |
| `lowThreshold` | double | Canny低阈值 | 30-100 |
| `highThreshold` | double | Canny高阈值 | 50-200 |
| `targetAngle` | double | 目标角度（度） | 0-360 |
| `angleTolerance` | double | 角度容差（±度） | 5-30 |
| `apertureSize` | int | Sobel算子孔径 | 3, 5, 7 |
| `useSubPixel` | bool | 是否使用亚像素 | true |
| `subPixelWinSize` | int | 亚像素窗口大小 | 5, 7, 9 |

### 参数调优建议

#### 阈值设置
- **噪声图像**: `lowThreshold=80, highThreshold=200`
- **清晰图像**: `lowThreshold=30, highThreshold=90`
- **高对比度**: `lowThreshold=100, highThreshold=250`

#### 角度容差
- **精确方向**: `tolerance=5°-10°`
- **宽松方向**: `tolerance=15°-30°`
- **多方向**: `tolerance=45°-90°`

#### Sobel孔径
- **标准检测**: `apertureSize=3`
- **平滑边缘**: `apertureSize=5`
- **粗糙边缘**: `apertureSize=7`

## 结果数据结构

### DirectionalCannyResult

```csharp
public class DirectionalCannyResult
{
    // 原始Canny边缘图像 (CV_8UC1)
    public Mat? CannyEdges { get; set; }
    
    // 方向筛选后的边缘图像 (CV_8UC1)
    public Mat? FilteredEdges { get; set; }
    
    // 梯度方向图像 (CV_32FC1, 单位：度)
    public Mat? GradientDirection { get; set; }
    
    // 梯度幅值图像 (CV_32FC1)
    public Mat? GradientMagnitude { get; set; }
    
    // 亚像素边缘点列表
    public List<Point2f> SubPixelEdges { get; set; }
    
    // 筛选后的亚像素边缘点
    public List<Point2f> FilteredSubPixelEdges { get; set; }
    
    // 统计信息
    public int TotalEdgePoints { get; set; }
    public int FilteredEdgePoints { get; set; }
    public double TargetAngle { get; set; }
    public double AngleTolerance { get; set; }
    
    // 释放资源
    public void Dispose();
}
```

## UI 集成

### 在 WPF 应用中使用

项目已包含完整的 WPF UI 实现：

1. **ViewModel**: `DirectionalCannyViewModel.cs`
2. **View**: `DirectionalCannyView.xaml`
3. **集成**: 已在 `MainWindow` 菜单中注册

启动应用后，从菜单选择 **"图像处理" -> "方向筛选Canny边缘检测"**

### UI 功能特性

- ✅ 实时参数调整（滑块控件）
- ✅ 预设参数快速应用
- ✅ 角度方向预设（水平、垂直、对角线等）
- ✅ 4分屏显示结果
  - 原始图像
  - Canny边缘
  - 筛选后边缘
  - 梯度方向可视化
- ✅ 统计信息实时更新
- ✅ 结果保存功能
- ✅ 测试图像生成

## 应用场景

### 1. 水平线检测（如水平线、地平线）
```csharp
var result = detector.DetectEdges(image, 50, 150, 0.0, 10.0);
```

### 2. 垂直线检测（如建筑边缘、柱子）
```csharp
var result = detector.DetectEdges(image, 50, 150, 90.0, 10.0);
```

### 3. 道路边缘检测（通常有特定角度）
```csharp
// 假设道路边缘约为30度
var result = detector.DetectEdges(image, 40, 120, 30.0, 15.0);
```

### 4. 文档边缘检测（矩形，0°和90°）
```csharp
var horizontal = detector.DetectEdges(image, 50, 150, 0.0, 10.0);
var vertical = detector.DetectEdges(image, 50, 150, 90.0, 10.0);
// 合并结果...
```

## 性能考虑

1. **计算复杂度**: O(width × height)
2. **内存占用**: 约 width × height × 20 字节
3. **亚像素优化**: 增加约 20-30% 的计算时间

## 注意事项

1. **输入图像必须是灰度图**（CV_8UC1）
2. **跨越0°的角度范围**: 自动处理（如350°到10°）
3. **内存管理**: 使用 `result.Dispose()` 释放资源
4. **线程安全**: 检测器不是线程安全的，多线程使用需创建多个实例

## 故障排除

### 问题1: 检测不到边缘
**解决**: 降低 `highThreshold`，增加 `angleTolerance`

### 问题2: 边缘太多噪声
**解决**: 提高 `lowThreshold`，减小 `angleTolerance`

### 问题3: 亚像素优化失败
**解决**: 检查图像质量，尝试调整 `subPixelWinSize`

### 问题4: 方向筛选不准确
**解决**: 
- 检查角度定义（0°=水平向右）
- 增加 `angleTolerance`
- 使用更大的 `apertureSize`

## 示例代码

完整的示例代码请参见：
- 测试程序: `TestDirectionalCanny.cs`
- WPF UI: `TulipAlg/Views/DirectionalCannyView.xaml`
- ViewModel: `TulipAlg/ViewModels/DirectionalCannyViewModel.cs`

## 技术实现细节

### 算法流程

1. **Sobel梯度计算** - 计算X和Y方向的梯度
2. **极坐标转换** - 计算梯度幅值和方向
3. **Canny边缘检测** - 标准OpenCV Canny算法
4. **方向筛选** - 逐像素检查梯度方向
5. **亚像素定位** - 使用CornerSubPix优化坐标

### 关键公式

```
梯度方向 = atan2(gradY, gradX) × 180 / π
梯度幅值 = sqrt(gradX² + gradY²)
角度归一化 = angle % 360
```

## 版本历史

- **v1.0** (2025-10-25): 初始版本
  - 基本方向筛选功能
  - 亚像素定位
  - WPF UI集成

## 许可证

本项目遵循与 TulipAlg 主项目相同的许可证。

## 联系方式

如有问题或建议，请通过项目 Issue 系统反馈。
