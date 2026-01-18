namespace Abb.DTOs
{
    public class UserDTOs
    {
            public class UserRequestDTO
            {
                public string? Username { get; set; }
                public string? Access { get; set; }
                public string? ConfirmationNumber { get; set; }
                public string? Email { get; set; } = null;
                public string? PhoneNumber { get; set; } = null;
                public string? Company { get; set; } = null;
                public int? PropertyId { get; set; } = null;
        }


        public class UserResponseDTO
            {
                public bool IsSuccess { get; set; }
                public string Message { get; set; }
                public UserDetail User { get; set; }
            }
        public class UserDetail
            {
                public int Id { get; set; }
                public string Username { get; set; }
                public string Password { get; set; }
                public string Access { get; set; }
                public string? FirstName { get; set; }
                public string? LastName { get; set; }
                public string? Email { get; set; }
                public string? PhoneNumber { get; set; }
                public string? ConfirmationNumber { get; set; }
                public string? Company { get; set; }
                public string? Address { get; set; }
                public string? City { get; set; }
                public string? State { get; set; }
                public string? Zip { get; set; }
                public string? Notes { get; set; }
                public int PropertyId { get; set; }
        }
        
    }
}
