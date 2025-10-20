# 3D Geometry Library - 快速开始指南

## 📦 安装

将 `Geometry3D` 文件夹复制到你的项目中，并确保命名空间引用：

```csharp
using TulipAlg.Core.Geometry3D;
using TulipAlg.Core.Geometry3D.Algorithms;
```

---

## 🚀 10分钟快速入门

### 1. 基础向量运算

```csharp
// 创建向量
Vector3 v1 = new Vector3(1, 2, 3);
Vector3 v2 = new Vector3(4, 5, 6);

// 基本运算
Vector3 sum = v1 + v2;              // (5, 7, 9)
Vector3 scaled = v1 * 2;            // (2, 4, 6)
double length = v1.Length();        // √14 ≈ 3.742

// 点积和叉积
double dot = v1.Dot(v2);            // 32
Vector3 cross = v1.Cross(v2);       // (-3, 6, -3)

// 归一化
Vector3 unit = v1.Normalize();      // 单位向量

// 夹角
double angle = v1.AngleTo(v2);      // 弧度
```

### 2. 点和直线

```csharp
// 创建点
Point3D p1 = new Point3D(0, 0, 0);
Point3D p2 = new Point3D(3, 4, 0);

// 距离计算
double distance = p1.DistanceTo(p2);  // 5.0

// 创建直线
Line3D line = Line3D.FromTwoPoints(p1, p2);

// 点到直线距离
Point3D testPoint = new Point3D(0, 5, 0);
double distToLine = line.DistanceToPoint(testPoint);

// 最近点
Point3D closest = line.ClosestPointTo(testPoint);
```

### 3. 平面操作

```csharp
// 方法1：通过法向量和点
Vector3 normal = new Vector3(0, 0, 1);  // Z轴方向
Point3D point = new Point3D(0, 0, 5);
Plane3D plane1 = new Plane3D(normal, point);

// 方法2：通过三点
Point3D p1 = new Point3D(0, 0, 0);
Point3D p2 = new Point3D(1, 0, 0);
Point3D p3 = new Point3D(0, 1, 0);
Plane3D plane2 = Plane3D.FromThreePoints(p1, p2, p3);

// 点到平面距离
double dist = plane1.DistanceToPoint(new Point3D(0, 0, 10));  // 5.0

// 投影点
Point3D projected = plane1.ProjectPoint(new Point3D(3, 4, 10));

// 直线与平面交点
Line3D line = new Line3D(new Point3D(0, 0, 10), new Vector3(0, 0, -1));
if (plane1.IntersectLine(line, out Point3D intersection, out double t))
{
    Console.WriteLine($"交点: {intersection}, t={t}");
}
```

### 4. 三角形与光线相交

```csharp
// 定义三角形
Triangle3D triangle = new Triangle3D(
    new Point3D(0, 0, 0),
    new Point3D(5, 0, 0),
    new Point3D(0, 5, 0)
);

// 光线
Point3D rayOrigin = new Point3D(1, 1, 5);
Vector3 rayDirection = new Vector3(0, 0, -1);

// 相交检测（Möller-Trumbore算法）
bool hit = triangle.RayIntersection(
    rayOrigin, 
    rayDirection, 
    out Point3D intersection,
    out double t,
    out double u,
    out double v
);

if (hit)
{
    Console.WriteLine($"相交点: {intersection}");
    Console.WriteLine($"重心坐标: ({u}, {v}, {1-u-v})");
}
```

### 5. AABB包围盒

```csharp
// 从点集创建
List<Point3D> points = new List<Point3D>
{
    new Point3D(0, 0, 0),
    new Point3D(10, 5, 3),
    new Point3D(-2, 8, 1)
};

BoundingBox3D bbox = BoundingBox3D.FromPoints(points);

// 属性
Point3D center = bbox.Center;
Vector3 size = bbox.Size;
double volume = bbox.Volume;

// 点包含测试
bool contains = bbox.Contains(new Point3D(5, 3, 1));

// 包围盒相交
BoundingBox3D bbox2 = new BoundingBox3D(
    new Point3D(5, 0, 0),
    new Point3D(15, 10, 5)
);
bool intersects = bbox.Intersects(bbox2);

// 光线相交
bool rayHit = bbox.RayIntersection(
    new Point3D(-10, 0, 0),
    Vector3.UnitX,
    out double tMin,
    out double tMax
);
```

