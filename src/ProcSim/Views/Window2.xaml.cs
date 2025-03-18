using System.Windows;

namespace ProcSim.Mockups
{
    public partial class OptionBWindow : Window
    {
        public OptionBWindow()
        {
            InitializeComponent();
        }

        private void FabAddProcess_Click(object sender, RoutedEventArgs e)
        {
            ModalOverlay.Visibility = Visibility.Visible;
        }

        private void ModalCancel_Click(object sender, RoutedEventArgs e)
        {
            ModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void ModalSave_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(modalProcessName.Text))
            {
                lbProcessList.Items.Add(modalProcessName.Text);
                modalProcessName.Clear();
                ModalOverlay.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Informe o nome do processo.");
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Play clicked");
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Pause clicked");
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Reset clicked");
        }

        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Salvar Configuração");
        }

        private void LoadConfig_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Carregar Configuração");
        }

        private void ToggleDarkMode_Click(object sender, RoutedEventArgs e)
        {
            if (tbDarkMode.IsChecked == true)
                MessageBox.Show("Dark Mode ativado");
            else
                MessageBox.Show("Dark Mode desativado");
        }

        private void CadastrarProcesso_Click(object sender, RoutedEventArgs e)
        {
            // Método alternativo, também pode abrir o modal
            ModalOverlay.Visibility = Visibility.Visible;
        }
    }
}
