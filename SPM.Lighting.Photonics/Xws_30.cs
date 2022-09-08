using JCTF.Common.IO;

namespace SPM.Lighting.Photonics
{
    public class Xws30 : Xws_device
    {
        public override string Id => "XWS_30";

        public Xws30(IBaseSerialPort serialPort) : base(serialPort) {
        }

        public Xws30(Port<Command> com) : base(com)
        {
        }
    }
}
