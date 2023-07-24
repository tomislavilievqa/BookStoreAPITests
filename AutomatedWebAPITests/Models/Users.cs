using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedWebAPITests.Models
{
    public class Users
    { 

        public string userID { get; set; } 
        public string username { get; set; } 
        public List<Books> books { get; set; }

    }
}
