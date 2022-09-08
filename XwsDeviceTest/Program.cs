using JCTF.Common.IO;
using SPM.Lighting.Photonics;
using System;

namespace XwsDeviceTest
{
    internal class Program
    {
        private static void Main(string[] args) {
            Console.WriteLine("Welcome to use XwsDeviceTest program!");

            var device = ChooseDevice();
            if (device is null) {
                Console.WriteLine("Device is not supported. Program exits.");
                Environment.Exit(0);
            }

            var exit = false;
            while (!exit) {
                PrintFunctionList();
                var input = "";
                while (string.IsNullOrEmpty(input)) {
                    Console.Write(">> ");
                    input = Console.ReadLine();
                }

                try {
                    switch (input.ToLower()) {
                        case "e":
                            exit = true;
                            break;

                        case "1":
                            device.ConnectServer("COM3, 115200, 8, 1, None");
                            break;

                        case "2":
                            device.DisconnectServer();
                            break;

                        case "3":
                            device.Open();
                            break;

                        case "4":
                            device.Close();
                            break;

                        case "5":
                            var status = device.GetStatus();
                            Console.Write($"Status is {status}");
                            break;

                        case "6":
                            var b = device.GetBrightness();
                            Console.WriteLine($"Brightness is {b}");
                            break;

                        case "7":
                            var t_head = device.GetTemperature(TemperatureTarget.Head);
                            Console.WriteLine($"Temperature of head is {t_head}");

                            var t_laser = device.GetTemperature(TemperatureTarget.Laser);
                            Console.WriteLine($"Temperature of laser is {t_laser}");
                            break;

                        case "8":
                            var t = device.GetUpTime();
                            Console.WriteLine($"Uptime is {t}");
                            break;

                        case "9":
                            var e = device.GetError();
                            Console.WriteLine($"error is/are {string.Join("\r\n", e)}");
                            break;

                        default:
                            Console.WriteLine("The function is not supported.");
                            break;
                    }
                }
                catch (DeviceException ex) {
                    Console.WriteLine($"Exception happens: {ex.Message}");
                }
            }

            Console.WriteLine("Bye bye!");
        }

        private static Xws_device ChooseDevice() {
            Console.WriteLine("Choose the device model:" + "\r\n" +
                              "0. XWS-30_com" + "\r\n" +
                              "1. XWS-30" + "\r\n" +
                              "2. XWS-65");
            Console.Write(">> ");
            var input = "";
            while (string.IsNullOrEmpty(input))
                input = Console.ReadLine();

            switch (input.ToLower()) {
                case "0":
                    return new Xws30(new ComPort("xws30_1",new Uri("LS1:COM1?baudrate=115200"),115200));

                case "1":
                    return new Xws30(new BaseSerialPort());

                case "2":
                    return new Xws65(new BaseSerialPort());

                default:
                    return null;
            }
        }

        private static void PrintFunctionList() {
            Console.WriteLine("\r\nPlease to choose a function:" + "\r\n" +
                              "1. Connect Server" + "\r\n" +
                              "2. Disconnect Server" + "\r\n" +
                              "3. Open" + "\r\n" +
                              "4. Close" + "\r\n" +
                              "5. Get Status" + "\r\n" +
                              "6. Get Brightness" + "\r\n" +
                              "7. Get Temperature" + "\r\n" +
                              "8. Get Uptime" + "\r\n" +
                              "9. Get Error" + "\r\n" +
                              "e. Exit the test");
        }
    }
}