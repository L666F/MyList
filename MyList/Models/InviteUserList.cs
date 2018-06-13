using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyList.Models
{
    public class InviteUserList
    {
        public int ID { get; set; }
        public int ListID { get; set; }
        public int InviterID { get; set; }
        public int InvitedID { get; set; }
        public DateTime Date { get; set; }
    }
}
