# CannySubPixelEdge - Canny/Devernay亚像素边缘检测

## 概述

CannySubPixelEdge是Canny/Devernay亚像素边缘检测算法的C++和C#实现。该算法基于Rafael Grompone von Gioi和Gregory Randall的论文实现。

**参考文献:**
- "A Sub-Pixel Edge Detector: an Implementation of the Canny/Devernay Algorithm"
- Image Processing On Line, 2017
- DOI: 10.5201/ipol.2017.216

## 特性

- ✅ **亚像素精度**: 检测到的边缘点具有亚像素级精度
- ✅ **自动链接**: 自动将边缘点链接成连续的曲线
- ✅ **闭合曲线识别**: 自动识别闭合曲线
- ✅ **高斯滤波**: 可选的高斯滤波预处理
- ✅ **滞后阈值**: Canny双阈值滞后处理，减少假边缘
- ✅ **C#封装**: 提供友好的C# API

## 架构

### C++ 层 (TulipAlg.CoreExtern)

1. **CannySubPixelEdge.h/cpp**: 核心C++实现
   - 高斯滤波
   - 梯度计算
   - 非极大值抑制
   - 亚像素校正
   - 边缘点链接
   - 滞后阈值

2. **CannySubPixelEdgeExport.h/cpp**: C风格导出接口
   - 供C# P/Invoke调用的C函数

### C# 层 (TulipAlg.Core)

1. **CannySubPixelEdge.cs**: C#封装类
   - P/Invoke声明
   - 托管对象封装
   - 自动内存管理

2. **CannySubPixelEdgeExample.cs**: 使用示例

## 数据结构

### C# API

#### EdgePoint
```csharp
public struct EdgePoint
{
    public double X { get; set; }  // 亚像素X坐标
    public double Y { get; set; }  // 亚像素Y坐标
}
```

#### EdgeCurve
```csharp
public class EdgeCurve
{
    public List<EdgePoint> Points { get; set; }  // 边缘点列表
    public bool IsClosed { get; set; }            // 是否闭合
    public int Length { get; }                    // 点数
    public double CalculateTotalLength();         // 计算总长度
}
```

#### CannyEdgeResult
```csharp
public class CannyEdgeResult
{
    public List<EdgeCurve> Curves { get; set; }   // 边缘曲线列表
    public int TotalPoints { get; set; }          // 总点数
    public int ImageWidth { get; set; }           // 图像宽度
    public int ImageHeight { get; set; }          // 图像高度
    public int CurveCount { get; }                // 曲线数量
    
    // 辅助方法
    public List<EdgePoint> GetAllPoints();
    public List<EdgeCurve> FilterByLength(int minLength);
    public List<EdgeCurve> FilterByTotalLength(double minTotalLength);
}
```

## 使用方法

### 基本用法

```csharp
using TulipAlg.Core;

// 创建检测器
using (var detector = new CannySubPixelEdge())
{
    // 从byte数组检测边缘
    byte[] image = LoadGrayscaleImage(); // 你的图像数据
    int width = 640;
    int height = 480;
    
    var result = detector.DetectEdgesFromBytes(
        image, width, height,
        sigma: 1.0,   // 高斯滤波标准差
        th_h: 20.0,   // 高阈值
        th_l: 10.0);  // 低阈值
    
    // 处理结果
    if (result != null)
    {
        Console.WriteLine($"检测到 {result.CurveCount} 条边缘");
        
        foreach (var curve in result.Curves)
        {
            Console.WriteLine($"曲线点数: {curve.Length}");
            foreach (var point in curve.Points)
            {
                // 处理每个边缘点
                Console.WriteLine($"点: ({point.X}, {point.Y})");
            }
        }
    }
}
```

### 使用double数组

```csharp
// 如果你已有double数组格式的图像
double[] image = new double[width * height];
// ... 填充图像数据 ...

var result = detector.DetectEdges(
    image, width, height,
    sigma: 1.0,
    th_h: 20.0,
    th_l: 10.0);
```

### 筛选结果

