namespace Abb.DTOs
{
    public class TransactionsDTO
    {
        public class ExpenseRequestDTO
        {
            public string Description { get; set; }
            public decimal Amount { get; set; }
            public DateOnly Date { get; set; }
            public string Category { get; set; }
            public int PropertyId { get; set; }
            public int Expense { get; set; }
            public int Payment { get; set; }
            public string Vendor { get; set; }
            public string PaymentType { get; set; }
        }

        public class TransactionResponseDTO
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
        }

        public class GetExpenseByAttributesRequestDTO
        {
            public DateOnly? StartDate { get; set; }
            public DateOnly? EndDate { get; set; }
            public string? Category { get; set; }
            public int PropertyId { get; set; }
        }

        public class GetExpenseResponseDTO 
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
            public List<ExpenseDetail> Expenses { get; set; }
        }

        public class ExpenseDetail
        {
            public string Description { get; set; }
            public decimal Amount { get; set; }
            public DateOnly Date { get; set; }
            public string Category { get; set; }
            public int Id { get; set; }
            public int PropertyId { get; set; }
            public int Payment { get; set; }
            public int Expense { get; set; }
            public string Vendor { get; set; }
            public string PaymentType { get; set; }

        }

        // ⭐ NEW: Mileage DTOs
        public class CreateMileageRequestDTO
        {
            public decimal Mileage { get; set; }
            public string? Description { get; set; }
            public DateTime? DateTimeInserted { get; set; }
            public int PropertyId { get; set; }
            public DateOnly Date { get; set; }
        }

        public class MileageDetail
        {
            public int Id { get; set; }
            public decimal Mileage { get; set; }
            public string? Description { get; set; }
            public DateTime DateTimeInserted { get; set; }
            public int PropertyId { get; set; }
            public DateOnly Date { get; set; }
        }

        public class GetMileageResponseDTO
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
            public List<MileageDetail> Mileages { get; set; } = new List<MileageDetail>();
        }
    }
}
