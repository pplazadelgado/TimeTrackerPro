namespace TimeTrackerPro.Models
{
    /// <summary>
    /// Nivel de desviación respecto a la fecha de entrega.
    /// Compartido entre modelos y ViewModels.
    /// </summary>
    public enum DeviationLevel
    {
        OnTrack,      // Verde: en plazo
        SlightDelay,  // Amarillo: riesgo leve
        Delayed       // Rojo: fuera de plazo
    }
}
