# TulipAlg.Core 几何功能更新说明

## 新增功能概览

本次更新为 `AlgGeometry` 类添加了完整的点-圆、圆-圆、点-区域相关的几何运算功能。

---

## 一、新增数据结构

### ArcD 结构体
表示圆弧，包含以下属性：
- `Center`: 圆心（PointD）
- `Radius`: 半径（double）
- `StartAngle`: 起始角度，单位：度（double）
- `EndAngle`: 结束角度，单位：度（double）
- `StartPoint`: 圆弧起点（只读属性）
- `EndPoint`: 圆弧终点（只读属性）

**注意**：角度从X轴正方向开始，逆时针方向为正。

---

## 二、点-圆功能（#region 点-圆）

### 1. 点与圆的位置关系

#### IsPointInsideCircle
判断点是否在圆内。
```csharp
public static bool IsPointInsideCircle(PointD point, CircleD circle, double tolerance = 1e-10)
```
- **参数**：
  - `point`: 待判断的点
  - `circle`: 圆
  - `tolerance`: 容差值（默认 1e-10）
- **返回值**：在圆内返回 `true`，否则返回 `false`

#### IsPointOnCircle
判断点是否在圆上。
```csharp
public static bool IsPointOnCircle(PointD point, CircleD circle, double tolerance = 1e-10)
```
- **参数**：同上
- **返回值**：在圆上返回 `true`，否则返回 `false`

#### IsPointOutsideCircle
判断点是否在圆外。
```csharp
public static bool IsPointOutsideCircle(PointD point, CircleD circle, double tolerance = 1e-10)
```
- **参数**：同上
- **返回值**：在圆外返回 `true`，否则返回 `false`

---

### 2. 点与圆心的连线

#### LineFromPointToCircleCenter
获取从点到圆心的连线。
```csharp
public static LineD LineFromPointToCircleCenter(PointD point, CircleD circle)
```
- **参数**：
  - `point`: 点
  - `circle`: 圆
- **返回值**：从点到圆心的线段（LineD）

---

### 3. 点到圆的最近点

#### ClosestPointOnCircle
计算圆周上距离给定点最近的点。
```csharp
public static PointD ClosestPointOnCircle(PointD point, CircleD circle)
```
- **参数**：
  - `point`: 外部点
  - `circle`: 圆
- **返回值**：圆周上的最近点
- **特殊情况**：如果点在圆心，返回圆右侧点（半径方向）

---

### 4. 点到圆的切线

#### TangentLinesFromPointToCircle
计算从点到圆的切线。
```csharp
public static List<LineD> TangentLinesFromPointToCircle(PointD point, CircleD circle, double tolerance = 1e-10)
```
- **参数**：
  - `point`: 外部点
  - `circle`: 圆
  - `tolerance`: 容差值
- **返回值**：切线列表
  - 点在圆内：返回空列表（0条切线）
  - 点在圆上：返回1条切线（垂直于半径）
  - 点在圆外：返回2条切线（从点到两个切点）

---

### 5. 点到圆弧的最近点

#### ClosestPointOnArc
计算圆弧上距离给定点最近的点。
```csharp
public static PointD ClosestPointOnArc(PointD point, ArcD arc)
```
- **参数**：
  - `point`: 点
  - `arc`: 圆弧
- **返回值**：圆弧上的最近点
- **算法说明**：
  - 如果点的角度在圆弧范围内，返回圆周上对应角度的点
  - 如果点的角度不在圆弧范围内，返回圆弧的端点之一（取距离较近的）

---

## 三、圆-圆功能（#region 圆-圆）

### 1. 两圆交点

#### CircleIntersectionPoints
计算两圆的交点。
```csharp
public static List<PointD> CircleIntersectionPoints(CircleD circle1, CircleD circle2, double tolerance = 1e-10)
```
- **参数**：
  - `circle1`: 第一个圆
  - `circle2`: 第二个圆
  - `tolerance`: 容差值
- **返回值**：交点列表
  - 0个交点：两圆相离、一圆包含另一圆、圆心重合
  - 1个交点：两圆外切或内切
  - 2个交点：两圆相交

---

### 2. 两圆距离

#### DistanceBetweenCircles
计算两圆之间的距离（圆周之间的最短距离）。
```csharp
public static double DistanceBetweenCircles(CircleD circle1, CircleD circle2)
```
- **参数**：
  - `circle1`: 第一个圆
  - `circle2`: 第二个圆
- **返回值**：两圆距离
  - **正值**：两圆相离，表示圆周之间的距离
  - **0**：两圆相切
  - **负值**：两圆相交或包含，表示重叠深度

#### DistanceBetweenCircleCenters
计算两圆圆心之间的距离。
```csharp
public static double DistanceBetweenCircleCenters(CircleD circle1, CircleD circle2)
```
- **参数**：同上
- **返回值**：圆心距离

