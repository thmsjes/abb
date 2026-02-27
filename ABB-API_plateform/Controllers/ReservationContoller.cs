using Abb.Business;
using ABB_API_plateform.Business;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static Abb.DTOs.ReservationDTOs;
using static Abb.DTOs.TransactionsDTO;

namespace ABB_API_plateform.Controllers
{
    [ApiController]
    [Route("api/")]
    [EnableCors]
    public class ReservationContoller : ControllerBase
    {
        private readonly IReservations _reservations;
        private readonly ILogging _logging;  

        public ReservationContoller(IReservations reservations, ILogging logging)
        {
            _reservations = reservations;
            _logging = logging;  
        }

        [HttpGet("getAllReservationsByPropertyId")]
        public async Task<List<GetReservationResponseDTO>> GetAllReservationsByPropertyId([FromQuery]int propertyId)
        {
            return await _reservations.GetAllReservationsByPropertyId(propertyId);
        }
        [HttpGet("getAllReservationsByReference")]
        public async Task<GetReservationResponseDTO> GetReservationByNumber([FromQuery] string confirmationNumber)
        {
            return await _reservations.GetReservationByNumber(confirmationNumber);
        }

        [HttpPost("createReservation")]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationRequestDTO request)
        {
            try
            {
                // ⭐ Log incoming request
                _logging.LogToFile($"CreateReservation called - ConfirmationNumber: {request?.ConfirmationNumber}");
                
                if (request == null)
                {
                    _logging.LogToFile("CreateReservation - Request is NULL");
                    return BadRequest(new TransactionResponseDTO 
                    { 
                        IsSuccess = false, 
                        Message = "Invalid request body" 
                    });
                }

                var result = await _reservations.CreateReservation(request);
                
                // ⭐ Log result
                _logging.LogToFile($"CreateReservation completed - Success: {result.IsSuccess}, Message: {result.Message}");
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                // ⭐ Catch ANY exception at controller level
                _logging.LogToFile($"CONTROLLER EXCEPTION in CreateReservation: {ex.GetType().Name} - {ex.Message} - StackTrace: {ex.StackTrace}");
                
                return StatusCode(500, new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Server error: {ex.Message}"
                });
            }
        }

        [HttpPut("updateReservation")]
        public async Task<TransactionResponseDTO> UpdateReservation([FromBody] CreateReservationRequestDTO request)
        {
            return await _reservations.UpdateReservation(request);
        }

        [HttpDelete("deleteReservation")]
        public async Task<TransactionResponseDTO> DeleteReservation([FromQuery] int id)
        {
            return await _reservations.DeleteReservation(id);
        }
    }
}
