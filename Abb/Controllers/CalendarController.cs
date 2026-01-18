using Abb.Data;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Abb.DTOs.CalendarDTOS;
using static Abb.DTOs.TransactionsDTO;

namespace Abb.Controllers
{
    [ApiController]
    [Route("api/")]
    [EnableCors]
    public class CalendarController : Controller
    {
        private readonly IProperties _properties;
        private readonly IUsersClass _usersClass;
        private readonly ICalendar _calendar;

        public CalendarController(IProperties properties, IUsersClass usersClass, ICalendar calendar)
        {
            _properties = properties;
            _usersClass = usersClass;
            _calendar = calendar;
        }

        [HttpGet("getAllEventsByProperty/{propertyId}")]
        public async Task<GetCalendarEventsResponseDTO> GetAllEventsByProperty(int propertyId)
        {
            return await _calendar.GetAllEventsByPropertyId(propertyId);
        }

        [HttpPost("createNewEvent")]
        public async Task<TransactionResponseDTO> CreateEvent(EventDetail request)
        {
            return await _calendar.CreateEvent(request);
        }

        [HttpPut("UpdateEvent")]
        public async Task<TransactionResponseDTO> UpdateEvent(EventDetail request) 
        {
            return await _calendar.UpdateEvent(request);
        }

        [HttpDelete("deleteEvent/{id}")]
        public async Task<TransactionResponseDTO> DeleteEvent(int id)
        {
            return await _calendar.DeleteEvent(id);
        }

    }
}
