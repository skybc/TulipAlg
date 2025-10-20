using System;

namespace TulipAlg.Core.Geometry3D
{
    /// <summary>
    /// 4×4 齐次变换矩阵
    /// 
    /// 📘 齐次坐标系统：
    /// 
    /// 三维点 (x, y, z) 的齐次坐标表示为 (x, y, z, 1)
    /// 三维向量 (x, y, z) 的齐次坐标表示为 (x, y, z, 0)
    /// 
    /// 4×4 矩阵可以表示：
    /// - 平移（Translation）
    /// - 旋转（Rotation）
    /// - 缩放（Scaling）
    /// - 投影（Projection）
    /// - 组合变换
    /// 
    /// 矩阵布局（行优先）：
    /// 
    /// [m00 m01 m02 m03]   [Xx Yx Zx Tx]
    /// [m10 m11 m12 m13] = [Xy Yy Zy Ty]
    /// [m20 m21 m22 m23]   [Xz Yz Zz Tz]
    /// [m30 m31 m32 m33]   [0  0  0  1 ]
    /// 
    /// 其中：
    /// - (Xx, Xy, Xz) 是 X 轴方向
    /// - (Yx, Yy, Yz) 是 Y 轴方向
    /// - (Zx, Zy, Zz) 是 Z 轴方向
    /// - (Tx, Ty, Tz) 是平移向量
    /// </summary>
    public struct Matrix4x4
    {
        #region Fields

        // 矩阵元素（行优先存储）
        public double M00, M01, M02, M03;
        public double M10, M11, M12, M13;
        public double M20, M21, M22, M23;
        public double M30, M31, M32, M33;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造矩阵（行优先）
        /// </summary>
        public Matrix4x4(
            double m00, double m01, double m02, double m03,
            double m10, double m11, double m12, double m13,
            double m20, double m21, double m22, double m23,
            double m30, double m31, double m32, double m33)
        {
            M00 = m00; M01 = m01; M02 = m02; M03 = m03;
            M10 = m10; M11 = m11; M12 = m12; M13 = m13;
            M20 = m20; M21 = m21; M22 = m22; M23 = m23;
            M30 = m30; M31 = m31; M32 = m32; M33 = m33;
        }

        #endregion

        #region Identity and Basic Matrices

        /// <summary>
        /// 单位矩阵
        /// </summary>
        public static Matrix4x4 Identity => new Matrix4x4(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        );

        /// <summary>
        /// 零矩阵
        /// </summary>
        public static Matrix4x4 Zero => new Matrix4x4(
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0
        );

        #endregion

        #region Translation

        /// <summary>
        /// 创建平移矩阵
        /// 
        /// 📘 公式：
        /// 
        /// [1 0 0 tx]
        /// [0 1 0 ty]
        /// [0 0 1 tz]
        /// [0 0 0 1 ]
        /// 
        /// 变换：P' = P + T
        /// </summary>
        public static Matrix4x4 CreateTranslation(double x, double y, double z)
        {
            return new Matrix4x4(
                1, 0, 0, x,
                0, 1, 0, y,
                0, 0, 1, z,
                0, 0, 0, 1
            );
        }

        /// <summary>
        /// 创建平移矩阵（向量形式）
        /// </summary>
        public static Matrix4x4 CreateTranslation(Vector3 translation)
        {
            return CreateTranslation(translation.X, translation.Y, translation.Z);
        }

        #endregion

        #region Scaling

        /// <summary>
        /// 创建缩放矩阵
        /// 
        /// 📘 公式：
        /// 
        /// [sx 0  0  0]
        /// [0  sy 0  0]
        /// [0  0  sz 0]
        /// [0  0  0  1]
        /// 
        /// 变换：P' = (sx·x, sy·y, sz·z)
        /// </summary>
        public static Matrix4x4 CreateScale(double sx, double sy, double sz)
        {
            return new Matrix4x4(
                sx, 0, 0, 0,
                0, sy, 0, 0,
                0, 0, sz, 0,
                0, 0, 0, 1
            );
        }

        /// <summary>
        /// 创建均匀缩放矩阵
        /// </summary>
        public static Matrix4x4 CreateScale(double scale)
        {
            return CreateScale(scale, scale, scale);
        }

        /// <summary>
        /// 创建缩放矩阵（向量形式）
        /// </summary>
        public static Matrix4x4 CreateScale(Vector3 scale)
        {
            return CreateScale(scale.X, scale.Y, scale.Z);
        }

        #endregion

