using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyList.Models
{
    public class ListClass
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public int UserID { get; set; }

        public List<ListItem> ListItems { get; set; }
    }
}
