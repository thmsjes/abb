using Abb.Business;
using ABB_API_plateform.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Transactions;
using static Abb.DTOs.AuthDtos;
using static Abb.DTOs.TransactionsDTO;

namespace ABB_API_plateform.Controllers
{
    [ApiController]
    [Route("api/")]
    [EnableCors]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactions _transactions;
        private readonly ILogging _logging;  

        public TransactionController(ITransactions transactions, ILogging logging)
        {
            _transactions = transactions;
            _logging = logging; 
        }

    

        [HttpPost("expense")]
        public async Task<TransactionResponseDTO> HandleExpense([FromBody] ExpenseRequestDTO request)
        {
            return await _transactions.RegisterExpense(request);
        }

        [HttpPost("expenseByAttribute")]
        public async Task<GetExpenseResponseDTO> HandleExpenseByAttribute([FromBody] GetExpenseByAttributesRequestDTO request)
        {
            return await _transactions.GetExpenseByAttributes(request);
        }

        [HttpDelete("deleteExpense/{expenseId}")] 
        public async Task<IActionResult> DeleteExpense(int expenseId)
        {
            try
            {
                _logging.LogToFile($"DeleteExpense called - ExpenseId: {expenseId}");

                if (expenseId <= 0)
                {
                    return BadRequest(new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid expense ID"
                    });
                }

                var result = await _transactions.DeleteExpense(expenseId);

                _logging.LogToFile($"DeleteExpense completed - Success: {result.IsSuccess}");

                if (result.IsSuccess)
                    return Ok(result);
                else
                    return NotFound(result);
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CONTROLLER EXCEPTION in DeleteExpense: {ex.Message}");
                return StatusCode(500, new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Server error: {ex.Message}"
                });
            }
        }


        [HttpGet("getMileage/{propertyId}")] 
        public async Task<IActionResult> GetMileage(int propertyId)
        {
            try
            {
                _logging.LogToFile($"GetMileage called - PropertyId: {propertyId}");

                if (propertyId <= 0)
                {
                    return BadRequest(new GetMileageResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid property ID",
                        Mileages = new List<MileageDetail>()
                    });
                }

                var result = await _transactions.GetMileageByPropertyId(propertyId);

                _logging.LogToFile($"GetMileage completed - Success: {result.IsSuccess}, Count: {result.Mileages.Count}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CONTROLLER EXCEPTION in GetMileage: {ex.Message}");
                return StatusCode(500, new GetMileageResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Server error: {ex.Message}",
                    Mileages = new List<MileageDetail>()
                });
            }
        }

        [HttpPost("mileage")]  
        public async Task<IActionResult> CreateMileage([FromBody] CreateMileageRequestDTO request)
        {
            try
            {
                _logging.LogToFile($"CreateMileage called - PropertyId: {request?.PropertyId}, Mileage: {request?.Mileage}");

                if (request == null)
                {
                    _logging.LogToFile("CreateMileage - Request is NULL");
                    return BadRequest(new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid request body"
                    });
                }

                if (request.Mileage < 0)
                {
                    return BadRequest(new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Mileage cannot be negative"
                    });
                }

                var result = await _transactions.CreateMileage(request);

                _logging.LogToFile($"CreateMileage completed - Success: {result.IsSuccess}");

                if (result.IsSuccess)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CONTROLLER EXCEPTION in CreateMileage: {ex.Message}");
                return StatusCode(500, new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Server error: {ex.Message}"
                });
            }
        }

        [HttpDelete("deleteMileage/{mileageId}")] 
        public async Task<IActionResult> DeleteMileage(int mileageId)
        {
            try
            {
                _logging.LogToFile($"DeleteMileage called - MileageId: {mileageId}");

                if (mileageId <= 0)
                {
                    return BadRequest(new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid mileage ID"
                    });
                }

                var result = await _transactions.DeleteMileage(mileageId);

                _logging.LogToFile($"DeleteMileage completed - Success: {result.IsSuccess}");

                if (result.IsSuccess)
                    return Ok(result);
                else
                    return NotFound(result);
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CONTROLLER EXCEPTION in DeleteMileage: {ex.Message}");
                return StatusCode(500, new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Server error: {ex.Message}"
                });
            }
        }
    }
}

