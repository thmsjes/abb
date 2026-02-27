using ABB_API_plateform.Business;
using BCrypt.Net;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Data;
using static Abb.DTOs.AuthDtos;

namespace Abb.Business
{
    // Define the interface OUTSIDE the class
    public interface IAuthentication
    {
        Task<int> RegisterUser(RegisterRequest request);
        Task<LoginResponseDTO> LoginUser(LoginRequestDTO request);
    }

    // Implementation class
    public class AuthenticationService : IAuthentication
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly ILogging _logging;

        public AuthenticationService(IConfiguration configuration, ILogging logging)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _configuration = configuration;
            _logging = logging;
        }
        public async Task<int> RegisterUser(RegisterRequest request)
        {
            // ⭐ Generate username if not provided
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                request.Username = $"USER_{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
            }

            // ⭐ Password is still required
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                request.Password = Guid.NewGuid().ToString("N").Substring(0, 12); // Generate a random password
            }

            // 1. Prepare Password Hash (only if password exists)
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 2. Prepare Username (only if username exists)
            string formattedUsername = !string.IsNullOrWhiteSpace(request?.Username)
                ? request.Username.Replace(" ", "").ToUpper()
                : null;

            string sql = "INSERT INTO Users (Username, HashedPassword, Access, LastLogin, Email, PhoneNumber, " +
                         "ConfirmationNumber, Company, Address, City, State, Zip, Notes, FirstName, LastName) " +
                         "OUTPUT INSERTED.Id " +
                         "VALUES (@username, @hash, @access, @lastLogin, @Email, @PhoneNumber, @ConfirmationNumber, " +
                         "@Company, @Address, @City, @State, @Zip, @Notes, @FirstName, @LastName)";

            int access = request.Admin ? 1 : request.Cleaner ? 2 : request.Maintenance ? 3 : 4;
            DateTime lastLogin = DateTime.Now;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        // Use (object)Value ?? DBNull.Value for all optional fields
                        cmd.Parameters.AddWithValue("@username", (object)formattedUsername ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@hash", (object)passwordHash ?? DBNull.Value);

                        cmd.Parameters.AddWithValue("@access", access);
                        cmd.Parameters.AddWithValue("@lastLogin", lastLogin);

                        // Handling other potentially null fields
                        cmd.Parameters.AddWithValue("@Email", (object)request.Email ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PhoneNumber", (object)request.PhoneNumber ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ConfirmationNumber", (object)request.ConfirmationNumber ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Company", (object)request.Company ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Address", (object)request.Address ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@City", (object)request.City ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@State", (object)request.State ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Zip", (object)request.Zip ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Notes", (object)request.Notes ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@FirstName", (object)request.FirstName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@LastName", (object)request.LastName ?? DBNull.Value);

                        await conn.OpenAsync();

                        // ExecuteScalar returns the ID from OUTPUT INSERTED.Id
                        object result = await cmd.ExecuteScalarAsync();

                        return result != null ? Convert.ToInt32(result) : -1;
                    }
                }
            }
            catch (SqlException ex)
            {
                _logging.LogToFile($"SQL Error during registration: {ex.Message}");
                return -1;
            }
        }

        public async Task<LoginResponseDTO> LoginUser(LoginRequestDTO request)
        {
            // 1. Clean the input username
            string cleanUsername = request.Username.Replace(" ", "").ToUpper();

            string storedHash = null;
            string accessLevel = null;
            string id = null;

            // 2. Fetch the User from DB
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT HashedPassword, Access, Id FROM Users WHERE Username = @username";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", cleanUsername);
                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            storedHash = reader["HashedPassword"].ToString();
                            accessLevel = reader["Access"].ToString();
                            id = reader["Id"].ToString();
                        }
                    }
                }
            }

            // 3. Verify Password & Generate Token
            if (storedHash != null && BCrypt.Net.BCrypt.Verify(request.Password, storedHash))
            {
                string token = GenerateJwtToken(cleanUsername, accessLevel, id);

                return new LoginResponseDTO
                {
                    Token = token,
                    IsSuccess = "true"
                };
            }

            return new LoginResponseDTO { IsSuccess = "false" };
        }

        private string GenerateJwtToken(string username, string access, string id)
        {
            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, username),
        new System.Security.Claims.Claim("AccessLevel", access), // Custom claim from DB
        new System.Security.Claims.Claim("OwnerId", id), // Custom claim from DB
        new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: credentials);

            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}