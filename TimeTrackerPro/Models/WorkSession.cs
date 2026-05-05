namespace TimeTrackerPro.Models
{
    public class WorkSession
    {
        public int Id { get; set; }

        public int SectionId {  get; set; }

        public DateTime StartTime {  get; set; }
        public DateTime? EndTime { get; set; }

        public string Notes { get; set; } = string.Empty;

        public bool IsManual {  get; set; }

        public double DurationHours
        {
            get
            {
                var end = EndTime ?? DateTime.Now;
                return (end - StartTime).TotalHours;
            }
        }

        public string DurationFormatted
        {
            get
            {
                var duration = EndTime.HasValue
                    ? EndTime.Value - StartTime
                    :DateTime.Now - StartTime;

                if (duration.TotalHours >= 1)
                    return $"{(int)duration.TotalHours}h {duration.Minutes}m";
                else
                    return $"{duration.Minutes}m {duration.Seconds}s";
            }
        }

        public bool IsActive => !EndTime.HasValue;
    }
}
