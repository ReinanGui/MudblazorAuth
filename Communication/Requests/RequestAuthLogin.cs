using System.ComponentModel.DataAnnotations;

namespace Communication.Requests
{
    public class RequestAuthLogin
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
