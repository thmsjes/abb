using Azure.Core;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using static Abb.DTOs.PropertyDTOs;
using static Abb.DTOs.UserDTOs;

namespace Abb.Data
{
    public interface IUsersClass
    {
        Task<List<UserDetail>> GetUsersByPropertyId(int propertyId);
        Task<UserResponseDTO> GetUser(int id);
        Task<UserResponseDTO> UpdateUser(UserDetail request);
        Task<UserResponseDTO> DeleteUser(int id);
        Task<PropertyOwner> GetPropertyOwner(int id);
        Task<PropertyResponseDTO> AppendOwner(PropertyResponseDTO property);

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

        public async Task<List<UserDetail>> GetUsersByPropertyId(int propertyId)
        {
            const string sql = @"
                            SELECT [Id]
                                  ,[UserName] AS Username
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
                                  ,[PropertyId]
                            FROM [ABB].[dbo].[Users]
                            WHERE [PropertyId] = @PropertyId";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    // Pass the propertyId into Dapper as an anonymous object
                    var users = await db.QueryAsync<UserDetail>(sql, new { PropertyId = propertyId });
                    
                    return users.ToList();
                }
                catch (Exception ex)
                {
                    // Logging can be added here
                    // Return an empty list to avoid null reference issues in the UI
                    return new List<UserDetail>();
                }
            }
        }
        public async Task<UserResponseDTO> GetUser(int id)
        {
            const string sql = @"
        SELECT [Id], [UserName] as Username, [HashedPassword] as Password, [Access], 
               [FirstName], [LastName], [Email], [PhoneNumber], [ConfirmationNumber], 
               [Company], [Address], [City], [State], [Zip], [Notes], [PropertyId]
        FROM [ABB].[dbo].[Users] 
        WHERE [Id] = @Id";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var userDetail = await db.QuerySingleOrDefaultAsync<UserDetail>(sql, new { Id = id });

                if (userDetail == null)
                    return new UserResponseDTO { IsSuccess = false, Message = "User not found" };
                userDetail.Password = null;
                return new UserResponseDTO { IsSuccess = true, Message = "Success", User = userDetail };
            }
        }
        public async Task<UserResponseDTO> UpdateUser(UserDetail request)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    //// 1. Fetch the CURRENT hashed password from the DB to compare
                    //const string getSql = "SELECT [HashedPassword] FROM [Users] WHERE [Id] = @Id";
                    //string existingHash = await db.QuerySingleOrDefaultAsync<string>(getSql, new { Id = request.Id });

                    //// 2. Check if the password provided in the request is different from the hash
                    //// If BCrypt.Verify returns false, the user provided a NEW plain-text password
                    //bool isNewPassword = string.IsNullOrEmpty(existingHash) ||
                    //                     !BCrypt.Net.BCrypt.Verify(request.Password, existingHash);

                    //if (isNewPassword && !string.IsNullOrEmpty(request.Password))
                    //{
                    //    // Hash the new plain-text password before saving
                    //    request.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    //}
                    //else
                    //{
                    //    // If the password hasn't changed, keep the existing hash
                    //    request.Password = existingHash;
                    //}

                    // 3. Perform the Update
                    const string updateSql = @"
                UPDATE [ABB].[dbo].[Users]
                SET 
                   
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

                    int rows = await db.ExecuteAsync(updateSql, request);
                    request.Password = null;

                    return new UserResponseDTO
                    {
                        IsSuccess = rows > 0,
                        Message = "User and password updated successfully.",
                        User = request
                    };
                }
                catch (Exception ex)
                {
                    return new UserResponseDTO { IsSuccess = false, Message = ex.Message };
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
        public async Task<PropertyResponseDTO> AppendOwner(PropertyResponseDTO property)
        {
            var propertyDetails = property.Property;
            var ownerId = propertyDetails.FirstOrDefault()?.OwnerId;
            if (ownerId < 0)
            {
                return property;
            }
            else 
            {
                var owner = await GetPropertyOwner((int)ownerId);
                property.Owner = owner;
                return property;
            }



        }

    }
}
