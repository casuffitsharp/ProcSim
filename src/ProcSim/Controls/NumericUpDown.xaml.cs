using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProcSim.Controls;

public partial class NumericUpDown : UserControl
{
    public NumericUpDown()
    {
        InitializeComponent();
    }

    // Propriedade de dependência para o valor atual
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(int), typeof(NumericUpDown),
            new PropertyMetadata(0, OnValueChanged));

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericUpDown control)
        {
            control.RaiseCanExecuteChanged();
        }
    }

    // Propriedade de dependência para o valor mínimo
    public static readonly DependencyProperty MinValueProperty =
        DependencyProperty.Register(nameof(MinValue), typeof(int), typeof(NumericUpDown),
            new PropertyMetadata(0));

    public int MinValue
    {
        get => (int)GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }

    // Propriedade de dependência para o valor máximo
    public static readonly DependencyProperty MaxValueProperty =
        DependencyProperty.Register(nameof(MaxValue), typeof(int), typeof(NumericUpDown),
            new PropertyMetadata(100));

    public int MaxValue
    {
        get => (int)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    // Comando para incrementar o valor
    private ICommand _incrementCommand;
    public ICommand IncrementCommand => _incrementCommand ??= new RelayCommand(
        _ =>
        {
            if (Value < MaxValue)
                Value++;
        },
        _ => Value < MaxValue);

    // Comando para decrementar o valor
    private ICommand _decrementCommand;
    public ICommand DecrementCommand => _decrementCommand ??= new RelayCommand(
        _ =>
        {
            if (Value > MinValue)
                Value--;
        },
        _ => Value > MinValue);

    // Método para atualizar o estado dos comandos
    private void RaiseCanExecuteChanged()
    {
        (IncrementCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (DecrementCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }
}

public class RelayCommand(Action<object> execute, Predicate<object> canExecute = null) : ICommand
{
    private readonly Action<object> _execute = execute ?? throw new ArgumentNullException(nameof(execute));

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter) => canExecute?.Invoke(parameter) ?? true;

    public void Execute(object parameter) => _execute(parameter);

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
