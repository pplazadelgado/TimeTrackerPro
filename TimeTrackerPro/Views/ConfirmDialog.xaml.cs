using System.Windows;

namespace TimeTrackerPro.Views
{
    /// <summary>
    /// Diálogo de confirmación reutilizable.
    /// Devuelve true si el usuario confirma, false si cancela.
    ///
    /// USO desde un ViewModel:
    ///   var ok = ConfirmDialog.Show(
    ///       owner: App.Current.MainWindow,
    ///       title: "Eliminar proyecto",
    ///       message: "¿Seguro que quieres eliminar...?",
    ///       confirmText: "Sí, eliminar");
    /// </summary>
    public partial class ConfirmDialog : Window
    {
        // Guardamos el resultado: true = confirmo, false = cancelo
        private bool _result;

        private ConfirmDialog(Window owner, string title, string message, string confirmText)
        {
            InitializeComponent();

            Owner = owner;

            //Rellenamos los textos desde el codigo
            TitleText.Text = title;
            MessageText.Text = message;
            ConfirmButton.Content = confirmText;
        }

        /// <summary>
        /// Método estático para mostrar el diálogo de forma simple.
        /// Devuelve true si el usuario pulsa el botón de confirmación.
        /// </summary>
        public static bool Show(Window owner, string title, string message, string confirmText = "Confirmar")
        {
            var dialog = new ConfirmDialog(owner, title, message, confirmText);
            dialog.ShowDialog();
            return dialog._result;
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            _result = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            _result = false;
            Close();
        }
    }
}