        #region Rotation

        /// <summary>
        /// 创建绕 X 轴的旋转矩阵
        /// 
        /// 📘 公式（右手坐标系，逆时针为正）：
        /// 
        /// [1   0      0     0]
        /// [0  cos(θ) -sin(θ) 0]
        /// [0  sin(θ)  cos(θ) 0]
        /// [0   0      0     1]
        /// </summary>
        /// <param name="angleRadians">旋转角度（弧度）</param>
        public static Matrix4x4 CreateRotationX(double angleRadians)
        {
            double cos = Math.Cos(angleRadians);
            double sin = Math.Sin(angleRadians);

            return new Matrix4x4(
                1, 0, 0, 0,
                0, cos, -sin, 0,
                0, sin, cos, 0,
                0, 0, 0, 1
            );
        }

        /// <summary>
        /// 创建绕 Y 轴的旋转矩阵
        /// 
        /// 📘 公式：
        /// 
        /// [ cos(θ)  0  sin(θ) 0]
        /// [  0      1   0     0]
        /// [-sin(θ)  0  cos(θ) 0]
        /// [  0      0   0     1]
        /// </summary>
        public static Matrix4x4 CreateRotationY(double angleRadians)
        {
            double cos = Math.Cos(angleRadians);
            double sin = Math.Sin(angleRadians);

            return new Matrix4x4(
                cos, 0, sin, 0,
                0, 1, 0, 0,
                -sin, 0, cos, 0,
                0, 0, 0, 1
            );
        }

        /// <summary>
        /// 创建绕 Z 轴的旋转矩阵
        /// 
        /// 📘 公式：
        /// 
        /// [cos(θ) -sin(θ) 0 0]
        /// [sin(θ)  cos(θ) 0 0]
        /// [ 0       0     1 0]
        /// [ 0       0     0 1]
        /// </summary>
        public static Matrix4x4 CreateRotationZ(double angleRadians)
        {
            double cos = Math.Cos(angleRadians);
            double sin = Math.Sin(angleRadians);

            return new Matrix4x4(
                cos, -sin, 0, 0,
                sin, cos, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            );
        }

        /// <summary>
        /// 创建绕任意轴的旋转矩阵（Rodrigues 旋转公式）
        /// 
        /// 📘 Rodrigues 公式：
        /// 
        /// R = I + sin(θ)·K + (1-cos(θ))·K²
        /// 
        /// 其中：
        /// - I 是单位矩阵
        /// - K 是反对称矩阵（叉积矩阵）：
        /// 
        ///     [ 0   -nz   ny]
        /// K = [ nz   0   -nx]
        ///     [-ny   nx   0 ]
        /// 
        /// 展开后：
        /// 
        /// R = [nx²(1-c)+c    nxny(1-c)-nzs  nxnz(1-c)+nys]
        ///     [nxny(1-c)+nzs  ny²(1-c)+c    nynz(1-c)-nxs]
        ///     [nxnz(1-c)-nys  nynz(1-c)+nxs  nz²(1-c)+c  ]
        /// 
        /// 其中 c = cos(θ), s = sin(θ), (nx,ny,nz) 是单位轴向量
        /// </summary>
        /// <param name="axis">旋转轴（单位向量）</param>
        /// <param name="angleRadians">旋转角度（弧度）</param>
        public static Matrix4x4 CreateRotation(Vector3 axis, double angleRadians)
        {
            axis = axis.Normalize();
            double cos = Math.Cos(angleRadians);
            double sin = Math.Sin(angleRadians);
            double oneMinusCos = 1.0 - cos;

            double nx = axis.X;
            double ny = axis.Y;
            double nz = axis.Z;

            return new Matrix4x4(
                nx * nx * oneMinusCos + cos,
                nx * ny * oneMinusCos - nz * sin,
                nx * nz * oneMinusCos + ny * sin,
                0,

                nx * ny * oneMinusCos + nz * sin,
                ny * ny * oneMinusCos + cos,
                ny * nz * oneMinusCos - nx * sin,
                0,

                nx * nz * oneMinusCos - ny * sin,
                ny * nz * oneMinusCos + nx * sin,
                nz * nz * oneMinusCos + cos,
                0,

                0, 0, 0, 1
            );
        }

