using System.Windows;

namespace ProcSim.Mockups
{
    public partial class OptionAWindow : Window
    {
        public OptionAWindow()
        {
            InitializeComponent();
        }

        private void AddProcess_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtProcessName.Text))
            {
                lbProcessList.Items.Add(txtProcessName.Text);
                txtProcessName.Clear();
            }
            else
            {
                MessageBox.Show("Informe o nome do processo.");
            }
        }
    }
}
