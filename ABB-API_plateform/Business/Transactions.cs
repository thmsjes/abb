using ABB_API_plateform.Business;
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
        Task<GetExpenseResponseDTO> GetExpenseByAttributes(GetExpenseByAttributesRequestDTO request);
        Task<TransactionResponseDTO> DeleteExpense(int expenseId);
        Task<GetMileageResponseDTO> GetMileageByPropertyId(int propertyId);
        Task<TransactionResponseDTO> CreateMileage(CreateMileageRequestDTO request);
        Task<TransactionResponseDTO> DeleteMileage(int mileageId);
    }

    public class TransactionsService : ITransactions
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly ILogging _logging;

        public TransactionsService(IConfiguration configuration, ILogging logging)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
            _configuration = configuration;
            _logging = logging;
        }

        public async Task<TransactionResponseDTO> RegisterExpense(ExpenseRequestDTO request)
        {
            const string sql = @"
                INSERT INTO [Transactions] 
                    ([Description], [Amount], [DateAdded], [Category], [PropertyId]) 
                VALUES 
                    (@Description, @Amount, @Date, @Category, @PropertyId)";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    int rowsAffected = await db.ExecuteAsync(sql, request);

                    return new TransactionResponseDTO
                    {
                        IsSuccess = rowsAffected > 0,
                        Message = rowsAffected > 0
                            ? "Expense registered successfully."
                            : "Failed to register expense."
                    };
                }
                catch (SqlException sqlEx)
                {
                    _logging.LogToFile($"SQL Error in RegisterExpense - Error: {sqlEx.Number} - {sqlEx.Message}");
                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Database error: {sqlEx.Message}"
                    };
                }
                catch (Exception ex)
                {
                    _logging.LogToFile($"RegisterExpense Exception - Error: {ex.Message}");
                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"An error occurred: {ex.Message}"
                    };
                }
            }
        }

        public async Task<GetExpenseResponseDTO> GetExpenseByAttributes(GetExpenseByAttributesRequestDTO request)
        {
            StringBuilder sql = new StringBuilder(@"
                SELECT [Id], [Description], [Amount], [DateAdded], [Category], [PropertyId] 
                FROM [Transactions] 
                WHERE 1=1 ");

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
                    var results = await db.QueryAsync<ExpenseDetail>(sql.ToString(), request);

                    return new GetExpenseResponseDTO
                    {
                        IsSuccess = true,
                        Message = $"Retrieved {results.Count()} expense(s)",
                        Expenses = results.ToList()
                    };
                }
                catch (SqlException sqlEx)
                {
                    _logging.LogToFile($"SQL Error in GetExpenseByAttributes - Error: {sqlEx.Number} - {sqlEx.Message}");
                    return new GetExpenseResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Database error: {sqlEx.Message}",
                        Expenses = new List<ExpenseDetail>()
                    };
                }
                catch (Exception ex)
                {
                    _logging.LogToFile($"GetExpenseByAttributes Exception - Error: {ex.Message}");
                    return new GetExpenseResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"An error occurred: {ex.Message}",
                        Expenses = new List<ExpenseDetail>()
                    };
                }
            }
        }

        public async Task<TransactionResponseDTO> DeleteExpense(int expenseId)
        {
            try
            {
                const string sql = "DELETE FROM [Transactions] WHERE [Id] = @Id";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    int rowsAffected = await db.ExecuteAsync(sql, new { Id = expenseId });

                    if (rowsAffected > 0)
                    {
                        return new TransactionResponseDTO
                        {
                            IsSuccess = true,
                            Message = "Expense deleted successfully"
                        };
                    }

                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Expense not found with ID: {expenseId}"
                    };
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = sqlEx.Number switch
                {
                    547 => "Cannot delete: Expense has related records",
                    _ => $"Database error ({sqlEx.Number}): {sqlEx.Message}"
                };

                _logging.LogToFile($"SQL Error in DeleteExpense - Id: {expenseId}, Error: {sqlEx.Number} - {sqlEx.Message}");

                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = errorMessage
                };
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"DeleteExpense Exception - Id: {expenseId}, Error: {ex.Message}");
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        // ⭐ NEW: Get Mileage by Property ID
        public async Task<GetMileageResponseDTO> GetMileageByPropertyId(int propertyId)
        {
            try
            {
                const string sql = @"
                    SELECT [Id], [Mileage], [Description], [DateTimeInserted], [PropertyId], [Date]
                    FROM [ABB].[dbo].[Mileage]
                    WHERE [PropertyId] = @PropertyId
                    ORDER BY [Date] DESC";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    var mileages = await db.QueryAsync<MileageDetail>(sql, new { PropertyId = propertyId });

                    return new GetMileageResponseDTO
                    {
                        IsSuccess = true,
                        Message = $"Retrieved {mileages.Count()} mileage record(s)",
                        Mileages = mileages.ToList()
                    };
                }
            }
            catch (SqlException sqlEx)
            {
                _logging.LogToFile($"SQL Error in GetMileageByPropertyId - PropertyId: {propertyId}, Error: {sqlEx.Number} - {sqlEx.Message}");
                return new GetMileageResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Database error: {sqlEx.Message}",
                    Mileages = new List<MileageDetail>()
                };
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"GetMileageByPropertyId Exception - PropertyId: {propertyId}, Error: {ex.Message}");
                return new GetMileageResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}",
                    Mileages = new List<MileageDetail>()
                };
            }
        }

        // ⭐ NEW: Create Mileage
        public async Task<TransactionResponseDTO> CreateMileage(CreateMileageRequestDTO request)
        {
            try
            {
                const string sql = @"
                    INSERT INTO [ABB].[dbo].[Mileage] 
                        ([Mileage], [Description], [DateTimeInserted], [PropertyId], [Date])
                    VALUES 
                        (@Mileage, @Description, @DateTimeInserted, @PropertyId, @Date);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    var dateTimeInserted = request.DateTimeInserted ?? DateTime.Now;

                    int id = await db.QuerySingleAsync<int>(sql, new
                    {
                        request.Mileage,
                        request.Description,
                        DateTimeInserted = dateTimeInserted,
                        request.PropertyId,
                        request.Date
                    });

                    if (id > 0)
                    {
                        return new TransactionResponseDTO
                        {
                            IsSuccess = true,
                            Message = $"Mileage created successfully with ID: {id}"
                        };
                    }

                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Failed to create mileage"
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

                _logging.LogToFile($"SQL Error in CreateMileage - PropertyId: {request.PropertyId}, Error: {sqlEx.Number} - {sqlEx.Message}");

                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = errorMessage
                };
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CreateMileage Exception - PropertyId: {request.PropertyId}, Error: {ex.Message}");
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        // ⭐ NEW: Delete Mileage
        public async Task<TransactionResponseDTO> DeleteMileage(int mileageId)
        {
            try
            {
                const string sql = "DELETE FROM [ABB].[dbo].[Mileage] WHERE [Id] = @Id";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    int rowsAffected = await db.ExecuteAsync(sql, new { Id = mileageId });

                    if (rowsAffected > 0)
                    {
                        return new TransactionResponseDTO
                        {
                            IsSuccess = true,
                            Message = "Mileage deleted successfully"
                        };
                    }

                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Mileage not found with ID: {mileageId}"
                    };
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = sqlEx.Number switch
                {
                    547 => "Cannot delete: Mileage has related records",
                    _ => $"Database error ({sqlEx.Number}): {sqlEx.Message}"
                };

                _logging.LogToFile($"SQL Error in DeleteMileage - Id: {mileageId}, Error: {sqlEx.Number} - {sqlEx.Message}");

                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = errorMessage
                };
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"DeleteMileage Exception - Id: {mileageId}, Error: {ex.Message}");
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
    }
}
