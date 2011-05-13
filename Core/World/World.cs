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
using System.Drawing;
using System.Threading;
using System.Net;
using Core.Net;
using Core.Scene;
using Core.World.Mods;

namespace Core.World {

public class World
{
   class   Demux :iDemux
   {
           public bool accept(UId RxID, out iWire RxWire)
           {
                   Session session = null;

                   lock(instance.sessions)
                   {
                       if (!instance.sessions.TryGetValue(RxID, out session))
                           session = new Player(instance, RxID);
                   }
                   
                   //Console.WriteLine("Session Accept {0} -> {1}", RxID.ToString(), session.GetHashCode());

                   return (RxWire = session.Rx) != null;
           }
   }

           Demux  demux;

   class   Mux :iMux
   {
   }
           Mux mux;

   public  struct Config
   {
           public GameShare Share;
   }

           Config config;

   internal Core.Net.UDP.Server server;

   internal Core.Net.UDP.Broadcast broadcast;

   internal SortedDictionary<UId, Session> sessions;

   internal Player[] players;

           Thread thread;
           Space  space;
           EventQueue queue;
           MessageQueue messg;

   World(ref Config config)
   {
           this.config = config;
           this.demux = new Demux();
           this.mux = new Mux();

           server = new Core.Net.UDP.Server(demux, IPAddress.Any, Proto.InPortNo);

           broadcast = new Core.Net.UDP.Broadcast(mux, Proto.OutPortNo);

           sessions = new SortedDictionary<UId, Session>(new UIdComparer());

           players = new Player[16];

           thread = new Thread(new ThreadStart(_perform));
           thread.IsBackground = true;
           thread.Start();

           Console.WriteLine("[World:{0}] Ready.", GetHashCode());
   }

   ~World()
   {
           Dispose();
   }

