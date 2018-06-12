using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyList.Models.ViewModels
{
    public class ProductVM
    {
        [Required]
        public List<MyProduct> Products { get; set; }

        public class MyProduct
        {
            [Required]
            public int ID { get; set; }
            [MaxLength(50, ErrorMessage = "The note should not exceed 50 characters in length.")]
            public string Note { get; set; }
        }
    }
}
