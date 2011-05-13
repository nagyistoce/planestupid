
using System;

namespace Core {

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

   public  UId(System.Net.IPEndPoint point)
  :this(point.Address.GetAddressBytes(), point.Port)
   {
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

           if ((re = c1 - c2) == 0)
           {
               re = lhs.Port - rhs.Port;
           }

               return re;
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
           get {return new UId(new byte[4], 0);}
   }
}

public class UIdComparer :System.Collections.Generic.IComparer<UId>
{
   public  int Compare(UId lhs, UId rhs)
   {
           return UId.cmp(lhs, rhs);
   }
}

public class UInfo
{
}

/*namespace Core*/ }
