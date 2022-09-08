using JCTF.Common.IO;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

[assembly: InternalsVisibleTo("SPM.Lighting.Photonics.UnitTest")]

namespace SPM.Lighting.Photonics
{
    /// Allows to know error flags if STATUS == 4
    /// Flags’ packed in 20-bit wide status word

    [Flags]
    public enum ErrorCode
    {
        /// <summary>
        /// No modifiers apply
        /// </summary>
        //NONE = 0x00000,

        /// <summary>
        /// Bit 00 – TEC over current channel #1
        /// </summary>
        TEC_CH1_OC = 0x00001,

        /// <summary>
        /// Bit 01 – TEC over current channel #2
        /// </summary>
        TEC_CH2_OC = 0x00002,

        /// <summary>
        /// Bit 02 – Abnormal system voltage 
        /// </summary>
        ABNOR_SYS_VOL = 0x00004,

        /// <summary>
        /// Bit 03 – TEC temperature sensor shorted
        /// </summary>
        TEC_TEMP_SHORTED = 0x00008,

        /// <summary>
        /// Bit 04 – TEC temperature sensor open
        /// </summary>
        TEC_TEMP_OPEN = 0x00010,

        /// <summary>
        /// Bit 05 – TEC FAN1 fail
        /// </summary>
        TEC_FAN1_FAIL = 0x00020,

        /// <summary>
        /// Bit 06 – TEC FAN2 fail
        /// </summary>
        TEC_FAN2_FAIL = 0x00040,

        /// <summary>
        /// Bit 07 – LASER fail(setpoint not reached)
        /// </summary>
        LASER_FAIL = 0x00080,

        /// <summary>
        /// Bit 08 – HDC fail(maybe head interface cable’s not attached)
        /// </summary>
        HDC_FAIL = 0x00100,

        /// <summary>
        /// Bit 09 – TEC link fail
        /// </summary>
        TEC_LINK_FAIL = 0x00200,

        /// <summary>
        /// Bit 10 – LASER link fail
        /// </summary>
        LASER_LINK_FAIL = 0x00400,

        /// <summary>
        /// Bit 11 – HDC link fail
        /// </summary>
        HDC_LINK_FAIL = 0x00800,

        /// <summary>
        /// Bit 12 – Plasma start fail(photodiode feedback)
        /// </summary>
        PLASMA_START_FAIL = 0x01000,

        /// <summary>
        /// Bit 13 – Plasma down(while in active mode)
        /// </summary>
        PLASMA_DOWN = 0x02000,

        /// <summary>
        /// Bit 14 – LASER overheat(> 35 *C)
        /// </summary>
        LASER_OVERHEAT = 0x04000,

        /// <summary>
        /// Bit 15 – Optical head overheat(> 80 *C)
        /// </summary>
        OPTICAL_HEAD_OVERHEAT = 0x08000,

        /// <summary>
        /// Bit 16 – LASER start fail(photodiode feedback)
        /// </summary>
        LASER_START_FAIL = 0x10000,

        /// <summary>
        /// Bit 17 – firmware error
        /// </summary>
        FW_ERROR = 0x20000,

        /// <summary>
        /// Bit 18 – hardware error
        /// </summary>
        HW_ERROR = 0x40000,

        /// <summary>
        /// Bit 19 – unknown error
        /// </summary>
        UNK_ERROR = 0x80000,

        /// <summary>
        /// Bit 23 – offline
        /// </summary>
        OFFLINE = 0x800000,

        /// <summary>
        /// END
        /// </summary>
        END,
    };

    public abstract class Xws_device : ILightingModule
    {
        /// <summary>
        /// STATUS
        /// 0 – IDLE(ready for operations)
        /// 1 – STARTING(Laser is starting)
        /// 2 – IGNITION(Plasma is triggered)
        /// 3 – PLASMA ON(Plasma is turned ON)
        /// 4 – ERROR
        /// </summary>
        public enum STATUS
        {
            IDLE = 0,
            STARTING,
            IGNITION,
            PLASMA_ON,
            ERROR
        }

