namespace Abb.DTOs
{
    public class ReservationDTOs
    {
        public class CreateReservationRequestDTO
        {
            public string ConfirmationNumber { get; set; }
            public int CustomerId { get; set; }
            public int PropertyId { get; set; }
            public int? StaffId { get; set; }
            public DateOnly CheckInDate { get; set; }
            public DateOnly CheckoutDate{ get; set; }
            public string? LockCode{ get; set; }
            public DateTime? CleaningDateTime { get; set; }
        }
        public class GetReservationResponseDTO
        {
            public int Id { get; set; }
            public string ConfirmationNumber { get; set; }
            public int CustomerId { get; set; }
            public int PropertyId { get; set; }
            public int StaffId { get; set; }
            public DateOnly CheckInDate { get; set; }
            public DateOnly CheckoutDate{ get; set; }
            public string LockCode{ get; set; }
            public DateTime CleaningDateTime { get; set; }
        }


    }
}
