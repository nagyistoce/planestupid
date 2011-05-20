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

namespace Core.Net.UDP {

public class TxWire :iWire
{
           Broadcast owner;
           List<TxWire> wires;

           EndPoint  point;

   public TxWire(Broadcast broadcast, List<TxWire> wires, EndPoint point)
   {
           this.owner = broadcast;
           this.wires = wires;
           this.point = point;

           lock(wires)
           {
                wires.Add(this);
           }
   }

   ~TxWire()
   {
           Dispose(this);
   }

   public override void enqueue(byte[] src)
   {
           ctx = 0;
           owner.socket.SendTo(src, point);

           #if DEBUG
           Console.WriteLine("Tx {0} {1} > {2}", src.Length, point.ToString(), src[0]);
           #endif
   }

   public override bool dequeue(out byte[] dst)
   {
           dst = null;
           return false;
   }

           bool disposed;

   public void Dispose(object auth)
   {
       if (disposed)
           return;

       if (auth == this)
       {
           lock(wires)
           {
                wires.Remove(this);
           }
       }

           disposed = true;
   }
}

/*namespace Core.Net.UDP*/ }
