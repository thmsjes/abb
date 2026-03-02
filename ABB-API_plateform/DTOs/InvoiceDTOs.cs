namespace Abb.DTOs
{
    public class InvoiceDTOs
    {
        public class CreateInvoiceRequestDTO
        {
            public string? InvoiceNumber { get; set; }
            public string? InvoiceDescription { get; set; }
            public int PropertyId { get; set; }
            public int StaffId { get; set; }
            public DateTime? DateCreated { get; set; }
            public DateTime? DateCompleted { get; set; }
            public decimal Amount { get; set; }
            public string? RepairDescription { get; set; }
            public string? CreatedBy { get; set; }
            public bool Paid { get; set; } = false;
            public DateTime? DatePaid { get; set; }
            public string? Type { get; set; }
            public DateTime? DateToBeAddressed { get; set; }
            public bool Pending { get; set; } = true;
            public bool Completed { get; set; } = false;
        }

        public class UpdateInvoiceRequestDTO
        {
            public int Id { get; set; }
            public string? InvoiceNumber { get; set; }
            public string? InvoiceDescription { get; set; }
            public int PropertyId { get; set; }
            public int StaffId { get; set; }
            public DateTime? DateCreated { get; set; }
            public DateTime? DateCompleted { get; set; }
            public decimal Amount { get; set; }
            public string? RepairDescription { get; set; }
            public string? CreatedBy { get; set; }
            public bool Paid { get; set; }
            public DateTime? DatePaid { get; set; }
            public string? Type { get; set; }
            public DateTime? DateToBeAddressed { get; set; }
            public bool Pending { get; set; }
            public bool Completed { get; set; }
        }

        public class InvoiceDetail
        {
            public int Id { get; set; }
            public string? InvoiceNumber { get; set; }
            public string? InvoiceDescription { get; set; }
            public int PropertyId { get; set; }
            public int StaffId { get; set; }
            public DateTime? DateCreated { get; set; }
            public DateTime? DateCompleted { get; set; }
            public decimal Amount { get; set; }
            public string? RepairDescription { get; set; }
            public string? CreatedBy { get; set; }
            public bool Paid { get; set; }
            public DateTime? DatePaid { get; set; }
            public string? Type { get; set; }
            public DateTime? DateToBeAddressed { get; set; }
            public bool Pending { get; set; }
            public bool Completed { get; set; }
        }

        public class InvoiceResponseDTO
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; } = string.Empty;
            public List<InvoiceDetail> Invoices { get; set; } = new List<InvoiceDetail>();
        }

        public class TransactionResponseDTO
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; } = string.Empty;
        }
    }
}