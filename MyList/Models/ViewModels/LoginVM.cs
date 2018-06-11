using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyList.Models.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "The username field cannot be empty.")]
        public string Username { get; set; }
        [Required(ErrorMessage = "The password field cannot be empty.")]
        public string Password { get; set; }
    }
}
