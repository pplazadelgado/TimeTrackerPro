using System.Windows;

namespace TimeTrackerPro.Views
{
    /// <summary>
    /// Lógica de interacción para ManualSessionDialog.xaml
    /// </summary>
    public partial class ManualSessionDialog : Window
    {
        // Propiedades que el ViewModel leera tras cerrar el dialog
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public string Notes {  get; private set; } = string.Empty;

        public ManualSessionDialog(string sectionName)
        {
            InitializeComponent();
            TitleText.Text = $"Añadir sesion manual - {sectionName}";
            DatePicker.SelectedDate = DateTime.Today;
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            // Validamos los campos antes de cerrar
            if (DatePicker.SelectedDate == null)
            {
                ShowError("Selecciona una fecha.");
                return;
            }

            if (!TimeSpan.TryParse(StartTimeBox.Text, out var startTs))
            {
                ShowError("Hora de inicio no válida. Usa el formato HH:mm");
                return;
            }

            if (!TimeSpan.TryParse(EndTimeBox.Text, out var endTs))
            {
                ShowError("Hora de fin no válida. Usa el formato HH:mm");
                return;
            }

            var date = DatePicker.SelectedDate.Value.Date;
            StartTime = date + startTs;
            EndTime = date + endTs;

            if (EndTime <= StartTime)
            {
                ShowError("La hora de fin debe ser posterior a la de inicio.");
                return;
            }

            Notes = NotesBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
