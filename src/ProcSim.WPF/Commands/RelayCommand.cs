using System.Windows.Input;

namespace ProcSim.Wpf.Commands;

public class RelayCommand(Action execute, Func<bool> canExecute = null) : ICommand
{
    public bool CanExecute(object parameter) => canExecute == null || canExecute();
    
    public void Execute(object parameter) => execute();
    
    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
