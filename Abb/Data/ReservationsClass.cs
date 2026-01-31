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
            List<GetReservationResponseDTO> response = new List<GetReservationResponseDTO>();

            const string sql = @"
                        SELECT Id, ConfirmationNumber, CustomerId, PropertyId, StaffId, CheckInDate, CheckoutDate, LockCode
                        FROM Reservations
                        WHERE PropertyId = @PropertyId";
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if ( !reader.HasRows) return null;

                    while (await reader.ReadAsync())
                    {
                        var reservation = new ReservationDTOs.GetReservationResponseDTO
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            ConfirmationNumber = reader["ConfirmationNumber"]?.ToString(),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            PropertyId = reader.GetInt32(reader.GetOrdinal("PropertyId")),
                            StaffId = reader.GetInt32(reader.GetOrdinal("StaffId")),
                            CheckInDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("CheckInDate"))),
                            CheckoutDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("CheckoutDate"))),
                            LockCode = reader["LockCode"]?.ToString()
                        };
                        response.Add(reservation);
                    }
                }
            }
            return response;
        }

        public async Task<GetReservationResponseDTO> GetReservationByNumber(string confirmationNumber)
        {
            if (string.IsNullOrWhiteSpace(confirmationNumber))
                return null;

            const string sql = @"
                        SELECT Id, ConfirmationNumber, CustomerId, PropertyId, StaffId, CheckInDate, CheckoutDate, LockCode
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

                    return new ReservationDTOs.GetReservationResponseDTO
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        ConfirmationNumber = reader["ConfirmationNumber"]?.ToString(),
                        CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                        PropertyId = reader.GetInt32(reader.GetOrdinal("PropertyId")),
                        StaffId = reader.GetInt32(reader.GetOrdinal("StaffId")),
                        CheckInDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("CheckInDate"))),
                        CheckoutDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("CheckoutDate"))),
                        LockCode = reader["LockCode"]?.ToString()
                    };
                }
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
                                [StaffId]
                                ,[CleaningDateTime]
                            ) 
                            VALUES (
                                @ConfirmationNumber, 
                                @CustomerId, 
                                @PropertyId, 
                                @CheckInDate, 
                                @CheckoutDate, 
                                @LockCode, 
                                @StaffId,
                                @CleaningDateTime

                            );
                            SELECT CAST(SCOPE_IDENTITY() as int);"; // Gets the last generated ID

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
                    request.CleaningDateTime
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

            const string sql = @"UPDATE [Reservations] -- Replace with your actual table name
                                SET 
                                    [CustomerId] = @CustomerId,
                                    [PropertyId] = @PropertyId,
                                    [CheckInDate] = @CheckInDate,
                                    [CheckoutDate] = @CheckoutDate,
                                    [LockCode] = @LockCode,
                                    [StaffId] = @StaffId,
                                    [CleaningDateTime] = @CleaningDateTime
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
                    request.CleaningDateTime
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
