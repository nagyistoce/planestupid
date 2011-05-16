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
using Core.Net;
using Core.Scene;

namespace Core.World {

public class Player :Session
{
           int   flags = Proto.F_TEST;
           int   perms = 0;
           int   score = 0;

           bool  fresh = true;
           string nick = null;

           EventQueue priv;
           EventQueue broad;


   public const int S_READY = 1;
   public const int S_SYNC = 2;

           int status;

   public Player(World world, UId id)
   :base(world, id, true, true, true)
   {
           priv = new EventQueue(Tx);
           broad = new EventQueue(Bx);

           Console.WriteLine("Player {0} On", GetHashCode());
   }

           byte op;

   protected override void execute()
   {
       try
       {
           while(!done)
           {
                   int n;
                   int r = 0;
                   byte[] recv = null;

               if (!Rx.dequeue(out recv))
                  break;

               if ((n = recv.Length) < 1)
                  break;

                  idle = 0;
                  //Console.WriteLine("Player:{0} Recv 0x{1} {2}bytes", Id.ToString(), Convert.ToString(recv[0], 16).PadLeft(2, '0'), recv.Length);

                  try 
                  {
                       switch(op = recv[r++])
                       {

                         case Proto.World.PING:
                         {
                              if ((perms & Proto.P_USER) == 0)
                                  break;

                                  PlayerEvent.Ping(priv);
                         }
                              break;

                         case Proto.World.HI:
                         {
                                  PlayerEvent.Hello(priv);
                         }
                              break;

                         case Proto.World.LOGIN:
                         case Proto.World.RENAME:
                         {
                              if (!Proto.gets(ref recv, ref r, out nick))
                                  break;

                                  locknick(ref nick);

                              if (fresh)
                              {
                                   if (!register())
                                       break;

                                      PlayerEvent.Login(priv, getflags(), getperms(), nick);
                                      PlayerEvent.Names(priv, world.players);
                                      fresh = false;
                              }

                                  PlayerEvent.Join(broad, getflags(), getperms(), nick);
                         }
                              break;

                         case Proto.World.LOGOUT:
                         {
                               PlayerEvent.Leave(broad, getflags(), "Left");
                               perms = 0;
                         }
                              break;

                         case Proto.World.PREP:
                         {
                              if ((perms & Proto.P_USER) == 0)
                                  break;

                              if ((perms & Proto.P_OP) == 0)
                                  break;

                                  world.PostGameMessage(new Message(getgid(), GameMessage.M_START, null));
                         }
                              break;

                         case Proto.World.READY:
                         {
                              if ((perms & Proto.P_USER) == 0)
                                  break;

                                  status |= S_READY;
                         }
                              break;

                         case Proto.World.SRT:
                         {
                              if ((perms & Proto.P_USER) == 0)
                                  break;

                                  world.PostGameMessage(new Message(getgid(), GameMessage.M_SRT, GameMessage.Pack(recv, r)));
                         }
                              break;

                         case Proto.World.SFL:
                         {
                              if ((perms & Proto.P_USER) == 0)
                                  break;

                                  world.PostGameMessage(new Message(getgid(), GameMessage.M_SFL, GameMessage.Pack(recv, r)));
                         }
                              break;

                         default:
                         {
                         }
                              break;
                       }
                       
                       priv.Send();
                       broad.Send();

                  }
                  catch(ServerException e)
                  {
                        Tx.enqueue(e.Serialize());
                  }

                        done = perms == 0;
           }
      }
      catch(ThreadAbortException e)
      {
            PlayerEvent.Leave(broad, getflags(), "Killed");
            broad.Send();
            Thread.Sleep(Proto.msPingTime / 5);
      }
      catch(Exception e)
      {
           Console.WriteLine("Player {0} Crashed: {1}", GetHashCode(), e.ToString());
           done = true;
      }
      finally
      {
           Console.WriteLine("Player {0} Dead.", GetHashCode());
           unregister();
           Dispose(this);
      }
   }

   /* misc functions */

   private bool register()
   {
           int sid = 0;
           int gid = 0;
           bool ok = false;

           lock(world) {

           int x;

           for (x = 0; x != world.players.Length; x++)
           {
              if (world.players[x] == null)
              {   world.players[x]  = this;
                  sid = x;
                  ok = true;
                  break;
              }
           }

           if  (ok)
           {
                  int y = 1;
                  bool found;

                  while(y < Proto.MaxGamers)
                  {
                       found = false;

                       for (x = 0; x != world.players.Length; x++)
                       {
                            if (world.players[x] != null)
                            {
                                if (world.players[x].getgid() == y)
                                {   found = true;
                                    break;
                                }
                            } 
                       }

                       if (!found)
                       {
                           gid = y;
                           break;
                       }

                       y++;
                  }

                  flags |= (gid << 4) | sid;

                  perms |= Proto.P_USER | Proto.P_VOICE;

              if (sid == 0)
                  perms |= Proto.P_OP;
           }
            /*lock*/ }

          return ok;
   }

