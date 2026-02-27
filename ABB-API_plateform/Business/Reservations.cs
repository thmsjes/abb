using Abb.Data;
using static Abb.DTOs.ReservationDTOs;
using static Abb.DTOs.TransactionsDTO;

namespace Abb.Business
{
    public interface IReservations
    {
        Task<List<GetReservationResponseDTO>> GetAllReservationsByPropertyId(int propertyId);
        Task<GetReservationResponseDTO> GetReservationByNumber(string reservationNumber);
        Task<TransactionResponseDTO> CreateReservation(CreateReservationRequestDTO request);
        Task<TransactionResponseDTO> UpdateReservation(CreateReservationRequestDTO request);
        Task<TransactionResponseDTO> DeleteReservation(int id);
    }
    
    public class ReservationsService(IReservationsClass reservationClass) : IReservations  // ⭐ Removed unused configuration parameter
    {
        public async Task<List<GetReservationResponseDTO>> GetAllReservationsByPropertyId(int propertyId)
        {
            return await reservationClass.GetAllReservationsByPropertyId(propertyId);
        }
        public async Task<GetReservationResponseDTO> GetReservationByNumber(string reservationNumber)
        {
            return await reservationClass.GetReservationByNumber(reservationNumber);
        }
        public async Task<bool> CheckIfReservationExists(string confirmationNumber)
        {
            GetReservationResponseDTO? reservation = await reservationClass.GetReservationByNumber(confirmationNumber);
            
            if (reservation != null && reservation.Id > 0)
            {
                return true;
            }
            
            return false;
        }

        public async Task<TransactionResponseDTO> CreateReservation(CreateReservationRequestDTO request)
        {
            bool exists = await CheckIfReservationExists(request.ConfirmationNumber);
            
            if (exists)
            {
                return await UpdateReservation(request);
            }
            
            return await reservationClass.CreateReservation(request);
        }

        public async Task<TransactionResponseDTO> UpdateReservation(CreateReservationRequestDTO request)
        {
            // Update reservation
            var updateResult = await reservationClass.UpdateReservationByNumber(request);

            // TODO: Update User if needed

            return updateResult;  // ⭐ Return actual result
        }

        public async Task<TransactionResponseDTO> DeleteReservation(int id)
        {
            return await reservationClass.DeleteReservation(id);
        }
    }
}