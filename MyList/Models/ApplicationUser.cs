using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyList.Models
{
    public class ApplicationUser
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [StringLength(maximumLength:255,MinimumLength=255)]
        public string Username { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        [Required]
        public string FullName { get; set; }
    }
}