   private void unregister()
   {
           lock(world) {
            world.players[getsid()] = null;
            perms  = 0;
            flags &= ~Proto._F_GID;
            flags &= ~Proto._F_SID;
           /*lock*/ }
   }

   private string rename(string nick)
   {
           int    npostfix = 1;
           string sbase;

           int n = 0;
           int x = nick.Length - 1;

           while(x > 0)
           {
              if (nick[x] >= '0' && nick[x] <= '9')
              {   npostfix += (int)Math.Pow(10, n++) * (nick[x] - '0');
                  x--;
              }
                  else
                  break;
           }

           if (x + n >= Proto.MaxNick)
               x = Proto.MaxNick - n;

           sbase = nick.Substring(0, x + 1);

           return  String.Format("{0}{1}", sbase, npostfix);
   }

   private void locknick(ref string nick)
   {
           bool unick;
           Player player;

           lock(world.sessions) {

           if  (nick.Length > Proto.MaxNick)
                nick = nick.Substring(0, Proto.MaxNick);

           do
           {
                unick = true;

                foreach(KeyValuePair<UId, Session> e in world.sessions)
                {
                    if (e.Value == this)
                        continue;

                    if (e.Value.GetType() != GetType())
                        continue;

                        player = (Player)e.Value;

                    if ((player.Permissions & Proto.P_USER) == 0)
                        continue;

                    if (player.Nick == nick)
                    {   nick = rename(nick);
                        unick = false;
                    }
               }
           }
           while(!unick);

           /*lock*/ }
   }

   private byte getflags()
   {
           return (byte)flags;
   }

   private byte getperms()
   {
           return (byte)perms;
   }

   public  int  getsid()
   {
           return flags & Proto._F_SID;
   }

   public  int  getgid()
   {
           return (flags & Proto._F_GID) >> 4;
   }

   public  int  getstatus()
   {
           return status;
   }

   public  void Post(Event Ev)
   {
           bool repost = true;

           switch(Ev.Eno)
           {
             case PlayerEvent.E_LIST:
                  repost = (status & S_SYNC) == 0;
                  status |= S_SYNC;
                  break;
           }

           if  (repost)
                Tx.enqueue(Ev.Send);
   }

   public  void ScorePlusPlus()
   {
           score++;
   }

   /* properties */

   public string Nick
   {
          get {return nick;}
   }

   public int Permissions
   {
          get {return perms;}
   }

   public int Flags
   {
          get {return getflags();}
   }

   public int Score
   {
          get {return score;}
   }
}

internal class PlayerEvent :Event
{
   public const int E_NONE = 0;
   public const int E_PING = 0x01;
   public const int E_HELLO = 0x02;
   public const int E_LOGIN = 0x03;
   public const int E_NAMES = 0x07;

   public const int E_JOIN = 0x10;
   public const int E_LEAVE = 0x11;

   public const int E_LOAD = 0x20;
   public const int E_LIST = 0x22;
   public const int E_SYNC = 0x23;
   public const int E_OVER = 0x2f;

   public PlayerEvent(EventQueue queue, int eno)
   :base(queue, eno)
   {
   }

