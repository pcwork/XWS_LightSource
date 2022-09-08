﻿namespace SPM.Lighting.Photonics
{
    public interface ILog
    {
        void Debug(string msg);
        void Info(string msg);
        void Warn(string msg);
        void Error(string msg);
        void Fatal(string msg);
    }
}