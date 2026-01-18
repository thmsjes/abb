using Abb.Business;
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

        public AuthenticationController(IAuthentication authentication)
        {
            _authentication = authentication;
        }
       
        [HttpPost("register")] 
        public async Task<bool> Register([FromBody] RegisterRequest request)
        {
            return await _authentication.RegisterUser(request);
        }

        [HttpPost("login")]
        public async Task<LoginResponseDTO> Login([FromBody] LoginRequestDTO request)
        {
            return await _authentication.LoginUser(request);
        }


    }
}