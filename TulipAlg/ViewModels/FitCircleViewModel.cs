using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TulipAlg.Core;

namespace TulipAlg.ViewModels
{
    /// <summary>
    /// 拟合圆功能视图模型
    /// </summary>
    public partial class FitCircleViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _pointsInput = "0,0; 10,0; 5,8.66";

        [ObservableProperty]
        private string _fitCircleResult = string.Empty;

        [ObservableProperty]
        private string _minEnclosingCircleResult = string.Empty;

        [RelayCommand]
        private void CalculateFitCircle()
        {
            try
            {
                var points = ParsePoints(PointsInput);
                if (points.Count < 3)
                {
                    FitCircleResult = "错误: 至少需要3个点";
                    return;
                }
                var result = AlgGeometry.FitCircleToPoints(points);
                FitCircleResult = $"拟合圆 - 圆心: ({result.Center.X:F2}, {result.Center.Y:F2}), 半径: {result.Radius:F2}";
            }
            catch (Exception ex)
            {
                FitCircleResult = $"错误: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CalculateMinEnclosingCircle()
        {
            try
            {
                var points = ParsePoints(PointsInput);
                if (points.Count < 1)
                {
                    MinEnclosingCircleResult = "错误: 至少需要1个点";
                    return;
                }
                var result = AlgGeometry.MinimumEnclosingCircle(points);
                MinEnclosingCircleResult = $"最小外接圆 - 圆心: ({result.Center.X:F2}, {result.Center.Y:F2}), 半径: {result.Radius:F2}";
            }
            catch (Exception ex)
            {
                MinEnclosingCircleResult = $"错误: {ex.Message}";
            }
        }

        private List<PointD> ParsePoints(string input)
        {
            var points = new List<PointD>();
            var pairs = input.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var coords = pair.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (coords.Length == 2 && 
                    double.TryParse(coords[0].Trim(), out double x) && 
                    double.TryParse(coords[1].Trim(), out double y))
                {
                    points.Add(new PointD(x, y));
                }
            }
            return points;
        }
    }
}
