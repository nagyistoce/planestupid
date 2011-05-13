using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Core;
using Core.Net;
using Core.Net.UDP;

namespace net1t
{
    class MainClass
    {
        public static void Main(string[] args)
        {
               Console.WriteLine("Phase 1...");

               UId a = new UId(IPAddress.Parse("127.0.0.1"));
               UId b = new UId(IPAddress.Parse("127.0.0.1"));

               int x = 0;
               bool t = false;
               bool done = false;

               while(!done)
               {
                     switch(x)
                     {
                       case 0:
                       {    t = UId.cmp(a, b) == 0; 
                            break;
                       }

                       case 1:
                       {
                            b = new UId(IPAddress.Parse("127.0.0.2"));
                            t = UId.cmp(a, b) < 0;
                            break;
                       }

                       case 2:
                       {
                            a = new UId(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10));
                            b = new UId(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10));
                            t = UId.cmp(a, b) == 0;
                            break;
                       }
 
                       case 3:
                       {
                            a = new UId(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10));
                            b = new UId(new IPEndPoint(IPAddress.Parse("127.0.0.2"), 10));
                            t = UId.cmp(a, b) != 0;
                            break;
                       }

                       case 4:
                       {
                            a = new UId(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10));
                            b = new UId(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
                            t = UId.cmp(a, b) == 0;
                            break;
                       }

                       case 5:
                       {
                            a = new UId(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 33251));
                            b = new UId(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 33252));
                            t = UId.cmp(a, b) != 0;
                            break;
                       }

                       default: 
                            done = true;
                            break;
                     }

                  if (!done)
                  {
                      if (t)
                          Console.WriteLine("Test {0}: OK", x);
                          else
                          Console.WriteLine("Test {0}: Failed", x);
                  }

                     x++;
               }

               Console.WriteLine("Phase 2...");
               SortedDictionary<UId, string> dict = new SortedDictionary<UId, string>(new UIdComparer());

               dict.Add(new UId(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 33257)), "c");
               dict.Add(new UId(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 33255)), "d");
               dict.Add(new UId(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 33253)), "x");
               dict.Add(new UId(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 33254)), "b");
               dict.Add(new UId(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 33251)), "a");

               string v = dict[new UId(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 33251))];

               if (v == "a")
                   Console.WriteLine("Test {0}: OK", x);
                   else
                   Console.WriteLine("Test {0}: Failed", x);


               foreach(KeyValuePair<UId, string> Kvp in dict)
               {
                       Console.Write("{0} ", Kvp.Value);
               }
  
        }
    }
}