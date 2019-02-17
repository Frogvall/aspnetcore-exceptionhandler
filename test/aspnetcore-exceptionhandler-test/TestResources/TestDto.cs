using System.ComponentModel.DataAnnotations;
using Frogvall.AspNetCore.ExceptionHandling.Attributes;

namespace Frogvall.AspNetCore.ExceptionHandling.Test.TestResources
{
    public class TestDto
    {
        [Required]
        public string NullableObject { get; set; }
        [RequireNonDefault]
        public int NonNullableObject { get; set; }
    }
}
