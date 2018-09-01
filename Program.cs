using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerSocket
{
	class Program
	{
		static void Main(string[] args)
		{
			Socket serverSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
			IPAddress ip = IPAddress.Any;
			IPEndPoint point = new IPEndPoint(ip, 2333);
			//socket绑定监听地址
			serverSocket.Bind(point);
			Console.WriteLine("Listen Success");
			//设置同时连接个数
			serverSocket.Listen(10);

			//利用线程后台执行监听,否则程序会假死
			Thread thread = new Thread(Listen);
			thread.IsBackground = true;
			thread.Start(serverSocket);

			Console.Read();
		}

		/// <summary>
		/// 监听连接
		/// </summary>
		/// <param name="o"></param>
		static void Listen(object o)
		{
			var serverSocket = o as Socket;
			while (true)
			{
				//等待连接并且创建一个负责通讯的socket
				var send = serverSocket.Accept();
				//获取链接的IP地址
				var sendIpoint = send.RemoteEndPoint.ToString();
				Console.WriteLine($"{sendIpoint}Connection");
				//开启一个新线程不停接收消息
				Thread thread = new Thread(Recive);
				thread.IsBackground = true;
				thread.Start(send);
			}
		}

		/// <summary>
		/// 接收消息
		/// </summary>
		/// <param name="o"></param>
		static void Recive(object o)
		{
			var send = o as Socket;
			while (true)
			{
				//获取发送过来的消息容器
				byte[] buffer = new byte[1024 * 1024 * 2];
				var effective = send.Receive(buffer);

				Console.WriteLine("effective: " + effective);

				//有效字节为0则跳过
				if (effective == 0)
				{
					break;
				}
				var str = Encoding.UTF8.GetString(buffer,0, effective);
				Console.WriteLine("from client: " + str);

				BattleLogic battleLogic = new BattleLogic ();
				battleLogic.init ();
				battleLogic.setBattleRecord (str);
				battleLogic.replayVideo();

				while (true) {
					battleLogic.updateLogic();
					if (battleLogic.m_bIsBattlePause) {
						break;
					}
				}
				Console.WriteLine("m_uGameLogicFrame: " + BattleLogic.s_uGameLogicFrame);
				string replyContent = BattleLogic.s_uGameLogicFrame.ToString ();
				var buffers = Encoding.UTF8.GetBytes(replyContent);
				send.Send(buffers);
				Console.WriteLine("send info to client");
			}
		}
	}
}