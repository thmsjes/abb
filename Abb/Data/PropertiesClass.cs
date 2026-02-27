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

        // Review CRUD methods
        Task<ReviewResponseDTO> GetAllReviews();
        Task<ReviewResponseDTO> GetReviewsByPropertyId(int propertyId);
        Task<ReviewResponseDTO> GetReviewById(int reviewId);
        Task<ReviewResponseDTO> CreateReview(CreateReviewRequestDTO request);
        Task<ReviewResponseDTO> UpdateReview(UpdateReviewRequestDTO request);
        Task<ReviewResponseDTO> DeleteReviewById(int id);
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
                    WHERE [PropertyId] = @propertyId";

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

        // ==================== REVIEW CRUD METHODS ====================

        public async Task<ReviewResponseDTO> GetAllReviews()
        {
            const string sql = @"
                SELECT [Id], [ReviewDate], [ReviewerName], [ReviewText], [PropertyId], [Score]
                FROM [ABB].[dbo].[Reviews]
                ORDER BY [ReviewDate] DESC";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var reviews = await db.QueryAsync<ReviewDetail>(sql);

                    return new ReviewResponseDTO
                    {
                        IsSuccess = true,
                        Message = "Reviews retrieved successfully.",
                        Reviews = reviews.ToList()
                    };
                }
                catch (Exception ex)
                {
                    return new ReviewResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve reviews: {ex.Message}",
                        Reviews = new List<ReviewDetail>()
                    };
                }
            }
        }

        public async Task<ReviewResponseDTO> GetReviewsByPropertyId(int propertyId)
        {
            const string sql = @"
                SELECT [Id], [ReviewDate], [ReviewerName], [ReviewText], [PropertyId], [Score]
                FROM [ABB].[dbo].[Reviews]
                WHERE [PropertyId] = @PropertyId
                ORDER BY [ReviewDate] DESC";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var reviews = await db.QueryAsync<ReviewDetail>(sql, new { PropertyId = propertyId });

                    return new ReviewResponseDTO
                    {
                        IsSuccess = true,
                        Message = $"Reviews for property {propertyId} retrieved successfully.",
                        Reviews = reviews.ToList()
                    };
                }
                catch (Exception ex)
                {
                    return new ReviewResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve reviews: {ex.Message}",
                        Reviews = new List<ReviewDetail>()
                    };
                }
            }
        }

        public async Task<ReviewResponseDTO> GetReviewById(int reviewId)
        {
            const string sql = @"
                SELECT [Id], [ReviewDate], [ReviewerName], [ReviewText], [PropertyId], [Score]
                FROM [ABB].[dbo].[Reviews]
                WHERE [Id] = @Id";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var review = await db.QuerySingleOrDefaultAsync<ReviewDetail>(sql, new { Id = reviewId });

                    if (review == null)
                    {
                        return new ReviewResponseDTO
                        {
                            IsSuccess = false,
                            Message = $"Review with ID {reviewId} was not found.",
                            Reviews = new List<ReviewDetail>()
                        };
                    }

                    return new ReviewResponseDTO
                    {
                        IsSuccess = true,
                        Message = "Review retrieved successfully.",
                        Reviews = new List<ReviewDetail> { review }
                    };
                }
                catch (Exception ex)
                {
                    return new ReviewResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Database error: {ex.Message}",
                        Reviews = new List<ReviewDetail>()
                    };
                }
            }
        }

        public async Task<ReviewResponseDTO> CreateReview(CreateReviewRequestDTO request)
        {
            const string sql = @"
                INSERT INTO [ABB].[dbo].[Reviews] 
                    ([ReviewDate], [ReviewerName], [ReviewText], [PropertyId], [Score])
                VALUES 
                    (@ReviewDate, @ReviewerName, @ReviewText, @PropertyId, @Score);
                
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var parameters = new
                    {
                        request.ReviewDate,
                        request.ReviewerName,
                        request.ReviewText,
                        request.PropertyId,
                        request.Score
                    };

                    int newId = await db.QuerySingleAsync<int>(sql, parameters);

                    var newReview = new ReviewDetail
                    {
                        Id = newId,
                        ReviewDate = request.ReviewDate,
                        ReviewerName = request.ReviewerName,
                        ReviewText = request.ReviewText,
                        PropertyId = request.PropertyId,
                        Score = request.Score
                    };

                    return new ReviewResponseDTO
                    {
                        IsSuccess = true,
                        Message = "Review created successfully.",
                        Reviews = new List<ReviewDetail> { newReview }
                    };
                }
                catch (Exception ex)
                {
                    return new ReviewResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Failed to create review: {ex.Message}",
                        Reviews = new List<ReviewDetail>()
                    };
                }
            }
        }

        public async Task<ReviewResponseDTO> UpdateReview(UpdateReviewRequestDTO request)
        {
            const string sql = @"
        UPDATE [ABB].[dbo].[Reviews]
        SET [ReviewerName] = @ReviewerName,
            [ReviewText] = @ReviewText,
            [Score] = @Score,
            [ReviewDate] = @ReviewDate
        WHERE [Id] = @Id";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var parameters = new
                    {
                        request.ReviewerName,
                        request.ReviewText,
                        request.Score,
                        request.Id,
                        request.ReviewDate  // Update to current time
                    };

                    int rowsAffected = await db.ExecuteAsync(sql, parameters);

                    if (rowsAffected == 0)
                    {
                        return new ReviewResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Review not found.",
                            Reviews = new List<ReviewDetail>()
                        };
                    }

                    // Fetch the updated review
                    return await GetReviewById(request.Id);
                }
                catch (Exception ex)
                {
                    return new ReviewResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Failed to update review: {ex.Message}",
                        Reviews = new List<ReviewDetail>()
                    };
                }
            }
        }

        public async Task<ReviewResponseDTO> DeleteReviewById(int id)
        {
            const string sql = "DELETE FROM [ABB].[dbo].[Reviews] WHERE [Id] = @Id";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    int rowsAffected = await db.ExecuteAsync(sql, new { Id = id });

                    return new ReviewResponseDTO
                    {
                        IsSuccess = rowsAffected > 0,
                        Message = rowsAffected > 0 ? "Review deleted successfully." : "Review not found.",
                        Reviews = new List<ReviewDetail>()
                    };
                }
                catch (Exception ex)
                {
                    return new ReviewResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Failed to delete review: {ex.Message}",
                        Reviews = new List<ReviewDetail>()
                    };
                }
            }
        }
    }
}