```csharp
var result = detector.DetectEdgesFromBytes(...);

// 只保留长度大于10的曲线
var longCurves = result.FilterByLength(10);

// 只保留欧几里得长度大于50的曲线
var longTotalCurves = result.FilterByTotalLength(50.0);

// 只保留闭合曲线
var closedCurves = result.Curves.FindAll(c => c.IsClosed);

// 获取所有边缘点（展平）
var allPoints = result.GetAllPoints();
```

## 参数说明

### sigma (高斯滤波标准差)
- **范围**: ≥ 0.0
- **建议值**: 0.5 ~ 2.0
- **说明**: 
  - 0.0 表示不进行高斯滤波
  - 较小的值保留更多细节，但可能产生噪声边缘
  - 较大的值减少噪声，但可能丢失细节

### th_h (高阈值)
- **范围**: > 0.0
- **建议值**: 根据图像而定，通常10.0 ~ 50.0
- **说明**: 
  - 梯度模值高于此阈值的点被认为是强边缘
  - 值越大，检测到的边缘越少但质量越高

### th_l (低阈值)
- **范围**: > 0.0, < th_h
- **建议值**: th_h / 2 ~ th_h / 3
- **说明**: 
  - 与强边缘连接的弱边缘点需要高于此阈值
  - 较低的值可以连接更多的边缘段

## 参数调优建议

### 对于噪声图像
```csharp
sigma: 1.5,   // 较大的滤波
th_h: 30.0,   // 较高的阈值
th_l: 15.0
```

### 对于清晰图像
```csharp
sigma: 0.5,   // 轻度滤波
th_h: 15.0,   // 较低的阈值
th_l: 7.0
```

### 对于细节丰富的图像
```csharp
sigma: 0.0,   // 不滤波
th_h: 10.0,   // 低阈值
th_l: 5.0
```

## 性能考虑

1. **图像尺寸**: 算法复杂度为O(n)，其中n为像素数
2. **高斯滤波**: 当sigma > 0时，会增加额外的计算时间
3. **内存**: 需要约 20 * width * height 字节的临时内存

## 注意事项

1. **图像格式**: 
   - 输入必须是灰度图像
   - byte数组：0-255
   - double数组：任意范围

2. **坐标系**: 
   - 原点在左上角
   - X轴向右，Y轴向下
   - 坐标为亚像素精度（浮点数）

3. **内存管理**: 
   - 使用`using`语句确保资源释放
   - 或手动调用`Dispose()`

4. **线程安全**: 
   - 每个实例不是线程安全的
   - 多线程使用时为每个线程创建独立实例

## 示例场景

### 圆形检测
```csharp
var result = detector.DetectEdgesFromBytes(image, w, h, 1.0, 20.0, 10.0);
var closedCurves = result.Curves.FindAll(c => c.IsClosed);
// 进一步拟合圆形...
```

### 线段检测
```csharp
var result = detector.DetectEdgesFromBytes(image, w, h, 1.0, 30.0, 15.0);
var straightCurves = result.Curves.FindAll(c => 
    !c.IsClosed && c.Length > 20);
// 进一步处理直线段...
```

### 轮廓提取
```csharp
var result = detector.DetectEdgesFromBytes(image, w, h, 1.5, 25.0, 12.0);
var mainContours = result.FilterByTotalLength(100.0);
// 使用轮廓进行分割或识别...
```

## 错误处理

```csharp
try
{
    using (var detector = new CannySubPixelEdge())
    {
        var result = detector.DetectEdgesFromBytes(...);
        if (result == null)
        {
            Console.WriteLine("检测失败");
        }
    }
}
catch (ObjectDisposedException)
{
    Console.WriteLine("检测器已释放");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"参数错误: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"错误: {ex.Message}");
}
```

## 编译要求

### C++ (TulipAlg.CoreExtern)
- Visual Studio 2022 或更高版本
- C++20 标准
- Windows SDK

### C# (TulipAlg.Core)
- .NET 8.0 或更高版本
- x64 平台

## 许可证

基于原始实现的GNU Affero General Public License v3.0

## 相关资源

- 原始论文: http://dx.doi.org/10.5201/ipol.2017.216
- IPOL项目: http://www.ipol.im/
