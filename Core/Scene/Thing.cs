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
using SdlDotNet;
using SdlDotNet.Graphics;
using Core.Net;

namespace Core.Scene {

public class Thing
{
   protected int  id;
   protected byte[] flags = new byte[4];
   protected Space space;

   private static int ID_MAX = 65534;

   protected vector pos = vector.Null;
   protected vector rot = vector.Null;

   protected Mesh   mesh;
   protected Surface surface;

   public interface Ai
   {
          void    Step(Thing body, float dt);
   }

   public  Ai     ai {get; set;}

   public const int INT_NO = 0;
   public const int INT_SOLID = 1; //downstreamed to the cortex

   protected int  mode = INT_NO;

   public const int TYPE_UNDEF = 0;
   public const int TYPE_LIGHT = 0x01;
   public const int TYPE_GENERIC = 0x02;
   public const int TYPE_TERRAIN = 0x10;
   public const int TYPE_STRIP = 0x11;
   public const int TYPE_PLANE0 = 0x20;
   public const int TYPE_PLANE1 = 0x21;
   public const int TYPE_COPTER = 0x22;
   public const int TYPE_UFO = 0x2f;

   protected int  type = TYPE_UNDEF;
  
   /*
      id < 0 tells the Thing to assign itself one
   */

   public Thing(Space space, int id, int type)
   {
           this.space = space;
           this.type = type;

           int  cnt = space.things.Count;

       if (cnt == ID_MAX + 1)
           throw new Exception("Exceeding world space");

           lock(space) {

                if (id < 0)
                {
                    id = cnt;

                    if (space.things.ContainsKey(id))
                    {
                        int  prev = -1;

                        foreach (int lookup in space.things.Keys)
                        {
                             if (lookup - prev > 1)
                             {
                                 id = (lookup + prev) / 2;
                                 break;
                             }
                                 prev = lookup;
                        }
                    }
                }

           this.id = id;

           space.things.Add(this.id, this);

           /*lock*/ }
   }

   ~Thing()
   {
           Dispose();
   }

   public  virtual bool  Step(float dt)
   {
           return false;
   }

   public  virtual void  Snap(float dt)
   {
   }

   public  virtual void  Draw(Surface target, int nDetail)
   {
   }

   public  virtual void  DrawGl(int nDetail)
   {
   }

   public  virtual void  CollideWith(Thing thing)
   {
   }

   public  virtual bool  Serialize(ref byte[] data, ref int s)
   {
           Proto.putv(ref data, ref s, flags);

           Proto.putf(ref data, ref s, pos.x);
           Proto.putf(ref data, ref s, pos.y);
           Proto.putf(ref data, ref s, pos.z);

           Proto.putf(ref data, ref s, rot.x);
           Proto.putf(ref data, ref s, rot.y);
           Proto.putf(ref data, ref s, rot.z);

           return true;
   }

   public  virtual bool  Deserialize(ref byte[] data, ref int r)
   {
           Proto.getv(ref data, ref r, ref flags);

           Proto.getf(ref data, ref r, out pos.x);
           Proto.getf(ref data, ref r, out pos.y);
           Proto.getf(ref data, ref r, out pos.z);

           Proto.getf(ref data, ref r, out rot.x);
           Proto.getf(ref data, ref r, out rot.y);
      if (!Proto.getf(ref data, ref r, out rot.z))
           return false;

           return true;
   }

   public  byte[] Flags
   {
           get {return flags;}
           set {flags = value;}
   }

   public  vector Pos
   {
           get {return pos;}
           set {pos = value;}
   }

   public  vector Rot
   {
           get {return rot;}
           set {rot = value;}
   }

   public  int Id
   {
           get {return id;}
   }

   public  int Type
   {
           get {return type;}
   }

           bool disposed;

   public  void Dispose()
   {
       lock(this) {

           if (disposed)
               return;

           lock(space) {

           space.things.Remove(id);
           this.id = -1;
           this.disposed = true;

          /*lock*/ }
       /*lock*/ }
   }   
}

/*namespace Core.Scene*/ }
