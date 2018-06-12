using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyList.Models
{
    public class ListProduct
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public int ListID { get; set; }
        [Required]
        public int ProductID { get; set; }
        [Required]
        public string Note { get; set; }
    }
}
