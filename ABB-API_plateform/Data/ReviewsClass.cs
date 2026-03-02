using ABB_API_plateform.Business;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using static Abb.DTOs.ReviewDTOs;

namespace Abb.Data
{
    public interface IReviewsClass
    {
        Task<List<ReviewDetail>> GetReviewsByPropertyId(int propertyId);
        Task<ReviewDetail?> GetReviewById(int id);
        Task<TransactionResponseDTO> CreateReview(CreateReviewRequestDTO request);
        Task<TransactionResponseDTO> UpdateReview(UpdateReviewRequestDTO request);  // ⭐ Changed parameter
        Task<TransactionResponseDTO> DeleteReview(int id);
    }

    public class ReviewsClass : IReviewsClass
    {
        private readonly string _connectionString;
        private readonly ILogging _logging;

        public ReviewsClass(IConfiguration configuration, ILogging logging)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
            _logging = logging;
        }

        public async Task<List<ReviewDetail>> GetReviewsByPropertyId(int propertyId)
        {
            try
            {
                const string sql = @"
                    SELECT [Id], [ReviewDate], [ReviewerName], [ReviewText], [PropertyId], [Score]
                    FROM [ABB].[dbo].[Reviews]
                    WHERE [PropertyId] = @PropertyId
                    ORDER BY [ReviewDate] DESC";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    var reviews = await db.QueryAsync<ReviewDetail>(sql, new { PropertyId = propertyId });
                    return reviews.ToList();
                }
            }
            catch (SqlException sqlEx)
            {
                _logging.LogToFile($"SQL Error in GetReviewsByPropertyId - PropertyId: {propertyId}, Error: {sqlEx.Number} - {sqlEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"Error in GetReviewsByPropertyId - PropertyId: {propertyId}, Error: {ex.Message}");
                throw;
            }
        }

        public async Task<ReviewDetail?> GetReviewById(int id)
        {
            try
            {
                const string sql = @"
                    SELECT [Id], [ReviewDate], [ReviewerName], [ReviewText], [PropertyId], [Score]
                    FROM [ABB].[dbo].[Reviews]
                    WHERE [Id] = @Id";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return await db.QuerySingleOrDefaultAsync<ReviewDetail>(sql, new { Id = id });
                }
            }
            catch (SqlException sqlEx)
            {
                _logging.LogToFile($"SQL Error in GetReviewById - Id: {id}, Error: {sqlEx.Number} - {sqlEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"Error in GetReviewById - Id: {id}, Error: {ex.Message}");
                throw;
            }
        }

        public async Task<TransactionResponseDTO> CreateReview(CreateReviewRequestDTO request)
        {
            try
            {
                const string sql = @"
                    INSERT INTO [ABB].[dbo].[Reviews] 
                        ([ReviewDate], [ReviewerName], [ReviewText], [PropertyId], [Score])
                    VALUES 
                        (@ReviewDate, @ReviewerName, @ReviewText, @PropertyId, @Score);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    // Use current date if not provided
                    var reviewDate = request.ReviewDate ?? DateTime.Now;
                    
                    int id = await db.QuerySingleAsync<int>(sql, new
                    {
                        ReviewDate = reviewDate,
                        request.ReviewerName,
                        request.ReviewText,
                        request.PropertyId,
                        request.Score
                    });

                    if (id > 0)
                    {
                        return new TransactionResponseDTO
                        {
                            IsSuccess = true,
                            Message = $"Review created successfully with ID: {id}"
                        };
                    }

                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Failed to create review"
                    };
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = sqlEx.Number switch
                {
                    547 => "Invalid PropertyId: Property does not exist",
                    515 => "Required field is missing or NULL",
                    _ => $"Database error ({sqlEx.Number}): {sqlEx.Message}"
                };

                _logging.LogToFile($"SQL Error in CreateReview - PropertyId: {request.PropertyId}, Error: {sqlEx.Number} - {sqlEx.Message}");

                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = errorMessage
                };
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CreateReview Exception - PropertyId: {request.PropertyId}, Error: {ex.Message}");
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        // ⭐ Updated to accept UpdateReviewRequestDTO and extract Id from it
        public async Task<TransactionResponseDTO> UpdateReview(UpdateReviewRequestDTO request)
        {
            try
            {
                const string sql = @"
                    UPDATE [ABB].[dbo].[Reviews]
                    SET 
                        [ReviewerName] = @ReviewerName,
                        [ReviewText] = @ReviewText,
                        [PropertyId] = @PropertyId,
                        [Score] = @Score,
                        [ReviewDate] = @ReviewDate
                    WHERE [Id] = @Id";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    var reviewDate = request.ReviewDate ?? DateTime.Now;
                    
                    int rowsAffected = await db.ExecuteAsync(sql, new
                    {
                        request.Id,  // ⭐ Id now comes from request body
                        request.ReviewerName,
                        request.ReviewText,
                        request.PropertyId,
                        request.Score,
                        ReviewDate = reviewDate
                    });

                    if (rowsAffected > 0)
                    {
                        return new TransactionResponseDTO
                        {
                            IsSuccess = true,
                            Message = "Review updated successfully"
                        };
                    }

                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Review not found with ID: {request.Id}"
                    };
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = sqlEx.Number switch
                {
                    547 => "Invalid PropertyId: Property does not exist",
                    515 => "Required field is missing or NULL",
                    _ => $"Database error ({sqlEx.Number}): {sqlEx.Message}"
                };

                _logging.LogToFile($"SQL Error in UpdateReview - Id: {request.Id}, Error: {sqlEx.Number} - {sqlEx.Message}");

                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = errorMessage
                };
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"UpdateReview Exception - Id: {request.Id}, Error: {ex.Message}");
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<TransactionResponseDTO> DeleteReview(int id)
        {
            try
            {
                const string sql = "DELETE FROM [ABB].[dbo].[Reviews] WHERE [Id] = @Id";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    int rowsAffected = await db.ExecuteAsync(sql, new { Id = id });

                    if (rowsAffected > 0)
                    {
                        return new TransactionResponseDTO
                        {
                            IsSuccess = true,
                            Message = "Review deleted successfully"
                        };
                    }

                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Review not found with ID: {id}"
                    };
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = sqlEx.Number switch
                {
                    547 => "Cannot delete: Review has related records",
                    _ => $"Database error ({sqlEx.Number}): {sqlEx.Message}"
                };

                _logging.LogToFile($"SQL Error in DeleteReview - Id: {id}, Error: {sqlEx.Number} - {sqlEx.Message}");

                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = errorMessage
                };
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"DeleteReview Exception - Id: {id}, Error: {ex.Message}");
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
    }
}