### 6. 矩阵变换

```csharp
Point3D point = new Point3D(1, 0, 0);

// 平移
Matrix4x4 translation = Matrix4x4.CreateTranslation(5, 3, 2);
Point3D translated = translation.Transform(point);

// 旋转（绕Z轴90度）
Matrix4x4 rotation = Matrix4x4.CreateRotationZ(Math.PI / 2);
Point3D rotated = rotation.Transform(point);  // 约(0, 1, 0)

// 缩放
Matrix4x4 scaling = Matrix4x4.CreateScale(2, 2, 2);
Point3D scaled = scaling.Transform(point);

// 组合变换（注意顺序：右到左）
Matrix4x4 combined = translation * rotation * scaling;
Point3D transformed = combined.Transform(point);

// Rodrigues旋转（绕任意轴）
Vector3 axis = new Vector3(1, 1, 1).Normalize();
Matrix4x4 rodrigues = Matrix4x4.CreateRotation(axis, Math.PI / 4);
Point3D result = rodrigues.Transform(point);
```

---

## 🧮 高级算法示例

### RANSAC 平面拟合

```csharp
// 准备点云（含噪声和异常值）
List<Point3D> pointCloud = LoadPointCloud();

// 配置RANSAC
var ransac = new RansacPlaneFitting(seed: 42)
{
    MaxIterations = 1000,
    DistanceThreshold = 0.01,
    MinInlierRatio = 0.8
};

// 拟合平面
var result = ransac.FitPlane(pointCloud);

Console.WriteLine($"拟合平面: {result.Plane}");
Console.WriteLine($"内点数: {result.InlierCount} / {pointCloud.Count}");
Console.WriteLine($"内点率: {result.InlierRatio:P2}");

// 获取内点
List<Point3D> inliers = result.Inliers;
```

### KD-Tree 最近邻搜索

```csharp
// 构建KD树
List<Point3D> points = GenerateRandomPoints(10000);
var kdTree = new KDTree(points);

// 单个最近邻
Point3D query = new Point3D(50, 50, 50);
Point3D nearest = kdTree.FindNearest(query);
Console.WriteLine($"最近点: {nearest}, 距离: {query.DistanceTo(nearest)}");

// K最近邻
int k = 10;
List<Point3D> kNearest = kdTree.FindKNearest(query, k);
foreach (var p in kNearest)
{
    Console.WriteLine($"  {p}, 距离: {query.DistanceTo(p):F3}");
}

// 范围查询
var bbox = new BoundingBox3D(
    new Point3D(40, 40, 40),
    new Point3D(60, 60, 60)
);
List<Point3D> inRange = kdTree.RangeQuery(bbox);
```

### ICP 点云配准

```csharp
// 加载源点云和目标点云
List<Point3D> source = LoadPointCloud("source.ply");
List<Point3D> target = LoadPointCloud("target.ply");

// 配置ICP
var icp = new ICP
{
    MaxIterations = 50,
    ConvergenceThreshold = 1e-6,
    MaxCorrespondenceDistance = 1.0
};

// 执行配准
var result = icp.Align(source, target);

Console.WriteLine($"收敛: {result.Converged}");
Console.WriteLine($"迭代次数: {result.Iterations}");
Console.WriteLine($"最终误差: {result.FinalError:F6}");

// 应用变换
List<Point3D> aligned = result.AlignedSource;

// 获取变换矩阵
Matrix4x4 transform = result.Transform;
```

### 凸包计算

```csharp
// 生成点云
List<Point3D> points = GenerateRandomPoints(100);

// 计算凸包
var convexHull = new ConvexHull3D();
var result = convexHull.Compute(points);

Console.WriteLine($"凸包顶点数: {result.Vertices.Count}");
Console.WriteLine($"凸包面数: {result.FaceCount}");

// 访问凸包面
foreach (var triangle in result.Triangles)
{
    Console.WriteLine($"面: {triangle}");
    double area = triangle.Area();
    Vector3 normal = triangle.Normal();
}
```

### 最小包围球

