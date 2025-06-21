using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProcSim.Views;

public partial class TextDialog : UserControl
{
    public TextDialog(string title, string text)
    {
        InitializeComponent();
        Title = title;
        FullText = text;
    }

    public static readonly DependencyProperty FullTextProperty = DependencyProperty.Register(nameof(FullText), typeof(string), typeof(TextDialog), new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(TextDialog), new PropertyMetadata(string.Empty));

    public string FullText
    {
        get => (string)GetValue(FullTextProperty);
        set => SetValue(FullTextProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static async Task ShowValidationErrorsAsync(string title, List<string> errors)
    {
        string text = string.Join(Environment.NewLine, errors.Select(e => "• " + e));
        TextDialog dlg = new(title, text);
        await DialogHost.Show(dlg);
    }

    public static async Task ShowTextAsync(string title, string text)
    {
        TextDialog dlg = new(title, text);
        await DialogHost.Show(dlg);
    }

    private void UserControl_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogHost.CloseDialogCommand.Execute(null, this);
            e.Handled = true;
        }
    }
}
