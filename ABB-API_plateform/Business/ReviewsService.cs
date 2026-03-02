using Abb.Data;
using static Abb.DTOs.ReviewDTOs;

namespace Abb.Business
{
    public interface IReviews
    {
        Task<ReviewResponseDTO> GetReviewsByPropertyId(int propertyId);
        Task<TransactionResponseDTO> CreateReview(CreateReviewRequestDTO request);
        Task<TransactionResponseDTO> UpdateReview(UpdateReviewRequestDTO request);  // ⭐ Changed parameter
        Task<TransactionResponseDTO> DeleteReview(int id);
    }

    public class ReviewsService(IReviewsClass reviewsClass) : IReviews
    {
        public async Task<ReviewResponseDTO> GetReviewsByPropertyId(int propertyId)
        {
            try
            {
                var reviews = await reviewsClass.GetReviewsByPropertyId(propertyId);
                
                return new ReviewResponseDTO
                {
                    IsSuccess = true,
                    Message = $"Retrieved {reviews.Count} review(s)",
                    Reviews = reviews
                };
            }
            catch (Exception ex)
            {
                return new ReviewResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error retrieving reviews: {ex.Message}",
                    Reviews = new List<ReviewDetail>()
                };
            }
        }

        public async Task<TransactionResponseDTO> CreateReview(CreateReviewRequestDTO request)
        {
            // Validate score is in valid range
            if (request.Score < 1 || request.Score > 5)
            {
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = "Score must be between 1 and 5"
                };
            }

            return await reviewsClass.CreateReview(request);
        }

        // ⭐ Updated to accept UpdateReviewRequestDTO
        public async Task<TransactionResponseDTO> UpdateReview(UpdateReviewRequestDTO request)
        {
            // Validate score
            if (request.Score < 1 || request.Score > 5)
            {
                return new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = "Score must be between 1 and 5"
                };
            }

            return await reviewsClass.UpdateReview(request);
        }

        public async Task<TransactionResponseDTO> DeleteReview(int id)
        {
            return await reviewsClass.DeleteReview(id);
        }
    }
}