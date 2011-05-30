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
using System.Drawing;
using Core.Scene;
using Core.Net;

namespace Core.World.Mods {

public class Vehicle :Thing
{
           Queue<PointF> path;
           PointF node;

   protected float lifetime = 30.0f;
   protected float runtime = 0.0f;

   protected vector velocity;

   public Vehicle(Space space, int id, int type)
   :base(space, id, type)
   {
         mode = INT_SOLID;
         path = new Queue<PointF>();
   }

           bool resync = true;

   public  override bool  Step(float dt)
   {
       if ((flags[Proto.Vehicle.SI_MODS] & Proto.Vehicle.MOD_LANDING) != 0)
       {
            lifetime -= dt;
       }

       if ((pos.x > 1.0f) || (pos.x < -1.0f) || (pos.y > 1.0f) || (pos.y < -1.0f))
       {
            lifetime -= dt;
       }

           PointF prev;

       if (path.Count != 0)
       {
           prev  = node;
           node  = path.Dequeue();
           pos.x = node.X;
           pos.y = node.Y;
           rot.z = (float)Math.Atan2(node.Y - prev.Y, node.X - prev.X);
       }   else
       {
           pos.x += dt * (float)Math.Cos(rot.z) * velocity.z;
           pos.y += dt * (float)Math.Sin(rot.z) * velocity.z;
       }

           return resync;
   }

   public  override void  Snap(float dt)
   {
           resync = false;
   }

   public  virtual  void CollideWith(Strip strip)
   {
       if ((flags[Proto.Vehicle.SI_MODS] & Proto.Vehicle.MOD_OWNED) == 0)
           return;

           pos.z = -1.0f;
           flags[Proto.Vehicle.SI_MODS] = (byte)(flags[Proto.Vehicle.SI_MODS] | Proto.Vehicle.MOD_LANDING);
           path  =  new Queue<PointF>();

           PointF   tp = new PointF(strip.Pos.x, strip.Pos.y);

       if (runtime > 0.0f)
       {
           if (pos.x >= tp.X)
           {
             //calculate touchdown point, considering runtime
               float   d = (float)(strip.Length - runtime * velocity.z);
               tp.X += d * strip.Alignment.x;
               tp.Y += d * strip.Alignment.y;
           }
       }

           PointF p = new PointF(pos.x, pos.y);
           double a = Math.Atan2(tp.Y - pos.y, tp.X - pos.x);

           while (true)
           {
                  p.X += (float)(Proto.dtRsTime * Math.Cos(a) * velocity.z);
                  p.Y += (float)(Proto.dtRsTime * Math.Sin(a) * velocity.z);

              if (p.X > tp.X)
                  break;

                  path.Enqueue(p);
           }

       if (runtime > 0.0f)
       {
           //8 points for stability (one wasn't enough)
           for (int n = 0; n != 8; n++)
           {    p.X += (float)(Proto.dtRsTime * strip.Alignment.x * velocity.z);
                p.Y += (float)(Proto.dtRsTime * strip.Alignment.y * velocity.z);
                path.Enqueue(p);
           }
       }   else
       {
           velocity = vector.Null;
       }

           SyncRt = true;
   }

   public  virtual  void CollideWith(Vehicle veh)
   {
           pos.z = -1.0f;
           flags[Proto.Vehicle.SI_MODS] = (byte)(flags[Proto.Vehicle.SI_MODS] | Proto.Vehicle.MOD_COLLIDED);
   }

   public  override bool  Serialize(ref byte[] data, ref int s)
   {
           base.Serialize(ref data, ref s);

       if (SyncRt)
       {
           Proto.putn(ref data, ref s, path.Count);

           foreach(PointF pt in path)
           {
                   Proto.putf(ref data, ref s, pt.X);
                   Proto.putf(ref data, ref s, pt.Y);
           }
       }   else
       {
           Proto.putn(ref data, ref s, 0);
       }

           return true;
   }

           float proximity;

   public  virtual void CheckCollisions(List<Vehicle> vehicles, ref int pointer, ref bool collided)
   {
           int x = pointer + 1;
           Vehicle v;

       if (pos.z >= 0.0f)
       {
           proximity = 1.0f;

           while (x < vehicles.Count)
           {
                  v = vehicles[x];

              if (v.pos.z == 0.0f)
              {
                  float d  = (float)Math.Sqrt(Math.Pow(v.pos.x - pos.x, 2.0f) +
                                              Math.Pow(v.pos.y - pos.y, 2.0f));

                  if (d < proximity)
                      proximity = d;

                  if (d < 0.08f)
                  {
                      CollideWith(v);
                      v.CollideWith(this);
 
                      collided = true;
                  }
              }
                  x++;
           }
       }

       if (proximity <= 0.2f)
           flags[Proto.Vehicle.SI_MODS] = (byte)(flags[Proto.Vehicle.SI_MODS] | Proto.Vehicle.MOD_PROXI);
           else
           flags[Proto.Vehicle.SI_MODS] = (byte)(flags[Proto.Vehicle.SI_MODS] & ~Proto.Vehicle.MOD_PROXI);

           pointer++;
   }


           bool SyncRt;

   private void SetRoute(Queue<PointF> Path)
   {
       if ((flags[Proto.Vehicle.SI_MODS] & Proto.Vehicle.MOD_LANDING) == 0)
       {
           path = Path;
         //SyncRt = true;
       }

   }

   public  Queue<PointF> Route
   {
           get {return path;}
           set {SetRoute(value);}
   }

   public  float Life
   {
           get {return lifetime;}
   }

   public  vector Velocity
   {
           get {return velocity;}
   }
}

public class Combat :Vehicle
{
  public Combat(Space space)
  :base(space, -1, TYPE_PLANE0)
  {
         runtime  = 35.0f;
         velocity = new vector(0.0f, 0.0f, 0.025f);
  }
}

public class Airliner :Vehicle
{
  public Airliner(Space space)
  :base(space, -1, TYPE_PLANE1)
  {
         runtime  = 40.0f;
         velocity = new vector(0.0f, 0.0f, 0.020f);
  }
}

public class Helicopter :Vehicle
{
  public Helicopter(Space space)
  :base(space, -1, TYPE_COPTER)
  {
         runtime  = 0.0f;
         velocity = new vector(0.0f, 0.0f, 0.010f);
  }
}

public class Ufo :Vehicle
{
  public Ufo(Space space)
  :base(space, -1, TYPE_UFO)
  {
  }
}

/* namespace Core.World.Mods */ }
