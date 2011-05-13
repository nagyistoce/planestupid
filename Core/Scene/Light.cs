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
using Core.Scene;
using Tao.OpenGl;

namespace Core.Scene {

public class Light :Thing
{
           int     num;
   public  float[] pos     = new float[]{0.0f, 0.0f, 2.0f, 1.0f};
   public  float[] ambient = new float[]{0.5f, 0.5f, 0.5f, 1.0f};
   public  float[] diffuse = new float[]{1.0f, 1.0f, 1.0f, 1.0f};

   public Light(Space space, int id, int num)
   :base(space, id, TYPE_LIGHT)
   {
           this.num = num;
           Reset();
           Enable();
   }

   public  void Reset()
   {
           Gl.glLightfv(num, Gl.GL_POSITION, pos);
           Gl.glLightfv(num, Gl.GL_AMBIENT, ambient);
           Gl.glLightfv(num, Gl.GL_DIFFUSE, diffuse);
   }

   public  void Enable(bool e)
   {
       if (e)
           Enable();
           else
           Disable();
   }

   public  void Enable()
   {
           Gl.glEnable(num);
   }

   public  void Disable()
   {
           Gl.glDisable(num);
   }
}

/*namespace Core.Scene */ }
