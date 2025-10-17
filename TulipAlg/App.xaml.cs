using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TulipAlg.Services;
using TulipAlg.ViewModels;
using TulipAlg.Views;

namespace TulipAlg
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 配置依赖注入
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();

            // 显示主窗口
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // 注册服务
            services.AddSingleton<INavigationService, NavigationService>();

            // 注册 ViewModels
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<PointToPointViewModel>();
            services.AddTransient<LineToLineViewModel>();
            services.AddTransient<PointToLineViewModel>();
            services.AddTransient<FitCircleViewModel>();
            services.AddTransient<PointToCircleViewModel>();
            services.AddTransient<CircleToCircleViewModel>();
            services.AddTransient<PointToRegionViewModel>();

            // 注册 Views
            services.AddTransient<PointToPointView>();
            services.AddTransient<LineToLineView>();
            services.AddTransient<PointToLineView>();
            services.AddTransient<FitCircleView>();
            services.AddTransient<PointToCircleView>();
            services.AddTransient<CircleToCircleView>();
            services.AddTransient<PointToRegionView>();

            // 注册 MainWindow
            services.AddSingleton<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
