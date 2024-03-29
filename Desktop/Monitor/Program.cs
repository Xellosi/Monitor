﻿using System;
using System.Windows.Forms;
using System.Threading;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace monitor
{
    class Program
    {
        public static int SimulatedPort = 20156;
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
        public static Action<object> SensorGenerate = (object t) => {
            //共6種 0:溫度過高 > 40, 1:瓦斯值異常  > 100, 2:火光反映  > 100, 3:有雨 ==0, 4:門開啟 <15, 5: 人體 ==1
            String report;
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Random rnd = new Random();
            var sign = (CancellationToken)t;
            while (!sign.IsCancellationRequested)
            {
                report = "";
                report += rnd.Next(20, 53).ToString()+",";
                report += rnd.Next(0, 110).ToString() + ",";
                report += rnd.Next(0, 110).ToString() + ",";
                if (rnd.Next(0, 10) == 0)
                    report += "1,";
                else
                    report += "0,";
                report += rnd.Next(0, 150).ToString() + ",";
                if (rnd.Next(0, 10) == 0)
                    report += "1";
                else
                    report += "0";
                byte[] sendbuf = Encoding.ASCII.GetBytes(report);
                s.SendTo(sendbuf, new IPEndPoint(IPAddress.Parse("127.0.0.1"), SimulatedPort));
                Console.WriteLine("1111"+DateTime.Now.ToLongTimeString());
                Thread.Sleep(1000);
            }
        };
    }
}