   private void _perform()
   {
           bool  halt = false;

           while (!halt)
           try
           {
                 space = new Space();
                 queue = new EventQueue(null);
                 messg = new MessageQueue();
                 
                 int status;
                 int[] n = new int[8];

                 Stack<Pair> Stack = new Stack<Pair>();
                 Stack.Push(Pair.Make(0, 0));

                 Terrain bTerrain = new Terrain(space);

                 int u_difficulty = 3;
                 int u_lost = 0;
                 int u_caught = 0;
                 int u_evo = 0; //game time
                 int t_w_kick = 0;

                 List<Thing> updates = new List<Thing>();
                 List<Thing> drops = new List<Thing>();
                 List<Vehicle> vehicles = new List<Vehicle>();
                 List<Strip> strips = new List<Strip>();

                 strips.Add(new Strip(space, Thing.TYPE_PLANE0, -0.30f, -0.36f));

                 List<UId> zombies = new List<UId>();

                 Random rnd = new Random();
                 Message msg = null;

                 while(Stack.Count > 0)
                 {
                       Pair cx = Stack.Peek();

                    if (messg.Count > 0)
                        msg = messg.Dequeue();
                        else
                        msg = new Message(0, Message.M_NONE, null);

                       try
                       {
                             switch (cx.x)
                             {
                               case  0x00:
                               {
                                     switch (msg.Mno)
                                     {
                                        case  GameMessage.M_START:
                                              cx.x = 0x10;
                                              break;

                                        case  GameMessage.M_KILL:
                                              halt = true;
                                              Stack.Pop();
                                              break;

                                        default:
                                              break;
                                     }
                               }
                                     break;

                               case  0x10:
                               {
                                     PlayerEvent.Load(queue);
                                     cx.x++;
                               }
                                     break;

                               case  0x11:
                               {
                                     if (t_w_kick > Proto.msLdTime)
                                     {
                                         //kick the assholes that refuse to load
                                     }

                                         int x = 0;
                                         int  ngamers = 0;
                                         int  rgamers = 0;
                                         bool hasop = true;

                                         while (x != players.Length)
                                         {
                                            if (players[x] != null)
                                            {
                                                if ((players[x].getstatus() & Player.S_SYNC) == 0)
                                                    players[x].Post(PlayerEvent.List(null, space));

                                                if (players[x].getgid() != 0)
                                                {
                                                    ngamers++;

                                                    if ((players[x].getstatus() & Player.S_READY) != 0)
                                                        rgamers++;
                                                }
                                            }

                                                x++;
                                         }

                                     if (ngamers > 0)
                                     {
                                         if (rgamers == ngamers)
                                         {
                                             cx.x = 0x20;
                                         }
                                     }

                                         Thread.Sleep(999 - Proto.msRsTime);
                                         t_w_kick += 1000;
                               }
                                     break;

                               case  0x20:
                               {
                                     if ((u_evo % 50) == 0)
                                     {
                                         if (vehicles.Count < u_difficulty) 
                                         {
                                             float    px =-1.0f + 2.0f * (float)rnd.NextDouble();
                                             float    py = 1.0f;

                                             Vehicle  vec = new Military(space);
                                                      vec.Pos = new vector(px, py, 0.0f);
                                                      vec.Rot = new vector( 0.0f, 0.0f, (float)Math.Atan2(-py, -px) );

                                             vehicles.Add(vec);
                                         }
                                     }

                                     switch (msg.Mno)
                                     {
                                       case  GameMessage.M_SRT:
                                            _msg_srt((byte)msg.Gno, msg.Data);
                                             break;

                                       case  GameMessage.M_SFL:
                                            _msg_sfl((byte)msg.Gno, msg.Data);
                                             break;

                                       case  GameMessage.M_KILL:
                                             halt = true;
                                             Stack.Pop();
                                             break;

                                       default: break;
                                     }

                                         space.Step(Proto.dtRsTime, ref updates);

                                         int k = 0;
                                         bool collided = false;
                                         foreach(Strip Strip in strips) Strip.CheckCollisions(vehicles);
                                         foreach(Vehicle Vehicle in vehicles) Vehicle.CheckCollisions(vehicles, ref k, ref collided);

                                         foreach(Vehicle Vehicle in vehicles)
                                         {
                                             if (Vehicle.Life <= 0.0f) 
                                             {
                                                 drops.Add(Vehicle);
                                             }
                                         }

                                         new ESync(queue, u_evo, players, updates, drops);

                                         foreach(Vehicle Vehicle in drops)
                                         {
                                                 vehicles.Remove(Vehicle);

                                             if ((Vehicle.Flags[Proto.Vehicle.SI_MODS] & Proto.Vehicle.MOD_LANDING) != 0)
                                             {
                                                 if (Vehicle.Flags[Proto.Vehicle.SI_OWNER] != 0)
                                                 {   gamerScorePlusPlus(Vehicle.Flags[Proto.Vehicle.SI_OWNER]);
                                                  
                                                     if ((++u_caught % 5) == 0)
                                                         if (u_difficulty  < 11)
                                                             u_difficulty++;
                                                 }
                                             }

                                                 Vehicle.Dispose();
                                         }

                                         updates.Clear();
                                         drops.Clear();

                                     if (collided)
                                     {
                                         new EOver(queue);
                                         cx.x = 0x21;
                                     }
                                         u_evo += 1;

                               }
                                     break;

                               case  0x21:
                               {
                                     lock(sessions) {

                                          foreach (KeyValuePair<UId, Session> Kvp in sessions)
                                          {
                                                   Kvp.Value.Dispose(null);
                                          }
                                     }

                                     Stack.Pop();
                               }
                                     break;
                             }

                             while(queue.Count > 0)
                             {
                                   int   x  = 0;
                                   Event Ev = queue.Dequeue();

                                   while (x < players.Length)
                                   {
                                      if (players[x] != null)
                                          players[x].Post(Ev);
                                          x++;
                                   }
                             }

                             lock(sessions) {

                                  foreach (KeyValuePair<UId, Session> Kvp in sessions)
                                  {
                                      if ((Kvp.Value.IdleTime += Proto.msRsTime) > Proto.msDcTime)
                                      {
                                          Kvp.Value.Dispose(null);

                                          for (int x = 0; x != players.Length; x++)
                                          {
                                               if (players[x] == Kvp.Value)
                                                   players[x]  = null;
                                          }

                                          zombies.Add(Kvp.Key);
                                      }
                                  }

                                  foreach (UId Id in zombies)
                                  {
                                          sessions.Remove(Id);
                                  }

                                          zombies.Clear();
                             /*lock*/}

                             Thread.Sleep(Proto.msRsTime);
                       }
                       catch(ThreadInterruptedException e)
                       {
                             Console.WriteLine("[World:{0}] Interrupted.", GetHashCode());
                       }
                       catch(Exception e)
                       {
                             Console.WriteLine("[World:{0}] Crashed: {1}", GetHashCode(), e.ToString());
                       }
                       finally
                       {
                            status = 0;
                       }
                 }
           }
           catch(Exception e)
           {
           }
   }

