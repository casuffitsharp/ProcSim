using System.Windows.Controls;
using System.Windows.Input;

namespace ProcSim.Views;

public partial class TaskManagerControl : UserControl
{
    public TaskManagerControl()
    {
        InitializeComponent();
    }

    private void ComboBox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is ComboBox cb && cb.IsDropDownOpen)
            cb.IsDropDownOpen = false;
    }
}
