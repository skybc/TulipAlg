/*----------------------------------------------------------------------------
  CannySubPixelEdge - C++ wrapper for Canny/Devernay's sub-pixel edge detector
  
  Based on the implementation by Rafael Grompone von Gioi and Gregory Randall
  Reference: "A Sub-Pixel Edge Detector: an Implementation of the Canny/Devernay
  Algorithm" by Rafael Grompone von Gioi and Gregory Randall,
  Image Processing On Line, 2017. DOI:10.5201/ipol.2017.216
----------------------------------------------------------------------------*/

#include "pch.h"
#include "CannySubPixelEdge.h"
#include <cmath>
#include <cstring>
#include <algorithm>

#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

namespace TulipAlg {

    // 构造函数
    CannySubPixelEdge::CannySubPixelEdge() {
    }

    // 析构函数
    CannySubPixelEdge::~CannySubPixelEdge() {
    }

    // 设置错误信息
    void CannySubPixelEdge::SetError(const char* msg) {
        m_lastError = msg ? msg : "";
    }

    // 内存分配
    void* CannySubPixelEdge::Malloc(size_t size) {
        if (size == 0) {
            SetError("xmalloc: zero size");
            return nullptr;
        }
        void* p = malloc(size);
        if (p == nullptr) {
            SetError("xmalloc: out of memory");
        }
        return p;
    }

    // 比较函数，考虑浮点数舍入误差
    bool CannySubPixelEdge::Greater(double a, double b) {
        if (a <= b) return false;
        if ((a - b) < 1000 * DBL_EPSILON) return false;
        return true;
    }

    // 欧几里得距离
    double CannySubPixelEdge::Dist(double x1, double y1, double x2, double y2) {
        return sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
    }

    // 计算高斯核
    void CannySubPixelEdge::GaussianKernel(double* kernel, int n, double sigma, double mean) {
        if (kernel == nullptr) {
            SetError("gaussian_kernel: kernel not allocated");
            return;
        }
        if (sigma <= 0.0) {
            SetError("gaussian_kernel: sigma must be positive");
            return;
        }

        double sum = 0.0;
        for (int i = 0; i < n; i++) {
            double val = ((double)i - mean) / sigma;
            kernel[i] = exp(-0.5 * val * val);
            sum += kernel[i];
        }

        // 归一化
        if (sum > 0.0) {
            for (int i = 0; i < n; i++) {
                kernel[i] /= sum;
            }
        }
    }

    // 高斯滤波
    double* CannySubPixelEdge::GaussianFilter(const double* image, int X, int Y, double sigma) {
        if (sigma <= 0.0) {
            SetError("gaussian_filter: sigma must be positive");
            return nullptr;
        }
        if (image == nullptr || X < 1 || Y < 1) {
            SetError("gaussian_filter: invalid image");
            return nullptr;
        }

        // 分配内存
        double* tmp = (double*)Malloc(X * Y * sizeof(double));
        double* out = (double*)Malloc(X * Y * sizeof(double));
        if (tmp == nullptr || out == nullptr) {
            if (tmp) free(tmp);
            if (out) free(out);
            return nullptr;
        }

        // 计算高斯核
        double prec = 3.0;
        int offset = (int)ceil(sigma * sqrt(2.0 * prec * log(10.0)));
        int n = 1 + 2 * offset;
        double* kernel = (double*)Malloc(n * sizeof(double));
        if (kernel == nullptr) {
            free(tmp);
            free(out);
            return nullptr;
        }
        GaussianKernel(kernel, n, sigma, (double)offset);

        int nx2 = 2 * X;
        int ny2 = 2 * Y;

        // X轴卷积
        for (int x = 0; x < X; x++) {
            for (int y = 0; y < Y; y++) {
                double val = 0.0;
                for (int i = 0; i < n; i++) {
                    int j = x - offset + i;
                    
                    // 对称边界条件
                    while (j < 0) j += nx2;
                    while (j >= nx2) j -= nx2;
                    if (j >= X) j = nx2 - 1 - j;

                    val += image[j + y * X] * kernel[i];
                }
                tmp[x + y * X] = val;
            }
        }

        // Y轴卷积
        for (int x = 0; x < X; x++) {
            for (int y = 0; y < Y; y++) {
                double val = 0.0;
                for (int i = 0; i < n; i++) {
                    int j = y - offset + i;
                    
                    // 对称边界条件
                    while (j < 0) j += ny2;
                    while (j >= ny2) j -= ny2;
                    if (j >= Y) j = ny2 - 1 - j;

                    val += tmp[x + j * X] * kernel[i];
                }
                out[x + y * X] = val;
            }
        }

        free(kernel);
        free(tmp);
        return out;
    }

