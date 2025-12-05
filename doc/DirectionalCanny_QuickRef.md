# 方向筛选Canny边缘检测 - 快速参考

## 快速示例

```csharp
using OpenCvSharp;
using TulipAlg.Core;

// 1. 加载图像
using var image = Cv2.ImRead("input.jpg", ImreadModes.Grayscale);

// 2. 创建检测器并执行
var detector = new DirectionalCannyEdge();
var result = detector.DetectEdges(
    image, 
    lowThreshold: 50, 
    highThreshold: 150,
    targetAngle: 0.0,      // 水平方向
    angleTolerance: 15.0   // ±15度
);

// 3. 查看结果
Console.WriteLine($"筛选后边缘点: {result.FilteredEdgePoints}");
Cv2.ImWrite("result.png", result.FilteredEdges);
result.Dispose();
```

## 常用角度预设

| 方向 | targetAngle | 说明 |
|------|-------------|------|
| 水平 | 0° 或 180° | 水平线、地平线 |
| 垂直 | 90° 或 270° | 垂直线、建筑边缘 |
| 对角线↗ | 45° | 左下到右上 |
| 对角线↘ | 135° | 左上到右下 |
| 全方向 | 任意 + tolerance=180° | 不筛选方向 |

## 参数快速选择

### 场景1: 清晰图像
```csharp
lowThreshold: 30, highThreshold: 90, angleTolerance: 10
```

### 场景2: 噪声图像
```csharp
lowThreshold: 80, highThreshold: 200, angleTolerance: 15
```

### 场景3: 高精度方向
```csharp
lowThreshold: 50, highThreshold: 150, angleTolerance: 5
```

## API 速查

### DetectEdges
```csharp
DirectionalCannyResult DetectEdges(
    Mat image,              // 灰度图输入
    double lowThreshold,    // 30-100 推荐
    double highThreshold,   // 50-200 推荐
    double targetAngle,     // 0-360度
    double angleTolerance,  // 5-30度推荐
    int apertureSize = 3,   // 3, 5, 7
    bool useSubPixel = true,
    int subPixelWinSize = 5 // 3, 5, 7, 9, 11
)
```

### 结果对象
```csharp
result.CannyEdges           // Mat: 原始边缘
result.FilteredEdges        // Mat: 筛选后边缘
result.GradientDirection    // Mat: 方向图
result.TotalEdgePoints      // int: 总点数
result.FilteredEdgePoints   // int: 筛选后点数
result.SubPixelEdges        // List<Point2f>: 亚像素点
```

## 可视化方法

```csharp
// 方法1: 彩色叠加
var vis = DirectionalCannyEdge.CreateVisualization(
    image, result.FilteredEdges, new Scalar(0, 255, 0));

// 方法2: 方向颜色图
var dirVis = DirectionalCannyEdge.CreateDirectionVisualization(
    result.GradientDirection, result.GradientMagnitude);
```

## 角度定义图示

```
        90°
         |
   135°  |  45°
         |
180° ----+---- 0°
         |
   225°  |  315°
         |
       270°
```

## UI 使用

1. 启动应用 -> "图像处理" -> "方向筛选Canny边缘检测"
2. 点击"加载图像"或"生成测试图像"
3. 调整参数（滑块）
4. 点击"执行边缘检测"
5. 查看4分屏结果

## 故障排除速查

| 问题 | 解决方案 |
|------|----------|
| 检测不到边缘 | 降低highThreshold |
| 边缘太多 | 提高lowThreshold |
| 方向不准 | 增加angleTolerance |
| 噪声太多 | 增大apertureSize |
| 亚像素失败 | 调整subPixelWinSize |

## 常见组合

### 水平线检测
```csharp
detector.DetectEdges(image, 50, 150, 0, 10);
```

### 垂直线检测
```csharp
detector.DetectEdges(image, 50, 150, 90, 10);
```

### 矩形检测（水平+垂直）
```csharp
var h = detector.DetectEdges(image, 50, 150, 0, 10);
var v = detector.DetectEdges(image, 50, 150, 90, 10);
```

### 所有边缘（不筛选）
```csharp
detector.DetectEdges(image, 50, 150, 0, 180);
```

## 性能提示

- 大图像: 降低分辨率后处理
- 实时处理: 设置 `useSubPixel = false`
- 批量处理: 重用同一个 detector 实例
- 内存优化: 及时调用 `result.Dispose()`

## 完整文档

详细文档请参见: `doc/DirectionalCanny_README.md`
