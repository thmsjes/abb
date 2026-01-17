namespace Abb.DTOs
{
    public class AuthDtos
    {
        public class RegisterRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public bool Admin { get; set; } = false;
            public bool Cleaner { get; set; } = false;
            public bool Maintenance { get; set; } = false;
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Email { get; set; }
            public string? PhoneNumber { get; set; }
            public string? ConfirmationNumber { get; set; }
            public string Company { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Zip { get; set; }
            public string Notes { get; set; }
        }
        
        public class LoginRequestDTO
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class LoginResponseDTO 
        { 
            public string Token { get; set; }
            public string IsSuccess { get; set; }   
            public string Message { get; set; }
        }
    }
}