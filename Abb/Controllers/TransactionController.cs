using Abb.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Transactions;
using static Abb.DTOs.AuthDtos;
using static Abb.DTOs.TransactionsDTO;

namespace Abb.Controllers
{
    [ApiController]
    [Route("api/")]
    [EnableCors]
    public class TransactionController : ControllerBase
        {
            private readonly ITransactions _transactions;

        public TransactionController(ITransactions transactions)
        {
            _transactions = transactions;
        }
        

        [HttpPost("expense")]
            public async Task<TransactionResponseDTO> HandleExpense([FromBody] ExpenseRequestDTO request)
            {
                return await _transactions.RegisterExpense(request);
            }
        [HttpGet("expenseByAttribute")]
            public async Task<GetExpenseResponseDTO> HandleExpense([FromQuery] GetExpenseByAttributesRequestDTO request)
            {
                return await _transactions.GetExpenseByAttributes(request);
            }
        [HttpPost("addMileage")]
        public async Task<TransactionResponseDTO> AddMileage([FromBody] MileageRequestDTO request)
        {
            return await _transactions.AddMileage(request);
        }

        [HttpGet("getMileage")]
        public async Task<List<MileageResponseDTO>> GetMileage([FromQuery] GetMileageByAttributesRequestDTO request)
        {
            return await _transactions.GetMileage(request);
        }

        [HttpDelete("DeleteTransaction")]
        public async Task<TransactionResponseDTO> DeleteTransaction([FromQuery] string id)
        {
            if(int.TryParse(id, out int result))
            {
                return await _transactions.DeleteTransaction(result);
            }


            return new TransactionResponseDTO
            {
                IsSuccess = false,
                Message = "No transactionId sent"
            };
        }


    }
}

