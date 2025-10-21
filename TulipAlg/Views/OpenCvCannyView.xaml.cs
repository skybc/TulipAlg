using System.Windows.Controls;
using TulipAlg.ViewModels;

namespace TulipAlg.Views
{
    /// <summary>
    /// OpenCvCannyView.xaml 的交互逻辑
    /// </summary>
    public partial class OpenCvCannyView : UserControl
    {
        public OpenCvCannyView(OpenCvCannyViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
