/*----------------------------------------------------------------------------
  CannySubPixelEdge - C++ wrapper for Canny/Devernay's sub-pixel edge detector
  
  Based on the implementation by Rafael Grompone von Gioi and Gregory Randall
  Reference: "A Sub-Pixel Edge Detector: an Implementation of the Canny/Devernay
  Algorithm" by Rafael Grompone von Gioi and Gregory Randall,
  Image Processing On Line, 2017. DOI:10.5201/ipol.2017.216
  
  Copyright (c) 2016-2017 rafael grompone von gioi <grompone@gmail.com>,
                          Gregory Randall <randall@fing.edu.uy>
  
  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU Affero General Public License as
  published by the Free Software Foundation, either version 3 of the
  License, or (at your option) any later version.
----------------------------------------------------------------------------*/

#pragma once

#ifdef TULIPALGCOREEXTERN_EXPORTS
#define CANNYSUBPIXEL_API __declspec(dllexport)
#else
#define CANNYSUBPIXEL_API __declspec(dllimport)
#endif

#include <vector>
#include <string>

namespace TulipAlg {

    // 边缘点结构
    struct EdgePoint {
        double x;
        double y;

        EdgePoint() : x(0.0), y(0.0) {}
        EdgePoint(double _x, double _y) : x(_x), y(_y) {}
    };

    // 边缘曲线结构
    struct EdgeCurve {
        std::vector<EdgePoint> points;
        bool isClosed;

        EdgeCurve() : isClosed(false) {}
    };

    // Canny亚像素边缘检测结果
    struct CannyEdgeResult {
        std::vector<EdgeCurve> curves;
        int totalPoints;
        int imageWidth;
        int imageHeight;

        CannyEdgeResult() : totalPoints(0), imageWidth(0), imageHeight(0) {}
    };

    // Canny亚像素边缘检测类
    class CANNYSUBPIXEL_API CannySubPixelEdge {
    public:
        CannySubPixelEdge();
        ~CannySubPixelEdge();

        // 执行Canny/Devernay亚像素边缘检测
        // image: 输入图像数据 (double数组，image[x+y*width])
        // width, height: 图像尺寸
        // sigma: 高斯滤波标准差 (0.0表示不进行滤波)
        // th_h: Canny滞后阈值的高阈值
        // th_l: Canny滞后阈值的低阈值
        bool DetectEdges(const double* image, int width, int height,
                        double sigma, double th_h, double th_l,
                        CannyEdgeResult& result);

        // 从字节图像执行边缘检测 (自动转换为double)
        bool DetectEdgesFromBytes(const unsigned char* image, int width, int height,
                                 double sigma, double th_h, double th_l,
                                 CannyEdgeResult& result);

        // 获取最后的错误信息
        const char* GetLastError() const { return m_lastError.c_str(); }

    private:
        std::string m_lastError;

        // 内部辅助函数
        void SetError(const char* msg);
        void* Malloc(size_t size);
        
        // 高斯滤波
        double* GaussianFilter(const double* image, int X, int Y, double sigma);
        void GaussianKernel(double* kernel, int n, double sigma, double mean);
        
        // 梯度计算
        void ComputeGradient(double* Gx, double* Gy, double* modG,
                           const double* image, int X, int Y);
        
        // 边缘点计算
        void ComputeEdgePoints(double* Ex, double* Ey, const double* modG,
                             const double* Gx, const double* Gy, int X, int Y);
        
        // 边缘点链接
        void ChainEdgePoints(int* next, int* prev, const double* Ex, const double* Ey,
                           const double* Gx, const double* Gy, int X, int Y);
        double Chain(int from, int to, const double* Ex, const double* Ey,
                    const double* Gx, const double* Gy, int X, int Y);
        
        // 滞后阈值处理
        void ThresholdsWithHysteresis(int* next, int* prev,
                                     const double* modG, int X, int Y,
                                     double th_h, double th_l);
        
        // 生成链接的边缘点列表
        void ListChainedEdgePoints(double** x, double** y, int* N,
                                  int** curve_limits, int* M,
                                  int* next, int* prev,
                                  const double* Ex, const double* Ey, int X, int Y);
        
        // 辅助函数
        bool Greater(double a, double b);
        double Dist(double x1, double y1, double x2, double y2);
    };

} // namespace TulipAlg
