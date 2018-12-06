using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyList.Models.VMs
{
    public class AddListItemVM
    {
        public int? ListID { get; set; }
        public string Title { get; set; }
        public string Note { get; set; }
        public bool? Checked { get; set; }
    }
}
