using Abb.Business;
using Abb.Data;
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

        public PropertyController(IProperties properties)
        {
            _properties = properties;
        }

        [HttpGet("getAllProperties")]
        public async Task<PropertyResponseDTO> GetAllProperties()
        {
            return await _properties.GetAllProperties();
        }
        [HttpGet("getPropertyById")]
        public async Task<PropertyResponseDTO> GetPropertyById([FromQuery]int propertyId)
        {
            return await _properties.GetPropertyById( propertyId);
        }
        [HttpPost("createProperty")]
        public async Task<PropertyResponseDTO> CreateProperty([FromBody]PropertyDetail property)
        {
            return await _properties.CreateProperty(property);
        }
        [HttpPut("updateProperty")]
        public async Task<PropertyResponseDTO> UpdateProperty([FromBody] PropertyDetail property)
        {
            return await _properties.CreateProperty(property);
        }
        [HttpDelete("deletePropertyById")]
        public async Task<PropertyResponseDTO> DeletePropertyById([FromQuery]int id)
        {
            return await _properties.DeletePropertyById(id);
        }
    }
}
