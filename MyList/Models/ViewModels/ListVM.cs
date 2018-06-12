using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyList.Models.ViewModels
{
    public class ListVM
    {
        [Required(ErrorMessage = "The list name is required.")]
        [MinLength(5, ErrorMessage = "The list name should be at least 5 characters long.")]
        [MaxLength(20, ErrorMessage = "The list name should not exceed 20 characters in length.")]
        public string Name { get; set; }
    }
}
