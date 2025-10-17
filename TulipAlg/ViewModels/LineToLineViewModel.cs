using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TulipAlg.Core;

namespace TulipAlg.ViewModels
{
    /// <summary>
    /// 线-线功能视图模型
    /// </summary>
    public partial class LineToLineViewModel : ObservableObject
    {
        // 线1
        [ObservableProperty]
        private double _line1StartX = 0;

        [ObservableProperty]
        private double _line1StartY = 0;

        [ObservableProperty]
        private double _line1EndX = 10;

        [ObservableProperty]
        private double _line1EndY = 10;

        // 线2
        [ObservableProperty]
        private double _line2StartX = 10;

        [ObservableProperty]
        private double _line2StartY = 0;

        [ObservableProperty]
        private double _line2EndX = 0;

        [ObservableProperty]
        private double _line2EndY = 10;

        // 结果
        [ObservableProperty]
        private string _parallelResult = string.Empty;

        [ObservableProperty]
        private string _perpendicularResult = string.Empty;

        [ObservableProperty]
        private string _intersectionResult = string.Empty;

        [ObservableProperty]
        private string _angleResult = string.Empty;

        [RelayCommand]
        private void CheckParallel()
        {
            try
            {
                var line1 = new LineD(new PointD(Line1StartX, Line1StartY), new PointD(Line1EndX, Line1EndY));
                var line2 = new LineD(new PointD(Line2StartX, Line2StartY), new PointD(Line2EndX, Line2EndY));
                var result = AlgGeometry.AreLinesParallel(line1, line2);
                ParallelResult = $"是否平行: {(result ? "是" : "否")}";
            }
            catch (Exception ex)
            {
                ParallelResult = $"错误: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CheckPerpendicular()
        {
            try
            {
                var line1 = new LineD(new PointD(Line1StartX, Line1StartY), new PointD(Line1EndX, Line1EndY));
                var line2 = new LineD(new PointD(Line2StartX, Line2StartY), new PointD(Line2EndX, Line2EndY));
                var result = AlgGeometry.AreLinesPerpendicular(line1, line2);
                PerpendicularResult = $"是否垂直: {(result ? "是" : "否")}";
            }
            catch (Exception ex)
            {
                PerpendicularResult = $"错误: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CalculateIntersection()
        {
            try
            {
                var line1 = new LineD(new PointD(Line1StartX, Line1StartY), new PointD(Line1EndX, Line1EndY));
                var line2 = new LineD(new PointD(Line2StartX, Line2StartY), new PointD(Line2EndX, Line2EndY));
                var result = AlgGeometry.Intersection(line1, line2);
                IntersectionResult = result.HasValue
                    ? $"交点: ({result.Value.X:F2}, {result.Value.Y:F2})"
                    : "交点: 无交点（平行或重合）";
            }
            catch (Exception ex)
            {
                IntersectionResult = $"错误: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CalculateAngle()
        {
            try
            {
                var line1 = new LineD(new PointD(Line1StartX, Line1StartY), new PointD(Line1EndX, Line1EndY));
                var line2 = new LineD(new PointD(Line2StartX, Line2StartY), new PointD(Line2EndX, Line2EndY));
                var result = AlgGeometry.AngleBetweenLines(line1, line2);
                AngleResult = $"夹角: {result:F2}°";
            }
            catch (Exception ex)
            {
                AngleResult = $"错误: {ex.Message}";
            }
        }
    }
}