        /// <summary>
        /// 从欧拉角创建旋转矩阵（ZYX 顺序）
        /// 
        /// 📘 组合顺序：R = Rz(yaw) · Ry(pitch) · Rx(roll)
        /// </summary>
        /// <param name="roll">绕 X 轴旋转（弧度）</param>
        /// <param name="pitch">绕 Y 轴旋转（弧度）</param>
        /// <param name="yaw">绕 Z 轴旋转（弧度）</param>
        public static Matrix4x4 CreateFromEulerAngles(double roll, double pitch, double yaw)
        {
            return CreateRotationZ(yaw) * CreateRotationY(pitch) * CreateRotationX(roll);
        }

        #endregion

        #region Look-At and View Matrix

        /// <summary>
        /// 创建视图矩阵（Look-At）
        /// 
        /// 📘 原理：
        /// 
        /// 构建相机坐标系：
        /// - Z 轴：forward = normalize(target - eye)
        /// - X 轴：right = normalize(forward × up)
        /// - Y 轴：up' = right × forward
        /// 
        /// 视图矩阵 = 旋转矩阵 · 平移矩阵
        /// 
        /// [right.x   right.y   right.z   0]   [1 0 0 -eye.x]
        /// [up.x      up.y      up.z      0] · [0 1 0 -eye.y]
        /// [-fwd.x    -fwd.y    -fwd.z    0]   [0 0 1 -eye.z]
        /// [0         0         0         1]   [0 0 0  1    ]
        /// </summary>
        public static Matrix4x4 CreateLookAt(Point3D eye, Point3D target, Vector3 up)
        {
            Vector3 forward = (target - eye).Normalize();
            Vector3 right = forward.Cross(up).Normalize();
            Vector3 newUp = right.Cross(forward);

            return new Matrix4x4(
                right.X, right.Y, right.Z, -right.Dot(eye.ToVector()),
                newUp.X, newUp.Y, newUp.Z, -newUp.Dot(eye.ToVector()),
                -forward.X, -forward.Y, -forward.Z, forward.Dot(eye.ToVector()),
                0, 0, 0, 1
            );
        }

        #endregion

        #region Projection

        /// <summary>
        /// 创建透视投影矩阵
        /// 
        /// 📘 公式（OpenGL 风格，右手坐标系）：
        /// 
        /// f = 1 / tan(fov/2)
        /// 
        /// [f/aspect  0      0                 0           ]
        /// [0         f      0                 0           ]
        /// [0         0  (far+near)/(near-far)  2far·near/(near-far)]
        /// [0         0     -1                 0           ]
        /// </summary>
        /// <param name="fovRadians">视场角（弧度）</param>
        /// <param name="aspectRatio">宽高比</param>
        /// <param name="nearPlane">近裁剪面</param>
        /// <param name="farPlane">远裁剪面</param>
        public static Matrix4x4 CreatePerspective(double fovRadians, double aspectRatio, double nearPlane, double farPlane)
        {
            double f = 1.0 / Math.Tan(fovRadians * 0.5);
            double rangeInv = 1.0 / (nearPlane - farPlane);

            return new Matrix4x4(
                f / aspectRatio, 0, 0, 0,
                0, f, 0, 0,
                0, 0, (farPlane + nearPlane) * rangeInv, 2 * farPlane * nearPlane * rangeInv,
                0, 0, -1, 0
            );
        }

        /// <summary>
        /// 创建正交投影矩阵
        /// 
        /// 📘 公式：
        /// 
        /// [2/(r-l)    0         0       -(r+l)/(r-l)]
        /// [0        2/(t-b)     0       -(t+b)/(t-b)]
        /// [0          0      -2/(f-n)   -(f+n)/(f-n)]
        /// [0          0         0            1      ]
        /// </summary>
        public static Matrix4x4 CreateOrthographic(double left, double right, double bottom, double top, double nearPlane, double farPlane)
        {
            double width = right - left;
            double height = top - bottom;
            double depth = farPlane - nearPlane;

            return new Matrix4x4(
                2 / width, 0, 0, -(right + left) / width,
                0, 2 / height, 0, -(top + bottom) / height,
                0, 0, -2 / depth, -(farPlane + nearPlane) / depth,
                0, 0, 0, 1
            );
        }

        #endregion

        #region Matrix Operations

