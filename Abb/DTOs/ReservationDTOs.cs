using System.Diagnostics.Contracts;

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
            public int GuestCount { get; set; }
            public bool Dogs { get; set; }
        }
        public class GetReservationResponseDTO
        {
            public int Id { get; set; }
            public string? ConfirmationNumber { get; set; }
            public int? CustomerId { get; set; }
            public int PropertyId { get; set; }
            public int StaffId { get; set; }
            public DateOnly CheckInDate { get; set; }
            public DateOnly CheckoutDate{ get; set; }
            public string? LockCode{ get; set; }
            public DateTime? CleaningDateTime { get; set; }
            public int GuestCount { get; set; }
            public bool Dogs { get; set; }
            
            // Computed properties for cleaner portal
            public bool IsCleaningScheduled => CleaningDateTime.HasValue;
            public bool IsCleaningPast => CleaningDateTime.HasValue && CleaningDateTime.Value < DateTime.Now;
            public bool IsCleaningUpcoming => CleaningDateTime.HasValue && CleaningDateTime.Value >= DateTime.Now;
            public string CleaningStatus
            {
                get
                {
                    if (!CleaningDateTime.HasValue)
                        return "Not Scheduled";
                    if (CleaningDateTime.Value < DateTime.Now)
                        return "Completed";
                    if (CleaningDateTime.Value.Date == DateTime.Today)
                        return "Today";
                    if (CleaningDateTime.Value.Date == DateTime.Today.AddDays(1))
                        return "Tomorrow";
                    return "Scheduled";
                }
            }
        }

        public class ReservationForGuestPortalResponseDTO
        {
            public string? ConfirmationNumber { get; set; }
            public DateOnly CheckInDate { get; set; }
            public DateOnly CheckoutDate { get; set; }
            public string? LockCode { get; set; }
            public string GuestCount { get; set; }
            public string Dogs { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? PropertyName { get; set; }
            public string? Address { get; set; }
            public string? City { get; set; }
            public string? State { get; set; }
            public string? Zip { get; set; }



        }
    }
}
