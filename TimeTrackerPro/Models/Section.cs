namespace TimeTrackerPro.Models
{
    public class Section
    {
        public int Id { get; set; }

        public int ProjectId {  get; set; }
        public int? ParentSectionId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public double EstimatedHours {  get; set; }
        public int Order {  get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public SectionStatus Status { get; set; } = SectionStatus.Pending;

        public List<Section> SubSections { get; set; } = new();
        public List<WorkSession> WorkSessions {  get; set; } = new();

        public double TotalWorkedHours =>
            WorkSessions.Sum(ws => ws.DurationHours);

        public double CompletionPercentage =>
            EstimatedHours > 0
                ? Math.Min(100, (TotalWorkedHours / EstimatedHours) * 100)
                : 0;

        public override string ToString() => Name;
    }

    public enum SectionStatus
    {
        Pending,
        InProgress,
        Completed,
        Blocked
    }
}
