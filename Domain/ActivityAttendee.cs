namespace Domain
{
    public class ActivityAttendee
    {
        // Relation to AppUser
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        // Relation to Activities
        public Guid ActivityId { get; set; }
        public Activity Activity { get; set; }

        // Specifications
        public bool IsHost { get; set; }
    }
}
