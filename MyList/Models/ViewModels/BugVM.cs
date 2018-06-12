using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyList.Models.ViewModels
{
    public class BugVM
    {
        [Required]
        [MinLength(5, ErrorMessage = "The title should be at least 5 characters long.")]
        [MaxLength(50, ErrorMessage = "The title should not exceed 50 characters in length.")]
        public string Title { get; set; }
        [Required]
        [MinLength(5, ErrorMessage = "The text should be at least 10 characters long.")]
        [MaxLength(500, ErrorMessage = "The text should not exceed 500 characters in length.")]
        public string Text { get; set; }
    }
}
