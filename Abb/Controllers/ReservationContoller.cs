using Abb.Business;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static Abb.DTOs.ReservationDTOs;
using static Abb.DTOs.TransactionsDTO;

namespace Abb.Controllers
{
    [ApiController]
    [Route("api/")]
    [EnableCors]

    public class ReservationContoller : ControllerBase
    {
        private readonly IReservations _reservations;

        public ReservationContoller(IReservations reservations)
        {
            _reservations = reservations;
        }

        [HttpGet("getAllReservationsByPropertyId")]
        public async Task<List<GetReservationResponseDTO>> GetAllReservationsByPropertyId([FromQuery]int propertyId)
        {
            return await _reservations.GetAllReservationsByPropertyId(propertyId);
        }

        [HttpPost("createReservation")]
        public async Task<TransactionResponseDTO> CreateReservation([FromBody] CreateReservationRequestDTO request)
        {
            return await _reservations.CreateReservation(request);
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
