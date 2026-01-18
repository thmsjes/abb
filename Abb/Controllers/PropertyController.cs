using Abb.Business;
using Abb.Data;
using Azure;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static Abb.DTOs.PropertyDTOs;
using static Abb.DTOs.ReservationDTOs;

namespace Abb.Controllers
{


    [ApiController]
    [Route("api/")]
    [EnableCors]
    public class PropertyController : Controller
    {
        private readonly IProperties _properties;
        private readonly IUsersClass _usersClass;

        public PropertyController(IProperties properties, IUsersClass usersClass)
        {
            _properties = properties;
            _usersClass = usersClass;
        }

        [HttpGet("getAllProperties")]
        public async Task<PropertyResponseDTO> GetAllProperties()
        {
            var property = await _properties.GetAllProperties();
            return await _usersClass.AppendOwner(property);
        }
        [HttpGet("getPropertyById")]
        public async Task<PropertyResponseDTO> GetPropertyById([FromQuery]int propertyId)
        {
            var property= await _properties.GetPropertyById( propertyId);
            return await _usersClass.AppendOwner(property);
        }
        [HttpPost("createProperty")]
        public async Task<PropertyResponseDTO> CreateProperty([FromBody]PropertyDetail property)
        {
            var response = await _properties.CreateProperty(property);
            return await _usersClass.AppendOwner(response);
        }
        [HttpPut("updateProperty")]
        public async Task<PropertyResponseDTO> UpdateProperty([FromBody] PropertyDetail property)
        {
            var response = await _properties.UpdateProperty(property);
            return await _usersClass.AppendOwner(response);
        }
        [HttpDelete("deletePropertyById")]
        public async Task<PropertyResponseDTO> DeletePropertyById([FromQuery]int id)
        {
            return await _properties.DeletePropertyById(id);
        }
    }
}
