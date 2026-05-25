using TimeTrackerPro.Helpers;

namespace TimeTrackerPro.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double WeeklyHours { get; set; } = 8;
        public DateTime CreatedAt { get; set; }= DateTime.Now;
        public ProjectStatus Status { get; set; } = ProjectStatus.Active;
        public DateTime? DeadlineDate { get; set; }

        public List<Section> Sections { get; set; } = new();
        public List<Expense> Expenses { get; set; } = new();

        public double TotalWorkedHours =>
            Sections.Sum(s => s.TotalWorkedHours);

        public double TotalEstimatedHours =>
            Sections.Sum(s => s.EstimatedHours);

        public double TotalExpenses =>
            Expenses.Sum(e => e.Amount);

        public override string ToString() => Name;

        /// <summary>
        /// Calcula el riesgo de deadline basándose en las horas restantes
        /// y las horas disponibles por semana.
        /// </summary>
        public DeviationLevel DeadlineRisk
        {
            get
            {
                if (DeadlineDate == null) return DeviationLevel.OnTrack;

                var daysLeft = (DeadlineDate.Value - DateTime.Now).TotalDays;
                var remaining = TotalEstimatedHours - TotalWorkedHours;
                var daysNeeded = WeeklyHours > 0
                    ? (remaining / WeeklyHours) * 7
                    : double.MaxValue;

                var deviation = daysNeeded - daysLeft;

                if (deviation <= 0) return DeviationLevel.OnTrack;
                if (deviation <= 7) return DeviationLevel.SlightDelay;
                return DeviationLevel.Delayed;
            }
        }

    }



    public enum ProjectStatus
    {
        Active,
        Paused,
        Completed,
        Archived
    }
}