   public static EPing Ping(EventQueue queue) {return new EPing(queue);}
   public static EHello Hello(EventQueue queue) {return new EHello(queue);}
   public static ELogin Login(EventQueue queue, byte f, byte p, string nick) {return new ELogin(queue, f, p, nick);}
   public static ENames Names(EventQueue queue, Player[] players) {return new ENames(queue, players);}
   public static EJoin Join(EventQueue queue, byte f, byte p, string nick) {return new EJoin(queue, f, p, nick);}
   public static ELeave Leave(EventQueue queue, byte f, string message) {return new ELeave(queue, f, message);}
   public static ELoad Load(EventQueue queue) {return new ELoad(queue);}
   public static EList List(EventQueue queue, Space space) {return new EList(queue, space);}
   //public static ESync Sync(EventQueue queue, Space space) {return new ESync(queue, space);}
}

internal class EPing :PlayerEvent
{
   public EPing(EventQueue queue)
   :base(queue, E_PING)
   {
           send = new byte[]{ Proto.Cortex.PONG };
   }
}

internal class EHello :PlayerEvent
{
   public EHello(EventQueue queue)
   :base(queue, E_HELLO)
   {
           send = new byte[]{
                  Proto.Cortex.HI,
                  Proto.MajorVersNo, Proto.MinorVersNo, Proto.MaxPlayers, 0
           };
   }
}

internal class ELogin :PlayerEvent
{
   public ELogin(EventQueue queue, byte f, byte p, string nick)
   :base(queue, E_LOGIN)
   {
           int s = 0;
           byte[] data = new byte[Proto.MaxNick + 2];

           Proto.putv(ref data, ref s, new byte[]{ Proto.Cortex.LOGIN, f, p });

           Proto.puts(ref data, ref s, nick);

           Proto.pack(ref data, ref s, ref send);
   }
}

internal class ENames :PlayerEvent
{
   public ENames(EventQueue queue, Player[] players)
   :base(queue, E_NAMES)
   {
         int s = 0;
         byte[] data = new byte[512];

         int x = 0;

                Proto.putv(ref data, ref s, new byte[]{ Proto.Cortex.NAMES });

         while (x != players.Length)
         {
                Player player = players[x];

            if (player != null)
            {
                Proto.putv(ref data, ref s, new byte[]{ (byte)player.Flags, (byte)player.Permissions });
                Proto.puts(ref data, ref s, player.Nick);
            }
                x++;
         }

                Proto.putv(ref data, ref s, new byte[]{ Proto.EONAMES });

                Proto.pack(ref data, ref s, ref send);
   }
}

internal class EJoin :PlayerEvent
{
   public EJoin(EventQueue queue, byte f, byte p, string nick)
   :base(queue, E_JOIN)
   {
           int s = 0;
           byte[] data = new byte[Proto.MaxNick + 2];

           Proto.putv(ref data, ref s, new byte[]{ Proto.Cortex.JOINED, f, p });

           Proto.puts(ref data, ref s, nick);

           Proto.pack(ref data, ref s, ref send);
   }
}

internal class ELeave :PlayerEvent
{
   public ELeave(EventQueue queue, byte f, string message)
   :base(queue, E_LEAVE)
   {
           int s = 0;
           byte[] data = new byte[512];

           Proto.putv(ref data, ref s, new byte[]{ Proto.Cortex.LEFT, f});

           Proto.puts(ref data, ref s, message);

           Proto.pack(ref data, ref s, ref send);
   }
}

internal class ELoad :PlayerEvent
{
   public ELoad(EventQueue queue)
   :base(queue, E_LOAD)
   {
           send = new byte[]{ Proto.Cortex.LOAD };
   }
}

internal class EList :PlayerEvent
{
   public EList(EventQueue queue, Space space)
   :base(queue, E_LIST)
   {
           int s = 0;
           byte[] data = new byte[16384];

           Proto.putv(ref data, ref s, new byte[]{ Proto.Cortex.LIST });

           space.Serialize(ref data, ref s);

           Proto.pack(ref data, ref s, ref send);
   }
}

internal class ESync :PlayerEvent
{
   public ESync(EventQueue queue, int evo, Player[] players, List<Thing> updates, List<Thing> drops)
   :base(queue, E_SYNC)
   {
           int s = 0;
           byte[] data = new byte[16384];

           /* pack timestamp */
           Proto.putv(ref data, ref s, new byte[]{ Proto.Cortex.SYNC });

           Proto.putn(ref data, ref s, evo);

           /* pack player scores */
           for (int x = 0; x != players.Length; x++)
           {
                if (players[x] != null)
                {
                    Proto.putv(ref data, ref s, new byte[]{ (byte)players[x].Flags });
                    Proto.putn(ref data, ref s, players[x].Score);
                }
           }

           Proto.putv(ref data, ref s, new byte[]{ Proto.EONAMES });

           /* pack objects to update */
           foreach(Thing thing in updates)
           {
                   Proto.putn(ref data, ref s, thing.Id);
                   Proto.putn(ref data, ref s, thing.Type);

                   thing.Serialize(ref data, ref s);
           }

                   Proto.putn(ref data, ref s, Proto.EOLIST);

           /* pack objects to destroy */
           foreach(Thing thing in drops)
           {
                   Proto.putn(ref data, ref s, thing.Id);
           }

                   Proto.putn(ref data, ref s, Proto.EOLIST);

           /**/
           Proto.pack(ref data, ref s, ref send);
   }
}

internal class EOver :PlayerEvent
{
   public EOver(EventQueue queue)
   :base(queue, E_OVER)
   {
           send = new byte[]{ Proto.Cortex.OVER };
   }
}
/* namespace Core.World */ }
