# CannySubPixelEdge 快速参考

## 快速开始

```csharp
using TulipAlg.Core;

// 1. 创建检测器
using var detector = new CannySubPixelEdge();

// 2. 准备图像数据 (灰度图像)
byte[] image = ...; // 你的图像数据
int width = 640;
int height = 480;

// 3. 执行检测
var result = detector.DetectEdgesFromBytes(
    image, width, height,
    sigma: 1.0,   // 高斯滤波
    th_h: 20.0,   // 高阈值
    th_l: 10.0);  // 低阈值

// 4. 使用结果
foreach (var curve in result.Curves)
{
    foreach (var point in curve.Points)
    {
        // 处理边缘点 (point.X, point.Y)
    }
}
```

## API 速查

### 主类
| 类名 | 说明 |
|------|------|
| `CannySubPixelEdge` | 边缘检测器主类 |
| `CannyEdgeResult` | 检测结果 |
| `EdgeCurve` | 单条边缘曲线 |
| `EdgePoint` | 边缘点 |

### 主要方法

#### DetectEdgesFromBytes
```csharp
CannyEdgeResult? DetectEdgesFromBytes(
    byte[] image,      // 输入图像 (0-255)
    int width,         // 宽度
    int height,        // 高度
    double sigma,      // 高斯滤波标准差
    double th_h,       // 高阈值
    double th_l);      // 低阈值
```

#### DetectEdges
```csharp
CannyEdgeResult? DetectEdges(
    double[] image,    // 输入图像 (double)
    int width,         // 宽度
    int height,        // 高度
    double sigma,      // 高斯滤波标准差
    double th_h,       // 高阈值
    double th_l);      // 低阈值
```

### 结果处理

```csharp
// 获取信息
int curveCount = result.CurveCount;        // 曲线数
int totalPoints = result.TotalPoints;      // 总点数

// 获取所有点
List<EdgePoint> allPoints = result.GetAllPoints();

// 筛选曲线
List<EdgeCurve> longCurves = result.FilterByLength(10);
List<EdgeCurve> longTotalCurves = result.FilterByTotalLength(50.0);

// 遍历曲线
foreach (var curve in result.Curves)
{
    int length = curve.Length;                     // 点数
    bool closed = curve.IsClosed;                  // 是否闭合
    double totalLength = curve.CalculateTotalLength(); // 总长度
    
    foreach (var point in curve.Points)
    {
        double x = point.X;  // 亚像素X坐标
        double y = point.Y;  // 亚像素Y坐标
    }
}
```

## 参数推荐值

### 通用场景
```csharp
sigma: 1.0, th_h: 20.0, th_l: 10.0
```

### 噪声图像
```csharp
sigma: 1.5, th_h: 30.0, th_l: 15.0
```

### 清晰图像
```csharp
sigma: 0.5, th_h: 15.0, th_l: 7.0
```

### 细节丰富
```csharp
sigma: 0.0, th_h: 10.0, th_l: 5.0
```

### 仅强边缘
```csharp
sigma: 1.0, th_h: 40.0, th_l: 20.0
```

## 常见用法

### 1. 基本检测
```csharp
using var detector = new CannySubPixelEdge();
var result = detector.DetectEdgesFromBytes(image, w, h, 1.0, 20.0, 10.0);
```

### 2. 只保留长曲线
```csharp
var result = detector.DetectEdgesFromBytes(image, w, h, 1.0, 20.0, 10.0);
var longCurves = result.FilterByLength(20);
```

### 3. 只保留闭合曲线
```csharp
var result = detector.DetectEdgesFromBytes(image, w, h, 1.0, 20.0, 10.0);
var closedCurves = result.Curves.FindAll(c => c.IsClosed);
```

### 4. 计算曲线统计
```csharp
var result = detector.DetectEdgesFromBytes(image, w, h, 1.0, 20.0, 10.0);
foreach (var curve in result.Curves)
{
    double avgX = curve.Points.Average(p => p.X);
    double avgY = curve.Points.Average(p => p.Y);
    double length = curve.CalculateTotalLength();
    Console.WriteLine($"中心: ({avgX}, {avgY}), 长度: {length}");
}
```

