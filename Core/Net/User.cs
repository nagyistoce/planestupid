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

namespace Core.Net {

public class UId
{
           byte[] host;
           int    port;

   public  int Flags {get; set;}

   public  UId(byte[] hostbytes, int port) 
   {
           this.host = hostbytes;
           this.port = port;
   }

   public  UId(UId src)
  :this(src.Host, src.Port)
   {
   }

   public  UId(System.Net.IPAddress IP)
  :this(IP.GetAddressBytes(), 0)
   {
   }

   public  UId(System.Net.IPEndPoint point)
  :this(point.Address.GetAddressBytes(), point.Port)
   {
   }

   public  override string ToString()
   {
           int n = 0;
           string re = "";

           while (n < host.Length)
           {
              if (n > 0)
                  re += ".";

                  re += host[n].ToString();
                  n++;
           }

           re += string.Format(":{0}", port);

           return re;
   }

   public  static int cmp(UId lhs, UId rhs)
   {
           int re = 0;
           int c1 = 0, n1 = 0;
           int c2 = 0, n2 = 0;

           while (c1 == c2)
           {
              if ((n1 >= lhs.Host.Length) || (n2 >= rhs.Host.Length))
              {
                  c1  = n1;
                  c2  = n2;
                  break;
              }

                  c1  = lhs.Host[n1];
                  c2  = rhs.Host[n2];

                  n1++;
                  n2++;
           }

                  re = c1 - c2;

           if (re == 0)
           {
               if ((lhs.Port != 0) && (rhs.Port != 0))
               {
                   re = lhs.Port - rhs.Port;
               }
           }

               return re;
   }
/*
   public  static bool   operator==(UId lhs, UId rhs)
   {
       if ((lhs == null) || (rhs == null))
           return false;
            
           return cmp(lhs, rhs) == 0;
   }

   public  static bool   operator!=(UId lhs, UId rhs)
   {
       if ((lhs == null) || (rhs == null))
           return false;
            
           return cmp(lhs, rhs) != 0;
   }
*/
   public  System.Net.IPAddress IPv4
   {
           get {return new System.Net.IPAddress(host);}
   }

   public  byte[] Host
   {
           get {return host;}
   }

   public  int Port
   {
           get {return port;}
   }

   public  static UId Null
   {
           get {return new UId(new byte[]{0, 0, 0, 0}, 0);}
   }
}

public class UIdComparer :System.Collections.Generic.IComparer<UId>
{
   public  int Compare(UId lhs, UId rhs)
   {
       if (lhs == null)
           lhs  = UId.Null;

       if (rhs == null)
           rhs  = UId.Null;

           return UId.cmp(lhs, rhs);
   }
}

public class UInfo
{
}
/*namespace Core.Net*/ }
