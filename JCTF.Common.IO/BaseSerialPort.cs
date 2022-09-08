using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JCTF.Common.IO
{
    public class BaseSerialPort : IBaseSerialPort
    {
        #region constructor
        public BaseSerialPort() {
            sp = new SerialPort();
        }
        #endregion

        #region public method
        public void SerialPortInit(string portName, int bondRate, int dataBits, StopBits stopBits, Parity parity)
        {
            if (sp.IsOpen) {
                return;
            }

            Config(portName, bondRate, dataBits, stopBits, parity);
        }

        public bool Config(string portName, int bondRate, int dataBits, StopBits stopBits, Parity parity)
        {
            if (sp.IsOpen) {
                return false;
            }

            sp.PortName = portName;
            sp.BaudRate = bondRate;
            sp.DataBits = dataBits;
            sp.StopBits = stopBits;
            sp.Parity = parity;

            return true;
        }


        public void SPOpen() {
            if (!sp.IsOpen) {
                try {
                    sp.Open();
                }
                catch (Exception ex) {
                    throw ex;
                }
                IsConnected = true;
            }
        }

        public bool IsOpen() {
            return sp.IsOpen;
        }

        public void SPClose() {
            if (sp.IsOpen) {
                try {
                    sp.Close();
                }
                catch (Exception ex) {
                    throw ex;
                }
                IsConnected = false;
            }
        }

        public byte[] ReadBase(string send, int RespLength) {
            //hybirdLock.Enter();

            if (IsClearCacheBeforeRead)
                ClearSerialCache();
            try {
                SPSend(send);
            }
            catch (Exception ex) {
                //hybirdLock.Leave();
                throw ex;
            }

            byte[] result = SPRecieved(sp, true, RespLength);
            //hybirdLock.Leave();

            return result;
        }

        public void ClearSerialCache() {
            SPRecieved(sp, false, 8);
        }

        public void Send(string send) {
            SPSend(send);
        }
        public byte[] Recieve(int RespLength) {
            return SPRecieved(sp, false, RespLength);
        }

        #endregion

        #region method
        protected virtual void SPSend(string data) {
            if (data != null && data.Length > 0) {
                try {
                    sp.WriteLine(data);
                }
                catch (Exception ex) {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 从串口接收数据，可以指定是否一定要接收到数据
        /// </summary>
        /// <param name="serialPort">串口对象</param>
        /// <param name="awaitData">是否必须要等待数据返回</param>
        /// <returns>结果数据对象</returns>
        protected byte[] SPRecieved(SerialPort serialPort, bool awaitData, int RespLength) {
            byte[] buffer = new byte[2048];
            DateTime start = DateTime.Now; // 开始时间，用于确认是否超时的信息
            int sp_receive = 0;
            int offset = 0;
            while (true) {
                try {
                    if ((DateTime.Now - start).TotalMilliseconds > ReceiveTimeout) {
                        throw new Exception("ReceiveTimeout,rec data length:" + offset.ToString());
                    }

                    if (/*RespLength < 10 &&*/ sp.BytesToRead == 0) {
                        return buffer;
                    }

                    if (sp.BytesToRead >= 1) {
                        sp_receive = sp.Read(buffer, sp_receive, sp.BytesToRead);
                        offset += sp_receive;

                        try {
                            string vResponseReadData = (System.Text.Encoding.Default.GetString(buffer)).TrimEnd('\0');
                        }
                        catch {

                        }

                    }

                    //25 for temprature sensor
                    //37 for FS 
                    if (offset > 0) {
                        if (offset >= RespLength || buffer[offset - 1] == 13)
                            break;
                    }

                    Thread.Sleep(sleepTime);
                }
                catch (Exception ex) {
                    throw ex;
                }
            }

            return buffer;
        }
        #endregion

        #region public properties
        public bool IsClearCacheBeforeRead {
            get { return isClearCacheBeforeRead; }
            set { isClearCacheBeforeRead = value; }
        }

        public int SleepTime {
            get { return sleepTime; }
            set { if (value > 0) sleepTime = value; }
        }

        public int ReceiveTimeout {
            get { return receiveTimeout; }
            set { receiveTimeout = value; }
        }

        public bool IsConnected {
            get { return isConnected; }
            set { isConnected = value; }
        }
        #endregion

        #region Object Override
        public override string ToString() {
            return "SerialBase";
        }
        #endregion

        #region Private Member
        private SerialPort sp = null;                    // 串口交互的核心
        private int receiveTimeout = 3000;               // 接收数据的超时时间//3000
        private int sleepTime = 10;                      // 睡眠的时间
        private bool isClearCacheBeforeRead = true;     // 是否在发送前清除缓冲
        private bool isConnected = false;
        #endregion
    }
}
