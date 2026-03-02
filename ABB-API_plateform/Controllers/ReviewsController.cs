using Abb.Business;
using ABB_API_plateform.Business;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static Abb.DTOs.ReviewDTOs;

namespace ABB_API_plateform.Controllers
{
    [ApiController]
    [Route("api/")]
    [EnableCors]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviews _reviews;
        private readonly ILogging _logging;

        public ReviewsController(IReviews reviews, ILogging logging)
        {
            _reviews = reviews;
            _logging = logging;
        }

        /// <summary>
        /// Get all reviews for a specific property
        /// </summary>
        [HttpGet("getReviewsByPropertyId/{propertyId}")]
        public async Task<IActionResult> GetReviewsByPropertyId(int propertyId)
        {
            try
            {
                _logging.LogToFile($"GetReviewsByPropertyId called - PropertyId: {propertyId}");
                
                var result = await _reviews.GetReviewsByPropertyId(propertyId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CONTROLLER EXCEPTION in GetReviewsByPropertyId: {ex.Message}");
                return StatusCode(500, new ReviewResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Server error: {ex.Message}",
                    Reviews = new List<ReviewDetail>()
                });
            }
        }

        [HttpPost("createReview")]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequestDTO request)
        {
            try
            {
                _logging.LogToFile($"CreateReview called - PropertyId: {request?.PropertyId}, Score: {request?.Score}");

                if (request == null)
                {
                    _logging.LogToFile("CreateReview - Request is NULL");
                    return BadRequest(new TransactionResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid request body"
                    });
                }

                var result = await _reviews.CreateReview(request);
                
                _logging.LogToFile($"CreateReview completed - Success: {result.IsSuccess}");

                if (result.IsSuccess)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CONTROLLER EXCEPTION in CreateReview: {ex.Message}");
                return StatusCode(500, new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Server error: {ex.Message}"
                });
            }
        }

        // ⭐ Updated: Id now comes from request body
        [HttpPut("updateReview")]
        public async Task<IActionResult> UpdateReview([FromBody] UpdateReviewRequestDTO request)
        {
            try
            {
                _logging.LogToFile($"UpdateReview called - Id: {request?.Id}");

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
                        Message = "Invalid review ID"
                    });
                }

                var result = await _reviews.UpdateReview(request);
                
                _logging.LogToFile($"UpdateReview completed - Success: {result.IsSuccess}");

                if (result.IsSuccess)
                    return Ok(result);
                else
                    return NotFound(result);
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CONTROLLER EXCEPTION in UpdateReview: {ex.Message}");
                return StatusCode(500, new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Server error: {ex.Message}"
                });
            }
        }

        [HttpDelete("deleteReview/{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                _logging.LogToFile($"DeleteReview called - Id: {id}");

                var result = await _reviews.DeleteReview(id);
                
                _logging.LogToFile($"DeleteReview completed - Success: {result.IsSuccess}");

                if (result.IsSuccess)
                    return Ok(result);
                else
                    return NotFound(result);
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"CONTROLLER EXCEPTION in DeleteReview: {ex.Message}");
                return StatusCode(500, new TransactionResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Server error: {ex.Message}"
                });
            }
        }
    }
}