   private void _msg_srt(byte gid, byte[] data)
   {
           int x = 0;

           int obj = 0;
           Proto.getn(ref data, ref x, out obj);

           Thing thing = null;
           if (!space.things.TryGetValue(obj, out thing))
               return;

           int cnt = 0;
           Proto.getn(ref data, ref x, out cnt);

           if (cnt == 0)
           return;

           Vehicle vehicle = (Vehicle)thing;

           float px;
           float py;

           int n = 0;

           vehicle.Route = new Queue<PointF>();

           while (n != cnt)
           {
                  Proto.getf(ref data, ref x, out px);
                  Proto.getf(ref data, ref x, out py);
                  vehicle.Route.Enqueue(new PointF(px, py));
                  n++;
           }
   }

   private void _msg_sfl(byte gid, byte[] data)
   {
           int x = 0;

           int obj = 0;
           Proto.getn(ref data, ref x, out obj);

           Thing thing = null;

           if (!space.things.TryGetValue(obj, out thing))
               return;

           byte[] flags = new byte[4];

           Proto.getv(ref data, ref x, ref flags);

           if ((flags[Proto.Vehicle.SI_MODS] & Proto.Vehicle.MOD_OWNED) != 0)
               flags[Proto.Vehicle.SI_OWNER] = gid;
               else
               flags[Proto.Vehicle.SI_OWNER] = 0;

           thing.Flags = flags;
   }

   private void _msg_up(byte gid, byte[] data)
   {
           int x = 0;

           int obj = 0;
           Proto.getn(ref data, ref x, out obj);

           Thing thing = null;
           if (!space.things.TryGetValue(obj, out thing))
               return;
   }

   private Player getGamerId(byte id)
   {
           foreach(Player p in players)
             if (p != null)
                 if (p.getgid() == id)
                     return p;

           return null;
   }

   private void gamerScorePlusPlus(byte gamer)
   {
           Player p = getGamerId(gamer);

       if (p != null)
           p.ScorePlusPlus();
   }

   public  void PostGameMessage(Message msg)
   {
       if (messg != null)
           messg.Enqueue(msg);
   }

           bool disposed;

   public  void Dispose()
   {
       if (disposed)
           return;

           disposed = true;

           if (thread.IsAlive)
           {
               PostGameMessage(new Message(0, GameMessage.M_KILL, null));
               thread.Join();
           }

           lock(sessions)
           {
                foreach(KeyValuePair<UId, Session> Ent in sessions)
                {
                        Ent.Value.Dispose(this);
                }
           }

           broadcast.Dispose();

           server.Dispose();

           Console.WriteLine("[World:{0}] Disposed.", GetHashCode());
           instance = null;
   }

   public  void  Waitfor()
   {
       if (thread.IsAlive)
       {
           thread.Join();
       }
   }

   private static World instance = null;

   public  static World New(ref Config config)
   {
       if (instance != null)
           return instance;

           return instance = new World(ref config);
   }

   public  static World Instance()
   {
           return instance;
   }
}

internal class GameMessage
{
   public const int M_START = 0x01;
   public const int M_SRT = 0x11;
   public const int M_SFL = 0x12;
   public const int M_KILL = 0xfe;

   public static byte[] Pack(byte[] data, long r)
   {
          long   len = data.Length - r;
          byte[] ret = new byte[len];
          Array.Copy(data, r, ret, 0, len);
          return ret;
   }
}

class ServerEvent :Event
{
   public const int E_NONE = 0;

   public ServerEvent(EventQueue queue, int eno)
   :base(queue, eno)
   {
   }
}

/*namespace Core.World*/ }
