
using System;
using System.Collections.Generic;

namespace Core {

/* World
*/

public interface iServer
{
       iWire RxOpen(UId key);
       iWire RxClose(UId key);
       void  Dispose();
       void  WaitFor();
}

public interface iBroadcast
{
       iWire TxOpen(UId key);
       iWire TxClose(UId key);
       void  Dispose();
       void  WaitFor();
}

public interface iMux
{
}

public interface iDemux
{
       bool  accept(UId key);
}

public class World
{
           SortedDictionary<UId, Session> sessions;

           iServer server;

   private class  Mux :iMux
   {
   }
           Mux    mux;

   private class  Demux :iDemux
   {
           public bool accept(UId key)
           {
               if (!instance.sessions.ContainsKey(key))
               {
                   new Fallback(key, true, true);
               }

                   return true;
           }
   }

           Demux  demux;

   private class  Session :IDisposable
   {
           UId    Id;
           iWire  Rx;
           iWire  Tx;

           public Session(UId key, bool receive, bool transmit)
           {
                   this.Id = key;

               if (receive)
                   this.Rx = instance.server.RxOpen(Id);

               if (transmit)
                   this.Tx = null;

                   instance.UpdateSession(Id, this);
           }

           ~Session()
           {
                   Dispose();
           }

           protected void recv()
           {
           }

           protected void send()
           {
           }

           private bool disposed;

           public void Dispose()
           {
               if (disposed)
                   return;

                   instance.UpdateSession(Id, null);

               if (Tx != null)
                   Tx = null;

               if (Rx != null)
                   instance.server.RxClose(Id);

                   disposed = true;
                   GC.SuppressFinalize(this);
           }
   }

   private class  Player
   {
   }

   private class  Fallback :Session
   {
           public Fallback(UId h, bool receive, bool transmit) :base(h, receive, transmit)
           {
           }
   }

           Player p1;
           Player p2;
           Player p3;
           Player p4;

   public  static int m_local = 1;
   public  static int m_network = 2;

           int mode;

           int gf;

   World()
   {
       try
       {
               sessions = new SortedDictionary<UId, Session>();

               mux = new Mux();

               demux = new Demux();

           if ((mode & m_local) != 0)
           {
               throw new Exception("Local Game not implemented as for now.");
           }   else
           {
               server = new Net.Server(demux);
           }

               gf = 1;
       }
       catch(Exception e)
       {
               Console.WriteLine("[World] Failed to instantiate: {0}", e.ToString());
               dispose();
       }
   }

   ~World()
   {
       if (gf != 0)
           dispose();
   }

   private void   dispose()
   {
       if (server != null)
       {
           server  = null;
       }

           gf = 0;
   }

   private void   UpdateSession(UId key, Session session)
   {
       if (session == null)
       {   DeleteSession(key);
           return;
       }

           sessions[key] = session;
   }

   private void   DeleteSession(UId key)
   {
           sessions.Remove(key);
   }

   private static World instance;

   public  static World Create()
   {
       if (instance == null)
       {
           instance  = new World();
       }

          return instance;
   }

   public  static World Destroy()
   {
       if (instance != null)
       {
           instance  = null;
       }
           return instance;
   }

   public  static World getInstance()
   {
           return instance;
   }

   public  static void  WaitFor()
   {
       if (instance.server != null)
           instance.server.WaitFor();
   }
 }

/*Cortex*/

/*namespace Core*/ }
