# 快速参考 - AlgGeometry 新增功能

## 🎯 点-圆功能

```csharp
// 位置关系判断
bool inside = AlgGeometry.IsPointInsideCircle(point, circle);
bool onCircle = AlgGeometry.IsPointOnCircle(point, circle);
bool outside = AlgGeometry.IsPointOutsideCircle(point, circle);

// 点与圆心连线
LineD line = AlgGeometry.LineFromPointToCircleCenter(point, circle);

// 切线计算（返回0/1/2条）
List<LineD> tangents = AlgGeometry.TangentLinesFromPointToCircle(point, circle);

// 最近点
PointD closest = AlgGeometry.ClosestPointOnCircle(point, circle);

// 圆弧最近点
ArcD arc = new ArcD(center, radius, startAngle, endAngle);
PointD closestOnArc = AlgGeometry.ClosestPointOnArc(point, arc);
```

## ⭕ 圆-圆功能

```csharp
// 交点（返回0/1/2个）
List<PointD> intersections = AlgGeometry.CircleIntersectionPoints(circle1, circle2);

// 距离（正=相离，0=相切，负=相交）
double distance = AlgGeometry.DistanceBetweenCircles(circle1, circle2);

// 圆心距离
double centerDist = AlgGeometry.DistanceBetweenCircleCenters(circle1, circle2);
```

## 📐 点-区域功能

```csharp
// 多边形（射线法）
List<PointD> polygon = new List<PointD> { p1, p2, p3, ... };
bool inPolygon = AlgGeometry.IsPointInPolygon(point, polygon);

// 圆形区域
bool inCircle = AlgGeometry.IsPointInCircleRegion(point, circle);
```

## 📊 新增数据结构

```csharp
// 圆弧定义（角度单位：度）
ArcD arc = new ArcD(
    center: new PointD(0, 0),
    radius: 5.0,
    startAngle: 0,    // 从X轴正方向
    endAngle: 90      // 逆时针到90度
);

// 属性
PointD start = arc.StartPoint;  // 起点
PointD end = arc.EndPoint;      // 终点
```

## 💡 常用技巧

### 容差值建议
```csharp
// 高精度（默认）
bool result = AlgGeometry.IsPointOnCircle(point, circle, 1e-10);

// 中等精度（毫米级）
bool result = AlgGeometry.IsPointOnCircle(point, circle, 1e-6);

// 低精度（像素级）
bool result = AlgGeometry.IsPointOnCircle(point, circle, 1e-3);
```

### 圆弧跨越0度
```csharp
// 从350度到10度的圆弧（跨越0度）
ArcD arc = new ArcD(center, radius, 350, 10);
// 算法会自动正确处理
```

### 判断两圆关系
```csharp
double dist = AlgGeometry.DistanceBetweenCircles(c1, c2);
if (dist > 0) 
    Console.WriteLine("相离");
else if (Math.Abs(dist) < 1e-10) 
    Console.WriteLine("相切");
else 
    Console.WriteLine("相交或包含");
```

## 🔧 运行示例

```csharp
// 运行所有示例（在控制台项目中）
GeometryExamples.RunAllExamples();

// 或单独运行
GeometryExamples.DemoPointCircleRelation();
GeometryExamples.DemoTangentLines();
GeometryExamples.DemoCircleIntersection();
// ...等等
```

## 📚 完整文档
查看 `README_GeometryUpdate.md` 获取详细说明
