using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coder
{
    class ClientCreationFailException : Exception
    {
        public string Name { get; }
        public ClientCreationFailException(string name, string message) : base(message)
        {
            Name = name;
        }
    }
}
