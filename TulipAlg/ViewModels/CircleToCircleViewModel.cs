using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TulipAlg.Core;

namespace TulipAlg.ViewModels
{
    /// <summary>
    /// 圆-圆功能视图模型
    /// </summary>
    public partial class CircleToCircleViewModel : ObservableObject
    {
        // 圆1
        [ObservableProperty]
        private double _circle1CenterX = 0;

        [ObservableProperty]
        private double _circle1CenterY = 0;

        [ObservableProperty]
        private double _circle1Radius = 5;

        // 圆2
        [ObservableProperty]
        private double _circle2CenterX = 8;

        [ObservableProperty]
        private double _circle2CenterY = 0;

        [ObservableProperty]
        private double _circle2Radius = 5;

        // 结果
        [ObservableProperty]
        private string _intersectionPointsResult = string.Empty;

        [ObservableProperty]
        private string _distanceResult = string.Empty;

        [RelayCommand]
        private void CalculateIntersectionPoints()
        {
            try
            {
                var circle1 = new CircleD(new PointD(Circle1CenterX, Circle1CenterY), Circle1Radius);
                var circle2 = new CircleD(new PointD(Circle2CenterX, Circle2CenterY), Circle2Radius);
                var result = AlgGeometry.CircleIntersectionPoints(circle1, circle2);
                
                if (result.Count == 0)
                {
                    IntersectionPointsResult = "交点: 无交点";
                }
                else if (result.Count == 1)
                {
                    IntersectionPointsResult = $"交点: ({result[0].X:F2}, {result[0].Y:F2})";
                }
                else
                {
                    IntersectionPointsResult = $"交点1: ({result[0].X:F2}, {result[0].Y:F2})\n交点2: ({result[1].X:F2}, {result[1].Y:F2})";
                }
            }
            catch (Exception ex)
            {
                IntersectionPointsResult = $"错误: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CalculateDistance()
        {
            try
            {
                var circle1 = new CircleD(new PointD(Circle1CenterX, Circle1CenterY), Circle1Radius);
                var circle2 = new CircleD(new PointD(Circle2CenterX, Circle2CenterY), Circle2Radius);
                var result = AlgGeometry.DistanceBetweenCircles(circle1, circle2);
                var centerDist = AlgGeometry.DistanceBetweenCircleCenters(circle1, circle2);
                
                DistanceResult = $"圆心距离: {centerDist:F2}\n圆周距离: {result:F2}";
                if (result > 0)
                    DistanceResult += "\n状态: 相离";
                else if (Math.Abs(result) < 1e-10)
                    DistanceResult += "\n状态: 相切";
                else
                    DistanceResult += "\n状态: 相交或包含";
            }
            catch (Exception ex)
            {
                DistanceResult = $"错误: {ex.Message}";
            }
        }
    }
}
