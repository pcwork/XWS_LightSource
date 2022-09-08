using System.Collections;
using System.Collections.Generic;
using JCTF.Common.IO;

namespace SPM.Lighting.Photonics
{
    public class Xws65 : Xws30
    {
        public override string Id => "XWS_65";

        public Xws65(IBaseSerialPort serialPort) : base(serialPort) {

        }
        /// <summary>
        /// ERROR=xxxxx
        ///
        /// Allows to know error flags if STATUS == 4
        /// Flags’ packed in 20-bit wide status word
        /// Bit 00 – TEC overcurrent channel #1
        /// Bit 01 – TEC overcurrent channel #2
        /// Bit 02 – Abnormal system voltage Bit 03 – TEC temperature sensor shorted
        /// Bit 04 – TEC temperature sensor open
        /// Bit 05 – TEC FAN1 fail
        /// Bit 06 – TEC FAN2 fail
        /// Bit 07 – LASER fail(setpoint not reached)
        /// Bit 08 – HDC fail(maybe head interface cable’s not attached)
        /// Bit 09 – TEC link fail
        /// Bit 10 – LASER link fail
        /// Bit 11 – HDC link fail
        /// Bit 12 – Plasma start fail(photodiode feedback)
        /// Bit 13 – Plasma down(while in active mode)
        /// Bit 14 – LASER overheat(> 35 *C)
        /// Bit 15 – Optical head overheat(> 80 *C)
        /// Bit 16 – LASER start fail(photodiode feedback)
        /// Bit 17 – firmware error
        /// Bit 18 – hardware error
        /// Bit 19 – unknown error
        /// 00000 means NO ERROR
        ///
        /// </summary>
        public readonly string[] ErrorString = new string[]
        {
            "TEC overcurrent channel #1",
            "TEC overcurrent channel #2",
            "Abnormal system voltage",
            "TEC temperature sensor shorted",
            "TEC temperature sensor open",
            "TEC FAN1 fail",
            "TEC FAN2 fail",
            "LASER fail (setpoint not reached)",
            "HDC fail (maybe head interface cable’s not attached)",
            "TEC link fail",
            "LASER link fail",
            "HDC link fail",
            "Plasma start fail (photodiode feedback)",
            "Plasma down (while in active mode)",
            "LASER overheat (> 35 *C)",
            "Optical head overheat (> 80 *C)",
            "LASER start fail (photodiode feedback)",
            "firmware error",
            "hardware error",
            "unknown error",
        };

        public override List<string> GetError()
        {
            //var flags = _serialPortSrv.Send("ERROR");
            int flags = 0x01;
            var errors = new List<string>();
            var ba = new BitArray(new int[] { flags });
            for (int i = 0; i < 20; i++) {
                if (ba.Get(i))
                    errors.Add(ErrorString[i]);
            }

            return errors;
        }


    }
}