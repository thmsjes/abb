using static Abb.DTOs.AuthDtos;
using Microsoft.Data.SqlClient;
using BCrypt.Net;
using System.ComponentModel;
using System.Data;

namespace Abb.Business
{
    // Define the interface OUTSIDE the class
    public interface IAuthentication
    {
        Task<bool> RegisterUser(RegisterRequest request);
        Task<LoginResponseDTO> LoginUser(LoginRequestDTO request);
    }

    // Implementation class
    public class AuthenticationService : IAuthentication
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public AuthenticationService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _configuration = configuration;
        }
        public async Task<bool> RegisterUser(RegisterRequest request)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            string sql = "INSERT INTO Users (Username, HashedPassword, Access, LastLogin, Email, PhoneNumber" +
                ",ConfirmationNumber, Company, Address, City, State, Zip, Notes, PropertyId, FirstName, LastName ) " +
                         "VALUES (@username, @hash, @access,@lastLogin, @Email, @PhoneNumber, @ConfirmationNumber," +
                         "@Company, @Address, @City, @State, @Zip, @Notes, @PropertyId, @FirstName, @LastName)";
            int access = request.Admin ? 1 : request.Cleaner ? 2 : request.Maintenance ? 3 : 4;
            DateTime lastLogin =  DateTime.Now;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", request.Username.Replace(" ", "").ToUpper());
                        cmd.Parameters.AddWithValue("@hash", passwordHash);
                        cmd.Parameters.AddWithValue("@access", access);
                        cmd.Parameters.AddWithValue("@lastLogin", lastLogin);
                        cmd.Parameters.AddWithValue("@Email", request.Email);
                        cmd.Parameters.AddWithValue("@PhoneNumber", request.PhoneNumber);
                        cmd.Parameters.AddWithValue("@ConfirmationNumber", request.ConfirmationNumber);
                        cmd.Parameters.AddWithValue("@Company", request.Company);
                        cmd.Parameters.AddWithValue("@Address", request.Address);
                        cmd.Parameters.AddWithValue("@City", request.City);
                        cmd.Parameters.AddWithValue("@State", request.State);
                        cmd.Parameters.AddWithValue("@Zip", request.Zip);
                        cmd.Parameters.AddWithValue("@Notes", request.Notes);
                        cmd.Parameters.AddWithValue("@PropertyId", request.PropertyId);
                        cmd.Parameters.AddWithValue("@FirstName", request.FirstName);
                        cmd.Parameters.AddWithValue("@LastName", request.LastName);
                        await conn.OpenAsync();
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                return false;
            }
        }

        public async Task<LoginResponseDTO> LoginUser(LoginRequestDTO request)
        {
            // 1. Clean the input username
            string cleanUsername = request.Username.Replace(" ", "").ToUpper();

            string storedHash = null;
            string accessLevel = null;
            int propertyId = -1;

            // 2. Fetch the User from DB
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT HashedPassword, Access, PropertyId FROM Users WHERE Username = @username";
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
                            propertyId = (int)reader["PropertyId"];
                        }
                    }
                }
            }

            // 3. Verify Password & Generate Token
            if (storedHash != null && BCrypt.Net.BCrypt.Verify(request.Password, storedHash))
            {
                string token = GenerateJwtToken(cleanUsername, accessLevel, propertyId);

                return new LoginResponseDTO
                {
                    Token = token,
                    IsSuccess = "true"
                };
            }

            return new LoginResponseDTO { IsSuccess = "false" };
        }

        private string GenerateJwtToken(string username, string access, int propertyId)
        {
            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, username),
        new System.Security.Claims.Claim("AccessLevel", access), // Custom claim from DB
        new System.Security.Claims.Claim("PropertyId", propertyId.ToString()), // Custom claim from DB
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