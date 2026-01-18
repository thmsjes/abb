using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using static Abb.DTOs.PropertyDTOs;

namespace Abb.Data
{
    public interface IProperties
    {
        Task<PropertyResponseDTO> GetAllProperties();
        Task<PropertyResponseDTO> GetPropertyById(int propertyId);
        Task<PropertyResponseDTO> CreateProperty(PropertyDetail newProperty);
        Task<PropertyResponseDTO> UpdateProperty(PropertyDetail updatedProperty);
        Task<PropertyResponseDTO> DeletePropertyById(int id);
    }

    public class PropertiesClass: IProperties
    {
        private readonly string _connectionString;
        public PropertiesClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<PropertyResponseDTO> GetAllProperties()
        {
            // Alias PropertyId to Id to match your PropertyDetail DTO
            const string sql = @"
                            SELECT [PropertyId]
                                  ,[PropertyName]
                                  ,[Address]
                                  ,[City]
                                  ,[State]
                                  ,[Zip]
                                  ,[OwnerId]
                            FROM [ABB].[dbo].[Properties]";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var properties = await db.QueryAsync<PropertyDetail>(sql);

                    return new PropertyResponseDTO
                    {
                        IsSuccess = true,
                        Message = "Properties retrieved successfully.",
                        Property = properties.ToList()
                    };
                }
                catch (Exception ex)
                {
                    return new PropertyResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve properties: {ex.Message}",
                        Property = new List<PropertyDetail>() // Return empty list instead of null
                    };
                }
            }
        }
        public async Task<PropertyResponseDTO> GetPropertyById(int propertyId)
        {
            const string sql = @"
                        SELECT [PropertyId] AS Id
                              ,[PropertyName]
                              ,[Address]
                              ,[City]
                              ,[State]
                              ,[Zip]
                              ,[OwnerId]
                        FROM [ABB].[dbo].[Properties]
                        WHERE [PropertyId] = @Id";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    // QuerySingleOrDefault returns one object or null if not found
                    var detail = await db.QuerySingleOrDefaultAsync<PropertyDetail>(sql, new { Id = propertyId });

                    if (detail == null)
                    {
                        return new PropertyResponseDTO
                        {
                            IsSuccess = false,
                            Message = $"Property with ID {propertyId} was not found.",
                            Property = new List<PropertyDetail>() // Return empty list, not null
                        };
                    }

                    return new PropertyResponseDTO
                    {
                        IsSuccess = true,
                        Message = "Property retrieved successfully.",
                        Property = new List<PropertyDetail> { detail }
                    };
                }
                catch (Exception ex)
                {
                    return new PropertyResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Database error: {ex.Message}",
                        Property = new List<PropertyDetail>()
                    };
                }
            }
        }
        public async Task<PropertyResponseDTO> CreateProperty(PropertyDetail newProperty)
        {
            const string sql = @"
                        INSERT INTO [ABB].[dbo].[Properties] 
                            ([PropertyName], [Address], [City], [State], [Zip], [OwnerId])
                        VALUES 
                            (@PropertyName, @Address, @City, @State, @Zip, @OwnerId);
                        
                        SELECT CAST(SCOPE_IDENTITY() as int);";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    int newId = await db.QuerySingleAsync<int>(sql, newProperty);
                    newProperty.PropertyId = newId; // Assign the new ID to the object

                    return new PropertyResponseDTO
                    {
                        IsSuccess = true,
                        Message = "Property created successfully.",
                        Property = new List<PropertyDetail> { newProperty }
                    };
                }
                catch (Exception ex)
                {
                    return new PropertyResponseDTO { IsSuccess = false, Message = ex.Message, Property = new List<PropertyDetail>() };
                }
            }
        }
        public async Task<PropertyResponseDTO> UpdateProperty(PropertyDetail updatedProperty)
        {
            const string sql = @"
                    UPDATE [ABB].[dbo].[Properties]
                    SET [PropertyName] = @PropertyName,
                        [Address] = @Address,
                        [City] = @City,
                        [State] = @State,
                        [Zip] = @Zip,
                        [OwnerId] = @OwnerId
                    WHERE [PropertyId] = @Id";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    int rowsAffected = await db.ExecuteAsync(sql, updatedProperty);

                    if (rowsAffected == 0)
                        return new PropertyResponseDTO { IsSuccess = false, Message = "Property not found.", Property = new List<PropertyDetail>() };

                    return new PropertyResponseDTO
                    {
                        IsSuccess = true,
                        Message = "Property updated successfully.",
                        Property = new List<PropertyDetail> { updatedProperty }
                    };
                }
                catch (Exception ex)
                {
                    return new PropertyResponseDTO { IsSuccess = false, Message = ex.Message, Property = new List<PropertyDetail>() };
                }
            }
        }
        public async Task<PropertyResponseDTO> DeletePropertyById(int id)
        {
            const string sql = "DELETE FROM [ABB].[dbo].[Properties] WHERE [PropertyId] = @Id";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    int rowsAffected = await db.ExecuteAsync(sql, new { Id = id });

                    return new PropertyResponseDTO
                    {
                        IsSuccess = rowsAffected > 0,
                        Message = rowsAffected > 0 ? "Property deleted." : "Property not found.",
                        Property = new List<PropertyDetail>()
                    };
                }
                catch (Exception ex)
                {
                    return new PropertyResponseDTO { IsSuccess = false, Message = ex.Message, Property = new List<PropertyDetail>() };
                }
            }
        }
    }
}
