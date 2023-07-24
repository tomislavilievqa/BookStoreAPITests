using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomatedWebAPITests.Models;

namespace AutomatedWebAPITests.TestHelpers
{
    public class NewBook
    {
        public string userId { get; set; }
        public List<Books> collectionOfIsbns { get; set; }
        public string isbn { get; set; }
    }
}
