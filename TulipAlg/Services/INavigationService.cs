using System;
using System.Windows.Controls;

namespace TulipAlg.Services
{
    /// <summary>
    /// 导航服务接口，用于管理页面切换
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// 导航到指定的视图类型
        /// </summary>
        /// <param name="viewType">视图类型</param>
        void NavigateTo(Type viewType);

        /// <summary>
        /// 当前视图变化事件
        /// </summary>
        event EventHandler<UserControl>? CurrentViewChanged;

        /// <summary>
        /// 获取当前视图
        /// </summary>
        UserControl? CurrentView { get; }
    }
}
