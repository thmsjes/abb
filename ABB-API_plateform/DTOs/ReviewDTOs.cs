namespace Abb.DTOs
{
    public class ReviewDTOs
    {
        public class CreateReviewRequestDTO
        {
            public string ReviewerName { get; set; } = string.Empty;
            public string ReviewText { get; set; } = string.Empty;
            public int PropertyId { get; set; }
            public int Score { get; set; }
            public DateTime? ReviewDate { get; set; }  // Optional, defaults to now if not provided
        }

        public class UpdateReviewRequestDTO
        {
            public int Id { get; set; }  // Review ID in body
            public string ReviewerName { get; set; } = string.Empty;
            public string ReviewText { get; set; } = string.Empty;
            public int PropertyId { get; set; }
            public int Score { get; set; }
            public DateTime? ReviewDate { get; set; }
        }

        public class ReviewResponseDTO
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; } = string.Empty;
            public List<ReviewDetail> Reviews { get; set; } = new List<ReviewDetail>();
        }

        public class ReviewDetail
        {
            public int Id { get; set; }
            public DateTime ReviewDate { get; set; }
            public string ReviewerName { get; set; } = string.Empty;
            public string ReviewText { get; set; } = string.Empty;
            public int PropertyId { get; set; }
            public int Score { get; set; }
        }

        public class TransactionResponseDTO
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; } = string.Empty;
        }
    }
}