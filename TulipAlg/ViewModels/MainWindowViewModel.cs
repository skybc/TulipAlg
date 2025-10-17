using System.Collections.ObjectModel;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TulipAlg.Services;
using TulipAlg.Views;
using MenuItem = TulipAlg.Models.MenuItem;

namespace TulipAlg.ViewModels
{
    /// <summary>
    /// 主窗口视图模型
    /// </summary>
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private UserControl? _currentView;

        [ObservableProperty]
        private ObservableCollection<MenuItem> _menuItems;

        public MainWindowViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            _navigationService.CurrentViewChanged += OnCurrentViewChanged;
            
            _menuItems = new ObservableCollection<MenuItem>();
            InitializeMenu();
        }

        private void OnCurrentViewChanged(object? sender, UserControl view)
        {
            CurrentView = view;
        }

        private void InitializeMenu()
        {
            var generalMenu = new MenuItem
            {
                Header = "通用",
                Children = new ObservableCollection<MenuItem>
                {
                    new MenuItem { Header = "点-点", ViewType = typeof(PointToPointView) },
                    new MenuItem { Header = "线-线", ViewType = typeof(LineToLineView) },
                    new MenuItem { Header = "点-线", ViewType = typeof(PointToLineView) },
                    new MenuItem { Header = "拟合圆", ViewType = typeof(FitCircleView) },
                    new MenuItem { Header = "点-圆", ViewType = typeof(PointToCircleView) },
                    new MenuItem { Header = "圆-圆", ViewType = typeof(CircleToCircleView) },
                    new MenuItem { Header = "点-区域", ViewType = typeof(PointToRegionView) }
                }
            };

            MenuItems.Add(generalMenu);
        }

        [RelayCommand]
        private void MenuItemSelected(MenuItem? menuItem)
        {
            if (menuItem?.ViewType != null)
            {
                _navigationService.NavigateTo(menuItem.ViewType);
            }
        }
    }
}
