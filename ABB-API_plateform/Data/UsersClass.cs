using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using Dapper;
using static Abb.DTOs.PropertyDTOs;
using static Abb.DTOs.UserDTOs;

namespace Abb.Data
{
    public interface IUsersClass
    {
        Task<List<UserDetail>> GetAllUsers();
        Task<UserResponseDTO> GetUser(int id);
        Task<UserResponseDTO> UpdateUser(UserDetail request);
        Task<UserResponseDTO> DeleteUser(int id);
        Task<PropertyOwner> GetPropertyOwner(int id);
    }   
    public class UsersClass: IUsersClass
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        public UsersClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _configuration = configuration;
        }

        public async Task<List<UserDetail>> GetAllUsers()
        {
            const string sql = @"
        SELECT [Id]
              ,[UserName] AS Username
              ,[HashedPassword] AS Password
              ,[Access]
              ,[FirstName]
              ,[LastName]
              ,[Email]
              ,[PhoneNumber]
              ,[ConfirmationNumber]
              ,[Company]
              ,[Address]
              ,[City]
              ,[State]
              ,[Zip]
              ,[Notes]
        FROM [ABB].[dbo].[Users]";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var users = await db.QueryAsync<UserDetail>(sql);
                return users.ToList();
            }
        }
        public async Task<UserResponseDTO> GetUser(int id)
        {
            const string sql = @"
        SELECT [Id], [UserName] as Username, [HashedPassword] as Password, [Access], 
               [FirstName], [LastName], [Email], [PhoneNumber], [ConfirmationNumber], 
               [Company], [Address], [City], [State], [Zip], [Notes]
        FROM [ABB].[dbo].[Users] 
        WHERE [Id] = @Id";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var userDetail = await db.QuerySingleOrDefaultAsync<UserDetail>(sql, new { Id = id });

                if (userDetail == null)
                    return new UserResponseDTO { IsSuccess = false, Message = "User not found" };

                return new UserResponseDTO { IsSuccess = true, Message = "Success", User = userDetail };
            }
        }

        public async Task<UserResponseDTO> UpdateUser(UserDetail request)
        {
            // Added [HashedPassword] = @Password to the SET clause
            const string sql = @"
        UPDATE [ABB].[dbo].[Users]
        SET [UserName] = @Username, 
            [HashedPassword] = @Password, 
            [Access] = @Access, 
            [FirstName] = @FirstName, 
            [LastName] = @LastName, 
            [Email] = @Email, 
            [PhoneNumber] = @PhoneNumber, 
            [Company] = @Company, 
            [Address] = @Address, 
            [City] = @City, 
            [State] = @State, 
            [Zip] = @Zip, 
            [Notes] = @Notes
        WHERE [Id] = @Id";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    // Dapper matches @Password to request.Password and @Username to request.Username
                    int rows = await db.ExecuteAsync(sql, request);

                    return new UserResponseDTO
                    {
                        IsSuccess = rows > 0,
                        Message = rows > 0 ? "User updated successfully." : "User not found; no rows updated.",
                        User = request
                    };
                }
                catch (Exception ex)
                {
                    return new UserResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Database error: {ex.Message}",
                        User = request
                    };
                }
            }
        }

        public async Task<UserResponseDTO> DeleteUser(int id)
        {
            const string sql = "DELETE FROM [ABB].[dbo].[Users] WHERE [Id] = @Id";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                int rows = await db.ExecuteAsync(sql, new { Id = id });

                return new UserResponseDTO
                {
                    IsSuccess = rows > 0,
                    Message = rows > 0 ? "User deleted" : "User not found or delete failed"
                };
            }
        }
        public async Task<PropertyOwner?> GetPropertyOwner(int ownerId)
        {
            // Selecting only the columns that exist in the DTO
            const string sql = @"
                        SELECT [Id]
                              ,[FirstName]
                              ,[LastName]
                              ,[Email]
                              ,[PhoneNumber]
                              ,[Address]
                              ,[City]
                              ,[State]
                              ,[Zip]
                              ,[Notes]
                        FROM [ABB].[dbo].[Users]
                        WHERE [Id] = @Id";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    // Dapper maps the columns directly to the PropertyOwner properties
                    return await db.QuerySingleOrDefaultAsync<PropertyOwner>(sql, new { Id = ownerId });
                }
                catch (Exception ex)
                {
                    // Log exception here as needed
                    throw new Exception($"Error retrieving owner info: {ex.Message}");
                }
            }
        }

    }
}
