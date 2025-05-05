using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DataSec.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "User Name is required.")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 100 characters.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
        public string Password { get; set; }
    }
}