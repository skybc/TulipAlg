/*----------------------------------------------------------------------------
  CannySubPixelEdge C Export Implementation
  C-style export functions for C# P/Invoke
----------------------------------------------------------------------------*/

#include "pch.h"
#include "CannySubPixelEdgeExport.h"
#include "CannySubPixelEdge.h"
#include <cstring>

using namespace TulipAlg;

// 创建CannySubPixelEdge实例
extern "C" CANNY_EXPORT void* CannySubPixelEdge_Create()
{
    try
    {
        return new CannySubPixelEdge();
    }
    catch (...)
    {
        return nullptr;
    }
}

// 销毁CannySubPixelEdge实例
extern "C" CANNY_EXPORT void CannySubPixelEdge_Destroy(void* instance)
{
    if (instance)
    {
        delete static_cast<CannySubPixelEdge*>(instance);
    }
}

// 从double数组检测边缘
extern "C" CANNY_EXPORT bool CannySubPixelEdge_DetectEdges(
    void* instance,
    const double* image,
    const unsigned char* mask,
    int width,
    int height,
    double sigma,
    double th_h,
    double th_l,
    double** x_out,
    double** y_out,
    int** curve_limits_out,
    int* totalPoints,
    int* curveCount)
{
    if (!instance || !image || !x_out || !y_out || !curve_limits_out || !totalPoints || !curveCount)
        return false;

    try
    {
        CannySubPixelEdge* detector = static_cast<CannySubPixelEdge*>(instance);
        CannyEdgeResult result;

        if (!detector->DetectEdges(image, mask, width, height, sigma, th_h, th_l, result))
            return false;

        // 计算总点数
        int N = result.totalPoints;
        int M = (int)result.curves.size();

        if (N == 0 || M == 0)
        {
            *x_out = nullptr;
            *y_out = nullptr;
            *curve_limits_out = nullptr;
            *totalPoints = 0;
            *curveCount = 0;
            return true;
        }

        // 分配内存
        double* x = (double*)malloc(N * sizeof(double));
        double* y = (double*)malloc(N * sizeof(double));
        int* limits = (int*)malloc((M + 1) * sizeof(int));

        if (!x || !y || !limits)
        {
            if (x) free(x);
            if (y) free(y);
            if (limits) free(limits);
            return false;
        }

        // 复制数据
        int pointIndex = 0;
        for (int k = 0; k < M; k++)
        {
            limits[k] = pointIndex;
            for (size_t i = 0; i < result.curves[k].points.size(); i++)
            {
                x[pointIndex] = result.curves[k].points[i].x;
                y[pointIndex] = result.curves[k].points[i].y;
                pointIndex++;
            }
        }
        limits[M] = pointIndex;

        *x_out = x;
        *y_out = y;
        *curve_limits_out = limits;
        *totalPoints = N;
        *curveCount = M;

        return true;
    }
    catch (...)
    {
        return false;
    }
}

// 从byte数组检测边缘
extern "C" CANNY_EXPORT bool CannySubPixelEdge_DetectEdgesFromBytes(
    void* instance,
    const unsigned char* image,
    const unsigned char* mask,
    int width,
    int height,
    double sigma,
    double th_h,
    double th_l,
    double** x_out,
    double** y_out,
    int** curve_limits_out,
    int* totalPoints,
    int* curveCount)
{
    if (!instance || !image || !x_out || !y_out || !curve_limits_out || !totalPoints || !curveCount)
        return false;

    try
    {
        CannySubPixelEdge* detector = static_cast<CannySubPixelEdge*>(instance);
        CannyEdgeResult result;

        if (!detector->DetectEdgesFromBytes(image, mask, width, height, sigma, th_h, th_l, result))
            return false;

        // 计算总点数
        int N = result.totalPoints;
        int M = (int)result.curves.size();

        if (N == 0 || M == 0)
        {
            *x_out = nullptr;
            *y_out = nullptr;
            *curve_limits_out = nullptr;
            *totalPoints = 0;
            *curveCount = 0;
            return true;
        }

        // 分配内存
        double* x = (double*)malloc(N * sizeof(double));
        double* y = (double*)malloc(N * sizeof(double));
        int* limits = (int*)malloc((M + 1) * sizeof(int));

        if (!x || !y || !limits)
        {
            if (x) free(x);
            if (y) free(y);
            if (limits) free(limits);
            return false;
        }

        // 复制数据
        int pointIndex = 0;
        for (int k = 0; k < M; k++)
        {
            limits[k] = pointIndex;
            for (size_t i = 0; i < result.curves[k].points.size(); i++)
            {
                x[pointIndex] = result.curves[k].points[i].x;
                y[pointIndex] = result.curves[k].points[i].y;
                pointIndex++;
            }
        }
        limits[M] = pointIndex;

        *x_out = x;
        *y_out = y;
        *curve_limits_out = limits;
        *totalPoints = N;
        *curveCount = M;

        return true;
    }
    catch (...)
    {
        return false;
    }
}

// 释放检测结果的内存
extern "C" CANNY_EXPORT void CannySubPixelEdge_FreeResult(
    double* x,
    double* y,
    int* curve_limits)
{
    if (x) free(x);
    if (y) free(y);
    if (curve_limits) free(curve_limits);
}

// 获取最后的错误信息
extern "C" CANNY_EXPORT const char* CannySubPixelEdge_GetLastError(void* instance)
{
    if (!instance)
        return nullptr;

    try
    {
        CannySubPixelEdge* detector = static_cast<CannySubPixelEdge*>(instance);
        return detector->GetLastError();
    }
    catch (...)
    {
        return nullptr;
    }
}
