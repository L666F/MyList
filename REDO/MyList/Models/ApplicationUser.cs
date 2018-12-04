using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyList.Models
{
    public class ApplicationUser
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool EmailVerified { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime RegistrationDatetime { get; set; }
    }
}
