namespace Abb.DTOs
{
    public class CalendarDTOS
    {

        public class  GetCalendarEventsResponseDTO
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
            public List<EventDetail> Events { get; set; } = new List<EventDetail>();

        }
        public class EventDetail
        {
            public int Id { get; set; }
            public string Event { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int PropertyId { get; set; }
            public DateTime? DateTimeInserted { get; set; }
            public bool Completed { get; set; } = false;
            public int UserId { get; set; }
        }
    }
}
