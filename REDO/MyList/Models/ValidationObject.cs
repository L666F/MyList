using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyList.Models
{
    public class ValidationObject
    {
        public List<FieldValidation> Fields { get; set; }

        public class FieldValidation
        {
            public string Field { get; set; }
            public List<string> ErrorMessages { get; set; }
        }
    }
}
