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

        public List<Section> Sections { get; set; } = new();
        public List<Expense> Expenses { get; set; } = new();

        public double TotalWorkedHours =>
            Sections.Sum(s => s.TotalWorkedHours);

        public double TotalEstimatedHours =>
            Sections.Sum(s => s.EstimatedHours);

        public double TotalExpenses =>
            Expenses.Sum(e => e.Amount);

        public override string ToString() => Name;
        
    }

    public enum ProjectStatus
    {
        Active,
        Paused,
        Completed,
        Archived
    }
}
