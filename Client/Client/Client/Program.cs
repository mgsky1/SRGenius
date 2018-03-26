/*
 * 关机重启精灵V1.0 控制台版
 * 客户端
 * Coded By Martin Huang
 * 2018.3.26
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace Client
{
    class Program
    {
        //调用Win的cmd执行控制台指令
        static void executeConstruction(String construction)
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = false;
            p.Start();
            p.StandardInput.WriteLine(construction+"\nexit");
            p.StandardInput.AutoFlush = true;
            p.WaitForExit();
        }
        static void Main(string[] args)
        {
            //获取当前计算机的可访问Internet的IP地址
            TcpClient client = new TcpClient();
            client.Connect("www.baidu.com", 80);
            String ip = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();
            client.Close();

            //监听本机43389端口，等待服务器传送指令
            Socket tcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse(ip);
            EndPoint point = new IPEndPoint(ipAddress, 43389);
            tcpServer.Bind(point);
            tcpServer.Listen(100);
            //Console.WriteLine("监听中...");

            //与服务器建立连接后，执行指令
            Socket clientSocket = tcpServer.Accept();
            //Console.WriteLine("已与服务器建立连接.");
            byte[] bconstruct = new byte[1024];
            int length = clientSocket.Receive(bconstruct);
            String construction = Encoding.UTF8.GetString(bconstruct, 0, length);
            executeConstruction(construction);
        }
    }
}
