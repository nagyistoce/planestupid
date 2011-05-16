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
using System.Drawing;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Core.Net;
using Core.Scene;

namespace Core.Cortex {

public class Cortex
{
   class   Demux :iDemux
   {
           public bool accept(UId RxID, out iWire RxWire)
           {
                   RxWire = null;
               if (UId.cmp(RxID, instance.Id) == 0)
                   RxWire = instance.Rx;

                   return RxWire != null;
           }
   }

           Demux  demux;

   class   Mux :iMux
   {
   }
           Mux mux;

           UId    Id;
           int    sid = -1;
           int    gid = -1;
           int    perms;
           string nick;

   public  interface iBase
   {
           //admin events
           void adDropped(Cortex sender);

           //userbase events ([names] is already locked for these events)
           void ubRehashed(Cortex sender, NameList users);
           void ubModified(Cortex sender, User user);
           void ubJoined(Cortex sender, User user);
           void ubLeft(Cortex sender, User user);

           //game events
           void gxLoad(Cortex sender);
           void gxReady(Cortex sender);
           void gxOver(Cortex sender);
   }

           iBase pBase;

   public  interface iDynamics
   {
           void dyStrobe(Cortex sender, byte[] data, ref int r);
           void dySync(Cortex sender, byte[] data, ref int r);
           void dyOver(Cortex sender, byte[] data, ref int r);
   }

           iDynamics pDynamics;

           iWire  Rx;
           iWire  Tx;

   public  struct Info
   {
           public  byte MajorVersNo;
           public  byte MinorVersNo;
   }

           Info   info;

   public  struct Config
   {
           public GameShare Share;
           public GamePolicy Policy;
           public string Host;
           public string Nick;
   }

           Config config;

   public  enum Error
   {
           Okay,
           Parse,
           Failed
   }

   private class ERecv :Exception
   {
           Error e;

           public ERecv(Error e)
           :base("Failed")
           {
                  this.e = e;
           }

           public Error Code
           {
                  get {return e;}
           }

           public static void Throw(Error e) {throw new ERecv(e);}
   }

           Core.Net.UDP.Server server;

           Core.Net.UDP.Broadcast broadcast;

           NameList names;

           Thread thread;

   Cortex(ref Config config)
   {
           this.info = new Info();
           this.config = config;
           this.demux = new Demux();
           this.mux = new Mux();

           IPAddress RemoteAddress;

       if (config.Policy == GamePolicy.Host)
           RemoteAddress = IPAddress.Loopback;
           else
           RemoteAddress = Dns.GetHostAddresses(config.Host)[0];

           Id = new UId(RemoteAddress.GetAddressBytes(), 0);

           server = new Core.Net.UDP.Server(demux, IPAddress.Any, Proto.OutPortNo);

           broadcast = new Core.Net.UDP.Broadcast(mux, Proto.InPortNo);

           Rx = server.RxOpen(Id);
           Rx.setListener(listen);

           Tx = broadcast.TxOpen(Id);

           names = new NameList();

           Console.WriteLine("[Cortex:{0}] Ready, on {1}.", GetHashCode(), RemoteAddress.ToString());
   }

   ~Cortex()
   {
           Dispose();
   }

           byte op;
           byte[] b = new byte[65536];
           Error error;

