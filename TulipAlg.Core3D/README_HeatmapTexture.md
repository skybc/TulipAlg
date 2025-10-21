# 热度图纹理生成器

TulipAlg.Core3D 中实现的热度图纹理生成和混合功能，基于 OpenCvSharp 实现。

## 功能概述

本模块提供以下核心功能：

1. **热度图生成** (`HeatmapGenerator`)
   - 基于高斯峰的标量场生成
   - 支持多种 OpenCV 色彩映射
   - 自定义高斯中心点配置

2. **纹理生成** (`TextureGenerator`)
   - 分形噪声纹理（多层高斯模糊）
   - 斜线交叉网格纹理（Hatch Pattern）
   - Perlin 噪声纹理（简化版）
   - 点状纹理

3. **纹理混合** (`HeatmapTextureBlender`)
   - Alpha 混合
   - 乘法混合
   - 叠加混合
   - 屏幕混合
   - 加法混合
   - 批量混合和对比

## 使用示例

### 1. 生成基础热度图

```csharp
using TulipAlg.Core3D;
using OpenCvSharp;

// 生成 640x480 的热度图
var heatmap = HeatmapGenerator.Generate(640, 480, colormap: ColormapTypes.Jet);

// 保存
Cv2.ImWrite("heatmap.png", heatmap);
heatmap.Dispose();
```

### 2. 自定义高斯峰

```csharp
// 定义自定义的高斯峰位置
var centers = new[]
{
    new HeatmapGenerator.GaussianCenter(100, 100, 0.08),  // (x, y, sigma比例)
    new HeatmapGenerator.GaussianCenter(500, 150, 0.12),
    new HeatmapGenerator.GaussianCenter(320, 350, 0.15),
};

var heatmap = HeatmapGenerator.Generate(640, 480, centers, ColormapTypes.Inferno);
```

### 3. 生成噪声纹理

```csharp
// 生成分形噪声纹理
var noiseTexture = TextureGenerator.GenerateNoiseTexture(
    width: 640, 
    height: 480, 
    scale: 24.0,      // 缩放因子
    octaves: 5,       // 层数
    seed: 42          // 随机种子
);
```

### 4. 生成斜线纹理

```csharp
// 生成斜线交叉网格纹理
var hatchTexture = TextureGenerator.GenerateHatchTexture(
    width: 640,
    height: 480,
    spacing: 18,         // 线条间距
    angleDegrees: 35,    // 旋转角度
    thickness: 2         // 线条粗细
);
```

### 5. 混合热度图和纹理

```csharp
// 生成热度图
using var heatmap = HeatmapGenerator.Generate(640, 480);

// 生成纹理
using var texture = TextureGenerator.GenerateNoiseTexture(640, 480);

// 混合（乘法模式）
var blended = HeatmapTextureBlender.Blend(
    heatmap, 
    texture, 
    BlendMode.Multiply, 
    alpha: 0.6
);

Cv2.ImWrite("blended.png", blended);
blended.Dispose();
```

### 6. 对比多种混合模式

```csharp
using var heatmap = HeatmapGenerator.Generate(400, 400);
using var texture = TextureGenerator.GenerateNoiseTexture(400, 400);

// 测试所有混合模式
var modes = new[] 
{ 
    BlendMode.Alpha, 
    BlendMode.Multiply, 
    BlendMode.Overlay,
    BlendMode.Screen,
    BlendMode.Add
};

var results = HeatmapTextureBlender.BlendMultiple(heatmap, texture, modes, 0.6);

// 创建对比图
var images = new List<Mat> { heatmap.Clone() };
images.AddRange(results.Values);

var comparison = HeatmapTextureBlender.CreateComparison(images.ToArray());
Cv2.ImWrite("comparison.png", comparison);

// 清理资源
foreach (var result in results.Values)
{
    result.Dispose();
}
foreach (var img in images)
{
    img.Dispose();
}
comparison.Dispose();
```

### 7. 使用预定义示例

```csharp
// 完整示例（原始、噪声混合、斜线混合三合一）
using var comprehensive = HeatmapTextureExample.ComprehensiveExample();
Cv2.ImWrite("comprehensive.png", comprehensive);

// 混合模式对比
using var blendModes = HeatmapTextureExample.BlendModeComparisonExample();
Cv2.ImWrite("blend_modes.png", blendModes);

// 色彩映射对比
using var colormaps = HeatmapTextureExample.ColormapComparisonExample();
Cv2.ImWrite("colormaps.png", colormaps);

// 保存所有示例到目录
HeatmapTextureExample.SaveAllExamples(@"D:\output");
```

