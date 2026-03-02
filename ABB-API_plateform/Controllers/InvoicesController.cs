using Abb.Business;
using ABB_API_plateform.Business;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static Abb.DTOs.InvoiceDTOs;

namespace ABB_API_plateform.Controllers
{
    [ApiController]
    [Route("api/")]
    [EnableCors]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoices _invoices;
        private readonly ILogging _logging;

        public InvoicesController(IInvoices invoices, ILogging logging)
        {
            _invoices = invoices;
            _logging = logging;
        }

        /// <summary>
        /// Get all invoices for a specific property
        /// </summary>
        [HttpGet("GetAllInvoicesByPropertyId")]
        public async Task<IActionResult> GetAllInvoicesByPropertyId([FromQuery] int propertyId)
        {
            try
            {
                _logging.LogToFile($"GetAllInvoicesByPropertyId called - PropertyId: {propertyId}");
                
                var result = await _invoices.GetAllInvoicesByPropertyId(propertyId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CONTROLLER EXCEPTION in GetAllInvoicesByPropertyId: {ex.Message}");
                return StatusCode(500, new InvoiceResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Server error: {ex.Message}",
                    Invoices = new List<InvoiceDetail>()
                });
            }
        }

        /// <summary>
        /// Get all unpaid invoices for a specific property
        /// </summary>
        [HttpGet("GetAllNotPaidInvoicesByPropertyId")]
        public async Task<IActionResult> GetAllNotPaidInvoicesByPropertyId([FromQuery] int propertyId)
        {
            try
            {
                _logging.LogToFile($"GetAllNotPaidInvoicesByPropertyId called - PropertyId: {propertyId}");
                
                var result = await _invoices.GetAllNotPaidInvoicesByPropertyId(propertyId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CONTROLLER EXCEPTION in GetAllNotPaidInvoicesByPropertyId: {ex.Message}");
                return StatusCode(500, new InvoiceResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Server error: {ex.Message}",
                    Invoices = new List<InvoiceDetail>()
                });
            }
        }

        /// <summary>
        /// Create a new invoice
        /// </summary>
        [HttpPost("createNewInvoice")]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequestDTO request)
        {
            try
            {
                _logging.LogToFile($"CreateInvoice called - PropertyId: {request?.PropertyId}, Amount: {request?.Amount}");

                if (request == null)
                {
                    _logging.LogToFile("CreateInvoice - Request is NULL");
                    return BadRequest(new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid request body"
                    });
                }

                var result = await _invoices.CreateInvoice(request);
                
                _logging.LogToFile($"CreateInvoice completed - Success: {result.IsSuccess}");

                if (result.IsSuccess)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CONTROLLER EXCEPTION in CreateInvoice: {ex.Message}");
                return StatusCode(500, new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Update an existing invoice
        /// </summary>
        [HttpPut("UpdateInvoice")]
        public async Task<IActionResult> UpdateInvoice([FromQuery] int invoiceId, [FromBody] UpdateInvoiceRequestDTO request)
        {
            try
            {
                _logging.LogToFile($"UpdateInvoice called - InvoiceId: {invoiceId}");

                if (request == null)
                {
                    return BadRequest(new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid request body"
                    });
                }

                if (invoiceId <= 0)
                {
                    return BadRequest(new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid invoice ID"
                    });
                }

                var result = await _invoices.UpdateInvoice(invoiceId, request);
                
                _logging.LogToFile($"UpdateInvoice completed - Success: {result.IsSuccess}");

                if (result.IsSuccess)
                    return Ok(result);
                else
                    return NotFound(result);
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CONTROLLER EXCEPTION in UpdateInvoice: {ex.Message}");
                return StatusCode(500, new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Delete an invoice
        /// </summary>
        [HttpDelete("DeleteInvoiceByInvoiceId")]
        public async Task<IActionResult> DeleteInvoice([FromQuery] int invoiceId)
        {
            try
            {
                _logging.LogToFile($"DeleteInvoice called - InvoiceId: {invoiceId}");

                var result = await _invoices.DeleteInvoice(invoiceId);
                
                _logging.LogToFile($"DeleteInvoice completed - Success: {result.IsSuccess}");

                if (result.IsSuccess)
                    return Ok(result);
                else
                    return NotFound(result);
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CONTROLLER EXCEPTION in DeleteInvoice: {ex.Message}");
                return StatusCode(500, new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }
    }
}
