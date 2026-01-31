using Abb.DTOs;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

using static Abb.DTOs.TransactionsDTO;

namespace Abb.Data
{

    public interface IInvoices
    {
        Task<TransactionResponseDTO> CreateInvoice(InvoiceRequestDTO request);
        Task<TransactionResponseDTO> UpdateInvoice(InvoiceRequestDTO request, int invoiceId);
        Task<List<InvoiceRequestDTO>> GetAllInvoicesByPropertyId(int propertyId);
        Task<List<InvoiceRequestDTO>> GetInvoicesPendingByPropertyId(int propertyId);
        Task<List<InvoiceRequestDTO>> GetAllNotPaidInvoicesByPropertyId(int propertyId);
        Task<TransactionResponseDTO> DeleteInvoiceByInvoiceId(int invoiceId);
    }

    public class InvoicesClass : IInvoices
    {
        private readonly string _connectionString;

        public InvoicesClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<TransactionResponseDTO> CreateInvoice(InvoiceRequestDTO request)
        {
            const string sql = @"
                INSERT INTO [ABB].[dbo].[Invoices] (
                    [InvoiceNumber], 
                    [InvoiceDescription], 
                    [PropertyId], 
                    [StaffId], 
                    [DateCreated], 
                    [DateCompleted], 
                    [Amount], 
                    [RepairDescription], 
                    [CreatedBy], 
                    [Paid], 
                    [DatePaid], 
                    [Type], 
                    [DateToBeAddressed], 
                    [Pending], 
                    [Completed]
                ) 
                VALUES (
                    @InvoiceNumber, 
                    @InvoiceDescription, 
                    @PropertyId, 
                    @StaffId, 
                    @DateCreated, 
                    @DateCompleted, 
                    @Amount, 
                    @RepairDescription, 
                    @CreatedBy, 
                    @Paid, 
                    @DatePaid, 
                    @Type, 
                    @DateToBeAddressed, 
                    @Pending, 
                    @Completed
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var parameters = new
                    {
                        request.InvoiceNumber,
                        request.InvoiceDescription,
                        request.PropertyId,
                        request.StaffId,
                        request.DateCreated,
                        request.DateCompleted,
                        request.Amount,
                        request.RepairDescription,
                        request.CreatedBy,
                        Paid = false,
                        request.DatePaid,
                        request.Type,
                        request.DateToBeAddressed,
                        Pending = true,
                        Completed = false
                    };

                    int id = await db.QuerySingleAsync<int>(sql, parameters);
                    
                    return new TransactionResponseDTO
                    {
                        IsSuccess = true,
                        Message = $"Invoice created successfully with ID: {id}"
                    };
                }
                catch (Exception ex)
                {
                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Error creating invoice: {ex.Message}"
                    };
                }
            }
        }

        public async Task<TransactionResponseDTO> UpdateInvoice(InvoiceRequestDTO request, int invoiceId)
        {
            const string sql = @"
                UPDATE [ABB].[dbo].[Invoices]
                SET [InvoiceNumber] = @InvoiceNumber,
                    [InvoiceDescription] = @InvoiceDescription,
                    [PropertyId] = @PropertyId,
                    [StaffId] = @StaffId,
                    [DateCreated] = @DateCreated,
                    [DateCompleted] = @DateCompleted,
                    [Amount] = @Amount,
                    [RepairDescription] = @RepairDescription,
                    [CreatedBy] = @CreatedBy,
                    [Paid] = @Paid,
                    [DatePaid] = @DatePaid,
                    [Type] = @Type,
                    [DateToBeAddressed] = @DateToBeAddressed,
                    [Pending] = @Pending,
                    [Completed] = @Completed
                WHERE [Id] = @InvoiceId";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var parameters = new
                    {
                        request.InvoiceNumber,
                        request.InvoiceDescription,
                        request.PropertyId,
                        request.StaffId,
                        request.DateCreated,
                        request.DateCompleted,
                        request.Amount,
                        request.RepairDescription,
                        request.CreatedBy,
                        request.Paid,
                        request.DatePaid,
                        request.Type,
                        request.DateToBeAddressed,
                        request.Pending,
                        request.Completed,
                        InvoiceId = invoiceId
                    };

                    int rowsAffected = await db.ExecuteAsync(sql, parameters);

                    return new TransactionResponseDTO
                    {
                        IsSuccess = rowsAffected > 0,
                        Message = rowsAffected > 0 
                            ? "Invoice updated successfully" 
                            : "Invoice not found or update failed"
                    };
                }
                catch (Exception ex)
                {
                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Error updating invoice: {ex.Message}"
                    };
                }
            }
        }

        public async Task<List<InvoiceRequestDTO>> GetAllInvoicesByPropertyId(int propertyId)
        {
            const string sql = @"
                SELECT [Id],
                       [InvoiceNumber],
                       [InvoiceDescription],
                       [PropertyId],
                       [StaffId],
                       [DateCreated],
                       [DateCompleted],
                       [Amount],
                       [RepairDescription],
                       [CreatedBy],
                       [Paid],
                       [DatePaid],
                       [Type],
                       [DateToBeAddressed],
                       [Pending],
                       [Completed]
                FROM [ABB].[dbo].[Invoices]
                WHERE [PropertyId] = @PropertyId
                ORDER BY [DateCreated] DESC";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var invoices = await db.QueryAsync<InvoiceRequestDTO>(sql, new { PropertyId = propertyId });
                    return invoices.ToList();
                }
                catch (Exception)
                {
                    return new List<InvoiceRequestDTO>();
                }
            }
        }

        public async Task<List<InvoiceRequestDTO>> GetInvoicesPendingByPropertyId(int propertyId)
        {
            const string sql = @"
                SELECT [Id],
                       [InvoiceNumber],
                       [InvoiceDescription],
                       [PropertyId],
                       [StaffId],
                       [DateCreated],
                       [DateCompleted],
                       [Amount],
                       [RepairDescription],
                       [CreatedBy],
                       [Paid],
                       [DatePaid],
                       [Type],
                       [DateToBeAddressed],
                       [Pending],
                       [Completed]
                FROM [ABB].[dbo].[Invoices]
                WHERE [PropertyId] = @PropertyId 
                  AND [Pending] = 1
                ORDER BY [DateCreated] DESC";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var invoices = await db.QueryAsync<InvoiceRequestDTO>(sql, new { PropertyId = propertyId });
                    return invoices.ToList();
                }
                catch (Exception)
                {
                    return new List<InvoiceRequestDTO>();
                }
            }
        }

        public async Task<List<InvoiceRequestDTO>> GetAllNotPaidInvoicesByPropertyId(int propertyId)
        {
            const string sql = @"
                SELECT [Id],
                       [InvoiceNumber],
                       [InvoiceDescription],
                       [PropertyId],
                       [StaffId],
                       [DateCreated],
                       [DateCompleted],
                       [Amount],
                       [RepairDescription],
                       [CreatedBy],
                       [Paid],
                       [DatePaid],
                       [Type],
                       [DateToBeAddressed],
                       [Pending],
                       [Completed]
                FROM [ABB].[dbo].[Invoices]
                WHERE [PropertyId] = @PropertyId 
                  AND ([Paid] = 0 OR [Paid] IS NULL)
                ORDER BY [DateCreated] DESC";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var invoices = await db.QueryAsync<InvoiceRequestDTO>(sql, new { PropertyId = propertyId });
                    return invoices.ToList();
                }
                catch (Exception)
                {
                    return new List<InvoiceRequestDTO>();
                }
            }
        }

        public async Task<TransactionResponseDTO> DeleteInvoiceByInvoiceId(int invoiceId)
        {
            const string sql = "DELETE FROM [ABB].[dbo].[Invoices] WHERE [Id] = @InvoiceId";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    int rowsAffected = await db.ExecuteAsync(sql, new { InvoiceId = invoiceId });

                    return new TransactionResponseDTO
                    {
                        IsSuccess = rowsAffected > 0,
                        Message = rowsAffected > 0 
                            ? "Invoice deleted successfully" 
                            : "Invoice not found or delete failed"
                    };
                }
                catch (Exception ex)
                {
                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Error deleting invoice: {ex.Message}"
                    };
                }
            }
        }

    }
}