---

## 四、点-区域功能（#region 点-区域）

### 1. 点是否在多边形内

#### IsPointInPolygon
使用射线法判断点是否在多边形内。
```csharp
public static bool IsPointInPolygon(PointD point, List<PointD> polygon)
```
- **参数**：
  - `point`: 待判断的点
  - `polygon`: 多边形顶点列表（按顺序，至少3个顶点）
- **返回值**：在多边形内返回 `true`，否则返回 `false`
- **算法**：射线法（从点向右发射射线，统计与多边形边的交点数量）
  - 奇数个交点：点在多边形内
  - 偶数个交点：点在多边形外

---

### 2. 点是否在圆形区域内

#### IsPointInCircleRegion
判断点是否在圆形区域内（包括圆周）。
```csharp
public static bool IsPointInCircleRegion(PointD point, CircleD circle, double tolerance = 1e-10)
```
- **参数**：
  - `point`: 待判断的点
  - `circle`: 圆形区域
  - `tolerance`: 容差值
- **返回值**：在区域内（包括圆周）返回 `true`，否则返回 `false`

---

## 五、使用示例

### 示例1：判断点与圆的位置关系
```csharp
CircleD circle = new CircleD(new PointD(0, 0), 5.0);
PointD point = new PointD(3, 4);

bool inside = AlgGeometry.IsPointInsideCircle(point, circle);
bool onCircle = AlgGeometry.IsPointOnCircle(point, circle);
bool outside = AlgGeometry.IsPointOutsideCircle(point, circle);
```

### 示例2：计算点到圆的切线
```csharp
CircleD circle = new CircleD(new PointD(0, 0), 5.0);
PointD point = new PointD(10, 0);

List<LineD> tangents = AlgGeometry.TangentLinesFromPointToCircle(point, circle);
// 从圆外的点会得到2条切线
```

### 示例3：计算两圆交点
```csharp
CircleD circle1 = new CircleD(new PointD(0, 0), 5.0);
CircleD circle2 = new CircleD(new PointD(6, 0), 5.0);

List<PointD> intersections = AlgGeometry.CircleIntersectionPoints(circle1, circle2);
// 两圆相交会得到2个交点
```

### 示例4：点到圆弧的最近点
```csharp
// 圆弧：圆心(0,0)，半径5，从0度到90度
ArcD arc = new ArcD(new PointD(0, 0), 5.0, 0, 90);
PointD point = new PointD(10, 5);

PointD closest = AlgGeometry.ClosestPointOnArc(point, arc);
```

### 示例5：判断点是否在多边形内
```csharp
List<PointD> square = new List<PointD>
{
    new PointD(0, 0),
    new PointD(10, 0),
    new PointD(10, 10),
    new PointD(0, 10)
};

PointD point = new PointD(5, 5);
bool isInside = AlgGeometry.IsPointInPolygon(point, square); // true
```

---

## 六、完整示例代码

`GeometryExamples.cs` 文件提供了所有功能的完整示例代码，可以通过以下方式运行：

```csharp
GeometryExamples.RunAllExamples();
```

这将依次演示所有新增功能的使用方法。

---

## 七、注意事项

1. **容差值**：默认容差值为 `1e-10`，用于浮点数比较。如果需要更宽松的判断，可以传入更大的容差值。

2. **角度单位**：所有角度参数和返回值都使用**度（°）**作为单位，而非弧度。

3. **圆弧角度**：圆弧的角度从X轴正方向开始，逆时针方向为正。角度会自动归一化到 [0, 360) 范围。

4. **射线法边界情况**：使用射线法判断点是否在多边形内时，点正好在边上的情况可能需要特殊处理。

5. **性能考虑**：复杂多边形的点包含判断时间复杂度为 O(n)，其中 n 是多边形顶点数量。

---

## 八、技术细节

### 两圆交点算法
使用解析几何方法：
1. 计算两圆心距离 d
2. 判断相对位置（相离/相切/相交/包含）
3. 对于相交情况，使用勾股定理计算交点坐标

### 点到圆弧最近点算法
1. 计算点相对圆心的角度
2. 判断角度是否在圆弧范围内
3. 若在范围内，返回圆周上对应角度的点
4. 若不在范围内，比较到两端点的距离，返回较近的端点

### 射线法（点在多边形内）
从点向右发射水平射线，统计与多边形边的交点数量。交点数为奇数则点在多边形内，偶数则在多边形外。

---

## 九、测试建议

建议测试以下边界情况：
1. 点在圆心时的各种操作
2. 圆半径为0的情况
3. 圆弧跨越0度的情况（如 StartAngle=350°, EndAngle=10°）
4. 两圆完全重合的情况
5. 退化的多边形（顶点共线）
6. 点正好在多边形边上或顶点上的情况

---

## 更新日期
2025年10月17日