        internal class SerialPortConfig
        {
            public string portName { set; get; }
            public int baudRate { set; get; }
            public int dataBits { set; get; }
            public StopBits stopBits { set; get; }
            public Parity parity { set; get; }

            public SerialPortConfig()
            {
                portName = "None";
                baudRate = 115200;
                dataBits = 8;
                stopBits = StopBits.One;
                parity = Parity.None;
            }
        }

        private const int DefaultResponseLength = 20;

        private const int DefaultWaitMs = 5000;

        private readonly IBaseSerialPort _serialPort = null;
        private readonly Port<Command> _comPort = null;


        private ILog _log;
        private ErrorCode _errorCode;

        protected Xws_device(IBaseSerialPort serialPort)
        {
            _serialPort = serialPort;
            ResponseLength = DefaultResponseLength;
            WaitTime = DefaultWaitMs;
        }

        protected Xws_device(Port<Command> comPort)
        {
            _comPort = comPort;
            WaitTime = DefaultWaitMs;
        }
        #region property

        public int ResponseLength {
            get;
            set;
        }

        public int WaitTime {
            get;
            set;
        }

        public abstract string Id { get; }

        #endregion property

        public void SetLogger(ILog log) => _log = log;

        #region ILightingModule

        public void Open()
        {
            if (_serialPort != null)
            {
                if (!_serialPort.IsOpen()) throw new DeviceException("Communication closed");
                _serialPort.Send("TURN_ON\r\n");
                Thread.Sleep(WaitTime);
                var response = GetRespond(ResponseLength);
                if (string.IsNullOrEmpty(response.Trim()))
                    throw new DeviceException("No response received");

                if (response.Split('=')[1] != "OK")
                    throw new DeviceException("Device response is not ok");

            }
            else
            {
                var cmd_req = new Command(CommandOptions.None, "TURN_ON");
                var exp_cmd = new Command();
                SendAndWaitForCommand(cmd_req, exp_cmd, WaitTime);
            }
        }

        public void Close()
        {
            if (_serialPort != null)
            {
                if (!_serialPort.IsOpen()) throw new DeviceException("Communication closed");

                _serialPort.Send("TURN_OFF\r\n");
                var response = GetRespond(ResponseLength);

                if (string.IsNullOrEmpty(response.Trim()))
                    throw new DeviceException("No response received");

                if (response.Split('=')[1] != "OK")
                    throw new DeviceException("Device response is not ok");
            }
            else
            {
                var cmd_req = new Command(CommandOptions.None, "TURN_OFF");
                var exp_cmd = new Command();
                SendAndWaitForCommand(cmd_req, exp_cmd, WaitTime);
            }
        }

        public void SetBrightness(int brightness)
        {
            throw new NotSupportedException();
        }

        public int GetBrightness()
        {
            if (_serialPort != null)
            {
                if (!_serialPort.IsOpen()) throw new DeviceException("Communication closed");

                _log?.Info($"[{this.Id}] GetBrightness --->>>");

                var command = "LASER_CUR\r\n";
                _log?.Debug($"[{this.Id}] Command:{command}");
                int brightness = 0;
                try
                {
                    _serialPort.Send(command);

                    Thread.Sleep(WaitTime);
                    var response = GetRespond(ResponseLength);

                    if (string.IsNullOrEmpty(response.Trim()))
                        throw new DeviceException("No response received");

                    _log?.Info($"[{this.Id}] Data:{response}");

                    brightness = (int)Convert.ToDouble(response.Split('=')[1]);
                }
                catch (Exception e) {
                    _log?.Error($"[{this.Id}] {e.Message}");
                    throw new DeviceException(e.Message, DPPMExceptionCode.ERROR_EXECUTE_COMMAND);
                }

                _log?.Info($"[{this.Id}] GetBrightness(Brightness:{brightness}) --->>>");

                return brightness;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// the format of the connection string is
        /// portName,baudRate,dataBits,stopBits,parity
        /// </summary>
        /// <param name="connection_string"></param>
        public void ConnectServer(string connection_string)
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen()) return;

                var config = ParseConnectionString(connection_string);
                if (config == null)
                {
                    throw new DeviceException($"connection string format incorrect");
                }

                _serialPort.Config(config.portName, config.baudRate, config.dataBits, config.stopBits, config.parity);

                _serialPort.SPOpen();
            }
            else
            {
                if (!_comPort.IsOpen())
                {
                    _comPort.Open();
                }
            }
        }

