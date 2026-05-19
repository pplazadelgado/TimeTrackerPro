using System.Windows;

namespace TimeTrackerPro.Views
{
    public partial class SplashScreenWindow : Window
    {
        public SplashScreenWindow()
        {
            InitializeComponent();
        }

        public void SetStatus(string message) =>
            Dispatcher.Invoke(() => StatusText.Text = message);
    }
}
