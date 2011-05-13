using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Core;
using Core.Net;
using Core.Net.UDP;
using Core.World;

namespace net2t
{
	class MainClass
	{
		public static void Main (string[] args)
		{
               World.Config Config = new World.Config();
               Config.Share = GameShare.Internet;

               World  world = World.New(ref Config);

               world.Waitfor();
               Console.WriteLine("Bye.");
		}
	}
}
