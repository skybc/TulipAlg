using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TulipAlg.Core;

namespace TulipAlg.ViewModels
{
    /// <summary>
    /// 点-区域功能视图模型
    /// </summary>
    public partial class PointToRegionViewModel : ObservableObject
    {
        // 点
        [ObservableProperty]
        private double _pointX = 5;

        [ObservableProperty]
        private double _pointY = 5;

        // 多边形顶点输入
        [ObservableProperty]
        private string _polygonInput = "0,0; 10,0; 10,10; 0,10";

        // 圆形区域
        [ObservableProperty]
        private double _regionCenterX = 5;

        [ObservableProperty]
        private double _regionCenterY = 5;

        [ObservableProperty]
        private double _regionRadius = 10;

        // 结果
        [ObservableProperty]
        private string _isInPolygonResult = string.Empty;

        [ObservableProperty]
        private string _isInCircleRegionResult = string.Empty;

        [RelayCommand]
        private void CheckIsInPolygon()
        {
            try
            {
                var point = new PointD(PointX, PointY);
                var polygon = ParsePoints(PolygonInput);
                if (polygon.Count < 3)
                {
                    IsInPolygonResult = "错误: 多边形至少需要3个顶点";
                    return;
                }
                var result = AlgGeometry.IsPointInPolygon(point, polygon);
                IsInPolygonResult = $"点是否在多边形内: {(result ? "是" : "否")}";
            }
            catch (Exception ex)
            {
                IsInPolygonResult = $"错误: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CheckIsInCircleRegion()
        {
            try
            {
                var point = new PointD(PointX, PointY);
                var circle = new CircleD(new PointD(RegionCenterX, RegionCenterY), RegionRadius);
                var result = AlgGeometry.IsPointInCircleRegion(point, circle);
                IsInCircleRegionResult = $"点是否在圆形区域内: {(result ? "是" : "否")}";
            }
            catch (Exception ex)
            {
                IsInCircleRegionResult = $"错误: {ex.Message}";
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
