using System;
using Microsoft.AspNetCore.Mvc;

namespace Frogvall.AspNetCore.ExceptionHandling.Test.TestResources
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestApiController : Controller
    {
        [HttpPost]
        public IActionResult PostApiTest([FromBody] TestDto testDto)
        {
            throw new NotImplementedException();
        }
    }
}
