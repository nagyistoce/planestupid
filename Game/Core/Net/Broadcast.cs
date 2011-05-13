
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Core.Net {

public class Broadcast
{
           Socket socket;
           Thread thread;

   public Broadcast(iDemux pdemux)
   {
           Console.WriteLine("[Broadcast] Ready.");
   }

   ~Broadcast()
   {
           Console.WriteLine("[Broadcast] Shutting down...");
   }

   private void  dispose()
   {
       if (thread.IsAlive)
       {   thread.Abort();
           thread.Join();
       }

           socket = null;
           thread = null;
   }
}

/*namespace Core.Net*/ }
