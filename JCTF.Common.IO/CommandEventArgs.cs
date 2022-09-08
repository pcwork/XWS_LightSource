using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTF.Common.IO
{
    public class CommandEventArgs<T> : EventArgs
    {
        private T command;

        public T Command
        {
            get { return command; }
        }

        public CommandEventArgs(T command)
        {
            this.command = command;
        }
    }
}
