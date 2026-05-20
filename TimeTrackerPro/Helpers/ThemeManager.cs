using System.IO;
using System.Text.Json;
using System.Windows;

namespace TimeTrackerPro.Helpers
{
    /// <summary>
    /// Gestiona el tema claro/oscuro de la aplicación.
    /// Persiste la preferencia en %AppData%\TimeTrackerPro\theme.json.
    /// </summary>
    public sealed class ThemeManager
    {
        private static readonly Lazy<ThemeManager> _instance = new(() => new ThemeManager());
        public static ThemeManager Instance => _instance.Value;

        private const string AppFolder = "TimeTrackerPro";
        private const string FileName  = "theme.json";

        private readonly Uri _darkUri =
            new("pack://application:,,,/Assets/ThemeDark.xaml", UriKind.Absolute);

        public bool IsDark { get; private set; }

        // El icono que muestra el botón: sol si estamos en oscuro, luna si en claro.
        public string ToggleIcon => IsDark ? "☀" : "🌙";

        private string PreferencePath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                AppFolder, FileName);

        private ThemeManager() { }

        /// <summary>Lee la preferencia guardada y aplica el tema al iniciar.</summary>
        public void LoadAndApply()
        {
            IsDark = false;
            try
            {
                if (File.Exists(PreferencePath))
                {
                    var json = File.ReadAllText(PreferencePath);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("mode", out var prop))
                        IsDark = prop.GetString() == "dark";
                }
            }
            catch { /* en caso de error preferimos modo claro */ }

            ApplyTheme();
        }

        /// <summary>Alterna entre claro y oscuro, guarda la preferencia y redibuja.</summary>
        public void Toggle()
        {
            IsDark = !IsDark;
            Save();
            ApplyTheme();
        }

        private void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(PreferencePath)!;
                Directory.CreateDirectory(dir);
                var mode = IsDark ? "dark" : "light";
                File.WriteAllText(PreferencePath, $"{{\"mode\":\"{mode}\"}}");
            }
            catch { }
        }

        private void ApplyTheme()
        {
            var dicts = Application.Current.Resources.MergedDictionaries;

            // Elimina cualquier overlay oscuro anterior.
            var existing = dicts.FirstOrDefault(d => d.Source == _darkUri);
            if (existing != null)
                dicts.Remove(existing);

            // En modo oscuro añadimos ThemeDark.xaml al final; como WPF
            // resuelve DynamicResource buscando del último al primero,
            // sus valores tienen precedencia sobre Theme.xaml.
            if (IsDark)
                dicts.Add(new ResourceDictionary { Source = _darkUri });
        }
    }
}
