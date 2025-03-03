using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExitException
{
    public class ThrowExitException : Exception
    {
        public ThrowExitException() : base("Returning to main menu.") { }
    }
}