   private void listen(iWire sender, byte[] recv)
   {
           int n;
           int r = 0, s = 0;

           byte[] send = null;

       if (!sender.dequeue(out recv))
           return;

           error = Error.Okay;

           //Console.WriteLine("Cortex: Recv 0x{0} {1}bytes", Convert.ToString(op, 16).PadLeft(2, '0'), recv.Length);

       if ((n = recv.Length) < 1)
           return;

           lock(this) {

           try
           {
                switch(op = recv[r++])
                {
                  case Proto.Cortex.PONG:
                       break;

                  case Proto.Cortex.HI:
                  {
                       if (recv.Length < 5)
                           ERecv.Throw(Error.Parse);

                           info.MajorVersNo = recv[r++];
                           info.MinorVersNo = recv[r++];
                  }
                       break;

                  case Proto.Cortex.LOGIN:
                  {
                       if (recv.Length < 3)
                           ERecv.Throw(Error.Parse);

                           int f = recv[r++];
                           sid   = f & 0x0f;
                           gid   = f & 0x30;

                           byte p = recv[r++];
                           perms  = p;

                           string nick;

                       if (!Proto.gets(ref recv, ref r, out nick))
                           ERecv.Throw(Error.Parse);

                       if (nick.Length == 0)
                           ERecv.Throw(Error.Parse);
                  }
                       break;

                  case Proto.Cortex.NAMES:
                  {
                       if (recv.Length < 3)
                           ERecv.Throw(Error.Parse);

                           lock(names) {

                                names.Clear();

                                try
                                {
                                      while(recv[r] != Proto.EONAMES)
                                      {
                                            User user = new User();
                                            user.flags = recv[r++];

                                        if ((user.flags & Proto.F_TEST) == 0)
                                             ERecv.Throw(Error.Parse);

                                        if ((user.flags & Proto._F_SID) == sid)
                                            user.flags |= Proto.F_SELF;

                                            user.perms = recv[r++];

                                        if (!Proto.gets(ref recv, ref r, out user.nick))
                                            ERecv.Throw(Error.Parse);

                                            Console.WriteLine(user.ToString());
                                            names.Add(user);
                                      }

                                      if (pBase != null)
                                          pBase.ubRehashed(this, names);
                                }
                                catch(Exception e)
                                {
                                      ERecv.Throw(Error.Parse);
                                }
                           }
                           /*lock*/
                  }
                       break;

                  case Proto.Cortex.JOINED:
                  {
                       if (recv.Length < 3)
                           ERecv.Throw(Error.Parse);

                           byte   f    = recv[r++];
                           byte   p    = recv[r++];
                           string nick = null;

                      if  (!Proto.gets(ref recv, ref r, out nick))
                           ERecv.Throw(Error.Parse);

                      lock(names) {

                           User u = names.getUser(f, true);
                           bool j = (u.flags & Proto.F_TEST) == 0;
                           u.flags = f;
                           u.perms = p;
                           u.nick  = nick;

                           if (u.getSID() == sid)
                               this.nick = nick;

                            if (pBase != null)
                            {
                               if (j)
                                   pBase.ubJoined(this, u);
                                   else
                                   pBase.ubModified(this, u);
                            }
                      }
                  }
                       break;

                  case Proto.Cortex.LEFT:
                  {
                       if (recv.Length < 3)
                           ERecv.Throw(Error.Parse);

                       lock(names) {

                           byte f = recv[r++];

                           int  x = names.getUserIndex(f);

                           User u = names[x];

                           if (x > 0)
                               names.RemoveAt(x);


                           if (pBase != null)
                               pBase.ubLeft(this, u);
                       }
                  }
                       break;

                  case Proto.Cortex.LOAD:
                  {
                       if (pBase != null)
                           pBase.gxLoad(this);
                  }
                       break;

                  case Proto.Cortex.LIST:
                  {
                       if (pDynamics != null)
                           pDynamics.dyStrobe(this, recv, ref r);
                  }
                       break;

                  case Proto.Cortex.SYNC:
                  {
                       if (pDynamics != null)
                           pDynamics.dySync(this, recv, ref r);
                  }
                       break;

                  case Proto.Cortex.OVER:
                  {
                       if (pDynamics != null)
                           pDynamics.dyOver(this, recv, ref r);
                  }
                       break;

                  case Proto.Cortex.KILL:
                       sid = 0;
                       gid = 0;
                       perms = 0;
                       nick = null;
                       break;
                }

                if (s > 0)
                    Proto.pack(ref b, ref s, ref send);

                if (send != null)
                    Tx.enqueue(send);
           }
           catch(ERecv e)
           {
                    error = e.Code;
                    Console.WriteLine("Cortex{0}: Error {1}", GetHashCode(), e.Code);
           }
           catch(Exception e)
           {
                    Console.WriteLine("Cortex{0}: Crashed {1}", GetHashCode(), e.ToString());
           }
           finally
           {
                    Monitor.Pulse(this);
           }
           /*lock*/ }
   }
/*
   private void  LockSession()
   {
           this.thread = new Thread(new ThreadStart(sync));
           this.thread.IsBackground = true;
           this.thread.Start();
   }

   private void  HaltSession()
   {
       if (thread.IsAlive)
       {
           thread.Abort();
           thread.Join();
       }
   }
*/
           bool live;
   static  int  msleep = Proto.msPingTime / 5;

   /*monitor*/
   private void  sync()
   {
           Rx.crx = 0;
           Tx.ctx = 0;
           
       try
       {
           while(live)
           {
              if (Rx.crx >= Proto.msPingTime)
                  ping();

              if (Rx.crx >= Proto.msDcTime)
                  drop();

              if (Tx.ctx >= Proto.msPingTime)
                  ping();

                  Rx.crx += msleep;
                  Tx.ctx += msleep;

                  Thread.Sleep(msleep);
           }
      }
      catch(ThreadAbortException e)
      {
            Console.WriteLine("[Cortex:{0}] Brutally killed...", GetHashCode());
      }
      finally
      {
      }
   }

   private void ping()
   {
           lock(this) {
               Tx.enqueue(new byte[]{ Proto.World.PING });
           /*lock*/ }
   }

   private void drop()
   {
           lock(this) {
                //Tx.enqueue(new byte[]{ Proto.World.PING });
                if (pBase != null)
                    pBase.adDropped(this);

           /*lock*/ }
   }

   private Error Connect()
   {
           lock(this) {

                int cnt;

                for (cnt = 0; cnt != 3; cnt++)
                {
                     Console.WriteLine("Connect: Attempt {0}/3...", cnt + 1);
                     Tx.enqueue(new byte[]{ Proto.World.HI });

                     if (Monitor.Wait(this, Proto.msPingTime))
                     {
                         if (op == Proto.Cortex.HI)
                         {
                             return error;
                         }
                     }
                }
           /*lock*/}

           return Error.Failed;
   }

