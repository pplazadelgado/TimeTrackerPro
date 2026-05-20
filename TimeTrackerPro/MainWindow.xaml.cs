using System.Windows;
using TimeTrackerPro.Helpers;

namespace TimeTrackerPro
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ThemeToggleBtn.Content = ThemeManager.Instance.ToggleIcon;
        }

        private void OnThemeToggle(object sender, RoutedEventArgs e)
        {
            ThemeManager.Instance.Toggle();
            ThemeToggleBtn.Content = ThemeManager.Instance.ToggleIcon;
        }
    }
}
