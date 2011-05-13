
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace Core.Net {

public class Server :iServer, IDisposable
{
           Socket socket;
           Thread thread;

           iDemux demux;
           SortedDictionary<UId, RxWire> wires;

   public Server(iDemux pdemux)
   {
           demux  = pdemux;

           wires  = new SortedDictionary<UId, RxWire>();
           

           socket = new Socket(AddressFamily.InterNetwork,
                               SocketType.Dgram, ProtocolType.Udp);

           IPAddress addr = IPAddress.Loopback;
           IPEndPoint point = new IPEndPoint(addr, Proto.portno);

       try
       {
           socket.Bind(point);

           thread = new Thread(new ThreadStart(_perform));
           thread.IsBackground = true;
           thread.Start();

           Console.WriteLine("[Server] Ready.");
       }
       catch(SocketException e)
       {
           socket = null;
           throw e;
       }
   }

   ~Server()
   {
           Console.WriteLine("[Server] Shutting down...");
           Dispose();
   }

   private void _perform()
   {
           int nby;
           byte[] recv = new byte[65536];

           EndPoint   rxep;
           UId        rxid;

       try
       {
           while (true)
           {
                  rxep = new IPEndPoint(IPAddress.Any, 0);
                  nby  = socket.ReceiveFrom(recv, ref rxep);
                  rxid = new UId(((IPEndPoint)rxep).Address.GetAddressBytes(), 
                                 ((IPEndPoint)rxep).Port);

                  byte[] data = new byte[nby];
                  Array.Copy(recv, data, nby);

                  RxWire wire = null;

              if (demux.accept(rxid))
              {
                  if (wires.TryGetValue(rxid, out wire))
                      wire.enqueue(data);
              }

           }
      }
      catch(ThreadAbortException e)
      {
            Console.WriteLine("[Server] Brutally killed...");
      }
      finally
      {
      }
   }

   public iWire RxOpen(UId key)
   {
           RxWire wire = null;

       if (!wires.TryGetValue(key, out wire))
           wire = new RxWire();
           return wire;
   }

   public iWire RxClose(UId key)
   {
           wires.Remove(key);
           return null;
   }

   private bool  disposed;

   public  void  Dispose()
   {
       if (disposed)
           return;

       if (thread.IsAlive)
       {   thread.Abort();
           thread.Join();
       }

           socket = null;
           thread = null;

           disposed = true;
           GC.SuppressFinalize(this);
   }

   public void  WaitFor()
   {
       if (thread != null)
       {
           if (thread.IsAlive)
               thread.Join();
       }
   }
}

/*namespace Core.Net*/ }
