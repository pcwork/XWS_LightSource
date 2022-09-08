using System.IO.Ports;

namespace JCTF.Common.IO
{
    public interface IBaseSerialPort
    {
        void SerialPortInit(string portName, int bondRate, int dataBits, StopBits stopBits, Parity parity);
        bool Config(string portName, int bondRate, int dataBits, StopBits stopBits, Parity parity);
        void SPOpen();
        bool IsOpen();
        void SPClose();
        byte[] ReadBase(string send, int RespLength);
        void ClearSerialCache();
        void Send(string send);
        byte[] Recieve(int RespLength);
    }
}