        /// <summary>
        /// 矩阵乘法
        /// 
        /// 📘 (AB)ij = Σk Aik·Bkj
        /// </summary>
        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
        {
            return new Matrix4x4(
                a.M00 * b.M00 + a.M01 * b.M10 + a.M02 * b.M20 + a.M03 * b.M30,
                a.M00 * b.M01 + a.M01 * b.M11 + a.M02 * b.M21 + a.M03 * b.M31,
                a.M00 * b.M02 + a.M01 * b.M12 + a.M02 * b.M22 + a.M03 * b.M32,
                a.M00 * b.M03 + a.M01 * b.M13 + a.M02 * b.M23 + a.M03 * b.M33,

                a.M10 * b.M00 + a.M11 * b.M10 + a.M12 * b.M20 + a.M13 * b.M30,
                a.M10 * b.M01 + a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31,
                a.M10 * b.M02 + a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32,
                a.M10 * b.M03 + a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33,

                a.M20 * b.M00 + a.M21 * b.M10 + a.M22 * b.M20 + a.M23 * b.M30,
                a.M20 * b.M01 + a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31,
                a.M20 * b.M02 + a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32,
                a.M20 * b.M03 + a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33,

                a.M30 * b.M00 + a.M31 * b.M10 + a.M32 * b.M20 + a.M33 * b.M30,
                a.M30 * b.M01 + a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31,
                a.M30 * b.M02 + a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32,
                a.M30 * b.M03 + a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33
            );
        }

        /// <summary>
        /// 变换点（齐次坐标 w=1）
        /// </summary>
        public Point3D Transform(Point3D point)
        {
            double x = M00 * point.X + M01 * point.Y + M02 * point.Z + M03;
            double y = M10 * point.X + M11 * point.Y + M12 * point.Z + M13;
            double z = M20 * point.X + M21 * point.Y + M22 * point.Z + M23;
            double w = M30 * point.X + M31 * point.Y + M32 * point.Z + M33;

            // 齐次除法
            if (Math.Abs(w) > 1e-10)
            {
                return new Point3D(x / w, y / w, z / w);
            }

            return new Point3D(x, y, z);
        }

        /// <summary>
        /// 变换向量（齐次坐标 w=0，忽略平移）
        /// </summary>
        public Vector3 TransformVector(Vector3 vector)
        {
            return new Vector3(
                M00 * vector.X + M01 * vector.Y + M02 * vector.Z,
                M10 * vector.X + M11 * vector.Y + M12 * vector.Z,
                M20 * vector.X + M21 * vector.Y + M22 * vector.Z
            );
        }

        /// <summary>
        /// 变换法向量（使用逆转置矩阵）
        /// 
        /// 📘 原理：法向量需要使用 (M⁻¹)ᵀ 变换以保持垂直性
        /// </summary>
        public Vector3 TransformNormal(Vector3 normal)
        {
            // 简化：对于正交矩阵，逆转置 = 原矩阵
            // 这里假设是刚体变换，直接使用旋转部分
            Vector3 result = TransformVector(normal);
            return result.Normalize();
        }

        #endregion

        #region Transpose and Inverse

        /// <summary>
        /// 转置矩阵
        /// </summary>
        public Matrix4x4 Transpose()
        {
            return new Matrix4x4(
                M00, M10, M20, M30,
                M01, M11, M21, M31,
                M02, M12, M22, M32,
                M03, M13, M23, M33
            );
        }

        /// <summary>
        /// 计算行列式（用于判断矩阵是否可逆）
        /// </summary>
        public double Determinant()
        {
            // 使用子式展开
            double det =
                M00 * (M11 * (M22 * M33 - M23 * M32) - M12 * (M21 * M33 - M23 * M31) + M13 * (M21 * M32 - M22 * M31)) -
                M01 * (M10 * (M22 * M33 - M23 * M32) - M12 * (M20 * M33 - M23 * M30) + M13 * (M20 * M32 - M22 * M30)) +
                M02 * (M10 * (M21 * M33 - M23 * M31) - M11 * (M20 * M33 - M23 * M30) + M13 * (M20 * M31 - M21 * M30)) -
                M03 * (M10 * (M21 * M32 - M22 * M31) - M11 * (M20 * M32 - M22 * M30) + M12 * (M20 * M31 - M21 * M30));

            return det;
        }

        #endregion

        #region String Representation

        public override string ToString()
        {
            return $"Matrix4x4:\n" +
                   $"[{M00:F3}, {M01:F3}, {M02:F3}, {M03:F3}]\n" +
                   $"[{M10:F3}, {M11:F3}, {M12:F3}, {M13:F3}]\n" +
                   $"[{M20:F3}, {M21:F3}, {M22:F3}, {M23:F3}]\n" +
                   $"[{M30:F3}, {M31:F3}, {M32:F3}, {M33:F3}]";
        }

        #endregion
    }
}
