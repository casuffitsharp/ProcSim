using System.Windows.Controls;
using ProcSim.WPF.ViewModels;

namespace ProcSim.WPF.Views;

public partial class ProcessRegistrationView : UserControl
{
    public ProcessRegistrationView()
    {
        InitializeComponent();
    }

    public ProcessRegistrationView(ProcessRegistrationViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
