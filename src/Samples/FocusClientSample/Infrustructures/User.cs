using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FocusClientSample.Infrustructures
{
    public class User : Entity
    {
        public string EmplID { get; set; }

        public string Username { get; set; }

        public string TecType { get; set; }
    }
}
