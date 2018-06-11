using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyList.Models.ViewModels
{
    public class ChangePasswordVM
    {
        [Required(ErrorMessage = "The old password is required.")]
        public string PreviousPassword { get; set; }
        [Required(ErrorMessage = "The new password is required.")]
        [MinLength(8, ErrorMessage = "The password has to contain at least 8 characters.")]
        [MaxLength(50, ErrorMessage = "The password should not exceed 50 characters in length.")]
        public string NewPassword { get; set; }
        public string Token { get; set; }
    }
}
