using Abb.Data;
using Abb.DTOs;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static Abb.DTOs.CalendarDTOS;
using static Abb.DTOs.TransactionsDTO;

namespace Abb.Controllers
{

    [ApiController]
    [Route("api/")]
    [EnableCors]
    public class InvoicesController : Controller
    {
        private readonly IProperties _properties;
        private readonly IUsersClass _usersClass;
        private readonly ICalendar _calendar;
        private readonly IInvoices _invoices;

        public InvoicesController(IProperties properties, IUsersClass usersClass, ICalendar calendar, IInvoices invoices)
        {
            _properties = properties;
            _usersClass = usersClass;
            _calendar = calendar;
            _invoices = invoices;
        }
        [HttpPost("createNewInvoice")]
        public async Task<TransactionResponseDTO> CreateInvoice(InvoiceRequestDTO request)
        {
            return await _invoices.CreateInvoice(request);
        }

        [HttpPut("UpdateInvoice")]
        public async Task<TransactionResponseDTO> UpdateInvoice(InvoiceRequestDTO request, int invoiceId)
        {
            return await _invoices.UpdateInvoice(request, invoiceId);
        }

        [HttpGet("GetAllInvoicesByPropertyId")]
        public async Task<List<InvoiceRequestDTO>> GetAllInvoicesByPropertyId( int propertyId)
        {
            return await _invoices.GetAllInvoicesByPropertyId( propertyId);
        }
        [HttpGet("GetInvoicesPendingByPropertyId")]
        public async Task<List<InvoiceRequestDTO>> GetInvoicesPendingByPropertyId( int propertyId)
        {
            return await _invoices.GetInvoicesPendingByPropertyId( propertyId);
        }
        [HttpGet("GetAllNotPaidInvoicesByPropertyId")]
        public async Task<List<InvoiceRequestDTO>> GetAllNotPaidInvoicesByPropertyId( int propertyId)
        {
            return await _invoices.GetAllNotPaidInvoicesByPropertyId( propertyId);
        }
        [HttpDelete("DeleteInvoiceByInvoiceId")]
        public async Task<TransactionResponseDTO> DeleteInvoiceByInvoiceId( int propertyId)
        {
            return await _invoices.DeleteInvoiceByInvoiceId( propertyId);
        }


    }
}