## 混合模式说明

| 模式 | 效果 | 适用场景 |
|------|------|----------|
| Alpha | 线性插值混合 | 基础叠加效果 |
| Multiply | 纹理作为亮度调制 | 添加细节，保持热度图主体 |
| Overlay | 结合 multiply 和 screen | 增强对比度 |
| Screen | 提亮效果 | 明亮纹理叠加 |
| Add | 简单相加 | 强化纹理效果 |

## 支持的色彩映射

OpenCV 提供的所有色彩映射都支持，常用的包括：

- `Jet` - 经典的蓝-青-黄-红渐变
- `Inferno` - 黑-紫-橙-黄（科学可视化）
- `Viridis` - 深蓝-绿-黄（色盲友好）
- `Hot` - 黑-红-黄-白（热力图）
- `Rainbow` - 彩虹色谱
- `Magma`, `Plasma`, `Cividis`, `Twilight` 等

## 性能考虑

1. **内存管理**：使用 `using` 语句或手动调用 `Dispose()` 释放 Mat 对象
2. **纹理缓存**：如果重复使用相同参数的纹理，考虑缓存生成的结果
3. **分辨率选择**：高分辨率图像会显著增加计算时间
4. **并行处理**：对于批量处理，可以使用 `Parallel.ForEach` 并行生成

## 参数调优建议

### 噪声纹理参数

- **scale**: 8-32 之间，值越大纹理越粗糙
- **octaves**: 3-6 之间，值越大细节越丰富
- **seed**: 固定种子可获得可重复的结果

### 斜线纹理参数

- **spacing**: 10-30 之间，值越小线条越密集
- **angleDegrees**: 15-45 之间，推荐 30-45 度
- **thickness**: 1-3 之间，根据分辨率调整

### 高斯峰参数

- **sigmaRatio**: 0.05-0.2 之间
  - < 0.1: 窄峰，局部热点
  - 0.1-0.15: 中等范围
  - > 0.15: 宽峰，大范围渐变

## API 参考

### HeatmapGenerator

```csharp
// 生成标量场
Mat GenerateScalarField(int width, int height, GaussianCenter[]? centers = null)

// 应用色彩映射
Mat ApplyColormap(Mat field, ColormapTypes colormap = ColormapTypes.Jet)

// 一步生成完整热度图
Mat Generate(int width, int height, GaussianCenter[]? centers = null, 
             ColormapTypes colormap = ColormapTypes.Jet)
```

### TextureGenerator

```csharp
// 噪声纹理
Mat GenerateNoiseTexture(int width, int height, double scale = 8.0, 
                         int octaves = 4, int? seed = null)

// 斜线纹理
Mat GenerateHatchTexture(int width, int height, int spacing = 12, 
                         double angleDegrees = 30, int thickness = 1)

// Perlin 噪声
Mat GeneratePerlinTexture(int width, int height, double frequency = 0.05, 
                          int? seed = null)

// 点状纹理
Mat GenerateDotTexture(int width, int height, double density = 0.01, 
                       int radius = 3, int? seed = null)
```

### HeatmapTextureBlender

```csharp
// 单次混合
Mat Blend(Mat heatBgr, Mat texture, BlendMode mode = BlendMode.Alpha, 
          double alpha = 0.5)

// 批量混合
Dictionary<BlendMode, Mat> BlendMultiple(Mat heatBgr, Mat texture, 
                                         BlendMode[] modes, double alpha = 0.5)

// 创建对比图
Mat CreateComparison(params Mat[] images)
```

## 与 Python 实现对照

本 C# 实现对应 Python (hot.py) 中的功能：

| Python 函数 | C# 类/方法 |
|-------------|-----------|
| `generate_scalar_field()` | `HeatmapGenerator.GenerateScalarField()` |
| `apply_colormap()` | `HeatmapGenerator.ApplyColormap()` |
| `generate_noise_texture()` | `TextureGenerator.GenerateNoiseTexture()` |
| `generate_hatch_texture()` | `TextureGenerator.GenerateHatchTexture()` |
| `blend_heat_with_texture()` | `HeatmapTextureBlender.Blend()` |

## 扩展建议

可以基于此实现添加：

1. **3D 热度图**：扩展到三维体数据可视化
2. **动画热度图**：生成时间序列的热度图动画
3. **交互式调整**：集成到 WPF UI 中进行实时参数调整
4. **数据导入**：支持从实际数据（如温度传感器）生成热度图
5. **GPU 加速**：使用 OpenCvSharp 的 CUDA 支持加速计算

## 许可证

与 TulipAlg 项目保持一致。
