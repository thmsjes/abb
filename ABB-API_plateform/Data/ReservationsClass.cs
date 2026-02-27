using Abb.Business;
using Abb.DTOs;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using static Abb.DTOs.ReservationDTOs;
using static Abb.DTOs.TransactionsDTO;
using ABB_API_plateform.Business;

namespace Abb.Data
{
    public interface IReservationsClass
    {
        Task<GetReservationResponseDTO> GetReservationByNumber(string confirmationNumber);
        Task<List<GetReservationResponseDTO>> GetAllReservationsByPropertyId(int propertyId);
        Task<TransactionResponseDTO> CreateReservation(CreateReservationRequestDTO request);
        Task<TransactionResponseDTO> UpdateReservationByNumber(CreateReservationRequestDTO request);
        Task<TransactionResponseDTO> DeleteReservation(int id);
    }
    
    public class ReservationsClass : IReservationsClass
    {
        private readonly string _connectionString;
        private readonly ILogging _logging;

        public ReservationsClass(IConfiguration configuration, ILogging logging)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
            _logging = logging;
        }

        public async Task<List<GetReservationResponseDTO>> GetAllReservationsByPropertyId(int propertyId)
        {
            List<GetReservationResponseDTO> response = new List<GetReservationResponseDTO>();

            try
            {
                const string sql = @"
                    SELECT Id, ConfirmationNumber, CustomerId, PropertyId, StaffId, 
                           CheckInDate, CheckoutDate, LockCode, CleaningDateTime, GuestCount, Dogs
                    FROM Reservations
                    WHERE PropertyId = @PropertyId";
                    
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync()) return response;

                        do
                        {
                            var reservation = new GetReservationResponseDTO
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                ConfirmationNumber = reader["ConfirmationNumber"]?.ToString(),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                PropertyId = reader.GetInt32(reader.GetOrdinal("PropertyId")),
                                StaffId = reader.GetInt32(reader.GetOrdinal("StaffId")),
                                CheckInDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("CheckInDate"))),
                                CheckoutDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("CheckoutDate"))),
                                LockCode = reader["LockCode"]?.ToString(),
                                CleaningDateTime = reader.IsDBNull(reader.GetOrdinal("CleaningDateTime")) 
                                    ? null 
                                    : reader.GetDateTime(reader.GetOrdinal("CleaningDateTime")),
                                GuestCount = reader.GetInt32(reader.GetOrdinal("GuestCount")),
                                Dogs = reader.GetBoolean(reader.GetOrdinal("Dogs"))
                            };
                            response.Add(reservation);
                        }
                        while (await reader.ReadAsync());
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logging.LogToFile($"SQL Error in GetAllReservationsByPropertyId - PropertyId: {propertyId}, Error: {sqlEx.Number} - {sqlEx.Message}");
                throw; // Re-throw to let caller handle
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"Error in GetAllReservationsByPropertyId - PropertyId: {propertyId}, Error: {ex.Message}");
                throw;
            }
            
            return response;
        }

        public async Task<GetReservationResponseDTO> GetReservationByNumber(string confirmationNumber)
        {
            if (string.IsNullOrWhiteSpace(confirmationNumber))
                return null;

            try
            {
                const string sql = @"
                    SELECT Id, ConfirmationNumber, CustomerId, PropertyId, StaffId, 
                           CheckInDate, CheckoutDate, LockCode, CleaningDateTime, GuestCount, Dogs
                    FROM Reservations
                    WHERE ConfirmationNumber = @ConfirmationNumber";

                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ConfirmationNumber", confirmationNumber);
                    await conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow))
                    {
                        if (!await reader.ReadAsync())
                            return null;

                        return new GetReservationResponseDTO
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            ConfirmationNumber = reader["ConfirmationNumber"]?.ToString(),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            PropertyId = reader.GetInt32(reader.GetOrdinal("PropertyId")),
                            StaffId = reader.GetInt32(reader.GetOrdinal("StaffId")),
                            CheckInDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("CheckInDate"))),
                            CheckoutDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("CheckoutDate"))),
                            LockCode = reader["LockCode"]?.ToString(),
                            CleaningDateTime = reader.IsDBNull(reader.GetOrdinal("CleaningDateTime")) 
                                ? null 
                                : reader.GetDateTime(reader.GetOrdinal("CleaningDateTime")),
                            GuestCount = reader.GetInt32(reader.GetOrdinal("GuestCount")),
                            Dogs = reader.GetBoolean(reader.GetOrdinal("Dogs"))
                        };
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logging.LogToFile($"SQL Error in GetReservationByNumber - ConfirmationNumber: {confirmationNumber}, Error: {sqlEx.Number} - {sqlEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"Error in GetReservationByNumber - ConfirmationNumber: {confirmationNumber}, Error: {ex.Message}");
                throw;
            }
        }

        public async Task<TransactionResponseDTO> CreateReservation(CreateReservationRequestDTO request)
        {
            try
            {
                const string sql = @"
                    INSERT INTO [Reservations] (
                        [ConfirmationNumber], 
                        [CustomerId], 
                        [PropertyId], 
                        [CheckInDate], 
                        [CheckoutDate], 
                        [LockCode], 
                        [StaffId],
                        [CleaningDateTime],
                        [GuestCount],
                        [Dogs]
                    ) 
                    VALUES (
                        @ConfirmationNumber, 
                        @CustomerId, 
                        @PropertyId, 
                        @CheckInDate, 
                        @CheckoutDate, 
                        @LockCode, 
                        @StaffId,
                        @CleaningDateTime,
                        @GuestCount,
                        @Dogs
                    );
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    int id = await db.QuerySingleAsync<int>(sql, request);
                    if (id == 0)
                    {
                        _logging.LogToFile($"Failed to create reservation for ConfirmationNumber: {request.ConfirmationNumber}");
                        return new TransactionResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Unable to create reservation"
                        };
                    }
                    else return new TransactionResponseDTO
                    {
                        IsSuccess = true,
                        Message = $"Reservation id: {id} created"
                    };
                }
            }
            catch (SqlException sqlEx)
            {
               
                string errorMessage = sqlEx.Number switch
                {
                    2627 => $"Duplicate reservation: Confirmation number '{request.ConfirmationNumber}' already exists", // Unique constraint violation
                    547 => "Invalid foreign key reference: Check CustomerId, PropertyId, or StaffId", // Foreign key violation
                    515 => "Required field is missing or NULL", // Cannot insert NULL
                    _ => $"Database error ({sqlEx.Number}): {sqlEx.Message}"
                };
                
                _logging.LogToFile($"SQL Error in CreateReservation - ConfirmationNumber: {request.ConfirmationNumber}, Error: {sqlEx.Number} - {sqlEx.Message}");
                
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = errorMessage
                };
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CreateReservation Exception - ConfirmationNumber: {request.ConfirmationNumber}, Error: {ex.Message}");
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<TransactionResponseDTO> UpdateReservationByNumber(CreateReservationRequestDTO request)
        {
            try 
            { 
                var response = new TransactionResponseDTO();

                const string sql = @"UPDATE [Reservations]
                    SET 
                        [CustomerId] = @CustomerId,
                        [PropertyId] = @PropertyId,
                        [CheckInDate] = @CheckInDate,
                        [CheckoutDate] = @CheckoutDate,
                        [LockCode] = @LockCode,
                        [StaffId] = @StaffId,
                        [CleaningDateTime] = @CleaningDateTime,
                        [GuestCount] = @GuestCount,
                        [Dogs] = @Dogs
                    WHERE [ConfirmationNumber] = @ConfirmationNumber;";
                    
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    int rowsAffected = await db.ExecuteAsync(sql, request);
                    if (rowsAffected > 0)
                    {
                        response.IsSuccess = true;
                        response.Message = "Reservation updated successfully";
                        return response;
                    }
                    response.IsSuccess = false;
                    response.Message = $"Reservation not found: {request.ConfirmationNumber}";
                    return response;
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = sqlEx.Number switch
                {
                    547 => "Invalid foreign key reference: Check CustomerId, PropertyId, or StaffId",
                    515 => "Required field is missing or NULL",
                    _ => $"Database error ({sqlEx.Number}): {sqlEx.Message}"
                };
                
                _logging.LogToFile($"SQL Error in UpdateReservationByNumber - ConfirmationNumber: {request.ConfirmationNumber}, Error: {sqlEx.Number} - {sqlEx.Message}");
                
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = errorMessage
                };
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"UpdateReservationByNumber Exception - ConfirmationNumber: {request.ConfirmationNumber}, Error: {ex.Message}");
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<TransactionResponseDTO> DeleteReservation(int id)
        {
            try
            {
                var response = new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = "Unable to delete reservation"
                };
                
                const string sql = "DELETE FROM [Reservations] WHERE [Id] = @Id";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    int rowsAffected = await db.ExecuteAsync(sql, new { Id = id });

                    if (rowsAffected > 0)
                    {
                        response.IsSuccess = true;
                        response.Message = "Reservation deleted successfully";
                        return response;
                    } 
                    else
                    {
                        response.Message = $"Reservation not found with Id: {id}";
                        return response;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = sqlEx.Number switch
                {
                    547 => "Cannot delete: Reservation has related records (foreign key constraint)",
                    _ => $"Database error ({sqlEx.Number}): {sqlEx.Message}"
                };
                
                _logging.LogToFile($"SQL Error in DeleteReservation - Id: {id}, Error: {sqlEx.Number} - {sqlEx.Message}");
                
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = errorMessage
                };
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"DeleteReservation Exception - Id: {id}, Error: {ex.Message}");
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
    }
}
