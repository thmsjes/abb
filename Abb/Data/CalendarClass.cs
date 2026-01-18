using Dapper;
using Microsoft.Data.SqlClient;
using static Abb.DTOs.CalendarDTOS;
using static Abb.DTOs.TransactionsDTO;

namespace Abb.Data
{
    public interface ICalendar
    {
        Task<GetCalendarEventsResponseDTO> GetAllEventsByPropertyId(int propertyId);
        Task<TransactionResponseDTO> CreateEvent(EventDetail request);
        Task<TransactionResponseDTO> UpdateEvent(EventDetail request);
        Task<TransactionResponseDTO> DeleteEvent(int id);
    }
    public class CalendarClass : ICalendar
    {
        private readonly string _connectionString;
        public CalendarClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<GetCalendarEventsResponseDTO> GetAllEventsByPropertyId(int propertyId)
        {
            try
            {
                var response = new GetCalendarEventsResponseDTO
                {
                    IsSuccess = false,
                    Message = "Failed to retrieve events.",
                    Events = new List<EventDetail>()
                };
                const string sql = @"
                            SELECT [Id]
                                  ,[Event]
                                  ,[StartDate]
                                  ,[EndDate]
                                  ,[PropertyId]
                                  ,[DateTimeInserted]
                                  ,[Completed]
                                  ,[UserId]
                            FROM [ABB].[dbo].[CalendarEvents]
                            WHERE [PropertyId] = @PropertyId";

                using (var connection = new SqlConnection(_connectionString))
                {
                    // QueryAsync maps the columns to the CalendarEvent properties automatically
                    var events = await connection.QueryAsync<EventDetail>(sql, new { PropertyId = propertyId });
                    response.IsSuccess = true;
                    response.Message = "Events retrieved successfully.";
                    response.Events = events.ToList();
                }

                return response;
            } catch (Exception ex)
            {
                return new GetCalendarEventsResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}",
                    Events = new List<EventDetail>()
                };
            }
        }

        public async Task<TransactionResponseDTO> CreateEvent(EventDetail request)
        {
            // 1. Define the SQL Query
            const string sql = @"
                    INSERT INTO [ABB].[dbo].[CalendarEvents] 
                        ([Event], [StartDate], [EndDate], [PropertyId], [DateTimeInserted], [Completed], [UserId])
                    VALUES 
                        (@Event, @StartDate, @EndDate, @PropertyId, GETDATE(), @Completed, @UserId);
                    
                    SELECT CAST(SCOPE_IDENTITY() as int);";

            try
            {
                // 2. Basic Validation (Logic Check)
                if (request.EndDate < request.StartDate)
                {
                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = "The end date cannot be earlier than the start date."
                    };
                }

                // 3. Database Operation
                using (var connection = new SqlConnection(_connectionString))
                {
                    // Execute the insert and capture the newly generated Identity ID
                    var newId = await connection.ExecuteScalarAsync<int>(sql, new
                    {
                        request.Event,
                        request.StartDate,
                        request.EndDate,
                        request.PropertyId,
                        request.Completed,
                        request.UserId
                    });

                    return new TransactionResponseDTO
                    {
                        IsSuccess = true,
                        Message = $"Event successfully created: {newId}.",
                    };
                }
            }
            catch (SqlException sqlEx)
            {

                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = "A database error occurred while saving the event."
                };
            }
            catch (Exception ex)
            {
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
            }
        }

        public async Task<TransactionResponseDTO> UpdateEvent(EventDetail request)
        {
            const string sql = @"
                UPDATE [ABB].[dbo].[CalendarEvents]
                SET [Event] = @Event,
                    [StartDate] = @StartDate,
                    [EndDate] = @EndDate,
                    [PropertyId] = @PropertyId,
                    [Completed] = @Completed,
                    [UserId] = @UserId
                WHERE [Id] = @Id";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    int rowsAffected = await connection.ExecuteAsync(sql, new
                    {
                        request.Event,
                        request.StartDate,
                        request.EndDate,
                        request.PropertyId,
                        request.Id,
                        request.Completed,
                        request.UserId
                    });

                    if (rowsAffected > 0)
                    {
                        return new TransactionResponseDTO { IsSuccess = true, Message = "Event updated successfully." };
                    }

                    return new TransactionResponseDTO { IsSuccess = false, Message = "Event record not found." };
                }
            }
            catch (Exception ex)
            {
                // Log ex here
                return new TransactionResponseDTO { IsSuccess = false, Message = $"Update failed: {ex.Message}" };
            }
        }
        public async Task<TransactionResponseDTO> DeleteEvent(int id)
        {
            const string sql = "DELETE FROM [ABB].[dbo].[CalendarEvents] WHERE [Id] = @Id";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    int rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

                    if (rowsAffected > 0)
                    {
                        return new TransactionResponseDTO { IsSuccess = true, Message = "Event deleted successfully." };
                    }

                    return new TransactionResponseDTO { IsSuccess = false, Message = "Event could not be found or was already deleted." };
                }
            }
            catch (Exception ex)
            {
                // Log ex here
                return new TransactionResponseDTO { IsSuccess = false, Message = $"Delete failed: {ex.Message}" };
            }
        }
    }
}
