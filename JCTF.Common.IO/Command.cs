using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace JCTF.Common.IO
{
    /// <summary>
    /// Modifiers for a <see cref="Command"/>
    /// </summary>
    [Flags]
    public enum CommandOptions
    {
        /// <summary>
        /// No modifiers apply
        /// </summary>
        None = 0,

        /// <summary>
        /// The <see cref="Command"/> is partial, meaning it may be trailed by zero, one or more arguments that are not specified
        /// </summary>
        Partial = 0x01,
    };

    public class Command : IComparable
    {
        private const string ARGUMENT_SEPARATOR = "=";
        private string m_func = string.Empty;
        private string m_args = string.Empty;
        private CommandOptions options = CommandOptions.None;

        /// <summary>
        /// Wildcard, used to specify an argument within a <see cref="Command"/> that is not specified in value.
        /// </summary>
        public const string Wildcard = "???";


        /// <summary>
        /// Function string
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the function string contains the separator charactor (,)</exception>
        public string Func
        {
            get { return m_func; }
            set
            {
                if (value.Contains(ARGUMENT_SEPARATOR))
                {
                    throw new ArgumentException("The function name may not contain a " + ARGUMENT_SEPARATOR);
                }
                m_func = value;
            }
        }

        /// <summary>
        /// Function parameters
        /// </summary>
        public string Args
        {
            get { return m_args; }
            set
            {
                if (value.Contains(ARGUMENT_SEPARATOR))
                {
                    throw new ArgumentException("The Args may not contain a " + ARGUMENT_SEPARATOR);
                }
                m_args = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        public Command()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="func">The function of the message</param>
        public Command(string func)
            : this(CommandOptions.None, func)
        {
            // do nothing
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="options">Command modifiers</param>
        /// <param name="func">The function of the message</param>
        public Command(CommandOptions options, string func)
        {
            if (func.Contains(ARGUMENT_SEPARATOR))
            {
                throw new ArgumentException("An Function may not contain a " + ARGUMENT_SEPARATOR);
            }

            this.options = options;
            this.m_func = func;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="func">The function of the message</param>
        /// <param name="args">The arguments for the message</param>
        public Command(string func, string args)
            : this(CommandOptions.None, func, args)
        {
            // do nothing
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="options">Command modifiers</param>
        /// <param name="func">The function of the message</param>
        /// <param name="args">The arguments for the message</param>
        public Command(CommandOptions options, string func, string args)
            : this(options, func)
        {

            if (string.IsNullOrEmpty(args)) return;

            if (args == Wildcard)
            {
                this.m_args = Wildcard;
            }
            else
            {
                this.m_args = args;
            }
        }

        /// <summary cref="Equals(object)"></summary>
        public static bool operator ==(Command cmd1, Command cmd2)
        {
            if (object.ReferenceEquals(cmd1, null))
                return object.ReferenceEquals(cmd2, null);
            return cmd1.Equals(cmd2);
        }

        /// <summary cref="Equals(object)"></summary>
        public static bool operator !=(Command cmd1, Command cmd2)
        {
            return !(cmd1 == cmd2);
        }

        /// <summary cref="IComparable.CompareTo"></summary>
        public int CompareTo(object obj)
        {
            int res;

            // throw an exception if the object is not of type Command
            if (!(obj is Command))
            {
                throw new ArgumentException();
            }

            Command cmd = obj as Command;
            if (obj == null)
                return 1;

            res = String.CompareOrdinal(this.Func, cmd.Func);
            if (res != 0)
            {
                return res;
            }

            if ((this.Args == Wildcard) && (cmd.Args != Wildcard))
            {
                return -1;
            }

            if ((this.Args != Wildcard) && (cmd.Args == Wildcard))
            {
                return 1;
            }

            res = String.CompareOrdinal(this.Args, cmd.Args);
            if (res != 0)
            {
                return res;
            }

            return 0;
        }

        /// <summary>
        /// Generates a string representation of the command
        /// </summary>
        /// <returns>String containing representation of command</returns>
        public override string ToString()
        {
            string str = Func;

            if (String.Equals(Args, Wildcard))
            {
                str += ARGUMENT_SEPARATOR + "(*)";
            }
            else
            {
                if (!string.IsNullOrEmpty(this.Args))
                {
                    str += ARGUMENT_SEPARATOR + "(" + Args;

                    if ((options & CommandOptions.Partial) == CommandOptions.Partial)
                    {
                        str += "...";
                    }

                    str += ")";
                }
            }
            str += "\r\n";

            return str;
        }

        /// <summary cref="Object.Equals(object)"></summary>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
                return true;

            Command cmd = obj as Command;
            if (cmd == null)
                return false;

            if (this.Func != cmd.Func)
            {
                return false;
            }


            if (((this.options & CommandOptions.Partial) != CommandOptions.Partial) &&
                    ((cmd.options & CommandOptions.Partial) != CommandOptions.Partial))
            {
                if ((this.Args != Wildcard) && (cmd.Args != Wildcard))
                {
                    return String.Equals(this.Args, cmd.Args);
                }
                else
                {
                    return true; //if wildcard, skip compare
                }
            }
            else
            {
                return true; //we skip args check for now
            }
        }

        /// <summary cref="Object.GetHashCode()"></summary>
        public override int GetHashCode()
        {
            return this.Func.GetHashCode() ^ this.Args.GetHashCode();
        }

    }
}
