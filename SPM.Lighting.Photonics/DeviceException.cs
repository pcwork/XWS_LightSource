using System;
using System.Runtime.Serialization;

namespace SPM.Lighting.Photonics
{
    public enum DPPMExceptionCode
    {
        ERROR_EXECUTE_COMMAND
    }

    [Serializable]
    public class DeviceException : Exception
    {
        public DPPMExceptionCode DPPMExceptionCode { get; }

        public DeviceException() {
        }

        public DeviceException(string message) : base(message) {
        }

        public DeviceException(string message, Exception innerException) : base(message, innerException) {
        }

        public DeviceException(string message, DPPMExceptionCode eRROR_EXECUTE_COMMAND) : this(message) {
            DPPMExceptionCode = eRROR_EXECUTE_COMMAND;
        }

        protected DeviceException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}