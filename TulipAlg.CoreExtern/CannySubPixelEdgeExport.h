/*----------------------------------------------------------------------------
  CannySubPixelEdge C Export Interface
  C-style export functions for C# P/Invoke
----------------------------------------------------------------------------*/

#pragma once

#ifdef __cplusplus
extern "C" {
#endif

#ifdef TULIPALGCOREEXTERN_EXPORTS
#define CANNY_EXPORT __declspec(dllexport)
#else
#define CANNY_EXPORT __declspec(dllimport)
#endif

// 创建CannySubPixelEdge实例
CANNY_EXPORT void* CannySubPixelEdge_Create();

// 销毁CannySubPixelEdge实例
CANNY_EXPORT void CannySubPixelEdge_Destroy(void* instance);

// 从double数组检测边缘
// 返回的x, y, curve_limits数组需要通过CannySubPixelEdge_FreeResult释放
CANNY_EXPORT bool CannySubPixelEdge_DetectEdges(
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
    int* curveCount);

// 从byte数组检测边缘
CANNY_EXPORT bool CannySubPixelEdge_DetectEdgesFromBytes(
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
    int* curveCount);

// 释放检测结果的内存
CANNY_EXPORT void CannySubPixelEdge_FreeResult(
    double* x,
    double* y,
    int* curve_limits);

// 获取最后的错误信息
CANNY_EXPORT const char* CannySubPixelEdge_GetLastError(void* instance);

#ifdef __cplusplus
}
#endif
