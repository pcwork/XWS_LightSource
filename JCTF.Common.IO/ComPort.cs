using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace JCTF.Common.IO
{
    public class ComPort : Port<Command>
    {
        private const string _port_NEWLINE_CHAR = "\r\n";
        private const int _inQueue_capacity = 50;
        private readonly System.IO.Ports.SerialPort _serialPort = new System.IO.Ports.SerialPort();
        private bool _closing = false;
        private static readonly object _queueLock = new object();
        private static readonly Queue<Command> _inQueue = new Queue<Command>(_inQueue_capacity);
        private string _bufCommand = "";

        #region ComPort constructor
        public ComPort(String name, Uri uri, int baudRate)
            : base(name, uri)
        {
            _serialPort.BaudRate = baudRate;
            _serialPort.NewLine = _port_NEWLINE_CHAR;
            _serialPort.PortName = uri.AbsolutePath;
            //serialPort.DtrEnable = true;
        }

        public ComPort(String name, Uri uri, int baudRate, int dataBits, StopBits stopBits, Parity parity)
            : this(name, uri, baudRate)
        {
            _serialPort.DataBits = dataBits;
            _serialPort.StopBits = stopBits;
            _serialPort.Parity = parity;
        }
        #endregion

        #region ComPort method
        public override void Open()
        {
            _serialPort.Open();
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
            _closing = false;
            _serialPort.DataReceived += DataReceivedEvent;
            this.CommandReceived += CommandReceivedEvent;
        }

        public override bool IsOpen()
        {
            return _serialPort.IsOpen;
        }

        public override void Close()
        {
            this.CommandReceived -= CommandReceivedEvent;
            _serialPort.DataReceived -= DataReceivedEvent;
            Monitor.Enter(_serialPort);
            _closing = true;
            Monitor.Exit(_serialPort);

            _serialPort.Close();
        }

        public override void Send(Command command)
        {
            Send(command.ToString());
            OnCommandSent(command);
        }

        //wait for a command to be received until defined timeout(in millisecond)
        public override Command Receive(int msTimeout)
        {
            Command command = null;

            lock (_queueLock)
            {
                if (_inQueue.Count == 0)
                {
                    Monitor.Wait(_queueLock, msTimeout);
                }
                if (_inQueue.Count > 0)
                {
                    command = _inQueue.Dequeue();
                }
            }

            return command;
        }

        public override List<Command> Receive(Command command, int msTimeout)
        {
            //Command command = null;
            List<Command> receivedCommands = new List<Command>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            bool found = false;
            int maxWaitTime;
            while (!found)
            {
                lock (_queueLock)
                {
                    found = _inQueue.Contains(command);
                    if (found)
                    {
                        while (_inQueue.Count > 0)
                        {
                            var inCmd = _inQueue.Dequeue();
                            receivedCommands.Add(inCmd);
                            if (inCmd == command)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        maxWaitTime = (int)(msTimeout - stopwatch.ElapsedMilliseconds);
                        if (maxWaitTime > 0)
                        {
                            Monitor.Wait(_queueLock, maxWaitTime);
                        }
                        else 
                        {
                            string errorString = string.Format("WaitForCommand({0}, {1}ms) timed out.", command.Func, msTimeout);
                            throw new TimeoutException(errorString);
                        }
                    }
                }
            }

            return receivedCommands;
        }

        public override void Flush()
        {
            lock (_queueLock)
            {
                _inQueue.Clear();
            }
            Monitor.Enter(_serialPort);
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
            Monitor.Exit(_serialPort);
        }

        public void Send(string data)
        {
            byte[] buf = new System.Text.ASCIIEncoding().GetBytes(data);
            _serialPort.Write(buf, 0, buf.Length);
        }

        #endregion

        #region ComPort Private event handler
        private void DataReceivedEvent(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            String s = "";
            Monitor.Enter(_serialPort);

            if (!_closing)
            {
                s = _serialPort.ReadExisting();
            }

            Monitor.Exit(_serialPort);

            if (s.Length > 0)
            {
                _bufCommand += s;

                // see if a command is done
                int idxEol;
                while ((idxEol = _bufCommand.IndexOf(_port_NEWLINE_CHAR)) != -1)
                {
                    string line = _bufCommand.Substring(0, idxEol);
                    _bufCommand = _bufCommand.Substring(idxEol + _port_NEWLINE_CHAR.Length);

                    line = line.Trim();
                    if (line.Length > 0)
                    {
                        Command cmd = new Command();
                        if (line.Contains("="))
                        {
                            cmd.Func = line.Split('=')[0];
                            cmd.Args = line.Split('=')[1];
                        }
                        else
                        {
                            cmd.Func = line;
                            cmd.Args = string.Empty;
                        }
                        OnCommandReceived(cmd);
                    }
                }
            }
        }

        private void CommandReceivedEvent(object sender, CommandEventArgs<Command> e)
        {
            lock (_queueLock)
            {
                if (_inQueue.Count == _inQueue_capacity)
                    _inQueue.Dequeue();
                _inQueue.Enqueue(e.Command);
                Monitor.Pulse(_queueLock);
            }
        }
        #endregion
    }
}
