namespace Abb.DTOs
{
    public class EventDTOs
    {
        public class CreateEventRequestDTO
        {
            public string Event { get; set; } = string.Empty;  // Description/Event name
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int PropertyId { get; set; }
            public int UserId { get; set; }
            public bool Completed { get; set; } = false;
            public DateTime? DateTimeInserted { get; set; }  // Optional, defaults to now if not provided
        }

        public class UpdateEventRequestDTO
        {
            public int Id { get; set; }  // Event ID
            public string Event { get; set; } = string.Empty;  // Description
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int PropertyId { get; set; }
            public int UserId { get; set; }
            public bool Completed { get; set; }
            public DateTime? DateTimeInserted { get; set; }
        }

        public class EventDetail
        {
            public int Id { get; set; }
            public string Event { get; set; } = string.Empty;
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int PropertyId { get; set; }
            public DateTime DateTimeInserted { get; set; }
            public bool Completed { get; set; }
            public int UserId { get; set; }
        }

        public class EventResponseDTO
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; } = string.Empty;
            public List<EventDetail> Events { get; set; } = new List<EventDetail>();
        }

        public class TransactionResponseDTO
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; } = string.Empty;
        }
    }
}