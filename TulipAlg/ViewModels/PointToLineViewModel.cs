using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TulipAlg.Core;

namespace TulipAlg.ViewModels
{
    /// <summary>
    /// 点-线功能视图模型
    /// </summary>
    public partial class PointToLineViewModel : ObservableObject
    {
        // 点
        [ObservableProperty]
        private double _pointX = 5;

        [ObservableProperty]
        private double _pointY = 5;

        // 线
        [ObservableProperty]
        private double _lineStartX = 0;

        [ObservableProperty]
        private double _lineStartY = 0;

        [ObservableProperty]
        private double _lineEndX = 10;

        [ObservableProperty]
        private double _lineEndY = 0;

        // 结果
        [ObservableProperty]
        private string _distanceResult = string.Empty;

        [ObservableProperty]
        private string _perpendicularPointResult = string.Empty;

        [ObservableProperty]
        private string _isOnLineResult = string.Empty;

        [RelayCommand]
        private void CalculateDistance()
        {
            try
            {
                var point = new PointD(PointX, PointY);
                var line = new LineD(new PointD(LineStartX, LineStartY), new PointD(LineEndX, LineEndY));
                var result = AlgGeometry.Distance(point, line);
                DistanceResult = $"点到线的距离: {result:F2}";
            }
            catch (Exception ex)
            {
                DistanceResult = $"错误: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CalculatePerpendicularPoint()
        {
            try
            {
                var point = new PointD(PointX, PointY);
                var line = new LineD(new PointD(LineStartX, LineStartY), new PointD(LineEndX, LineEndY));
                var result = AlgGeometry.PerpendicularPoint(point, line);
                PerpendicularPointResult = $"垂足点: ({result.X:F2}, {result.Y:F2})";
            }
            catch (Exception ex)
            {
                PerpendicularPointResult = $"错误: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CheckIsOnLine()
        {
            try
            {
                var point = new PointD(PointX, PointY);
                var line = new LineD(new PointD(LineStartX, LineStartY), new PointD(LineEndX, LineEndY));
                var result = AlgGeometry.IsPointOnLine(point, line);
                IsOnLineResult = $"点是否在线上: {(result ? "是" : "否")}";
            }
            catch (Exception ex)
            {
                IsOnLineResult = $"错误: {ex.Message}";
            }
        }
    }
}