    // 计算图像梯度
    void CannySubPixelEdge::ComputeGradient(double* Gx, double* Gy, double* modG,
                                           const double* image, int X, int Y) {
        if (Gx == nullptr || Gy == nullptr || modG == nullptr || image == nullptr) {
            SetError("compute_gradient: invalid input");
            return;
        }

        // 使用中心差分近似图像梯度
        for (int x = 1; x < (X - 1); x++) {
            for (int y = 1; y < (Y - 1); y++) {
                Gx[x + y * X] = image[(x + 1) + y * X] - image[(x - 1) + y * X];
                Gy[x + y * X] = image[x + (y + 1) * X] - image[x + (y - 1) * X];
                modG[x + y * X] = sqrt(Gx[x + y * X] * Gx[x + y * X] + 
                                      Gy[x + y * X] * Gy[x + y * X]);
            }
        }
    }

    // 计算亚像素边缘点
    void CannySubPixelEdge::ComputeEdgePoints(double* Ex, double* Ey, const double* modG,
                                             const double* Gx, const double* Gy, int X, int Y) {
        if (Ex == nullptr || Ey == nullptr || modG == nullptr || 
            Gx == nullptr || Gy == nullptr) {
            SetError("compute_edge_points: invalid input");
            return;
        }

        // 初始化所有像素为非边缘点
        for (int i = 0; i < X * Y; i++) {
            Ex[i] = Ey[i] = -1.0;
        }

        // 探索内部像素（保留2像素边距）
        for (int x = 2; x < (X - 2); x++) {
            for (int y = 2; y < (Y - 2); y++) {
                int Dx = 0;
                int Dy = 0;
                double mod = modG[x + y * X];
                double L = modG[x - 1 + y * X];
                double R = modG[x + 1 + y * X];
                double U = modG[x + (y + 1) * X];
                double D = modG[x + (y - 1) * X];
                double gx = fabs(Gx[x + y * X]);
                double gy = fabs(Gy[x + y * X]);

                // 水平或垂直局部最大值检测
                if (Greater(mod, L) && !Greater(R, mod) && gx >= gy) {
                    Dx = 1; // 水平边缘
                }
                else if (Greater(mod, D) && !Greater(U, mod) && gx <= gy) {
                    Dy = 1; // 垂直边缘
                }

                // Devernay亚像素校正
                if (Dx > 0 || Dy > 0) {
                    double a = modG[x - Dx + (y - Dy) * X];
                    double b = modG[x + y * X];
                    double c = modG[x + Dx + (y + Dy) * X];
                    double offset = 0.5 * (a - c) / (a - b - b + c);

                    Ex[x + y * X] = x + offset * Dx;
                    Ey[x + y * X] = y + offset * Dy;
                }
            }
        }
    }

    // 边缘点链接评分函数
    double CannySubPixelEdge::Chain(int from, int to, const double* Ex, const double* Ey,
                                   const double* Gx, const double* Gy, int X, int Y) {
        if (Ex == nullptr || Ey == nullptr || Gx == nullptr || Gy == nullptr) {
            SetError("chain: invalid input");
            return 0.0;
        }
        if (from < 0 || to < 0 || from >= X * Y || to >= X * Y) {
            SetError("chain: one of the points is out the image");
            return 0.0;
        }

        // 检查点是否不同且为有效边缘点
        if (from == to) return 0.0;
        if (Ex[from] < 0.0 || Ey[from] < 0.0 || Ex[to] < 0.0 || Ey[to] < 0.0) {
            return 0.0;
        }

        // 检查梯度方向一致性
        double dx = Ex[to] - Ex[from];
        double dy = Ey[to] - Ey[from];
        if ((Gy[from] * dx - Gx[from] * dy) * (Gy[to] * dx - Gx[to] * dy) <= 0.0) {
            return 0.0;
        }

        // 返回链接评分
        if ((Gy[from] * dx - Gx[from] * dy) >= 0.0) {
            return 1.0 / Dist(Ex[from], Ey[from], Ex[to], Ey[to]); // 前向链接
        }
        else {
            return -1.0 / Dist(Ex[from], Ey[from], Ex[to], Ey[to]); // 后向链接
        }
    }