        public void DisconnectServer()
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen())
                    _serialPort.SPClose();
            }
            else
            {
                if (_comPort.IsOpen())
                {
                    _comPort.Close();
                }
            }
        }

        /// <summary>
        /// The response of the up time is UPTIME=xxx hours xx minutes
        /// </summary>
        /// <returns></returns>
        public double GetUpTime() 
        {
            if (_serialPort != null)
            {
                if (!_serialPort.IsOpen()) throw new DeviceException("Communication closed");

                _serialPort.Send("UPTIME\r\n");
                Thread.Sleep(WaitTime);
                var response = GetRespond(ResponseLength);
                if (string.IsNullOrEmpty(response.Trim()))
                    throw new DeviceException("No response received");

                try
                {
                    var text = response.Split('=')[1];
                    var elems = text.Split(' ');
                    var hours = elems[0];
                    var minutes = elems[2];

                    return Convert.ToDouble(hours) + Convert.ToDouble(minutes) / 60;
                }
                catch (Exception ex)
                {
                    throw new DeviceException($"UPTIME can't be retrieved, the response was [{response}]", ex);
                }
            }
            else
            {
                var cmd_req = new Command(CommandOptions.None, "UPTIME");
                var exp_cmd = new Command(CommandOptions.Partial, "UPTIME");
                var last_cmd = SendAndWaitForCommand(cmd_req, exp_cmd, WaitTime);
                var elems = last_cmd.Args.Split(' ');
                var hours = elems[0];
                var minutes = elems[2];
                return Convert.ToDouble(hours) + Convert.ToDouble(minutes) / 60;
            }
        }


        public double GetTemperature(TemperatureTarget target)
        {
            if (_serialPort != null)
            {
                if (!_serialPort.IsOpen()) throw new DeviceException("Communication closed");

                switch (target)
                {
                    case TemperatureTarget.Head:
                        _serialPort.Send("HEAD_TEMP");
                        break;

                    case TemperatureTarget.Laser:
                        _serialPort.Send("LASER_TEMP");
                        break;

                    default:
                        throw new NotSupportedException($"{target.ToString()} is not supported");
                }

                Thread.Sleep(WaitTime);
                var response = GetRespond(ResponseLength);
                if (string.IsNullOrEmpty(response.Trim()))
                    throw new DeviceException("No response received");
                return Convert.ToDouble(response.Split('=')[1]);
            }
            else
            {
                Command cmd_req;
                Command exp_cmd;
                switch (target)
                {
                    case TemperatureTarget.Head:
                        cmd_req = new Command(CommandOptions.None, "HEAD_TEMP");
                        exp_cmd = new Command(CommandOptions.Partial, "HEAD_TEMP");
                        break;

                    case TemperatureTarget.Laser:
                        cmd_req = new Command(CommandOptions.None, "LASER_TEMP");
                        exp_cmd = new Command(CommandOptions.Partial, "LASER_TEMP");
                        break;

                    default:
                        throw new NotSupportedException($"{target.ToString()} is not supported");
                }

                var last_cmd = SendAndWaitForCommand(cmd_req, exp_cmd, WaitTime);
                return Convert.ToDouble(last_cmd.Args);
            }
        }

        public virtual List<string> GetError()
        {
            string err_str = string.Empty;
            var errorMsgs = new List<string>();

            if (_serialPort != null)
            {
                _serialPort.Send("ERROR");
                Thread.Sleep(150);
                var response = GetRespond(ResponseLength);
                if (string.IsNullOrEmpty(response.Trim()))
                    throw new DeviceException("No response received");
                err_str = response.Split('=')[1];
            }
            else
            {
                var cmd_req = new Command(CommandOptions.None, "ERROR");
                var exp_cmd = new Command(CommandOptions.Partial, "ERROR");
                var last_cmd = SendAndWaitForCommand(cmd_req, exp_cmd, WaitTime);
                err_str = last_cmd.Args;
            }

            if (!string.IsNullOrEmpty(err_str))
            {
                _errorCode = (ErrorCode)Convert.ToUInt64(err_str, 16);
                if ((uint)_errorCode > 0)
                {
                    foreach (ErrorCode e in Enum.GetValues(typeof(ErrorCode)))
                    {
                        if (_errorCode.HasFlag(e))
                        {
                            errorMsgs.Add(e.ToString());
                        }
                    }
                }
                else
                {
                    errorMsgs.Add("NONE");
                }
            }
            return errorMsgs;
        }

        #endregion ILightingModule

        public virtual STATUS GetStatus()
        {
            if (_serialPort != null)
            {
                if (!_serialPort.IsOpen()) throw new DeviceException("Communication closed");

                _serialPort.Send("STATUS\r\n");
                Thread.Sleep(WaitTime);
                var response = GetRespond(ResponseLength);
                if (string.IsNullOrEmpty(response.Trim()))
                    throw new DeviceException("No response received");
                var status = (STATUS)Convert.ToInt32(response.Split('=')[1]);
                return status;
            }
            else
            {
                var cmd_req = new Command(CommandOptions.None, "STATUS");
                var exp_cmd = new Command(CommandOptions.Partial, "STATUS");
                var last_cmd = SendAndWaitForCommand(cmd_req, exp_cmd, WaitTime);
                var status = (STATUS)Convert.ToInt32(last_cmd.Args);
                return status;
            }
        }

        #region internal and private method

        private string GetRespond(int lengthOfResponse)
        {
            var response = _serialPort.Recieve(lengthOfResponse);
            return Encoding.Default.GetString(response);
        }

        private Command SendAndWaitForCommand(Command cmd_req, Command cmd_exp, int timeout)
        {
            Command last_cmd = new Command();

            // send cmd_req if not empty
            if (!string.IsNullOrEmpty(cmd_req.Func))
            {
                _comPort.Send(cmd_req);
            }

            // wait for cmd_exp if not empty
            if (!string.IsNullOrEmpty(cmd_exp.Func))
            {
                var resp_cmdlist = _comPort.Receive(cmd_exp, timeout);
                if (resp_cmdlist.Count > 0)
                {
                    last_cmd = resp_cmdlist.FindLast(cmd_exp.Equals);
                }
                else
                {
                    string errorString = string.Format("WaitForCommand({0}, {1}ms) timed out.", cmd_exp.Func, timeout);
                    throw new TimeoutException(errorString);
                }
            }

            return last_cmd;
        }

        internal SerialPortConfig ParseConnectionString(string connection_string)
        {
            var elems = connection_string.Split(',');
            if (elems.Length != 5) return null;

            try
            {
                var spc = new SerialPortConfig();
                spc.portName = elems[0];
                spc.baudRate = Convert.ToInt32(elems[1]);
                spc.dataBits = Convert.ToInt32(elems[2]);
                spc.stopBits = (StopBits)Enum.Parse(typeof(StopBits), elems[3]);
                spc.parity = (Parity)Enum.Parse(typeof(Parity), elems[4]);

                return spc;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion internal and private method
    }
}