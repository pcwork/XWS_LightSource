using JCTF.Common.IO;
using Moq;
using System.IO.Ports;
using System.Text;
using Xunit;

namespace SPM.Lighting.Photonics.UnitTest
{
    public class XwsDeviceTest
    {
        [Fact]
        public void IdShouldWork() {
            var xws30 = new Xws30(new BaseSerialPort());
            Assert.Equal("XWS_30", xws30.Id);

            var xws65 = new Xws65(new BaseSerialPort());
            Assert.Equal("XWS_65", xws65.Id);
        }

        [Theory]
        [InlineData("COM1,115200,8,1,None", "COM1", 115200, 8, StopBits.One, Parity.None)]
        [InlineData("COM2,57600,8,2,Odd", "COM2", 57600, 8, StopBits.Two, Parity.Odd)]
        [InlineData("COM3,9600,8,1,Even", "COM3", 9600, 8, StopBits.One, Parity.Even)]
        public void ParseConnectionStringShouldWork(string connectionString,
            string expectedPortName,
            int expectedBaudrate,
            int expectedDatabits,
            StopBits expectedStopBits,
            Parity expectedParity) {
            var serialPortMock = new Mock<IBaseSerialPort>();
            var xws30 = new Xws30(serialPortMock.Object);
            var cfg = xws30.ParseConnectionString(connectionString);

            Assert.Equal(expectedPortName, cfg.portName);
            Assert.Equal(expectedBaudrate, cfg.baudRate);
            Assert.Equal(expectedDatabits, cfg.dataBits);
            Assert.Equal(expectedStopBits, cfg.stopBits);
            Assert.Equal(expectedParity, cfg.parity);
        }

        [Fact]
        public void ParseWrongConnectionStringShouldWork() {
            var serialPortMock = new Mock<IBaseSerialPort>();
            var xws30 = new Xws30(serialPortMock.Object);
            var cfg = xws30.ParseConnectionString("a, b, c");
            Assert.Null(cfg);
        }

        [Fact]
        public void GetStatusShouldWork() {
            var serialPortMock = new Mock<IBaseSerialPort>();
            serialPortMock.Setup(s => s.IsOpen()).Returns(true);
            serialPortMock.Setup(s => s.Send("STATUS"));
            serialPortMock.Setup(sp => sp.Recieve(20)).Returns(Encoding.ASCII.GetBytes("STATUS=1\r\n".ToCharArray()));
            var xws30 = new Xws30(serialPortMock.Object);
            var status = xws30.GetStatus();
            Assert.Equal(Xws_device.STATUS.STARTING, status);
        }

        [Fact]
        public void GetBrightnessShouldWork() {
            var serialPortMock = new Mock<IBaseSerialPort>();
            var logMock = new Mock<ILog>();
            serialPortMock.Setup(s => s.IsOpen()).Returns(true);
            serialPortMock.Setup(s => s.Send("LASER_CUR"));
            serialPortMock.Setup(sp => sp.Recieve(20)).Returns(Encoding.ASCII.GetBytes("LASER_CUR=9.123\r\n".ToCharArray()));
            var xws30 = new Xws30(serialPortMock.Object);
            xws30.SetLogger(logMock.Object);

            var brightness = xws30.GetBrightness();
            Assert.Equal(9, brightness);
        }

        [Fact]
        public void GetUpTimeShouldWork()
        {
            var serialPortMock = new Mock<IBaseSerialPort>();
            serialPortMock.Setup(s => s.IsOpen()).Returns(true);
            serialPortMock.Setup(s => s.Send("UPTIME\r\n"));
            serialPortMock.Setup(sp => sp.Recieve(20)).Returns(Encoding.ASCII.GetBytes("UPTIME=999 hours 12 minutes\r\n".ToCharArray()));
            var xws30 = new Xws30(serialPortMock.Object);

            var uptime = xws30.GetUpTime();
            Assert.Equal(999.2, uptime);
        }
    }
}