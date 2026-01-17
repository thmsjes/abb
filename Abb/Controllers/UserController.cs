using Abb.Business;
using Abb.Data;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Abb.DTOs.UserDTOs;
using static Abb.DTOs.PropertyDTOs;

namespace Abb.Controllers
{
    [ApiController]
    [Route("api/")]
    [EnableCors]
    public class UserController : ControllerBase
    {
        private readonly IUsersClass userClass;

        public UserController(IUsersClass _userClass)
        {
            _userClass = userClass;
        }

        // GET: UserController/AllUsers
        [HttpGet("AllUserByPropertyId")]
        public async Task<List<UserDetail>> GetAllUsers()
        {
           return await userClass.GetAllUsers();
        }
        // GET: UserController/User
        [HttpGet("User")]
        public async Task<UserResponseDTO> GetUser([FromQuery] int id)
        {
            return await userClass.GetUser(id);
        }

        // Put: UserController/User
        [HttpPut("User")]
        public async Task<UserResponseDTO> UpdateUser(UserDetail request)
        {
            return await userClass.UpdateUser(request);
        }

        // Delete: UserController/User
        [HttpDelete("User")]
        public async Task<UserResponseDTO> DeleteUser(int id)
        {
            return await userClass.DeleteUser(id);
        }

        [HttpGet("owner")]
        public async Task<PropertyOwner> GetOwner(int id)
        {
            return await userClass.GetPropertyOwner(id);
        }

    }
}