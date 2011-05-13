using System;

namespace Core.Net {

public class TxWire :iWire
{
   public TxWire()
   {
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
   }

   public  void dequeue(out byte[] dst)
   {
           dst = null;
   }

   public  void drop()
   {
   }
}

/*namespace Core.Net*/ }
