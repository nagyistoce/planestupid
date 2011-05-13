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
/*
public interface iWire
{
       void enqueue(byte[] src);
       void dequeue(out byte[] dst);
}
 */

public class iWire
{
          public int ctx;
          public int crx;

   public delegate void FListener(iWire sender, byte[] data);

   public virtual void enqueue(byte[] src)
   {
          ctx = 0;
   }

   public virtual bool dequeue(out byte[] dst)
   {
          crx = 0;
          dst = null;
          return false;
   }

   protected FListener function;

   public bool setListener(FListener function)
   {
       if (this.function != null)
           if (this.function != function)
               return false;

           this.function = function;
           return true;
   }

   public FListener Listener
   {
          get {return function;}
          set {setListener(value);}
   }
}

/*namespace Core.Net*/ }
