using Abb.Business;
using Abb.DTOs;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using static Abb.DTOs.ReservationDTOs;
using static Abb.DTOs.TransactionsDTO;

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

        public ReservationsClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<List<GetReservationResponseDTO>> GetAllReservationsByPropertyId(int propertyId)
        {
            try
            {
                List<GetReservationResponseDTO> response = new List<GetReservationResponseDTO>();

                const string sql = @"
                        SELECT Id, ConfirmationNumber, CustomerId, PropertyId, StaffId, CheckInDate, CheckoutDate, LockCode, CleaningDateTime, GuestCount, Dogs
                        FROM Reservations
                        WHERE PropertyId = @PropertyId
                        ORDER BY CheckInDate DESC";
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows) return null;

                        while (await reader.ReadAsync())
                        {
                            var reservation = new ReservationDTOs.GetReservationResponseDTO
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                ConfirmationNumber = reader["ConfirmationNumber"]?.ToString(),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                PropertyId = reader.GetInt32(reader.GetOrdinal("PropertyId")),
                                StaffId = reader.IsDBNull(reader.GetOrdinal("StaffId"))
                                        ? 0
                                        : reader.GetInt32(reader.GetOrdinal("StaffId")),
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
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<GetReservationResponseDTO> GetReservationByNumber(string confirmationNumber)
        {
            if (string.IsNullOrWhiteSpace(confirmationNumber))
                return null;

            const string sql = @"
        SELECT Id, ConfirmationNumber, CustomerId, PropertyId, StaffId, CheckInDate, 
               CheckoutDate, LockCode, CleaningDateTime, GuestCount, Dogs
        FROM [ABB].[dbo].[Reservations]
        WHERE [ConfirmationNumber] = @ConfirmationNumber";

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    // Explicitly define the parameter type to match your DB schema
                    cmd.Parameters.Add("@ConfirmationNumber", SqlDbType.NVarChar).Value = confirmationNumber.Trim();

                    await conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow))
                    {
                        if (!await reader.ReadAsync())
                            return null; // No record found

                        return new GetReservationResponseDTO
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            ConfirmationNumber = reader["ConfirmationNumber"]?.ToString(),
                            // Use IsDBNull checks for any field that COULD be null in SQL
                            CustomerId = reader.IsDBNull(reader.GetOrdinal("CustomerId")) ? 0 : reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            PropertyId = reader.IsDBNull(reader.GetOrdinal("PropertyId")) ? 0 : reader.GetInt32(reader.GetOrdinal("PropertyId")),
                            StaffId = reader.IsDBNull(reader.GetOrdinal("StaffId")) ? 0 : reader.GetInt32(reader.GetOrdinal("StaffId")),
                            CheckInDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("CheckInDate"))),
                            CheckoutDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("CheckoutDate"))),
                            LockCode = reader["LockCode"]?.ToString(),
                            CleaningDateTime = reader.IsDBNull(reader.GetOrdinal("CleaningDateTime"))
                                               ? null
                                               : reader.GetDateTime(reader.GetOrdinal("CleaningDateTime")),
                            GuestCount = reader.IsDBNull(reader.GetOrdinal("GuestCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("GuestCount")),
                            Dogs = !reader.IsDBNull(reader.GetOrdinal("Dogs")) && reader.GetBoolean(reader.GetOrdinal("Dogs"))
                        };
                    }
                }
            }
            catch (SqlException ex)
            {
                // Log this using your logging framework
                // _logger.LogError(ex, "Database error finding reservation {Ref}", confirmationNumber);
                throw new Exception("Database connectivity error.", ex);
            }
        }
        public async Task<TransactionResponseDTO> CreateReservation(CreateReservationRequestDTO request)
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
                // Convert DateOnly to DateTime using .ToDateTime(TimeOnly.MinValue)
                var parameters = new
                {
                    request.ConfirmationNumber,
                    request.CustomerId,
                    request.PropertyId,
                    CheckInDate = request.CheckInDate.ToDateTime(TimeOnly.MinValue),
                    CheckoutDate = request.CheckoutDate.ToDateTime(TimeOnly.MinValue),
                    request.LockCode,
                    request.StaffId,
                    request.CleaningDateTime,
                    request.GuestCount,
                    request.Dogs
                };

                int id = await db.QuerySingleAsync<int>(sql, parameters);
                if (id == 0)
                {
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

        public async Task<TransactionResponseDTO> UpdateReservationByNumber(CreateReservationRequestDTO request)
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
                var parameters = new
                {
                    request.CustomerId,
                    request.PropertyId,
                    CheckInDate = request.CheckInDate.ToDateTime(TimeOnly.MinValue),
                    CheckoutDate = request.CheckoutDate.ToDateTime(TimeOnly.MinValue),
                    request.LockCode,
                    request.StaffId,
                    request.ConfirmationNumber,
                    request.CleaningDateTime,
                    request.GuestCount,
                    request.Dogs
                };

                int rowsAffected = await db.ExecuteAsync(sql, parameters);
                if (rowsAffected > 0)
                {
                    response.IsSuccess = true;
                    return response;
                }
                response.IsSuccess = false;
                response.Message = $"Unable to update reservation: {request.ConfirmationNumber}";
                return response;
            }
        }

        public async Task<TransactionResponseDTO> DeleteReservation(int id)
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

                if(rowsAffected > 0)
                {
                    response.IsSuccess = true;
                    response.Message = "";
                    return response;
                } else
                {
                    return response;
                }
            }
        }
    }
}
