using ABB_API_plateform.Business;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using static Abb.DTOs.EventDTOs;

namespace Abb.Data
{
    public interface IEventsClass
    {
        Task<List<EventDetail>> GetAllEventsByPropertyId(int propertyId);
        Task<EventDetail?> GetEventById(int id);
        Task<TransactionResponseDTO> CreateEvent(CreateEventRequestDTO request);
        Task<TransactionResponseDTO> UpdateEvent(UpdateEventRequestDTO request);
        Task<TransactionResponseDTO> DeleteEvent(int id);
    }

    public class EventsClass : IEventsClass
    {
        private readonly string _connectionString;
        private readonly ILogging _logging;

        public EventsClass(IConfiguration configuration, ILogging logging)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
            _logging = logging;
        }

        public async Task<List<EventDetail>> GetAllEventsByPropertyId(int propertyId)
        {
            try
            {
                const string sql = @"
                    SELECT [Id], [Event], [StartDate], [EndDate], [PropertyId], 
                           [DateTimeInserted], [Completed], [UserId]
                    FROM [ABB].[dbo].[CalendarEvents]
                    WHERE [PropertyId] = @PropertyId
                    ORDER BY [StartDate] ASC";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    var events = await db.QueryAsync<EventDetail>(sql, new { PropertyId = propertyId });
                    return events.ToList();
                }
            }
            catch (SqlException sqlEx)
            {
                _logging.LogToFile($"SQL Error in GetAllEventsByPropertyId - PropertyId: {propertyId}, Error: {sqlEx.Number} - {sqlEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"Error in GetAllEventsByPropertyId - PropertyId: {propertyId}, Error: {ex.Message}");
                throw;
            }
        }

        public async Task<EventDetail?> GetEventById(int id)
        {
            try
            {
                const string sql = @"
                    SELECT [Id], [Event], [StartDate], [EndDate], [PropertyId], 
                           [DateTimeInserted], [Completed], [UserId]
                    FROM [ABB].[dbo].[CalendarEvents]
                    WHERE [Id] = @Id";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return await db.QuerySingleOrDefaultAsync<EventDetail>(sql, new { Id = id });
                }
            }
            catch (SqlException sqlEx)
            {
                _logging.LogToFile($"SQL Error in GetEventById - Id: {id}, Error: {sqlEx.Number} - {sqlEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"Error in GetEventById - Id: {id}, Error: {ex.Message}");
                throw;
            }
        }

        public async Task<TransactionResponseDTO> CreateEvent(CreateEventRequestDTO request)
        {
            try
            {
                const string sql = @"
                    INSERT INTO [ABB].[dbo].[CalendarEvents] 
                        ([Event], [StartDate], [EndDate], [PropertyId], [DateTimeInserted], [Completed], [UserId])
                    VALUES 
                        (@Event, @StartDate, @EndDate, @PropertyId, @DateTimeInserted, @Completed, @UserId);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    // Use current date/time if not provided
                    var dateTimeInserted = request.DateTimeInserted ?? DateTime.Now;
                    
                    int id = await db.QuerySingleAsync<int>(sql, new
                    {
                        request.Event,
                        request.StartDate,
                        request.EndDate,
                        request.PropertyId,
                        DateTimeInserted = dateTimeInserted,
                        request.Completed,
                        request.UserId
                    });

                    if (id > 0)
                    {
                        return new TransactionResponseDTO
                        {
                            IsSuccess = true,
                            Message = $"Event created successfully with ID: {id}"
                        };
                    }

                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Failed to create event"
                    };
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = sqlEx.Number switch
                {
                    547 => "Invalid PropertyId or UserId: Referenced record does not exist",
                    515 => "Required field is missing or NULL",
                    _ => $"Database error ({sqlEx.Number}): {sqlEx.Message}"
                };

                _logging.LogToFile($"SQL Error in CreateEvent - PropertyId: {request.PropertyId}, Error: {sqlEx.Number} - {sqlEx.Message}");

                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = errorMessage
                };
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CreateEvent Exception - PropertyId: {request.PropertyId}, Error: {ex.Message}");
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<TransactionResponseDTO> UpdateEvent(UpdateEventRequestDTO request)
        {
            try
            {
                const string sql = @"
                    UPDATE [ABB].[dbo].[CalendarEvents]
                    SET 
                        [Event] = @Event,
                        [StartDate] = @StartDate,
                        [EndDate] = @EndDate,
                        [PropertyId] = @PropertyId,
                        [Completed] = @Completed,
                        [UserId] = @UserId,
                        [DateTimeInserted] = @DateTimeInserted
                    WHERE [Id] = @Id";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    var dateTimeInserted = request.DateTimeInserted ?? DateTime.Now;
                    
                    int rowsAffected = await db.ExecuteAsync(sql, new
                    {
                        request.Id,
                        request.Event,
                        request.StartDate,
                        request.EndDate,
                        request.PropertyId,
                        request.Completed,
                        request.UserId,
                        DateTimeInserted = dateTimeInserted
                    });

                    if (rowsAffected > 0)
                    {
                        return new TransactionResponseDTO
                        {
                            IsSuccess = true,
                            Message = "Event updated successfully"
                        };
                    }

                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Event not found with ID: {request.Id}"
                    };
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = sqlEx.Number switch
                {
                    547 => "Invalid PropertyId or UserId: Referenced record does not exist",
                    515 => "Required field is missing or NULL",
                    _ => $"Database error ({sqlEx.Number}): {sqlEx.Message}"
                };

                _logging.LogToFile($"SQL Error in UpdateEvent - Id: {request.Id}, Error: {sqlEx.Number} - {sqlEx.Message}");

                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = errorMessage
                };
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"UpdateEvent Exception - Id: {request.Id}, Error: {ex.Message}");
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<TransactionResponseDTO> DeleteEvent(int id)
        {
            try
            {
                const string sql = "DELETE FROM [ABB].[dbo].[CalendarEvents] WHERE [Id] = @Id";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    int rowsAffected = await db.ExecuteAsync(sql, new { Id = id });

                    if (rowsAffected > 0)
                    {
                        return new TransactionResponseDTO
                        {
                            IsSuccess = true,
                            Message = "Event deleted successfully"
                        };
                    }

                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Event not found with ID: {id}"
                    };
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = sqlEx.Number switch
                {
                    547 => "Cannot delete: Event has related records",
                    _ => $"Database error ({sqlEx.Number}): {sqlEx.Message}"
                };

                _logging.LogToFile($"SQL Error in DeleteEvent - Id: {id}, Error: {sqlEx.Number} - {sqlEx.Message}");

                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = errorMessage
                };
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"DeleteEvent Exception - Id: {id}, Error: {ex.Message}");
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
    }
}