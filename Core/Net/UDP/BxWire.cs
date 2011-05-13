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

public class BxWire :iWire
{
           Broadcast owner;
           List<TxWire> wires;

   public BxWire(Broadcast broadcast, List<TxWire> wires)
   {
           this.owner = broadcast;
           this.wires = wires;
   }

   ~BxWire()
   {
           Dispose(this);
   }

   public override void enqueue(byte[] src)
   {
           lock(wires)
           {
                foreach(TxWire wire in wires)
                {
                        wire.enqueue(src);
                }
           }
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
       }
   }
}

/*namespace Core.Net.UDP*/ }
