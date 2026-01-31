using BCrypt.Net;
using Dapper;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Data;
using System.Text;
using static Abb.DTOs.TransactionsDTO;

namespace Abb.Business
{

    public interface ITransactions
    {
        Task<TransactionResponseDTO> RegisterExpense(ExpenseRequestDTO request);
        Task<TransactionResponseDTO> AddMileage(MileageRequestDTO request);
        Task<List<MileageResponseDTO>> GetMileage(GetMileageByAttributesRequestDTO request);
        Task<GetExpenseResponseDTO> GetExpenseByAttributes(GetExpenseByAttributesRequestDTO request);
        Task<TransactionResponseDTO> DeleteTransaction(int transactionId);

    }
    public class TransactionsService : ITransactions
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        public TransactionsService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _configuration = configuration;
        }
        public async Task<TransactionResponseDTO> RegisterExpense(ExpenseRequestDTO request)
        {
            const string sql = @"
                            INSERT INTO [Transactions] 
                                ([Description], [Amount], [DateAdded], [Category], [PropertyId], [PaymentType], [Expense], [Payment], [Vendor]) 
                            VALUES 
                                (@Description, @Amount, @DateAdded, @Category, @PropertyId, @PaymentType, @Expense, @Payment, @Vendor)";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    // Logic: If Date is default (0001-01-01), use Today.
                    // We also convert DateOnly to DateTime to avoid Dapper mapping errors.
                    var dateToSave = request.Date == default
                        ? DateTime.Today
                        : request.Date.ToDateTime(TimeOnly.MinValue);
                    var expense = request.Category == "Payment" ? false : true;
                    int rowsAffected = await db.ExecuteAsync(sql, new
                    {
                        request.Description,
                        request.Amount,
                        DateAdded = dateToSave, // Use our calculated date
                        request.Category,
                        request.PropertyId,
                        request.PaymentType,
                        Expense = expense,  
                        Payment = !expense, 
                        request.Vendor
                    });

                    return new TransactionResponseDTO
                    {
                        IsSuccess = rowsAffected > 0,
                        Message = rowsAffected > 0
                            ? "Expense registered successfully."
                            : "Failed to register expense."
                    };
                }
                catch (Exception ex)
                {
                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Database error: {ex.Message}"
                    };
                }
            }
        }
        public async Task<GetExpenseResponseDTO> GetExpenseByAttributes(GetExpenseByAttributesRequestDTO request)
        {
            // 1. Base Query - Using AS Date to map DateAdded to the ExpenseDetail.Date property
            StringBuilder sql = new StringBuilder(@"
                                SELECT [Id], 
                                       [Description], 
                                       [Amount], 
                                       [DateAdded] AS Date, 
                                       [Category], 
                                       [PropertyId], 
                                       [Expense] ,
                                       [Payment] 
                                FROM [Transactions] 
                                WHERE 1=1 ");

            // 2. Dynamic Filtering
            if (request.PropertyId > 0)
                sql.Append(" AND PropertyId = @PropertyId");

            if (request.StartDate.HasValue)
                sql.Append(" AND DateAdded >= @StartDate");

            if (request.EndDate.HasValue)
                sql.Append(" AND DateAdded <= @EndDate");

            if (!string.IsNullOrEmpty(request.Category))
                sql.Append(" AND Category = @Category");

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var parameters = new
                    {
                        request.PropertyId,
                        StartDate = request.StartDate?.ToDateTime(TimeOnly.MinValue),
                        EndDate = request.EndDate?.ToDateTime(TimeOnly.MaxValue),
                        request.Category,

                    };

                    var results = await db.QueryAsync<ExpenseDetail>(sql.ToString(), parameters);

                    return new GetExpenseResponseDTO
                    {
                        IsSuccess = true,
                        Expenses = results.ToList()
                    };
                }
                catch (Exception ex)
                {
                    return new GetExpenseResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Error retrieving expenses: {ex.Message}"
                    };
                }
            }
        }
        public async Task<TransactionResponseDTO> AddMileage(MileageRequestDTO request)
        {

            string sql = @"INSERT INTO [dbo].[Mileage] 
                   ([Mileage], [Description], [DateTimeInserted], [PropertyId], [Date]) 
                   VALUES (@Mileage, @Description, @DateTimeInserted, @PropertyId, @Date)";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                // Dapper automatically maps these properties to the @parameters in the SQL string
                var parameters = new
                {
                    Mileage = request.Mileage,
                    Description = request.Description,
                    DateTimeInserted = DateTime.Now,
                    Date = request.Date,
                    PropertyId = request.PropertyId
                };

                try
                {
                    int rowsAffected = db.Execute(sql, parameters);
                    Console.WriteLine($"Successfully inserted {rowsAffected} record(s).");
                    return new TransactionResponseDTO {IsSuccess = true };
                }
                catch (Exception ex)
                {
                    // Handle exceptions (logging, etc)
                    Console.WriteLine(ex.Message);
                    return new TransactionResponseDTO { IsSuccess = false, Message = ex.Message.ToString() };

                }
            }

        }
        public async Task<List<MileageResponseDTO>> GetMileage(GetMileageByAttributesRequestDTO request)
        {
            // 1. Base Query
            var sql = @"SELECT [id]
                      ,[Mileage]
                      ,[Description]
                      ,[DateTimeInserted]
                      ,[PropertyId]
                      ,[Date]
                FROM [ABB].[dbo].[Mileage]
                WHERE [PropertyId] = @PropertyId";

            // 2. Add Optional Date Filtering
            if (request.StartDate.HasValue)
            {
                sql += " AND [DateTimeInserted] >= @StartDate";
            }

            if (request.EndDate.HasValue)
            {
                // Add 1 day to the EndDate to ensure the query includes the entire final day
                sql += " AND [DateTimeInserted] < @EndDatePlusOne";
            }

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                // 3. Execute the query using Dapper's QueryAsync
                // Dapper handles the mapping from the SQL columns to your DTO properties
                var results = await db.QueryAsync<MileageResponseDTO>(sql, new
                {
                    PropertyId = request.PropertyId,
                    StartDate = request.StartDate?.ToDateTime(TimeOnly.MinValue),
                    EndDatePlusOne = request.EndDate?.ToDateTime(TimeOnly.MinValue).AddDays(1)
                });

                // 4. Manually map to handle DateOnly conversion
                var mileageList = results.Select(static row => new MileageResponseDTO
                {
                    Mileage = row.Mileage,
                    Description = row.Description,
                    DateTimeInserted = row.DateTimeInserted,
                    PropertyId = row.PropertyId,
                    Date = row.Date
                }).ToList();

                return mileageList;
            }
        }
        public async Task<TransactionResponseDTO> DeleteTransaction(int transactionId)
        {
            const string sql = "DELETE FROM [ABB].[dbo].[Transactions] WHERE [Id] = @TransactionId";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    int rowsAffected = await db.ExecuteAsync(sql, new { TransactionId = transactionId });

                    return new TransactionResponseDTO
                    {
                        IsSuccess = rowsAffected > 0,
                        Message = rowsAffected > 0
                            ? "Transaction deleted successfully"
                            : "Transaction not found or delete failed"
                    };
                }
                catch (Exception ex)
                {
                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Error deleting transaction: {ex.Message}"
                    };
                }
            }
        }

    }
}
