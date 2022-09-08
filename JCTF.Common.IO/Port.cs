using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTF.Common.IO
{
    public abstract class Port<T>
    {
        private String m_name;
        private Uri m_uri;
        private readonly Object m_commandReceivedEventLock = new Object();
        private readonly Object m_commandSentEventLock = new Object();
        private EventHandler<CommandEventArgs<T>> m_commandReceived;
        private EventHandler<CommandEventArgs<T>> m_commandSent;

        public string Name
        {
            get => m_name;
        }

        public Uri Uri
        {
            get { return m_uri; }
        }

        #region Port Events

        /// <summary>
        /// Occurs when a command is received.
        /// </summary>
        public event EventHandler<CommandEventArgs<T>> CommandReceived
        {
            add
            {
                lock (m_commandReceivedEventLock)
                {
                    m_commandReceived += value;
                }
            }

            remove
            {
                lock (m_commandReceivedEventLock)
                {
                    m_commandReceived -= value;
                }
            }
        }

        /// <summary>
        /// Occurs when a command is sent.
        /// </summary>
        public event EventHandler<CommandEventArgs<T>> CommandSent
        {
            add
            {
                lock (m_commandSentEventLock)
                {
                    m_commandSent += value;
                }
            }

            remove
            {
                lock (m_commandSentEventLock)
                {
                    m_commandSent -= value;
                }
            }
        }
        #endregion

        #region Port Controls
        /// <summary>
        /// Open the port and give it a name.
        /// </summary>
        /// <param name='name'>The name to assign to the port.</param>
        public Port(String name, Uri uri)
        {
            m_name = name;
            m_uri = uri;
        }

        /// <summary>
        /// Open the port.
        /// </summary>
        public abstract void Open();

        /// <summary>
        /// Return if the port is open.
        /// </summary>
        /// <returns>true if open, false otherwise</returns>
        public abstract bool IsOpen();

        /// <summary>
        /// Close the port.
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// Send the specified command.
        /// </summary>
        /// <param name='command'>The command to send.</param>
        public abstract void Send(T cmd);

        /// <summary>
        /// Receive a command.
        /// </summary>
        /// <param name='timeout_ms'>The timeout(ms) to Receive.</param>
        /// <returns>command, if nothing received, null is returned</returns>
        public abstract T Receive(int timeout_ms);

        public abstract List<T> Receive(T cmd, int msTimeout);

        /// <summary>
        /// flush port buffer( in & out) .
        /// </summary>
        public abstract void Flush();
        #endregion

        #region "Port Event publisher"
        // Wrap the event in a protected virtual method
        // to enable derived classes to raise the event

        /// <summary>
        /// Raises the CommandReceived event.
        /// </summary>
        /// <param name='command'>The received command.</param>
        protected virtual void OnCommandReceived(T command)
        {
            CommandEventArgs<T> e = new CommandEventArgs<T>(command);

            EventHandler<CommandEventArgs<T>> handler;
            lock (m_commandReceivedEventLock)
            {
                handler = m_commandReceived;
            }

            // Raise the event in a thread-safe manner using the ?. operator.
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the CommandSent event.
        /// </summary>
        /// <param name='command'>The sent command.</param>
        protected virtual void OnCommandSent(T command)
        {
            CommandEventArgs<T> e = new CommandEventArgs<T>(command);

            EventHandler<CommandEventArgs<T>> handler;
            lock (m_commandSentEventLock)
            {
                handler = m_commandSent;
            }

            // Raise the event in a thread-safe manner using the ?. operator.
            handler?.Invoke(this, e);
        }

        #endregion
    }
}
