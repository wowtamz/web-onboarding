//-------------------------
// Author: Michael Adolf
//-------------------------

using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace SoPro24Team06.Models
{
    public class ApplicationUser : IdentityUser
    {
        [JsonProperty("fullName")]
        public string? FullName { get; set; }
    }
}
