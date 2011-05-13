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
using SdlDotNet.Core;
using SdlDotNet.Input;
using SdlDotNet.Audio;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Sprites;

namespace Game.Ui.Controls {

public class Hourglass :Widget
{
   public  string Caption {get; set;}

             Surface snapshots;

   public  Hourglass(Widget parent, int x, int y) 
   :base(parent, x, y, 32, 32, true)
   {
           snapshots = new Surface("res/hourglass.png");
   }

           bool show;
           bool resync = true;

           int  index = 0;
           float timer = 0.0f;

   public  override bool Sync(float dt)
   {
       if (show)
       {
               timer += dt;

           if (timer >= 0.1f)
           {
               index++;

               if (index == 8)
                   index  = 0;

               resync = true;

               timer -= 0.1f;
           }
       }

           return resync;
   }

   public  override void Draw(Surface dst)
   {
       if (resync)
       {
           Surface.Fill(Color.FromArgb(0, 0, 0, 0));
           
           if (show)
           {
               Surface.Blit(snapshots, new Point(0, 0),
                            new Rectangle(index * 32, 0, 32, 32));
           }

           Surface.Update();

           resync = false;
       }
 
           dst.Blit(this);
   }

   public  void Show()
   {
           show = true;
           resync = true;
           index = 0;
   }

   public  void Hide()
   {
           show = false;
           resync = true;
   }
}

/*namespace Game.Ui.Controls*/ }
