using Abb.Data;
using static Abb.DTOs.ReservationDTOs;
using static Abb.DTOs.TransactionsDTO;

namespace Abb.Business
{
    public interface IReservations
    {
        Task<List<GetReservationResponseDTO>> GetAllReservationsByPropertyId(int propertyId);
        Task<TransactionResponseDTO> CreateReservation(CreateReservationRequestDTO request);
        Task<TransactionResponseDTO> UpdateReservation(CreateReservationRequestDTO request);
        Task<TransactionResponseDTO> DeleteReservation(int id);
    }
    public class ReservationsService(IConfiguration configuration, IReservationsClass reservationClass) : IReservations
    {

        public async Task<List<GetReservationResponseDTO>> GetAllReservationsByPropertyId(int propertyId)
        {
            return await reservationClass.GetAllReservationsByPropertyId(propertyId);
        }

        private async Task<bool> CheckIfReservationExists(string confirmationNumber)
        {
            GetReservationResponseDTO reservation = await reservationClass.GetReservationByNumber(confirmationNumber);
            if(reservation.Id > 0)
            {
                return true;
            }
            else return false;
        }

        public async Task<TransactionResponseDTO> CreateReservation(CreateReservationRequestDTO request)
        {
            var response = new TransactionResponseDTO();
            bool exists = await CheckIfReservationExists(request.ConfirmationNumber);
            if (exists)
            {
                return await UpdateReservation(request);
            }
            return await reservationClass.CreateReservation(request);
        }

        public async Task<TransactionResponseDTO> UpdateReservation(CreateReservationRequestDTO request)
        {
            //Update reservation
            var response = new TransactionResponseDTO();
            var updateResult= await reservationClass.UpdateReservationByNumber(request);


            //Update User

            return response;
        }

        public async Task<TransactionResponseDTO> DeleteReservation(int id)
        {
            return await reservationClass.DeleteReservation(id);
        }


    }
}