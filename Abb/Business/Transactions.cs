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
                                ([Description], [Amount], [DateAdded], [Category], [PropertyId]) 
                            VALUES 
                                (@Description, @Amount, @DateAdded, @Category, @PropertyId)";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    // Logic: If Date is default (0001-01-01), use Today.
                    // We also convert DateOnly to DateTime to avoid Dapper mapping errors.
                    var dateToSave = request.Date == default
                        ? DateTime.Today
                        : request.Date.ToDateTime(TimeOnly.MinValue);

                    int rowsAffected = await db.ExecuteAsync(sql, new
                    {
                        request.Description,
                        request.Amount,
                        DateAdded = dateToSave, // Use our calculated date
                        request.Category,
                        request.PropertyId
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
                                       [PropertyId] 
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
                        request.Category
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

    }
}
