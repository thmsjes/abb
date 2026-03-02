using Abb.Data;
using static Abb.DTOs.EventDTOs;

namespace Abb.Business
{
    public interface IEvents
    {
        Task<EventResponseDTO> GetAllEventsByPropertyId(int propertyId);
        Task<TransactionResponseDTO> CreateEvent(CreateEventRequestDTO request);
        Task<TransactionResponseDTO> UpdateEvent(UpdateEventRequestDTO request);
        Task<TransactionResponseDTO> DeleteEvent(int id);
    }

    public class EventsService(IEventsClass eventsClass) : IEvents
    {
        public async Task<EventResponseDTO> GetAllEventsByPropertyId(int propertyId)
        {
            try
            {
                var events = await eventsClass.GetAllEventsByPropertyId(propertyId);
                
                return new EventResponseDTO
                {
                    IsSuccess = true,
                    Message = $"Retrieved {events.Count} event(s)",
                    Events = events
                };
            }
            catch (Exception ex)
            {
                return new EventResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error retrieving events: {ex.Message}",
                    Events = new List<EventDetail>()
                };
            }
        }

        public async Task<TransactionResponseDTO> CreateEvent(CreateEventRequestDTO request)
        {
            // Validate dates
            if (request.EndDate < request.StartDate)
            {
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = "End date cannot be before start date"
                };
            }

            return await eventsClass.CreateEvent(request);
        }

        public async Task<TransactionResponseDTO> UpdateEvent(UpdateEventRequestDTO request)
        {
            // Validate dates
            if (request.EndDate < request.StartDate)
            {
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = "End date cannot be before start date"
                };
            }

            return await eventsClass.UpdateEvent(request);
        }

        public async Task<TransactionResponseDTO> DeleteEvent(int id)
        {
            return await eventsClass.DeleteEvent(id);
        }
    }
}