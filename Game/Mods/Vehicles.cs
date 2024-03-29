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
using SdlDotNet;
using SdlDotNet.Graphics;
using Core.Scene;
using Core.Net;
using Game.Ui;

namespace Game.Mods {

public class Vehicle :Thing
{
   protected float rolltime;
   protected vector velocity;
   protected Surface model;
   protected Surface fx;

   public class Path :Queue<PointF>  {}

           Path path;
           PointF node;

   protected int width = 48;
   protected int height = 48;

   public Vehicle(Space space, int id, int type)
   :base(space, id, type)
   {
         mode = INT_SOLID;
   }

           int tstep;
           int tfx;
           int alpha = 255;
           float talpha;

   public  override bool  Step(float dt)
   {
           pos.x += dt * (float)Math.Cos(rot.z) * velocity.z;
           pos.y += dt * (float)Math.Sin(rot.z) * velocity.z;

      if  (ai != null)
           ai.Step(this, dt);

      if ((flags[Proto.Vehicle.SI_MODS] & Proto.Vehicle.MOD_LANDING) != 0)
      {
              talpha += dt;
          if (talpha > rolltime)
          {
              if (alpha > 0)
                  alpha -= 1;
          }
      }

           return true;
   }

   public  override void  Snap(float dt)
   {
           float x = 0;

      /*
      if ((flags[Proto.Vehicle.SI_MODS] & Proto.Vehicle.MOD_LANDING) != 0)
           if (path != null)
               path  = null;
     */   
           while (x < dt)
           {
              if (path != null)
              {
                  PointF prev;

                  if (path.Count != 0)
                  {
                      prev  = node;
                      node  = path.Dequeue();

                      rot.z = (float)Math.Atan2(node.Y - prev.Y, node.X - prev.X);

                      if (path.Count == 0)
                          path = null;
                  }
              }

                  x += Proto.dtRsTime;
          }
   }

   public  override void  Draw(Surface target, int nDetail)
   {
           int index = (int)Math.Round(rot.z  * 4.0f / Math.PI);

           if (index < 0)
               index = 8 + index;

               Point   p = space.SpaceToDesktop(pos.x, pos.y);
                       p.Offset(-width / 2, -height / 2);

               Rectangle r = new Rectangle(index * width, 0, width, height);

            if (++tfx > 3)
                tfx = 0;

                Rectangle f = new Rectangle(tfx * width, 0, width, height);

            if (((flags[Proto.Vehicle.SI_MODS] & Proto.Vehicle.MOD_COLLIDED) != 0) || (alpha < 255))
            {
                if (tstep++ > 255)
                    tstep = 0;

                if ((tstep % 8) > 4)
                {
                     target.Blit(model, p, r);

                     if (fx != null)
                         target.Blit(fx, p, f);
                }

            }   else
            {
                target.Blit(model, p, r);

                if (fx != null)
                    target.Blit(fx, p, f);
            }
   }

   public  override bool  Deserialize(ref byte[] data, ref int r)
   {
           base.Deserialize(ref data, ref r);

           int npts;

           Proto.getn(ref data, ref r, out npts);

      if (npts > 0)
      {
               int x = 0;
               float px;
               float py;

               path = new Vehicle.Path();
 
               while (x != npts)
               {
                      Proto.getf(ref data, ref r, out px);
                      Proto.getf(ref data, ref r, out py);
                      path.Enqueue(new PointF(px, py));
                      x++;
               }
      }
 
           return true;
   }

   public  Path Route
   {
           get {return path;}
           set {path = value;}
   }

   public  vector Velocity
   {
           get {return velocity;}
   }
}

public class Combat :Vehicle
{
  public Combat(Space space, int id)
  :base(space, id, TYPE_PLANE0)
  {
         model = new Surface("scene/plane-red.png");
         velocity = new vector(0.0f, 0.0f, 0.025f);
         rolltime = 15;
  }
}

public class Airliner :Vehicle
{
  public Airliner(Space space, int id)
  :base(space, id, TYPE_PLANE1)
  {
         model = new Surface("scene/plane-blue.png");
         velocity = new vector(0.0f, 0.0f, 0.020f);
         rolltime = 15;
  }
}

public class Helicopter :Vehicle
{
  public Helicopter(Space space, int id)
  :base(space, id, TYPE_COPTER)
  {
         width = 96;
         height = 96;
         model = new Surface("scene/copter-body.png");
         fx = new Surface("scene/copter-prop.png");
         velocity = new vector(0.0f, 0.0f, 0.010f);
         rolltime = 0;
  }
}

public class Ufo :Vehicle
{
  public Ufo(Space space, int id)
  :base(space, id, TYPE_UFO)
  {
  }
}

/* namespace Game.Mods */ }
