using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TulipAlg.Core;

namespace TulipAlg.ViewModels
{
    /// <summary>
    /// 点-圆功能视图模型
    /// </summary>
    public partial class PointToCircleViewModel : ObservableObject
    {
        // 点
        [ObservableProperty]
        private double _pointX = 15;

        [ObservableProperty]
        private double _pointY = 0;

        // 圆
        [ObservableProperty]
        private double _circleCenterX = 0;

        [ObservableProperty]
        private double _circleCenterY = 0;

        [ObservableProperty]
        private double _circleRadius = 10;

        // 结果
        [ObservableProperty]
        private string _isInsideResult = string.Empty;

        [ObservableProperty]
        private string _isOnCircleResult = string.Empty;

        [ObservableProperty]
        private string _isOutsideResult = string.Empty;

        [ObservableProperty]
        private string _closestPointResult = string.Empty;

        [RelayCommand]
        private void CheckIsInside()
        {
            try
            {
                var point = new PointD(PointX, PointY);
                var circle = new CircleD(new PointD(CircleCenterX, CircleCenterY), CircleRadius);
                var result = AlgGeometry.IsPointInsideCircle(point, circle);
                IsInsideResult = $"点是否在圆内: {(result ? "是" : "否")}";
            }
            catch (Exception ex)
            {
                IsInsideResult = $"错误: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CheckIsOnCircle()
        {
            try
            {
                var point = new PointD(PointX, PointY);
                var circle = new CircleD(new PointD(CircleCenterX, CircleCenterY), CircleRadius);
                var result = AlgGeometry.IsPointOnCircle(point, circle);
                IsOnCircleResult = $"点是否在圆上: {(result ? "是" : "否")}";
            }
            catch (Exception ex)
            {
                IsOnCircleResult = $"错误: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CheckIsOutside()
        {
            try
            {
                var point = new PointD(PointX, PointY);
                var circle = new CircleD(new PointD(CircleCenterX, CircleCenterY), CircleRadius);
                var result = AlgGeometry.IsPointOutsideCircle(point, circle);
                IsOutsideResult = $"点是否在圆外: {(result ? "是" : "否")}";
            }
            catch (Exception ex)
            {
                IsOutsideResult = $"错误: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CalculateClosestPoint()
        {
            try
            {
                var point = new PointD(PointX, PointY);
                var circle = new CircleD(new PointD(CircleCenterX, CircleCenterY), CircleRadius);
                var result = AlgGeometry.ClosestPointOnCircle(point, circle);
                ClosestPointResult = $"圆上最近点: ({result.X:F2}, {result.Y:F2})";
            }
            catch (Exception ex)
            {
                ClosestPointResult = $"错误: {ex.Message}";
            }
        }
    }
}
