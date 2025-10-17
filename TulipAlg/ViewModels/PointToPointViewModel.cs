using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TulipAlg.Core;

namespace TulipAlg.ViewModels
{
    /// <summary>
    /// 点-点功能视图模型
    /// </summary>
    public partial class PointToPointViewModel : ObservableObject
    {
        // 输入参数
        [ObservableProperty]
        private double _pointX = 10;

        [ObservableProperty]
        private double _pointY = 10;

        [ObservableProperty]
        private double _dx = 5;

        [ObservableProperty]
        private double _dy = 5;

        [ObservableProperty]
        private double _centerX = 0;

        [ObservableProperty]
        private double _centerY = 0;

        [ObservableProperty]
        private double _angleDegrees = 45;

        [ObservableProperty]
        private double _point2X = 20;

        [ObservableProperty]
        private double _point2Y = 20;

        // 输出结果
        [ObservableProperty]
        private string _translateResult = string.Empty;

        [ObservableProperty]
        private string _rotateResult = string.Empty;

        [ObservableProperty]
        private string _distanceResult = string.Empty;

        [ObservableProperty]
        private string _midPointResult = string.Empty;

        [RelayCommand]
        private void CalculateTranslate()
        {
            try
            {
                var point = new PointD(PointX, PointY);
                var result = AlgGeometry.Translate(point, Dx, Dy);
                TranslateResult = $"平移后: ({result.X:F2}, {result.Y:F2})";
            }
            catch (Exception ex)
            {
                TranslateResult = $"错误: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CalculateRotate()
        {
            try
            {
                var point = new PointD(PointX, PointY);
                var center = new PointD(CenterX, CenterY);
                var result = AlgGeometry.Rotate(point, center, AngleDegrees);
                RotateResult = $"旋转后: ({result.X:F2}, {result.Y:F2})";
            }
            catch (Exception ex)
            {
                RotateResult = $"错误: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CalculateDistance()
        {
            try
            {
                var point1 = new PointD(PointX, PointY);
                var point2 = new PointD(Point2X, Point2Y);
                var result = AlgGeometry.Distance(point1, point2);
                DistanceResult = $"距离: {result:F2}";
            }
            catch (Exception ex)
            {
                DistanceResult = $"错误: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CalculateMidPoint()
        {
            try
            {
                var point1 = new PointD(PointX, PointY);
                var point2 = new PointD(Point2X, Point2Y);
                var result = AlgGeometry.MidPoint(point1, point2);
                MidPointResult = $"中点: ({result.X:F2}, {result.Y:F2})";
            }
            catch (Exception ex)
            {
                MidPointResult = $"错误: {ex.Message}";
            }
        }
    }
}
