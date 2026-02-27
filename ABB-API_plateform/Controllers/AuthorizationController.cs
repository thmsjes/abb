using Abb.Business;
using ABB_API_plateform.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static Abb.DTOs.AuthDtos;

namespace ABB_API_plateform.Controllers
{
    [ApiController]
    [Route("api/")]
    [EnableCors]

    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthentication _authentication; 
        private readonly ILogging _logging;

        public AuthenticationController(IAuthentication authentication, ILogging logging)
        {
            _authentication = authentication;
            _logging = logging;
        }

        [HttpPost("register")]
        public async Task<RegistrationResponseDTO> Register([FromBody] RegisterRequest request)
        {
            try
            {
                RegistrationResponseDTO response = new RegistrationResponseDTO();
                _logging.LogToFile($"Registering user: {request.Email}");
                int id =  await _authentication.RegisterUser(request);
                response.Id = id;
                if (id < 1)
                {
                    response.IsSuccess = false;
                }
                else 
                {
                    response.IsSuccess = true; 
                }
                    return response;
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"Error registering user: {ex.Message}");
                throw;
            }
        }

        [HttpPost("login")]
        public async Task<LoginResponseDTO> Login([FromBody] LoginRequestDTO request)
        {
            return await _authentication.LoginUser(request);
        }


    }
}