    // 链接边缘点
    void CannySubPixelEdge::ChainEdgePoints(int* next, int* prev, const double* Ex, const double* Ey,
                                           const double* Gx, const double* Gy, int X, int Y) {
        if (next == nullptr || prev == nullptr || Ex == nullptr || 
            Ey == nullptr || Gx == nullptr || Gy == nullptr) {
            SetError("chain_edge_points: invalid input");
            return;
        }

        // 初始化为未链接状态
        for (int i = 0; i < X * Y; i++) {
            next[i] = prev[i] = -1;
        }

        // 尝试每个点建立局部链
        for (int x = 2; x < (X - 2); x++) {
            for (int y = 2; y < (Y - 2); y++) {
                if (Ex[x + y * X] >= 0.0 && Ey[x + y * X] >= 0.0) {
                    int from = x + y * X;
                    double fwd_s = 0.0;
                    double bck_s = 0.0;
                    int fwd = -1;
                    int bck = -1;

                    // 尝试所有相距不超过2像素的邻居
                    for (int i = -2; i <= 2; i++) {
                        for (int j = -2; j <= 2; j++) {
                            int to = x + i + (y + j) * X;
                            double s = Chain(from, to, Ex, Ey, Gx, Gy, X, Y);

                            if (s > fwd_s) {
                                fwd_s = s;
                                fwd = to;
                            }
                            if (s < bck_s) {
                                bck_s = s;
                                bck = to;
                            }
                        }
                    }

                    // 前向链接
                    if (fwd >= 0 && next[from] != fwd) {
                        int alt = prev[fwd];
                        if (alt < 0 || Chain(alt, fwd, Ex, Ey, Gx, Gy, X, Y) < fwd_s) {
                            if (next[from] >= 0) {
                                prev[next[from]] = -1;
                            }
                            next[from] = fwd;
                            if (alt >= 0) {
                                next[alt] = -1;
                            }
                            prev[fwd] = from;
                        }
                    }

                    // 后向链接
                    if (bck >= 0 && prev[from] != bck) {
                        int alt = next[bck];
                        if (alt < 0 || Chain(alt, bck, Ex, Ey, Gx, Gy, X, Y) > bck_s) {
                            if (alt >= 0) {
                                prev[alt] = -1;
                            }
                            next[bck] = from;
                            if (prev[from] >= 0) {
                                next[prev[from]] = -1;
                            }
                            prev[from] = bck;
                        }
                    }
                }
            }
        }
    }

    // 带滞后的阈值处理
    void CannySubPixelEdge::ThresholdsWithHysteresis(int* next, int* prev,
                                                     const double* modG, int X, int Y,
                                                     double th_h, double th_l) {
        if (next == nullptr || prev == nullptr || modG == nullptr) {
            SetError("thresholds_with_hysteresis: invalid input");
            return;
        }

        int* valid = (int*)Malloc(X * Y * sizeof(int));
        if (valid == nullptr) return;

        for (int i = 0; i < X * Y; i++) {
            valid[i] = 0;
        }

        // 验证所有高于th_h或与之连接且高于th_l的边缘点
        for (int i = 0; i < X * Y; i++) {
            if ((prev[i] >= 0 || next[i] >= 0) && !valid[i] && modG[i] >= th_h) {
                valid[i] = 1;

                // 前向跟随链
                for (int j = i; j >= 0; j = next[j]) {
                    int k = next[j];
                    if (k < 0 || valid[k]) break;
                    
                    if (modG[k] < th_l) {
                        next[j] = -1;
                        prev[k] = -1;
                        break;
                    }
                    else {
                        valid[k] = 1;
                    }
                }

                // 后向跟随链
                for (int j = i; j >= 0; j = prev[j]) {
                    int k = prev[j];
                    if (k < 0 || valid[k]) break;
                    
                    if (modG[k] < th_l) {
                        prev[j] = -1;
                        next[k] = -1;
                        break;
                    }
                    else {
                        valid[k] = 1;
                    }
                }
            }
        }

        // 移除任何剩余的非有效链接点
        for (int i = 0; i < X * Y; i++) {
            if ((prev[i] >= 0 || next[i] >= 0) && !valid[i]) {
                prev[i] = next[i] = -1;
            }
        }

        free(valid);
    }

    // 生成链接的边缘点列表
    void CannySubPixelEdge::ListChainedEdgePoints(double** x, double** y, int* N,
                                                  int** curve_limits, int* M,
                                                  int* next, int* prev,
                                                  const double* Ex, const double* Ey, 
                                                  int X, int Y) {
        // 分配内存
        *x = (double*)Malloc(X * Y * sizeof(double));
        *y = (double*)Malloc(X * Y * sizeof(double));
        *curve_limits = (int*)Malloc(X * Y * sizeof(int));
        
        if (*x == nullptr || *y == nullptr || *curve_limits == nullptr) {
            if (*x) free(*x);
            if (*y) free(*y);
            if (*curve_limits) free(*curve_limits);
            *x = *y = nullptr;
            *curve_limits = nullptr;
            return;
        }

        *N = 0;
        *M = 0;

        // 复制链接的边缘点到输出
        for (int i = 0; i < X * Y; i++) {
            if (prev[i] >= 0 || next[i] >= 0) {
                (*curve_limits)[*M] = *N;
                ++(*M);

                // 找到链的开始
                int k = i;
                int n;
                for (k = i; (n = prev[k]) >= 0 && n != i; k = n);

                // 跟随边缘点链
                do {
                    (*x)[*N] = Ex[k];
                    (*y)[*N] = Ey[k];
                    ++(*N);

                    n = next[k];
                    next[k] = -1;
                    prev[k] = -1;
                    k = n;
                } while (k >= 0);
            }
        }
        (*curve_limits)[*M] = *N;
    }

