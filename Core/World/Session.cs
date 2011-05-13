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
using System.Threading;
using Core.Net;

namespace Core.World {

public class Session
{
   protected World world;
   protected UId   Id;

   internal iWire Rx;
   internal iWire Tx;
   protected iWire Bx;

           Thread thread;

   public Session(World world, UId id, bool spawn, bool receive, bool transmit)
   {
           this.world = world;
           this.Id = id;

       if (receive)
           this.Rx = world.server.RxOpen(this.Id);

       if (transmit)
           this.Tx = world.broadcast.TxOpen(this.Id);
 
           this.Bx = world.broadcast.TxOpen(null);

           lock(world.sessions)
           {
               world.sessions.Add(Id, this);
           }

       if (spawn)
       {
           thread = new Thread(new ThreadStart(execute));
           thread.IsBackground = true;
           thread.Start();
       }
   }

   ~Session()
   {
           Dispose(this);
   }

   protected int  idle;
   protected bool done;

   protected virtual void execute()
   {
   }


   public  int IdleTime
   {
           get {return idle;}
           set {idle = value;}
   }

           bool disposed;

   public void Dispose(object auth)
   {
       if (disposed)
           return;

           disposed = true;

       if (auth == this)
       {
           lock(world.sessions)
           {
               world.sessions.Remove(Id);
           }
       }

       if (thread != null)
       {
           if (thread.IsAlive)
           {
               if (!done) 
                   thread.Abort();

               thread.Join();
           }
       }

       if (Tx != null)
           Tx = world.broadcast.TxClose(Tx);

       if (Rx != null)
           Rx  = world.server.RxClose(Rx);

   }

   public UId ID
   {
          get {return Id;}
   }
}

/*namespace Core.World*/ }
