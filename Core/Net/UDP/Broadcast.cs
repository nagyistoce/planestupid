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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Core.Net.UDP {

public class Broadcast :IDisposable
{
   internal Socket socket;
   internal List<TxWire> wires;
           BxWire broadcast;

           Thread thread;

           iMux mux;
           int  port;

   public Broadcast(iMux mux, int port)
   {
           this.mux = mux;
           this.port = port;

           this.socket = new Socket(AddressFamily.InterNetwork,
                         SocketType.Dgram, ProtocolType.Udp);

           this.wires = new List<TxWire>();

           this.broadcast = new BxWire(this, wires);

           Console.WriteLine("[Broadcast:{0}] Ready.", GetHashCode());
   }

   ~Broadcast()
   {
           Dispose();
   }

   public  iWire TxOpen(UId key)
   {
       if (key != null)
           return new TxWire(this, wires, new IPEndPoint(key.IPv4, port));
           else
           return broadcast;
   }

   public  iWire TxClose(iWire wire)
   {
           TxWire TxWire = (TxWire)wire;

       if (TxWire != null)
           TxWire.Dispose(TxWire);

           return null;
   }

           bool disposed;

   public void  Dispose()
   {
       if (disposed)
           return;

       if (thread != null)
       {
           if (thread.IsAlive)
           {   thread.Abort();
               thread.Join();
           }
       }

           socket.Close();
           socket = null;
           thread = null;

           Console.WriteLine("[Broadcast:{0}] Disposed.", GetHashCode());
           disposed = true;
   }
}

/*namespace Core.Net.UDP*/ }
