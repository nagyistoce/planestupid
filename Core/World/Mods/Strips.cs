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

public class Strip :Thing
{
   protected int filter;
   protected vector a;
   protected const double l = 1.00f;
   protected const double r = 0.1f;

   public Strip(Space space, int flt, float px, float py, float drx, float dry)
   :base(space, -1, TYPE_STRIP)
   {
          filter = flt;
          a     = new vector(drx, dry, 0.0f);
          pos.x = px;
          pos.y = py;
   }

   public void CheckCollisions(List<Vehicle> vehicles)
   {
           foreach (Vehicle veh in vehicles)
             if (veh.Type == filter)
                 if (veh.Pos.z == 0.0f)
                     if (Math.Sqrt( Math.Pow(veh.Pos.x - pos.x, 2.0) + 
                                    Math.Pow(veh.Pos.y - pos.y, 2.0) ) <= r)
                                    CollideWith(veh);
   }

   public virtual void CollideWith(Vehicle veh)
   {
           veh.CollideWith(this);
   }

   public vector Alignment
   {
          get {return a;}
   }

   public double Length
   {
          get {return l;}
   }
}

/* namespace Core.World.Mods */ }
