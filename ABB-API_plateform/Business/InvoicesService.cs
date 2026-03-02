using Abb.Data;
using static Abb.DTOs.InvoiceDTOs;

namespace Abb.Business
{
    public interface IInvoices
    {
        Task<InvoiceResponseDTO> GetAllInvoicesByPropertyId(int propertyId);
        Task<InvoiceResponseDTO> GetAllNotPaidInvoicesByPropertyId(int propertyId);
        Task<TransactionResponseDTO> CreateInvoice(CreateInvoiceRequestDTO request);
        Task<TransactionResponseDTO> UpdateInvoice(int invoiceId, UpdateInvoiceRequestDTO request);
        Task<TransactionResponseDTO> DeleteInvoice(int invoiceId);
    }

    public class InvoicesService(IInvoicesClass invoicesClass) : IInvoices
    {
        public async Task<InvoiceResponseDTO> GetAllInvoicesByPropertyId(int propertyId)
        {
            try
            {
                var invoices = await invoicesClass.GetAllInvoicesByPropertyId(propertyId);
                
                return new InvoiceResponseDTO
                {
                    IsSuccess = true,
                    Message = $"Retrieved {invoices.Count} invoice(s)",
                    Invoices = invoices
                };
            }
            catch (Exception ex)
            {
                return new InvoiceResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error retrieving invoices: {ex.Message}",
                    Invoices = new List<InvoiceDetail>()
                };
            }
        }

        public async Task<InvoiceResponseDTO> GetAllNotPaidInvoicesByPropertyId(int propertyId)
        {
            try
            {
                var invoices = await invoicesClass.GetAllNotPaidInvoicesByPropertyId(propertyId);
                
                return new InvoiceResponseDTO
                {
                    IsSuccess = true,
                    Message = $"Retrieved {invoices.Count} unpaid invoice(s)",
                    Invoices = invoices
                };
            }
            catch (Exception ex)
            {
                return new InvoiceResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error retrieving unpaid invoices: {ex.Message}",
                    Invoices = new List<InvoiceDetail>()
                };
            }
        }

        public async Task<TransactionResponseDTO> CreateInvoice(CreateInvoiceRequestDTO request)
        {
            // Validate amount
            if (request.Amount < 0)
            {
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = "Amount cannot be negative"
                };
            }

            return await invoicesClass.CreateInvoice(request);
        }

        public async Task<TransactionResponseDTO> UpdateInvoice(int invoiceId, UpdateInvoiceRequestDTO request)
        {
            // Validate amount
            if (request.Amount < 0)
            {
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = "Amount cannot be negative"
                };
            }

            // Validate paid status
            if (request.Paid && !request.DatePaid.HasValue)
            {
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = "DatePaid is required when invoice is marked as paid"
                };
            }

            return await invoicesClass.UpdateInvoice(invoiceId, request);
        }

        public async Task<TransactionResponseDTO> DeleteInvoice(int invoiceId)
        {
            return await invoicesClass.DeleteInvoice(invoiceId);
        }
    }
}