### 5. 转换为其他格式
```csharp
var result = detector.DetectEdgesFromBytes(image, w, h, 1.0, 20.0, 10.0);

// 转换为PointD列表
var pointDList = result.GetAllPoints()
    .Select(p => new PointD(p.X, p.Y))
    .ToList();

// 转换为LineD列表（相邻点连线）
var lines = new List<LineD>();
foreach (var curve in result.Curves)
{
    for (int i = 0; i < curve.Points.Count - 1; i++)
    {
        var p1 = curve.Points[i];
        var p2 = curve.Points[i + 1];
        lines.Add(new LineD(
            new PointD(p1.X, p1.Y),
            new PointD(p2.X, p2.Y)));
    }
}
```

### 6. 参数自动调优
```csharp
double[] sigmas = { 0.5, 1.0, 1.5, 2.0 };
double[] thresholds = { 10.0, 15.0, 20.0, 25.0, 30.0 };

var bestResult = (CannyEdgeResult?)null;
int maxCurves = 0;

using var detector = new CannySubPixelEdge();
foreach (var sigma in sigmas)
{
    foreach (var th_h in thresholds)
    {
        var result = detector.DetectEdgesFromBytes(
            image, w, h, sigma, th_h, th_h / 2.0);
        
        if (result != null && result.CurveCount > maxCurves)
        {
            maxCurves = result.CurveCount;
            bestResult = result;
        }
    }
}
```

## 常见问题

### Q: 没有检测到边缘？
- 检查阈值是否过高
- 尝试降低 th_h 和 th_l
- 确认图像是否有明显边缘

### Q: 检测到太多噪声？
- 增大 sigma 进行更强的滤波
- 提高 th_h 和 th_l 阈值
- 使用 FilterByLength() 过滤短曲线

### Q: 边缘不连续？
- 降低 th_l 使更多点被连接
- 增大 sigma 平滑图像
- 检查图像质量

### Q: 内存不足？
- 处理较小的图像区域
- 减少同时检测的图像数量
- 及时释放不用的结果

### Q: 性能慢？
- 减小 sigma（或设为0）
- 处理缩小的图像
- 使用多线程并行处理多张图像

## 性能提示

1. **复用检测器**: 
   ```csharp
   using var detector = new CannySubPixelEdge();
   foreach (var img in images)
   {
       var result = detector.DetectEdgesFromBytes(...);
   }
   ```

2. **批处理**:
   ```csharp
   var detectors = Enumerable.Range(0, threadCount)
       .Select(_ => new CannySubPixelEdge()).ToArray();
   
   Parallel.For(0, images.Length, i =>
   {
       var detector = detectors[i % threadCount];
       var result = detector.DetectEdgesFromBytes(...);
   });
   
   foreach (var d in detectors) d.Dispose();
   ```

3. **图像预处理**: 在调用检测前进行缩放和降噪

## 集成示例

### 与OpenCvSharp集成
```csharp
using OpenCvSharp;
using TulipAlg.Core;

// 加载图像
using var mat = Cv2.ImRead("image.jpg", ImreadModes.Grayscale);
byte[] imageData = new byte[mat.Width * mat.Height];
Marshal.Copy(mat.Data, imageData, 0, imageData.Length);

// 检测边缘
using var detector = new CannySubPixelEdge();
var result = detector.DetectEdgesFromBytes(
    imageData, mat.Width, mat.Height, 1.0, 20.0, 10.0);

// 在图像上绘制边缘
using var color = mat.CvtColor(ColorConversionCodes.GRAY2BGR);
foreach (var curve in result.Curves)
{
    for (int i = 0; i < curve.Points.Count - 1; i++)
    {
        var p1 = new Point((int)curve.Points[i].X, (int)curve.Points[i].Y);
        var p2 = new Point((int)curve.Points[i + 1].X, (int)curve.Points[i + 1].Y);
        Cv2.Line(color, p1, p2, Scalar.Red, 2);
    }
}
Cv2.ImShow("Edges", color);
Cv2.WaitKey();
```

## 完整示例

参见 `CannySubPixelEdgeExample.cs` 获取更多示例。
