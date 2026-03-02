using Abb.Business;
using ABB_API_plateform.Business;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static Abb.DTOs.EventDTOs;

namespace ABB_API_plateform.Controllers
{
    [ApiController]
    [Route("api/")]
    [EnableCors]
    public class EventsController : ControllerBase
    {
        private readonly IEvents _events;
        private readonly ILogging _logging;

        public EventsController(IEvents events, ILogging logging)
        {
            _events = events;
            _logging = logging;
        }

 
        [HttpGet("getAllEventsByProperty/{propertyId}")]
        public async Task<IActionResult> GetAllEventsByPropertyId(int propertyId)
        {
            try
            {
                _logging.LogToFile($"GetAllEventsByPropertyId called - PropertyId: {propertyId}");
                
                var result = await _events.GetAllEventsByPropertyId(propertyId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CONTROLLER EXCEPTION in GetAllEventsByPropertyId: {ex.Message}");
                return StatusCode(500, new EventResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Server error: {ex.Message}",
                    Events = new List<EventDetail>()
                });
            }
        }


        [HttpPost("createNewEvent")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequestDTO request)
        {
            try
            {
                _logging.LogToFile($"CreateEvent called - PropertyId: {request?.PropertyId}, Event: {request?.Event}");

                if (request == null)
                {
                    _logging.LogToFile("CreateEvent - Request is NULL");
                    return BadRequest(new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid request body"
                    });
                }

                var result = await _events.CreateEvent(request);
                
                _logging.LogToFile($"CreateEvent completed - Success: {result.IsSuccess}");

                if (result.IsSuccess)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CONTROLLER EXCEPTION in CreateEvent: {ex.Message}");
                return StatusCode(500, new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Server error: {ex.Message}"
                });
            }
        }


        [HttpPut("UpdateEvent")]
        public async Task<IActionResult> UpdateEvent([FromBody] UpdateEventRequestDTO request)
        {
            try
            {
                _logging.LogToFile($"UpdateEvent called - Id: {request?.Id}");

                if (request == null)
                {
                    return BadRequest(new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid request body"
                    });
                }

                if (request.Id <= 0)
                {
                    return BadRequest(new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid event ID"
                    });
                }

                var result = await _events.UpdateEvent(request);
                
                _logging.LogToFile($"UpdateEvent completed - Success: {result.IsSuccess}");

                if (result.IsSuccess)
                    return Ok(result);
                else
                    return NotFound(result);
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CONTROLLER EXCEPTION in UpdateEvent: {ex.Message}");
                return StatusCode(500, new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Server error: {ex.Message}"
                });
            }
        }


        [HttpDelete("deleteEvent/{eventId}")]
        public async Task<IActionResult> DeleteEvent(int eventId)
        {
            try
            {
                _logging.LogToFile($"DeleteEvent called - Id: {eventId}");

                var result = await _events.DeleteEvent(eventId);
                
                _logging.LogToFile($"DeleteEvent completed - Success: {result.IsSuccess}");

                if (result.IsSuccess)
                    return Ok(result);
                else
                    return NotFound(result);
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CONTROLLER EXCEPTION in DeleteEvent: {ex.Message}");
                return StatusCode(500, new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }
    }
}
