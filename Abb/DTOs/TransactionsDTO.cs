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
        }




    }
}
