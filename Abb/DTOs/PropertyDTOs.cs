namespace Abb.DTOs
{
    public class PropertyDTOs
    {
        public class PropertyCreateRequestDTO
        {
            public int? PropertyId { get; set; } = null;
            public string? PropertyName { get; set; } = null;
            public string? Address { get; set; } = null;
            public string? City { get; set; } = null;
            public string? State { get; set; } = null;
            public string? Zip { get; set; } = null;
            public int ?OwnerId { get; set; }

        }
        public class PropertyResponseDTO
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
            public List<PropertyDetail> Property { get; set; }
            public PropertyOwner Owner { get; set; }
        }
        public class PropertyDetail
        {
            public int? PropertyId { get; set; } = null;
            public string? PropertyName { get; set; } = null;
            public string? Address { get; set; } = null;
            public string? City { get; set; } = null;
            public string? State { get; set; } = null;
            public string? Zip { get; set; } = null;
            public int? OwnerId { get; set; }
        }

        public class PropertyOwner
        {
            public int Id { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Email { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Address { get; set; }
            public string? City { get; set; }
            public string? State { get; set; }
            public string? Zip { get; set; }
            public string? Notes { get; set; }
        }

        public class ReviewDetail
            {
                public int Id { get; set; }
                public DateTime ReviewDate { get; set; }
                public string ReviewerName { get; set; }
                public string ReviewText { get; set; }
                public int PropertyId { get; set; }
                public string Score { get; set; }
        }

        public class CreateReviewRequestDTO
            {
                public string ReviewerName { get; set; }
                public string ReviewText { get; set; }
                public int PropertyId { get; set; }
                public string Score { get; set; }
                public DateTime ReviewDate { get; set; }
        }

        public class UpdateReviewRequestDTO
            {
                public int Id { get; set; }
                public string ReviewerName { get; set; }
                public string ReviewText { get; set; }
                public string Score { get; set; }
                public DateTime ReviewDate { get; set; }
        }

        public class ReviewResponseDTO
            {
                public bool IsSuccess { get; set; }
                public string Message { get; set; }
                public List<ReviewDetail> Reviews { get; set; } = new List<ReviewDetail>();
            }
    }
}


