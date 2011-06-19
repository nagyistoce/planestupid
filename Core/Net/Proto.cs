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

namespace Core.Net {

public class Proto
{
          public const int InPortNo = 0x2708;
          public const int OutPortNo = 0x2709;

          public const byte MajorVersNo = 0;
          public const byte MinorVersNo = 1;
 
          public const byte MaxPlayers = 8;
          public const byte MaxGamers = 2;
          public const byte MaxNick = 16;

          public const byte EONAMES = 0; //end-of-list for NAMES command
          public const int EOLIST = -1; //end of object list

          public const int  msPingTime = 5000;
          public const int  msDcTime = 50000; //disconnect time
          public const int  msLdTime = 10000; //load time
          public const int  msRsTime = 250; //time between scene syncs [ms]
          public const float dtRsTime = msRsTime / 1000.0f; //time between scene syncs [s]

          //flags
          public const int _F_SID = 0x0f;
          public const int _F_GID = 0x30;
          public const int _F_FLAGS = _F_SID | _F_GID;

          public const int  F_TEST = 0x40;
          public const int  F_SELF = 0x80;

          //permissions
          public const byte P_USER = 0x01;
          public const byte P_VOICE = 0x02;
          public const byte P_OP = 0x08;

   public class  World
   {
          public const byte NOP = 0;
          public const byte PING = 0x01;
          public const byte HI = 0x02;
          public const byte LOGIN = 0x03;
          public const byte RENAME = LOGIN;
          public const byte LOGOUT = 0x04;
          public const byte MESSAGE = 0x05;

          public const byte PREP = 0x20;
          public const byte READY = 0x21;
          public const byte SRT = 0x24;
          public const byte SFL = 0x25;
   }

   public class  Cortex
   {
          public const byte NOP = 0;
          public const byte PONG = 0x01;
          public const byte HI = 0x02;
          public const byte LOGIN = 0x03;
          public const byte LOGOUT = 0x05;
          public const byte NAMES = 0x07;

          public const byte JOINED = 0x10;
          public const byte LEFT = 0x11;
          public const byte NOTIFY = 0x13;

          public const byte LOAD = 0x20;
          public const byte LIST = 0x22;
          public const byte SYNC = 0x23;
          public const byte OVER = 0x2e;

          public const byte ERROR = 0xf0;
          public const byte KILL = 0xf1;
   }

   public class Vehicle
   {
          public const int SI_MODS = 0;
          public const int SI_OWNER = 1;

          public const byte MOD_OWNED = 1;
          public const byte MOD_PROXI= 2;
          public const byte MOD_LANDING = 8;
          public const byte MOD_COLLIDED = 0x80;
   }

          static System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding();
          static System.Text.ASCIIEncoding ascii = new System.Text.ASCIIEncoding();

   public static bool  getv(ref byte[] data, ref int r, ref byte[] pv)
   {
           int end = r + pv.Length;

       if (end > data.Length)
           return false;

           int x = 0;

           for(x = 0; x != pv.Length; x++)
           {
               pv[x] = data[r++];
           }

           return true;
   }

   public static bool  getn(ref byte[] data, ref int r, out int pn)
   {
       int end = r + sizeof(Int32);

       if (end > data.Length)
       {   pn = 0;
           return false;
       }

           pn = BitConverter.ToInt32(data, r);
           r = end;
           return true;
   }

   public static bool  getf(ref byte[] data, ref int r, out float pf)
   {
       int end = r + sizeof(Single);

       if (end > data.Length)
       {   pf = 0.0f;
           return false;
       }

           pf = BitConverter.ToSingle(data, r);
           r = end;
           return true;
   }

   public static bool  gets(ref byte[] data, ref int r, out string ps)
   {
           ps = null;

       if (r == data.Length)
           return false;

           int z = data[r++];

       if (z == 0)
           return true;

           int end = r + z;
       if (end > data.Length)
           return false;

           ps = utf8.GetString(data, r, z);
           r  = end;

           return true;
   }

   public static bool  putv(ref byte[] data, ref int s, byte[] pv)
   {
           int end = s + pv.Length;

       if (end >= data.Length)
           return false;

           Array.Copy(pv, 0, data, s, pv.Length);

           s = end;
           return true;
   }

   public static bool  putn(ref byte[] data, ref int s, int pn)
   {
          return putv(ref data, ref s, BitConverter.GetBytes(pn));
   }

   public static bool  putf(ref byte[] data, ref int s, float pf)
   {
          return putv(ref data, ref s, BitConverter.GetBytes(pf));
   }

   public static bool  puts(ref byte[] data, ref int s, string ps)
   {
           int len = 0;

       if (ps != null)
           len = ps.Length;

       if (len > 255)
           return false;

       if (len + 1 > data.Length)
           return false;

           data[s++] = (byte)len;

       if (len > 0)
           return putv(ref data, ref s, utf8.GetBytes(ps));

           return true;
   }

   public static bool  pack(ref byte[] data, ref int s, ref byte[] send)
   {
       if (data.Length > 65536)
           return false;

       if (data.Length == s)
       {   send = data;
           return true;
       }

       if (s > 65536)
           return false;

           send = new byte[s];

           System.Array.Copy(data, send, s);

           return true;
   }
}

public class EventQueue :Queue<Event>
{
           iWire Tx;

   public EventQueue(iWire Tx)
   :base()
   {
           this.Tx = Tx;
   }

   public void Send()
   {
           Event Ev;

           while(Count > 0)
           {
                 Ev = Dequeue();
                 Tx.enqueue(Ev.Send);
           }
   }
}

public class Event
{
   protected EventQueue queue;

   public const int E_NONE = 0;

           int eno = E_NONE;

   protected byte[] recv = null;
   protected byte[] send = null;

   public Event(EventQueue queue, int eno)
   {
           this.queue = queue;
           this.eno = eno;

       if (queue != null)
           queue.Enqueue(this);
   }

   public int Eno
   {
          get {return eno;}
   }

   public byte[] Send
   {
          get {return send;}
   }
}

public class MessageQueue :Queue<Message>
{
}

public class Message
{
           int gno = 0;

   public const int M_NONE = 0;

           int mno = M_NONE;

           byte[] data;

   public Message(int gno, int mno, byte[] data)
   {
           this.gno = gno;
           this.mno = mno;
           this.data = data;
   }

   public int Gno
   {
          get {return gno;}
   }

   public int Mno
   {
          get {return mno;}
   }

   public byte[] Data
   {
          get {return data;}
   }
}

public class ServerException :Exception
{
           byte Op;
           int Code;

   public ServerException(byte Op, int Code)
   {
           this.Op = Op;
           this.Code = Code;
   }

   public ServerException(byte[] data)
   {
          int r = 0;
          Deserialize(ref data, ref r);
   }

   public byte[] Serialize()
   {
          byte[] ret = new byte[8];
          Serialize(ref ret);
          return ret;
   }

   public virtual void Serialize(ref byte[] data)
   {
           int s = 0;
           Proto.putv(ref data, ref s, new byte[]{ Proto.Cortex.ERROR, Op });
           Proto.putn(ref data, ref s, Code);
   }

   public virtual void Deserialize(ref byte[] data, ref int r)
   {
           Op = data[r++];
           Proto.getn(ref data, ref r, out Code);
   }
}

/*namespace Core.Net*/ }
