using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyList.Models
{
    public class EmailVerification
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public DateTime Sent { get; set; }
        public string RandomString { get; set; }
        public bool Verified { get; set; }
        public DateTime VerifiedOn { get; set; }
    }
}
