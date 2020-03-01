using System;
using System.Net;
using Frogvall.AspNetCore.ExceptionHandling.Attributes;
using Frogvall.AspNetCore.ExceptionHandling.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Frogvall.AspNetCore.ExceptionHandling.Test.TestResources
{
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        [HttpPost]
        public IActionResult PostTest([FromBody] TestDto testDto)
        {
            if (testDto.NonNullableObject < 0)
            {
                var zero = 0;
                var provokeException = 1 / zero;
            }

            if (testDto.NonNullableObject > 3)
                throw new TestException3(TestEnum.MyThirdValue, "Object > 4", 
                    new TestContext {TestValue = "Test1"},
                    new TestDeveloperContext {TestValue = "Test1"});
            if (testDto.NonNullableObject > 2)
                throw new TestException("Object > 2", 
                    new TestContext {TestValue = "Test1"}, 
                    new TestDeveloperContext {TestValue = "Test1"});
            if (testDto.NonNullableObject > 1)
                throw new TestException2("Object > 1", 
                    new TestContext {TestValue = "Test2"}, 
                    new TestDeveloperContext {TestValue = "Test2"});
            return Ok();
        }

        [HttpPost("NoExceptionNo20x")]
        [SkipModelValidationFilter]
        public IActionResult PostTestNoExceptionNo20x([FromBody] TestDto testDto)
        {
            return BadRequest("Returning 400 without ApiError syntax");
        }

        [HttpPost("NoValidation")]
        [SkipModelValidationFilter]
        public IActionResult PostTestNoValidation([FromBody] TestDto testDto)
        {
            return Ok();
        }

        [HttpGet("Cancellation")]
        public IActionResult Cancellation()
        {
            throw new OperationCanceledException();
        }
    }
}