   private Error Login(string Nick)
   {
       if (live)
           return Rename(Nick);

           lock(this) {

                int s = 0;
                byte[] send = null;

                b[s++] = Proto.World.LOGIN;

                Proto.puts(ref b, ref s, Nick);

                Proto.pack(ref b, ref s, ref send);

                int cnt;

                for (cnt = 0; cnt != 3; cnt++)
                {
                     Tx.enqueue(send);
                     Console.WriteLine("Login: Attempt {0}/3...", cnt + 1);

                     if (Monitor.Wait(this, Proto.msPingTime))
                     {
                         if ((op == Proto.Cortex.LOGIN) || (op == Proto.Cortex.NAMES))
                         {
                             if (error == Error.Okay)
                             {   thread = new Thread(new ThreadStart(sync));
                                 thread.IsBackground = true;
                                 thread.Start();
                                 live = true;
                             }

                                 return error;
                         }
                     }
                }
           /*lock*/}

           return Error.Failed;
   }

   private Error Rename(string Nick)
   {
           lock(this) {

                int s = 0;
                byte[] send = null;

                b[s++] = Proto.World.LOGIN;

                Proto.puts(ref b, ref s, Nick);

                Proto.pack(ref b, ref s, ref send);

                Console.WriteLine("Rename {0}", Nick);
                Tx.enqueue(send);

                int x = 0;

                while (x < 3)
                {
                        Monitor.Wait(this, Proto.msPingTime);

                    if (op == Proto.Cortex.JOINED)
                        break;
 
                        x++;
                }
     
           /*lock*/}

           return Error.Okay;
   }

   private Error Prepare()
   {
           lock(this) {

                int cnt;

                for (cnt = 0; cnt != 3; cnt++)
                {
                     Console.WriteLine("Prepare: Attempt {0}/3...", cnt + 1);
                     Tx.enqueue(new byte[]{ Proto.World.PREP });

                     if (Monitor.Wait(this, Proto.msPingTime))
                     {
                         if (op == Proto.Cortex.LOAD)
                         {
                             return error;
                         }
                     }
                }

           /*lock*/}

           return Error.Failed;
   }

   private Error Ready()
   {
           lock(this) {

                int cnt;

                Console.WriteLine("Ready (single shot)");
                Tx.enqueue(new byte[]{ Proto.World.READY });
           }

           return Error.Okay;
   }

   private Error Srt(int id, PointF[] route)
   {
           lock(this) {

                int s = 0;

                byte[] send = null;

                Proto.putv(ref b, ref s, new byte[]{ Proto.World.SRT });

                Proto.putn(ref b, ref s, id);

                Proto.putn(ref b, ref s, route.Length);

                foreach(PointF pt in route)
                {
                        Proto.putf(ref b, ref s, pt.X);
                        Proto.putf(ref b, ref s, pt.Y);
                }

                Proto.pack(ref b, ref s, ref send);

                Tx.enqueue(send);

              //Console.WriteLine("CORTEX SRT {0} {1}bytes", id, send.Length);
           }

           return Error.Okay;
   }

   private Error Sfl(int id, byte[] flags)
   {
           lock(this) {

                int s = 0;

                byte[] send = null;

                Proto.putv(ref b, ref s, new byte[]{ Proto.World.SFL });

                Proto.putn(ref b, ref s, id);

                Proto.putv(ref b, ref s, flags);

                Proto.pack(ref b, ref s, ref send);

                Tx.enqueue(send);
           }

           return Error.Okay;
   }

           bool disposed;

   public  void Dispose()
   {
       if (disposed)
           return;

       if (thread != null)
       {
           if (thread.IsAlive)
           {
               live = false;
               thread.Join();
           }
       }

           broadcast.TxClose(Tx);

           server.RxClose(Rx);

           broadcast.Dispose();

           server.Dispose();

           Console.WriteLine("[Cortex:{0}] Disposed.", GetHashCode());
           instance = null;
           disposed = true;
   }

   private static Cortex instance = null;

   public  static Cortex New(ref Config config)
   {
       if (instance != null)
           return instance;

           return instance = new Cortex(ref config);
   }

   /* peer signals */
   public  static Error psProbe()
   {
           return instance.Connect();
   }

   public  static Error psLogin(string Nick)
   {
           return instance.Login(Nick);
   }

   public  static Error psPrepare()
   {
           return instance.Prepare();
   }

   public  static Error psReady()
   {
           return instance.Ready();
   }

   public  static Error psRoute(int id, PointF[] route)
   {
           return instance.Srt(id, route);
   }

   public  static Error psFlags(int id, byte[] flags)
   {
           return instance.Sfl(id, flags);
   }

   /**/
   public  static string Nick
   {
           get   {return instance.nick;}
   }

   public  static byte GID
   {
           get  {return (byte)instance.gid;}
   }

   public  static NameList Names
   {
           get   {return instance.names;}
   }

   public  static iBase Base
   {
           get {return instance.pBase;}
           set {instance.pBase = value;}
   }

   public  static iDynamics Dynamics
   {
           get {return instance.pDynamics;}
           set {instance.pDynamics = value;}
   }

   /* instance */
   public  static Cortex Instance()
   {
           return instance;
   }
}

/* namespace Core.Cortex */ }