```csharp
List<Point3D> points = GenerateRandomPoints(50);

// 计算最小包围球（Welzl算法）
var sphere = BoundingSphere.ComputeMinimalSphere(points);

Console.WriteLine($"球心: {sphere.Center}");
Console.WriteLine($"半径: {sphere.Radius}");

// 验证所有点在球内
bool allInside = points.All(p => sphere.Contains(p));
Console.WriteLine($"所有点在球内: {allInside}");

// 球相交测试
var sphere2 = new BoundingSphere(new Point3D(10, 0, 0), 5);
bool intersects = sphere.Intersects(sphere2);
```

---

## 🔧 实用工具函数

### 生成测试数据

```csharp
// 生成随机点云
List<Point3D> GenerateRandomPoints(int count, double range = 100.0)
{
    var random = new Random();
    return Enumerable.Range(0, count)
        .Select(_ => new Point3D(
            random.NextDouble() * range,
            random.NextDouble() * range,
            random.NextDouble() * range
        ))
        .ToList();
}

// 生成平面附近的噪声点云
List<Point3D> GeneratePlanePoints(int count, Plane3D plane, double noise = 0.1)
{
    var random = new Random();
    var points = new List<Point3D>();
    
    for (int i = 0; i < count; i++)
    {
        // 在平面上随机生成点
        double x = random.NextDouble() * 10 - 5;
        double y = random.NextDouble() * 10 - 5;
        double z = (-plane.Normal.X * x - plane.Normal.Y * y - plane.D) / plane.Normal.Z;
        
        var point = new Point3D(x, y, z);
        
        // 添加噪声
        point = point + random.NextDouble() * noise * plane.Normal;
        points.Add(point);
    }
    
    return points;
}
```

### 坐标系转换

```csharp
// 世界坐标 -> 相机坐标
Point3D WorldToCamera(Point3D worldPoint, Point3D cameraPos, Vector3 forward, Vector3 up)
{
    // 构建视图矩阵
    var viewMatrix = Matrix4x4.CreateLookAt(cameraPos, cameraPos + forward, up);
    return viewMatrix.Transform(worldPoint);
}

// 相机坐标 -> 屏幕坐标（透视投影）
Point3D CameraToScreen(Point3D cameraPoint, double fov, double aspect, double near, double far)
{
    var projMatrix = Matrix4x4.CreatePerspective(fov, aspect, near, far);
    return projMatrix.Transform(cameraPoint);
}
```

---

## ⚡ 性能优化技巧

### 1. 使用平方距离

```csharp
// 慢
if (p1.DistanceTo(p2) < threshold)
{
    // ...
}

// 快（避免sqrt）
double thresholdSq = threshold * threshold;
if (p1.DistanceSquaredTo(p2) < thresholdSq)
{
    // ...
}
```

### 2. 批量变换

```csharp
// 预计算变换矩阵
Matrix4x4 transform = Matrix4x4.CreateTranslation(5, 3, 2) 
                    * Matrix4x4.CreateRotationZ(Math.PI / 4);

// 批量应用
var transformed = points
    .AsParallel()  // 并行化
    .Select(p => transform.Transform(p))
    .ToList();
```

### 3. 空间索引

```csharp
// 对大量查询使用KD-Tree
var kdTree = new KDTree(largePointCloud);

foreach (var query in queries)
{
    var nearest = kdTree.FindNearest(query);  // O(log n)
}
```

---

## 📚 更多示例

完整示例代码请参见：
- `Geometry3DExamples.cs` - 10个详细示例
- `Geometry3D_README.md` - 完整文档

---

## 🐛 常见问题

### Q: 如何判断点在三角形内？
```csharp
var triangle = new Triangle3D(v0, v1, v2);
bool inside = triangle.ContainsPoint(testPoint);
```

### Q: 如何计算两平面的交线？
```csharp
if (plane1.IntersectPlane(plane2, out Line3D intersection))
{
    Console.WriteLine($"交线: {intersection}");
}
```

### Q: 如何处理共面/共线情况？
大部分方法内置了退化情况处理，会返回 `null` 或抛出异常。

---

## 💡 下一步

- 阅读完整文档：`Geometry3D_README.md`
- 运行示例：`Geometry3DExamples.RunAllExamples()`
- 查看算法原理：每个类都有详细的数学公式注释

---

**祝编程愉快！** 🎉
