using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyList.Models.ViewModels
{
    public class RemoveProductVM
    {
        [Required(ErrorMessage = "The list of products to remove is required")]
        public List<int> IDs { get; set; }
    }
}
