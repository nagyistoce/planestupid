
using System;

namespace Core {

public interface iWire
{
   void enqueue(byte[] src);
   void dequeue(out byte[] dst);
   void drop();
}

/*namespace Core*/ }
