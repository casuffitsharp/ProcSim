using System.Windows.Controls;
using System.Windows.Data;

namespace ProcSim.Views;

public partial class SimulationControlView : UserControl
{
    public SimulationControlView()
    {
        InitializeComponent();
    }

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
    private void Slider_DragCompleted(object sender, object e)
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
    {
        if (sender is Slider slider)
        {
            BindingExpression binding = slider.GetBindingExpression(Slider.ValueProperty);
            binding?.UpdateSource();
        }
    }
}
