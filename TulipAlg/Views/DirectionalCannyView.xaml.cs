using System.Windows.Controls;
using TulipAlg.ViewModels;

namespace TulipAlg.Views
{
    /// <summary>
    /// DirectionalCannyView.xaml 的交互逻辑
    /// </summary>
    public partial class DirectionalCannyView : UserControl
    {
        private DirectionalCannyViewModel _viewModel;

        public DirectionalCannyView(DirectionalCannyViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            _viewModel = viewModel;
        }
    }
}
