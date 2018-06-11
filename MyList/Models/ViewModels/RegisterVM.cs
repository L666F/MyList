using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyList.Models.ViewModels
{
    public class RegisterVM
    {
        [Required]
        [MinLength(5, ErrorMessage = "The username has to contain at least 5 characters.")]
        [MaxLength(20, ErrorMessage = "The username should not exceed 20 characters in length.")]
        public string Username { get; set; }
        [Required]
        [MinLength(8, ErrorMessage = "The password has to contain at least 8 characters.")]
        [MaxLength(50, ErrorMessage = "The password should not exceed 50 characters in length.")]
        public string Password { get; set; }
        [Required]
        [MinLength(8,ErrorMessage = "The full name has to contain at least 8 characters.")]
        [MaxLength(50, ErrorMessage = "The full name should not exceed 50 characters in length.")]
        public string FullName { get; set; }
    }
}
