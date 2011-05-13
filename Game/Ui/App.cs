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
using System.Drawing;
using System.Collections.Generic;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Sprites;

namespace Game.Ui {

public class App :Window
{
   public App() 
   :base(null, 0, 0, Desktop.Width, Desktop.Height, false)
   {
         ResetEnabled(this, true);
   }

   public  virtual  void DrawGl(float dt)
   {
   }

   public  override void Draw(Surface dst)
   {
       if (resync)
       {
           if (bgimage != null)
               dst.Blit(bgimage);
               else
               dst.Fill(bgcolor);

           lock(this)       
           {
                foreach(Widget w in widgets)
                {
                   w.Draw(dst);
                }
           }

           dst.Update();
           resync = false;
       }
   }
}

/*namespace Game.Ui*/ }
