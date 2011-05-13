/*
PlaneStupid
Copyright (c) 2011 spaceape

This software is provided 'as-is', without any express or implied
warranty. In no event will the authors be held liable for any damages
arising from the use of this software.

Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:

    1. The origin of this software must not be misrepresented; you must not
    claim that you wrote the original software. If you use this software
    in a product, an acknowledgment in the product documentation would be
    appreciated but is not required.

    2. Altered source versions must be plainly marked as such, and must not be
    misrepresented as being the original software.

    3. This notice may not be removed or altered from any source
    distribution.
*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace Core.Net.UDP {

public class Server :IDisposable
{
           Socket socket;
           Thread thread;

           iDemux demux;

   public Server(iDemux demux, IPEndPoint point)
   {
           this.demux  = demux;

           this.socket = new Socket(AddressFamily.InterNetwork,
                         SocketType.Dgram, ProtocolType.Udp);

       try
       {
           socket.Bind(point);

           this.thread = new Thread(new ThreadStart(_perform));
           this.thread.IsBackground = true;
           this.thread.Start();

           Console.WriteLine("[Server:{0}] Ready.", GetHashCode());
       }
       catch(Exception e)
       {
           socket = null;
           throw e;
       }
   }

   public Server(iDemux demux, IPAddress addr, int port)
   :this(demux, new IPEndPoint(addr, port))
   {
   }

   ~Server()
   {
           Dispose();
   }

   private void _perform()
   {
           int nby;
           byte[] recv = new byte[65536];

           EndPoint   rxep = new IPEndPoint(IPAddress.Any, 0);
           UId        rxid;

       try
       {
           while (true)
           {
                  nby  = socket.ReceiveFrom(recv, ref rxep);

                  rxid = new UId((IPEndPoint)rxep);
           
                  iWire  Rx;

              if (demux.accept(rxid, out Rx))
              {
                  byte[] data = new byte[nby];
                  Array.Copy(recv, data, nby);
                  Rx.enqueue(data);
              }
           }
      }
      catch(ThreadAbortException e)
      {
            Console.WriteLine("[Server:{0}] Brutally killed...", GetHashCode());
      }
      finally
      {
      }
   }

   public  RxWire RxOpen(UId key)
   {
           return new RxWire(this);
   }

   public  RxWire RxClose(iWire wire)
   {
           RxWire RxWire = (RxWire)wire;

       if (RxWire != null)
           RxWire.Dispose(this);

           return null;
   }

           bool disposed;

   public  void Dispose()
   {
       if (disposed)
           return;

       if (thread.IsAlive)
       {
           thread.Abort();
           thread.Join();
       }

            socket.Close();
            socket = null;
            thread = null;

            Console.WriteLine("[Server:{0}] Disposed.", GetHashCode());
            disposed = true;
   }

   public  void Waitfor()
   {
       if (thread.IsAlive)
       {
           thread.Join();
       }
   }
}

/*namespace Core.Net.UDP */ }
