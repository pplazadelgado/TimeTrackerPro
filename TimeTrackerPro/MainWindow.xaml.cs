using System.Windows;
using TimeTrackerPro.Helpers;

namespace TimeTrackerPro
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ThemeManager.Instance.LoadAndApply();
            ThemeToggleButton.Content = ThemeManager.Instance.ToggleIcon;
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Instance.Toggle();
            ThemeToggleButton.Content = ThemeManager.Instance.ToggleIcon;
        }
    }
}
