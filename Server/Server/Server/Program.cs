/*
 * 关机重启精灵V1.0 控制台版
 * 服务器端
 * Coded by Martin Huang
 * 2018.3.26
 * 
 * 制作目的：因为拿家里的一台大硬盘台式机做文件存储服务器，然后它外接的显示器“太高端”：把我眼睛给弄坏了，为了尽量
 *           少去碰那个显示器，于是就做了这个小软件。这个小软件同样可用于学校机房批量关机重启。
 * 
 * 思路：其实很简单，经测试后发现，只有服务器端才能一直监听某个端口，所以，将客户端作为“服务器”端监听端口
 *       “客户端”向“服务器”发送指令信息。由“客户端”根据获取的可用IP，主动连接“服务器”
 *
 * 最后感谢1. https://www.cnblogs.com/ouyangJJ/p/5811754.html => 教我C#下的Socket编程
 *         2. https://www.cnblogs.com/lijianda/archive/2017/03/23/6604651.html => 教我在本机众多IP中获取可用IP
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;


namespace Server
{
    class Program
    {
        //使用Win的cmd中Ping命令，对所设定的IP段的每个IP进行连通测试，把能连通的存进列表
        static List<IPAddress> getIP()
        {
            List<IPAddress> ips = new List<IPAddress>();
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = false;
            for (int i = 100; i <= 115; i++ )
            {
                p.Start();
                p.StandardInput.WriteLine("arp -a 192.168.1."+i+"\nexit");
                p.StandardInput.AutoFlush = true;
                p.WaitForExit();
                String msg = p.StandardOutput.ReadToEnd();
                if (!msg.Contains("未找到 ARP 项"))
                {
                    ips.Add(IPAddress.Parse("192.168.1." + i));
                }
                p.Close();
            }
            return ips;
        }

        //向客户端发送指令
        static void sendConstruction(IPAddress ip, String msg)
        {

             Socket tcpclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
             EndPoint point = new IPEndPoint(ip, 43389);
             tcpclient.Connect(point);
             tcpclient.Send(Encoding.UTF8.GetBytes(msg));
            
        }

        //对所有的机器发送指令
        static void allConstruction(List<IPAddress> list , String cmd)
        {
           foreach(IPAddress ip in list)
          {
              try
              {
                  sendConstruction(ip, cmd);
                  Console.WriteLine(ip + "指令发送成功！");
              }
              catch (SocketException e)
              {
                  Console.WriteLine(ip + "指令发送失败！");
                  continue;
              }
               
          }
        }

        //对特定的机器发送指令
        static void specificConstruction(List<IPAddress> list , String cmd , int index)
        {
            try
            {
                sendConstruction(list[index], cmd);
            }
            catch (SocketException e)
            {
                Console.WriteLine("指令发送失败！");
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine("输入有误");
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("正在扫描局域网在线计算机，ip范围：192.168.1.100 - 192.168.1.115");
           List<IPAddress> list = getIP();
           Console.WriteLine("先已有连接计算机：");
           for (int i = 0; i < list.Count; i++ )
           {
               Console.WriteLine("【" + i + "】 =》 " + list[i]);
           }
           while(true)
           {
               Console.WriteLine("按s关闭所有电脑，按r重启所有电脑,按【序号】#s关闭特定计算机，按【序号】#r重启特定计算机，按q退出");
               string operation = Console.ReadLine();

               if (operation.Length == 1)
               {
                   if (operation == "s")
                   {
                       allConstruction(list, "shutdown -s\n");
                   }
                   else if (operation == "r")
                   {
                       allConstruction(list, "shutdown -r\n");
                   }
                   else if(operation == "q")
                   {
                       break;
                   }
               }
               else
               {
                   String[] temp = operation.Split('#');
                   if (temp[1] == "s")
                   {
                       specificConstruction(list, "shutdown -s\n", int.Parse(temp[0]));
                   }
                   else if (temp[1] == "r")
                   {
                       specificConstruction(list, "shutdown -r\n", int.Parse(temp[0]));
                   }
               }
           }
       }          
    }
}
