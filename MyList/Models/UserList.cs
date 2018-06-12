using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyList.Models
{
    public class UserList
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public int UserID { get; set; }
        [Required]
        public int ListID { get; set; }
    }
}
