using System.Collections.Generic;

namespace SPM.Lighting.Photonics
{
    public enum TemperatureTarget
    {
        Laser,
        Head
    }

    public interface ILightingModule
    {
        /// <summary>
        /// 打开
        /// </summary>
        void Open();

        /// <summary>
        /// 关闭
        /// </summary>
        void Close();

        /// <summary>
        /// 设置亮度：voltage参照brightness设置转换为0-255
        /// </summary>
        /// <param name="brightness">亮度， 0-255</param>
        void SetBrightness(int brightness);

        /// <summary>
        /// 获取亮度
        /// </summary>
        /// <returns>亮度， 0-255</returns>
        int GetBrightness();

        /// <summary>
        /// 连接设备
        /// </summary>
        /// <param name="connection_string">connect string</param>
        void ConnectServer(string connection_string);

        /// <summary>
        /// 断开连接
        /// </summary>
        void DisconnectServer();

        /// <summary>
        /// 获取光源运行时长
        /// </summary>
        double GetUpTime();

        /// <summary>
        /// 获取光源温度
        /// </summary>
        double GetTemperature(TemperatureTarget target);

        /// <summary>
        /// 获取故障信息
        /// </summary>
        List<string> GetError();
    }
}