using Abb.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Linq;
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
        Task<ReservationForGuestPortalResponseDTO> GetAllReservationByReference(string confirmationNumber);
    }

    public class ReservationsService(IConfiguration configuration, IReservationsClass reservationClass , IUsersClass userClass, IProperties properties ) : IReservations
    {
        

        public async Task<List<GetReservationResponseDTO>> GetAllReservationsByPropertyId(int propertyId)
        {
            return await reservationClass.GetAllReservationsByPropertyId(propertyId);
        }

        private async Task<bool> CheckIfReservationExists(string confirmationNumber)
        {
            GetReservationResponseDTO reservation = await reservationClass.GetReservationByNumber(confirmationNumber);
            if(reservation==null)
            {
                return false;
            }
            else return true;
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
           return await reservationClass.UpdateReservationByNumber(request);
        }

        public async Task<TransactionResponseDTO> DeleteReservation(int id)
        {
            return await reservationClass.DeleteReservation(id);
        }

        public async Task<ReservationForGuestPortalResponseDTO> GetAllReservationByReference(string confirmationNumber)
        {
            ReservationForGuestPortalResponseDTO response = new ReservationForGuestPortalResponseDTO();
            var reservation = await reservationClass.GetReservationByNumber(confirmationNumber);

            if (reservation==null || !reservation.CustomerId.HasValue)
            {
                return new ReservationForGuestPortalResponseDTO();
            }
            response.ConfirmationNumber = reservation.ConfirmationNumber;
            response.CheckInDate = reservation.CheckInDate;
            response.CheckoutDate = reservation.CheckoutDate;
            response.Dogs = reservation.Dogs?"Yes":"No";
            response.GuestCount = reservation.GuestCount.ToString();
            response.LockCode = reservation.LockCode!??"Not assigned yet";

            var customerInfo = await userClass.GetUser((int)reservation.CustomerId);
            response.FirstName = customerInfo.User.FirstName;
            response.LastName = customerInfo.User.LastName;

            var propertyInfo = await properties.GetPropertyById(reservation.PropertyId);

            response.PropertyName = propertyInfo.Property.FirstOrDefault()?.PropertyName;
            response.Address = propertyInfo.Property.FirstOrDefault()?.Address;
            response.City = propertyInfo.Property.FirstOrDefault()?.City;
            response.State = propertyInfo.Property.FirstOrDefault()?.State;
            response.Zip = propertyInfo.Property.FirstOrDefault()?.Zip;

            return response;
        }


    }
}