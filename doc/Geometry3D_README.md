# 3D Geometry Library for C#

一个功能完整的三维几何库，提供点云处理、几何计算、变换矩阵和高级算法支持。

## 📋 目录

- [核心数据结构](#核心数据结构)
- [向量与矩阵运算](#向量与矩阵运算)
- [几何关系计算](#几何关系计算)
- [高级算法](#高级算法)
- [使用示例](#使用示例)
- [数学公式参考](#数学公式参考)

---

## 🎯 核心数据结构

### Point3D - 三维点

表示三维空间中的点。

**属性：**
- `X`, `Y`, `Z` - 坐标值

**方法：**
- `DistanceTo(Point3D)` - 欧氏距离：$d = \sqrt{(x_2-x_1)^2 + (y_2-y_1)^2 + (z_2-z_1)^2}$
- `DistanceSquaredTo(Point3D)` - 平方距离（性能优化）
- `Lerp(Point3D, Point3D, double)` - 线性插值

**运算符：**
- `Point + Vector` - 点平移
- `Point - Point` - 得到向量

---

### Vector3 - 三维向量

表示方向和大小。

**基本属性：**
- `X`, `Y`, `Z` - 分量
- `UnitX`, `UnitY`, `UnitZ` - 单位向量

**方法：**

#### 长度与归一化
```csharp
double Length()           // ||v|| = √(x² + y² + z²)
double LengthSquared()    // ||v||² = x² + y² + z²
Vector3 Normalize()       // û = v / ||v||
```

#### 点积与叉积
```csharp
double Dot(Vector3 other)      // v₁ · v₂ = x₁x₂ + y₁y₂ + z₁z₂
Vector3 Cross(Vector3 other)   // v₁ × v₂
```

**数学公式：**

点积：
$$\vec{v_1} \cdot \vec{v_2} = |\vec{v_1}| |\vec{v_2}| \cos(\theta)$$

叉积：
$$\vec{v_1} \times \vec{v_2} = \begin{vmatrix} \vec{i} & \vec{j} & \vec{k} \\ x_1 & y_1 & z_1 \\ x_2 & y_2 & z_2 \end{vmatrix}$$

#### 向量投影
```csharp
Vector3 ProjectOnto(Vector3 target)  // proj_v₂(v₁)
double AngleTo(Vector3 other)         // θ = arccos(v₁·v₂ / ||v₁||||v₂||)
Vector3 Reflect(Vector3 normal)       // r = v - 2(v·n)n
```

---

### Line3D - 三维直线

参数方程：$L(t) = P + t \cdot \vec{d}$

**构造：**
```csharp
new Line3D(Point3D point, Vector3 direction)
Line3D.FromTwoPoints(Point3D p1, Point3D p2)
```

**距离计算：**

点到直线距离：
$$d = \frac{||\vec{PQ} \times \vec{d}||}{||\vec{d}||}$$

两直线最短距离（异面）：
$$d = \frac{|(\vec{P_2} - \vec{P_1}) \cdot (\vec{d_1} \times \vec{d_2})|}{||\vec{d_1} \times \vec{d_2}||}$$

---

### Plane3D - 平面

隐式方程：$n \cdot P + D = 0$

**构造：**
```csharp
new Plane3D(Vector3 normal, double d)
new Plane3D(Vector3 normal, Point3D point)
Plane3D.FromThreePoints(Point3D p0, Point3D p1, Point3D p2)
```

**点到平面距离：**
$$d = \frac{|n \cdot P + D|}{||n||}$$

由于法向量已归一化，$||n|| = 1$：
$$d = |n \cdot P + D|$$

**直线与平面交点：**

设直线 $L(t) = P + t \cdot \vec{d}$，代入平面方程：
$$n \cdot (P + t \cdot \vec{d}) + D = 0$$
$$t = -\frac{n \cdot P + D}{n \cdot \vec{d}}$$

---

### Triangle3D - 三角形

由三个顶点定义：$V_0, V_1, V_2$

**基本性质：**

法向量：
$$\vec{n} = \frac{(V_1 - V_0) \times (V_2 - V_0)}{||(V_1 - V_0) \times (V_2 - V_0)||}$$

面积：
$$A = \frac{1}{2} ||(V_1 - V_0) \times (V_2 - V_0)||$$

重心：
$$G = \frac{V_0 + V_1 + V_2}{3}$$

**重心坐标：**

平面上任意点 $P$ 可表示为：
$$P = u \cdot V_0 + v \cdot V_1 + w \cdot V_2$$

其中 $u + v + w = 1$

点在三角形内部当且仅当 $u, v, w \in [0, 1]$

**Möller-Trumbore 光线相交算法：**

设光线 $R(t) = O + t \cdot \vec{D}$

求解方程组：
$$\begin{bmatrix} -\vec{D} & (V_1-V_0) & (V_2-V_0) \end{bmatrix} \begin{bmatrix} t \\ u \\ v \end{bmatrix} = O - V_0$$

使用 Cramer 法则：
$$t = \frac{(V_2-V_0) \cdot \vec{Q}}{(V_1-V_0) \cdot \vec{P}}$$
$$u = \frac{(O-V_0) \cdot \vec{P}}{(V_1-V_0) \cdot \vec{P}}$$
$$v = \frac{\vec{D} \cdot \vec{Q}}{(V_1-V_0) \cdot \vec{P}}$$

其中：
- $\vec{P} = \vec{D} \times (V_2-V_0)$
- $\vec{Q} = (O-V_0) \times (V_1-V_0)$

---

### BoundingBox3D - 轴对齐包围盒

**属性：**
- `Min`, `Max` - 最小/最大点
- `Center` - 中心点
- `Size` - 尺寸
- `Volume` - 体积

**相交检测（分离轴定理）：**

两个AABB相交当且仅当在所有三个轴上投影都重叠：
```
box1.max.x ≥ box2.min.x && box1.min.x ≤ box2.max.x &&
box1.max.y ≥ box2.min.y && box1.min.y ≤ box2.max.y &&
box1.max.z ≥ box2.min.z && box1.min.z ≤ box2.max.z
```

**光线相交（Slab方法）：**

对每个轴计算进入/离开时间：
$$t_{min,x} = \frac{min.x - origin.x}{direction.x}$$
$$t_{max,x} = \frac{max.x - origin.x}{direction.x}$$

最终：
$$t_{enter} = \max(t_{min,x}, t_{min,y}, t_{min,z})$$
$$t_{exit} = \min(t_{max,x}, t_{max,y}, t_{max,z})$$

相交条件：$t_{enter} \leq t_{exit}$ 且 $t_{exit} \geq 0$

---

## 🔧 向量与矩阵运算

### Matrix4x4 - 4×4变换矩阵

齐次坐标变换矩阵。

**矩阵布局：**
$$\begin{bmatrix} 
X_x & Y_x & Z_x & T_x \\
X_y & Y_y & Z_y & T_y \\
X_z & Y_z & Z_z & T_z \\
0 & 0 & 0 & 1
\end{bmatrix}$$

**平移矩阵：**
$$T = \begin{bmatrix} 
1 & 0 & 0 & t_x \\
0 & 1 & 0 & t_y \\
0 & 0 & 1 & t_z \\
0 & 0 & 0 & 1
\end{bmatrix}$$

**缩放矩阵：**
$$S = \begin{bmatrix} 
s_x & 0 & 0 & 0 \\
0 & s_y & 0 & 0 \\
0 & 0 & s_z & 0 \\
0 & 0 & 0 & 1
\end{bmatrix}$$

**旋转矩阵：**

绕X轴：
$$R_x(\theta) = \begin{bmatrix} 
1 & 0 & 0 & 0 \\
0 & \cos\theta & -\sin\theta & 0 \\
0 & \sin\theta & \cos\theta & 0 \\
0 & 0 & 0 & 1
\end{bmatrix}$$

绕Y轴：
$$R_y(\theta) = \begin{bmatrix} 
\cos\theta & 0 & \sin\theta & 0 \\
0 & 1 & 0 & 0 \\
-\sin\theta & 0 & \cos\theta & 0 \\
0 & 0 & 0 & 1
\end{bmatrix}$$

绕Z轴：
$$R_z(\theta) = \begin{bmatrix} 
\cos\theta & -\sin\theta & 0 & 0 \\
\sin\theta & \cos\theta & 0 & 0 \\
0 & 0 & 1 & 0 \\
0 & 0 & 0 & 1
\end{bmatrix}$$

**Rodrigues 旋转公式（绕任意轴）：**

$$R = I + \sin\theta \cdot K + (1-\cos\theta) \cdot K^2$$

其中 $K$ 是反对称矩阵：
$$K = \begin{bmatrix} 
0 & -n_z & n_y \\
n_z & 0 & -n_x \\
-n_y & n_x & 0
\end{bmatrix}$$

展开后：
$$R = \begin{bmatrix} 
n_x^2(1-c)+c & n_xn_y(1-c)-n_zs & n_xn_z(1-c)+n_ys \\
n_xn_y(1-c)+n_zs & n_y^2(1-c)+c & n_yn_z(1-c)-n_xs \\
n_xn_z(1-c)-n_ys & n_yn_z(1-c)+n_xs & n_z^2(1-c)+c
\end{bmatrix}$$

其中 $c = \cos\theta$, $s = \sin\theta$

---

## 🧮 高级算法

### RANSAC 平面拟合

**算法流程：**

1. 随机选择3个不共线的点
2. 构建平面
3. 统计内点数量（距离 < 阈值）
4. 重复N次，选择最佳模型
5. 用所有内点重新拟合（最小二乘）

**最小二乘平面拟合：**

1. 计算质心：$\bar{C} = \frac{1}{n}\sum_i P_i$
2. 中心化：$P'_i = P_i - \bar{C}$
3. 构建协方差矩阵：
$$M = \begin{bmatrix} 
\sum (x')^2 & \sum x'y' & \sum x'z' \\
\sum x'y' & \sum (y')^2 & \sum y'z' \\
\sum x'z' & \sum y'z' & \sum (z')^2
\end{bmatrix}$$
4. 法向量 = 最小特征值对应的特征向量
5. $D = -\vec{n} \cdot \bar{C}$

**使用示例：**
```csharp
var ransac = new RansacPlaneFitting
{
    MaxIterations = 1000,
    DistanceThreshold = 0.01,
    MinInlierRatio = 0.8
};

var result = ransac.FitPlane(pointCloud);
Console.WriteLine($"内点：{result.InlierCount}, 平面：{result.Plane}");
```

---

### KD-Tree 最近邻搜索

**构建算法：**

```
BuildTree(points, depth):
    if points.isEmpty():
        return null
    
    axis = depth % 3
    median = findMedian(points, axis)
    
    return Node(
        point: median,
        left: BuildTree(points < median, depth+1),
        right: BuildTree(points >= median, depth+1)
    )
```

**最近邻搜索（带剪枝）：**

```
NearestNeighbor(node, target, best):
    if node is null:
        return best
    
    if distance(node.point, target) < distance(best, target):
        best = node.point
    
    if target[axis] < node.point[axis]:
        near, far = node.left, node.right
    else:
        near, far = node.right, node.left
    
    best = NearestNeighbor(near, target, best)
    
    # 剪枝检查
    if |target[axis] - node.point[axis]| < distance(best, target):
        best = NearestNeighbor(far, target, best)
    
    return best
```

**时间复杂度：**
- 构建：O(n log n)
- 查询：平均 O(log n)，最坏 O(n)

**使用示例：**
```csharp
var kdTree = new KDTree(pointCloud);
var nearest = kdTree.FindNearest(queryPoint);
var kNearest = kdTree.FindKNearest(queryPoint, 10);
```

---

### ICP (Iterative Closest Point)

**算法步骤：**

```
ICP(source, target):
    repeat until convergence:
        1. 匹配：对每个源点找目标点云中的最近点
        2. 估计：计算最优刚体变换 (R, t)
        3. 应用：source ← R·source + t
        4. 检查：计算误差，判断收敛
```

**刚体变换估计（SVD方法）：**

1. 计算质心：
$$\bar{s} = \frac{1}{n}\sum_i s_i, \quad \bar{t} = \frac{1}{n}\sum_i t_i$$

2. 中心化：
$$s'_i = s_i - \bar{s}, \quad t'_i = t_i - \bar{t}$$

3. 协方差矩阵：
$$H = \sum_i s'_i \cdot (t'_i)^T$$

4. SVD分解：
$$H = U \Sigma V^T$$

5. 旋转矩阵：
$$R = V U^T$$

若 $\det(R) < 0$（反射），修正：
$$R = V \begin{bmatrix} 1 & 0 & 0 \\ 0 & 1 & 0 \\ 0 & 0 & -1 \end{bmatrix} U^T$$

6. 平移向量：
$$\vec{t} = \bar{t} - R \bar{s}$$

**使用示例：**
```csharp
var icp = new ICP
{
    MaxIterations = 50,
    ConvergenceThreshold = 1e-6
};

var result = icp.Align(sourceCloud, targetCloud);
Console.WriteLine($"收敛：{result.Converged}, 误差：{result.FinalError}");
```

---

### QuickHull 3D 凸包

**算法思想：**

1. 找初始四面体（最远的4个点）
2. 对每个面：
   - 找所有在面外侧的点
   - 若无外侧点，该面是凸包的一部分
   - 否则找最远点作为apex
   - 删除从apex可见的面
   - 构建新面连接apex到地平线(horizon)
3. 递归处理新面

**点在面外侧判定：**
$$\text{distance} = \vec{n} \cdot (P - P_0) > 0$$

**时间复杂度：**
- 平均：O(n log n)
- 最坏：O(n²)

**使用示例：**
```csharp
var convexHull = new ConvexHull3D();
var result = convexHull.Compute(points);
Console.WriteLine($"顶点：{result.Vertices.Count}, 面：{result.FaceCount}");
```

---

### Welzl 最小包围球

**递归算法：**

```
MiniBall(P, R):
    if P = ∅ or |R| = 4:
        return Sphere(R)
    
    p = random point from P
    B = MiniBall(P \ {p}, R)
    
    if p ∈ B:
        return B
    else:
        return MiniBall(P \ {p}, R ∪ {p})
```

**边界球构造：**

1点：$(P, 0)$

2点：中点和半径
$$C = \frac{P_1 + P_2}{2}, \quad r = \frac{||P_2 - P_1||}{2}$$

3点：外接圆

4点：外接球（求解线性方程组）

**时间复杂度：** 期望 O(n)

**使用示例：**
```csharp
var sphere = BoundingSphere.ComputeMinimalSphere(points);
Console.WriteLine($"球心：{sphere.Center}, 半径：{sphere.Radius}");
```

---

## 💡 完整使用示例

### 示例：点云平面拟合与可视化

```csharp
using TulipAlg.Core.Geometry3D;
using TulipAlg.Core.Geometry3D.Algorithms;

// 生成噪声点云
var points = GenerateNoisyPlanePoints(100, 20); // 100内点, 20异常值

// RANSAC拟合
var ransac = new RansacPlaneFitting
{
    MaxIterations = 500,
    DistanceThreshold = 0.3
};

var result = ransac.FitPlane(points);

// 输出结果
Console.WriteLine($"拟合平面：{result.Plane}");
Console.WriteLine($"内点率：{result.InlierRatio:P2}");

// 计算点到平面的距离
foreach (var point in points)
{
    double distance = result.Plane.DistanceToPoint(point);
    bool isInlier = distance < ransac.DistanceThreshold;
    Console.WriteLine($"点{point}: 距离={distance:F3}, {(isInlier ? "内点" : "异常值")}");
}
```

### 示例：光线追踪

```csharp
// 定义场景
var triangles = new List<Triangle3D>
{
    new Triangle3D(
        new Point3D(0, 0, 0),
        new Point3D(10, 0, 0),
        new Point3D(0, 10, 0)
    ),
    // ... 更多三角形
};

// 光线
var ray = (origin: new Point3D(5, 5, 10), direction: new Vector3(0, 0, -1));

// 相交检测
foreach (var tri in triangles)
{
    if (tri.RayIntersection(ray.origin, ray.direction, 
        out var intersection, out double t, out _, out _))
    {
        Console.WriteLine($"相交于：{intersection}, 距离：{t}");
    }
}
```

### 示例：点云配准

```csharp
// 源点云和目标点云
var source = LoadPointCloud("source.ply");
var target = LoadPointCloud("target.ply");

// ICP配准
var icp = new ICP
{
    MaxIterations = 100,
    ConvergenceThreshold = 1e-6
};

var result = icp.Align(source, target);

// 应用变换
var aligned = source.Select(p => result.Transform.Transform(p)).ToList();

// 保存结果
SavePointCloud("aligned.ply", aligned);
Console.WriteLine($"配准完成，误差：{result.FinalError:F6}");
```

---

## 📚 扩展建议

### 1. 点云处理
- **法向量估计**：使用PCA或邻域拟合
- **点云滤波**：统计滤波、体素滤波
- **特征提取**：FPFH、SHOT描述子

### 2. 姿态估计
- **PnP算法**：从2D-3D对应求相机位姿
- **本质矩阵**：从特征匹配恢复相机运动
- **Bundle Adjustment**：全局优化

### 3. 投影与相机
- **针孔相机模型**：
$$\begin{bmatrix} u \\ v \\ 1 \end{bmatrix} = K [R | t] \begin{bmatrix} X \\ Y \\ Z \\ 1 \end{bmatrix}$$

其中内参矩阵：
$$K = \begin{bmatrix} f_x & 0 & c_x \\ 0 & f_y & c_y \\ 0 & 0 & 1 \end{bmatrix}$$

### 4. 碰撞检测
- **GJK算法**：通用凸体碰撞检测
- **SAT**：分离轴定理
- **层次包围体**：BVH树

### 5. 网格处理
- **细分**：Loop、Catmull-Clark
- **简化**：QEM误差度量
- **法向量计算**：加权平均

---

## 📖 参考资料

### 书籍
1. *Real-Time Collision Detection* - Christer Ericson
2. *3D Math Primer for Graphics and Game Development* - Fletcher Dunn
3. *Multiple View Geometry in Computer Vision* - Hartley & Zisserman
4. *Computational Geometry: Algorithms and Applications* - de Berg et al.

### 论文
1. RANSAC: Fischler & Bolles (1981)
2. ICP: Besl & McKay (1992)
3. QuickHull: Barber et al. (1996)
4. Möller-Trumbore: Möller & Trumbore (1997)

### 在线资源
- [Geometry Central](http://geometry-central.net/)
- [Real-Time Rendering Resources](http://www.realtimerendering.com/)

---

## 🔧 性能优化建议

1. **使用 `DistanceSquaredTo` 代替 `DistanceTo`** 避免开方运算
2. **KD-Tree批量查询** 减少树遍历次数
3. **SIMD加速** 使用 `System.Numerics.Vector`
4. **并行化** 用 `Parallel.For` 处理大规模点云
5. **空间索引** 使用八叉树或网格

---

## ⚠️ 注意事项

1. **数值稳定性**：注意浮点误差，使用epsilon比较
2. **退化情况**：检查共线、共面等特殊情况
3. **坐标系**：本库使用右手坐标系
4. **单位**：确保角度单位统一（弧度）

---

## 📄 许可证

MIT License

---

**作者：** TulipAlg Team  
**版本：** 1.0.0  
**日期：** 2025-10-20
