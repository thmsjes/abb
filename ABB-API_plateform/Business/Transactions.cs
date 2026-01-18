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
            // Added PropertyId to the column list and values
            const string sql = @"
        INSERT INTO [Transactions] 
            ([Description], [Amount], [DateAdded], [Category], [PropertyId]) 
        VALUES 
            (@Description, @Amount, @Date, @Category, @PropertyId)";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    // Dapper automatically maps request properties to the @ parameters.
                    // Note: Ensure request.Date is already a DateTime or handle conversion before calling.
                    int rowsAffected = await db.ExecuteAsync(sql, request);

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
            // 1. Base Query - Added PropertyId to the SELECT and WHERE
            StringBuilder sql = new StringBuilder(@"
        SELECT [Id], [Description], [Amount], [DateAdded], [Category], [PropertyId] 
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
                    // 3. Dapper executes and maps everything in one line
                    // We pass the 'request' object; Dapper matches names to the @parameters
                    var results = await db.QueryAsync<ExpenseDetail>(sql.ToString(), request);

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
