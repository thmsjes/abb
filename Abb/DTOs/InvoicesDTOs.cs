namespace Abb.DTOs
{
    public class InvoiceRequestDTO
    {
        public int? Id { get; set; }
        public string? InvoiceNumber { get; set; }
        public string InvoiceDescription { get; set; }
        public int PropertyId { get; set; }
        public int StaffId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateCompleted { get; set; }
        public string? Amount { get; set; }
        public string? RepairDescription { get; set; }
        public int CreatedBy { get; set; }
        public bool? Paid { get; set; }
        public DateTime? DatePaid { get; set; }
        public string? Type { get; set; }
        public DateTime? DateToBeAddressed { get; set; }
        public bool? Pending { get; set; }
        public bool? Completed { get; set; }
    }
}
