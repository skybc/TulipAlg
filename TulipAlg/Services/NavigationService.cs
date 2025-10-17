using System;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace TulipAlg.Services
{
    /// <summary>
    /// 导航服务实现类
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private UserControl? _currentView;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public event EventHandler<UserControl>? CurrentViewChanged;

        public UserControl? CurrentView
        {
            get => _currentView;
            private set
            {
                _currentView = value;
                if (_currentView != null)
                {
                    CurrentViewChanged?.Invoke(this, _currentView);
                }
            }
        }

        public void NavigateTo(Type viewType)
        {
            if (viewType == null)
                throw new ArgumentNullException(nameof(viewType));

            var view = _serviceProvider.GetService(viewType) as UserControl;
            if (view == null)
                throw new InvalidOperationException($"无法创建类型 {viewType.Name} 的视图");

            CurrentView = view;
        }
    }
}
