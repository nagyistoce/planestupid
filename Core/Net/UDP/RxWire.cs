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
using System.Threading;

namespace Core.Net.UDP {

public class RxWire :iWire
{
           Server owner;

           Queue<byte[]> queue;

   public RxWire(Server server)
   {
           owner = server;
           queue = new Queue<byte[]>();

           lock(owner) {
                //owner.wires.Add(this);
           /*lock*/}
   }

   ~RxWire()
   {
           Dispose(this);
   }

           bool   sync;

   public override void enqueue(byte[] src)
   {
           lock(this)
           {
               crx = 0;

               queue.Enqueue(src);

               if (function != null)
               {
                   sync = true;

                   try
                   { 
                          function(this, null);
                   }
                   finally
                   {
                          sync = false;
                   }
               }

               Monitor.Pulse(this);

               #if DEBUG
               //Console.WriteLine("Rx {0} {1} > {2}", "-", src.Length, src[0]);
               #endif
           }
   }

   public override bool dequeue(out byte[] dst)
   {
           lock(this)
           {
                try
                {
                   if (!sync)
                   {
                       if (queue.Count == 0)
                           Monitor.Wait(this);
                   }

                       byte[] recv = queue.Dequeue();
                       //dst = queue.Dequeue();
                       /*decode*/
                       dst = recv;
                }
              //catch (SynchronizationLockException e)
              //catch (ThreadInterruptedException e)
                catch (Exception e)
                {
                       Console.WriteLine("RxWire[{0}] {1}", this.GetHashCode(), e.Message);
                       dst = null;
                }

                       Monitor.Pulse(this);
           }

           return dst != null;
   }

           bool disposed;

   public void Dispose(object auth)
   {
       if (disposed)
           return;

       if (auth == this)
       {
           lock(owner) {
                //owner.wires.Remove(this);
           /*lock*/}
       }

           disposed = true;
   }
}

/*namespace Core.Net.UDP*/ }
