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
            private readonly ITransactions _transacations;

        public TransactionController(ITransactions transacations)
        {
            _transacations = transacations;
        }

        [HttpPost("expense")]
            public async Task<TransactionResponseDTO> HandleExpense([FromBody] ExpenseRequestDTO request)
            {
                return await _transacations.RegisterExpense(request);
            }
        [HttpGet("expenseByAttribute")]
            public async Task<GetExpenseResponseDTO> HandleExpense([FromBody] GetExpenseByAttributesRequestDTO request)
            {
                return await _transacations.GetExpenseByAttributes(request);
            }
        }
    }