    // 主检测函数
    bool CannySubPixelEdge::DetectEdges(const double* image, int width, int height,
                                       double sigma, double th_h, double th_l,
                                       CannyEdgeResult& result) {
        if (image == nullptr || width <= 0 || height <= 0) {
            SetError("DetectEdges: invalid input parameters");
            return false;
        }

        result.curves.clear();
        result.totalPoints = 0;
        result.imageWidth = width;
        result.imageHeight = height;

        // 分配工作内存
        double* Gx = (double*)Malloc(width * height * sizeof(double));
        double* Gy = (double*)Malloc(width * height * sizeof(double));
        double* modG = (double*)Malloc(width * height * sizeof(double));
        double* Ex = (double*)Malloc(width * height * sizeof(double));
        double* Ey = (double*)Malloc(width * height * sizeof(double));
        int* next = (int*)Malloc(width * height * sizeof(int));
        int* prev = (int*)Malloc(width * height * sizeof(int));

        if (!Gx || !Gy || !modG || !Ex || !Ey || !next || !prev) {
            if (Gx) free(Gx);
            if (Gy) free(Gy);
            if (modG) free(modG);
            if (Ex) free(Ex);
            if (Ey) free(Ey);
            if (next) free(next);
            if (prev) free(prev);
            SetError("DetectEdges: memory allocation failed");
            return false;
        }

        // 高斯滤波（如果sigma > 0）
        double* gauss = nullptr;
        if (sigma > 0.0) {
            gauss = GaussianFilter(image, width, height, sigma);
            if (gauss == nullptr) {
                free(Gx); free(Gy); free(modG); free(Ex); free(Ey); free(next); free(prev);
                return false;
            }
            ComputeGradient(Gx, Gy, modG, gauss, width, height);
        }
        else {
            ComputeGradient(Gx, Gy, modG, image, width, height);
        }

        // 计算边缘点
        ComputeEdgePoints(Ex, Ey, modG, Gx, Gy, width, height);

        // 链接边缘点
        ChainEdgePoints(next, prev, Ex, Ey, Gx, Gy, width, height);

        // 应用滞后阈值
        ThresholdsWithHysteresis(next, prev, modG, width, height, th_h, th_l);

        // 生成输出列表
        double* x = nullptr;
        double* y = nullptr;
        int* curve_limits = nullptr;
        int N = 0, M = 0;
        
        ListChainedEdgePoints(&x, &y, &N, &curve_limits, &M, 
                            next, prev, Ex, Ey, width, height);

        // 转换为输出结构
        if (x && y && curve_limits) {
            for (int k = 0; k < M; k++) {
                EdgeCurve curve;
                int start = curve_limits[k];
                int end = curve_limits[k + 1];
                
                for (int i = start; i < end; i++) {
                    curve.points.push_back(EdgePoint(x[i], y[i]));
                }
                
                // 检查是否为闭合曲线
                if (end > start) {
                    if (x[start] == x[end - 1] && y[start] == y[end - 1]) {
                        curve.isClosed = true;
                    }
                }
                
                result.curves.push_back(curve);
            }
            result.totalPoints = N;

            free(x);
            free(y);
            free(curve_limits);
        }

        // 释放内存
        if (gauss) free(gauss);
        free(Gx);
        free(Gy);
        free(modG);
        free(Ex);
        free(Ey);
        free(next);
        free(prev);

        return true;
    }

    // 从字节图像检测边缘
    bool CannySubPixelEdge::DetectEdgesFromBytes(const unsigned char* image, 
                                                 int width, int height,
                                                 double sigma, double th_h, double th_l,
                                                 CannyEdgeResult& result) {
        if (image == nullptr || width <= 0 || height <= 0) {
            SetError("DetectEdgesFromBytes: invalid input parameters");
            return false;
        }

        // 转换为double数组
        double* doubleImage = (double*)Malloc(width * height * sizeof(double));
        if (doubleImage == nullptr) {
            return false;
        }

        for (int i = 0; i < width * height; i++) {
            doubleImage[i] = (double)image[i];
        }

        bool success = DetectEdges(doubleImage, width, height, sigma, th_h, th_l, result);
        
        free(doubleImage);
        return success;
    }

} // namespace TulipAlg
