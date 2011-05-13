using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace Core.Net {

public class RxWire :iWire
{
           Queue<byte[]> queue;
           bool busy;

   public RxWire()
   {
          queue = new Queue<byte[]>();
   }

   public  int enable()
   {
        return 0;
   }

   public  int suspend()
   {
        return 0;
   }

   public  void enqueue(byte[] src)
   {
           lock(this)
           {
                Console.WriteLine("Rx in {0}", src.Length);
                queue.Enqueue(src);
                Monitor.Pulse(this);
           }
   }

   public  void dequeue(out byte[] dst)
   {
           lock(this)
           {
                try
                {
                       Monitor.Wait(this);

                /* TODO: Checksumming and other low-level protocol shit
                               dst  = new byte[0];
                       byte[]  data = queue.Dequeue();
                               Array.Resize<byte>(ref dst, 4);
                */

                       dst = queue.Dequeue();
                       Console.WriteLine("Rx out {0}", dst.Length);
                }
                catch (SynchronizationLockException e)
                {
                       dst = null;
                }
                catch (ThreadInterruptedException e)
                {
                       dst = null;
                }

                       Monitor.Pulse(this);
           }
   }

   public  void drop()
   {
   }
}

/*namespace Core.Net*/ }
