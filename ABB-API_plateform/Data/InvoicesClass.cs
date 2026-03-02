using ABB_API_plateform.Business;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using static Abb.DTOs.InvoiceDTOs;

namespace Abb.Data
{
    public interface IInvoicesClass
    {
        Task<List<InvoiceDetail>> GetAllInvoicesByPropertyId(int propertyId);
        Task<List<InvoiceDetail>> GetAllNotPaidInvoicesByPropertyId(int propertyId);
        Task<InvoiceDetail?> GetInvoiceById(int id);
        Task<TransactionResponseDTO> CreateInvoice(CreateInvoiceRequestDTO request);
        Task<TransactionResponseDTO> UpdateInvoice(int invoiceId, UpdateInvoiceRequestDTO request);
        Task<TransactionResponseDTO> DeleteInvoice(int invoiceId);
    }

    public class InvoicesClass : IInvoicesClass
    {
        private readonly string _connectionString;
        private readonly ILogging _logging;

        public InvoicesClass(IConfiguration configuration, ILogging logging)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
            _logging = logging;
        }

        public async Task<List<InvoiceDetail>> GetAllInvoicesByPropertyId(int propertyId)
        {
            try
            {
                const string sql = @"
                    SELECT [Id], [InvoiceNumber], [InvoiceDescription], [PropertyId], 
                           [StaffId], [DateCreated], [DateCompleted], [Amount], [RepairDescription],
                           [CreatedBy], [Paid], [DatePaid], [Type], [DateToBeAddressed], 
                           [Pending], [Completed]
                    FROM [ABB].[dbo].[Invoices]
                    WHERE [PropertyId] = @PropertyId
                    ORDER BY [DateCreated] DESC";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    var invoices = await db.QueryAsync<InvoiceDetail>(sql, new { PropertyId = propertyId });
                    return invoices.ToList();
                }
            }
            catch (SqlException sqlEx)
            {
                _logging.LogToFile($"SQL Error in GetAllInvoicesByPropertyId - PropertyId: {propertyId}, Error: {sqlEx.Number} - {sqlEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"Error in GetAllInvoicesByPropertyId - PropertyId: {propertyId}, Error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<InvoiceDetail>> GetAllNotPaidInvoicesByPropertyId(int propertyId)
        {
            try
            {
                const string sql = @"
                    SELECT [Id], [InvoiceNumber], [InvoiceDescription], [PropertyId], 
                           [StaffId], [DateCreated], [DateCompleted], [Amount], [RepairDescription],
                           [CreatedBy], [Paid], [DatePaid], [Type], [DateToBeAddressed], 
                           [Pending], [Completed]
                    FROM [ABB].[dbo].[Invoices]
                    WHERE [PropertyId] = @PropertyId AND [Paid] = 0
                    ORDER BY [DateCreated] DESC";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    var invoices = await db.QueryAsync<InvoiceDetail>(sql, new { PropertyId = propertyId });
                    return invoices.ToList();
                }
            }
            catch (SqlException sqlEx)
            {
                _logging.LogToFile($"SQL Error in GetAllNotPaidInvoicesByPropertyId - PropertyId: {propertyId}, Error: {sqlEx.Number} - {sqlEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"Error in GetAllNotPaidInvoicesByPropertyId - PropertyId: {propertyId}, Error: {ex.Message}");
                throw;
            }
        }

        public async Task<InvoiceDetail?> GetInvoiceById(int id)
        {
            try
            {
                const string sql = @"
                    SELECT [Id], [InvoiceNumber], [InvoiceDescription], [PropertyId], 
                           [StaffId], [DateCreated], [DateCompleted], [Amount], [RepairDescription],
                           [CreatedBy], [Paid], [DatePaid], [Type], [DateToBeAddressed], 
                           [Pending], [Completed]
                    FROM [ABB].[dbo].[Invoices]
                    WHERE [Id] = @Id";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return await db.QuerySingleOrDefaultAsync<InvoiceDetail>(sql, new { Id = id });
                }
            }
            catch (SqlException sqlEx)
            {
                _logging.LogToFile($"SQL Error in GetInvoiceById - Id: {id}, Error: {sqlEx.Number} - {sqlEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"Error in GetInvoiceById - Id: {id}, Error: {ex.Message}");
                throw;
            }
        }

        public async Task<TransactionResponseDTO> CreateInvoice(CreateInvoiceRequestDTO request)
        {
            try
            {
                const string sql = @"
                    INSERT INTO [ABB].[dbo].[Invoices] 
                        ([InvoiceNumber], [InvoiceDescription], [PropertyId], [StaffId], 
                         [DateCreated], [DateCompleted], [Amount], [RepairDescription], [CreatedBy],
                         [Paid], [DatePaid], [Type], [DateToBeAddressed], [Pending], [Completed])
                    VALUES 
                        (@InvoiceNumber, @InvoiceDescription, @PropertyId, @StaffId,
                         @DateCreated, @DateCompleted, @Amount, @RepairDescription, @CreatedBy,
                         @Paid, @DatePaid, @Type, @DateToBeAddressed, @Pending, @Completed);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    var dateCreated = request.DateCreated ?? DateTime.Now;
                    
                    int id = await db.QuerySingleAsync<int>(sql, new
                    {
                        request.InvoiceNumber,
                        request.InvoiceDescription,
                        request.PropertyId,
                        request.StaffId,
                        DateCreated = dateCreated,
                        request.DateCompleted,
                        request.Amount,
                        request.RepairDescription,
                        request.CreatedBy,
                        request.Paid,
                        request.DatePaid,
                        request.Type,
                        request.DateToBeAddressed,
                        request.Pending,
                        request.Completed
                    });

                    if (id > 0)
                    {
                        return new TransactionResponseDTO
                        {
                            IsSuccess = true,
                            Message = $"Invoice created successfully with ID: {id}"
                        };
                    }

                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Failed to create invoice"
                    };
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = sqlEx.Number switch
                {
                    547 => "Invalid PropertyId or StaffId: Referenced record does not exist",
                    515 => "Required field is missing or NULL",
                    2627 => $"Duplicate invoice number: '{request.InvoiceNumber}' already exists",
                    _ => $"Database error ({sqlEx.Number}): {sqlEx.Message}"
                };

                _logging.LogToFile($"SQL Error in CreateInvoice - PropertyId: {request.PropertyId}, Error: {sqlEx.Number} - {sqlEx.Message}");

                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = errorMessage
                };
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CreateInvoice Exception - PropertyId: {request.PropertyId}, Error: {ex.Message}");
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<TransactionResponseDTO> UpdateInvoice(int invoiceId, UpdateInvoiceRequestDTO request)
        {
            try
            {
                const string sql = @"
                    UPDATE [ABB].[dbo].[Invoices]
                    SET 
                        [InvoiceNumber] = @InvoiceNumber,
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
                    WHERE [Id] = @Id";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    int rowsAffected = await db.ExecuteAsync(sql, new
                    {
                        Id = invoiceId,
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
                        request.Completed
                    });

                    if (rowsAffected > 0)
                    {
                        return new TransactionResponseDTO
                        {
                            IsSuccess = true,
                            Message = "Invoice updated successfully"
                        };
                    }

                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Invoice not found with ID: {invoiceId}"
                    };
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = sqlEx.Number switch
                {
                    547 => "Invalid PropertyId or StaffId: Referenced record does not exist",
                    515 => "Required field is missing or NULL",
                    2627 => $"Duplicate invoice number: '{request.InvoiceNumber}' already exists",
                    _ => $"Database error ({sqlEx.Number}): {sqlEx.Message}"
                };

                _logging.LogToFile($"SQL Error in UpdateInvoice - Id: {invoiceId}, Error: {sqlEx.Number} - {sqlEx.Message}");

                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = errorMessage
                };
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"UpdateInvoice Exception - Id: {invoiceId}, Error: {ex.Message}");
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<TransactionResponseDTO> DeleteInvoice(int invoiceId)
        {
            try
            {
                const string sql = "DELETE FROM [ABB].[dbo].[Invoices] WHERE [Id] = @Id";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    int rowsAffected = await db.ExecuteAsync(sql, new { Id = invoiceId });

                    if (rowsAffected > 0)
                    {
                        return new TransactionResponseDTO
                        {
                            IsSuccess = true,
                            Message = "Invoice deleted successfully"
                        };
                    }

                    return new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Invoice not found with ID: {invoiceId}"
                    };
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = sqlEx.Number switch
                {
                    547 => "Cannot delete: Invoice has related records",
                    _ => $"Database error ({sqlEx.Number}): {sqlEx.Message}"
                };

                _logging.LogToFile($"SQL Error in DeleteInvoice - Id: {invoiceId}, Error: {sqlEx.Number} - {sqlEx.Message}");

                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = errorMessage
                };
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"DeleteInvoice Exception - Id: {invoiceId}, Error: {ex.Message}");
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
    }
}