using System.Windows.Controls;
using System.Windows.Data;

namespace ProcSim.Views;

public partial class SimulationControlView : UserControl
{
    public SimulationControlView()
    {
        InitializeComponent();
    }

    private void Slider_DragCompleted(object sender, object e)
    {
        if (sender is Slider slider)
        {
            BindingExpression binding = slider.GetBindingExpression(Slider.ValueProperty);
            binding?.UpdateSource();
        }
    }
}
