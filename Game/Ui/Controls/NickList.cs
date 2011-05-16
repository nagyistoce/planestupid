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
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Sprites;

namespace Game.Ui.Controls {

public class Nick :Widget
{
           int    id;
           string name = "[none]";
           int    team = 0;

           Surface snick;
           Surface sname;
           Surface sbtn;
           Surface steam;

           SdlDotNet.Graphics.Font font;

   public Nick(Widget parent, int uid, int x, int y)
   :base(parent, x, y, 192, 20, true)
   {
           id = uid;

           Surface.Alpha = 192;
           font = new SdlDotNet.Graphics.Font("res/andalemo.ttf", 12);

           snick = new Surface("res/bxnick.png");
           setName(name);
           setTeam(0);
   }

   public  void setName(string name)
   {
       if ((this.name = name) == null)
           this.name = "<unknown>";

           this.sname = font.Render(name, Color.White, true);
           resync = true;
   }

   public  void setTeam(int team)
   {
           this.team = team;

       if (team > 3)
           team = 3;
           
           sbtn  = new Surface(
                   String.Format("res/bxteam-{0}.png", team));

           resync = true;
   }

   protected bool resync;

   public  override bool Sync(float dt)
   {
            return resync;
   }

   public  override void Draw(Surface dst)
   {
       if (resync)
       {
           Surface.Fill(Color.FromArgb(0, 0, 0, 0));

           Surface.Blit(snick, new Point(0, 0));

           Surface.Blit(sname, new Rectangle(4, 2, 136, 16));

           Surface.Blit(sbtn,  new Point(144, 0));

           Surface.Update();

           resync = false;
       }
 
           dst.Blit(this);
   }

   public  int Id
   {
           get {return id;}
   }
}

public class NickList :Widget
{
           List<Nick> names;

           bool enabled = true;

   public NickList(Widget parent, int x, int y)
   :base(parent, x, y, 192, 192, true)
   {
           names = new List<Nick>();
   }

   public  void Add(int uid, string name, int team)
   {
       int px = 0;
       int py = names.Count * 24;

           lock(names) {

                Nick nick = new Nick(this, uid, px, py);
                nick.setName(name);
                nick.setTeam(team);
                names.Add(nick);

           }
           resync = true;
   }

   private bool Find(int uid, out Nick ret)
   {
           lock(names) {

                foreach(Nick nick in names)
                {
                   if (nick.Id == uid)
                   {   ret = nick;
                       return true;
                   }
                }
           }

           ret = null;
           return false;
   }

   public  void UpdateNick(int uid, string Nick)
   {
           Nick nick;

       if (Find(uid, out nick))
       {
           nick.setName(Nick);
           resync = true;
       }
   } 

   public  void UpdateTeam(int uid, int Team)
   {
           Nick nick;

       if (Find(uid, out nick))
       {
           nick.setTeam(Team);
           resync = true;
       }
   }

   public  void Drop(int uid)
   {
           Nick nick;

       if (Find(uid, out nick))
       {
           lock(names) {

                names.Remove(nick);

                int py = 0;

                foreach(Nick ent in names)
                {
                        ent.Y = py;
                        py += ent.Height + 4;
                }
           
           }

           resync = true;
       }
   }

   public  void Clear()
   {
           lock(names) {

                names.Clear();

           }
           resync |= true;
   }

   protected bool idle = false;
   protected bool resync = true;

   public  override bool Sync(float dt)
   {
       if (idle)
           return false;

           lock(names) {

               foreach (Nick nick in names)
               {
                   resync |= nick.Sync(dt);
               }
           }

               return resync;
   }

   public  override void Draw(Surface dst)
   {
       if (idle && !resync)
           return;

           lock(names) {

                Surface.Fill(Color.FromArgb(0, 0, 0, 0));

                foreach (Nick nick in names)
                {
                         nick.Draw(Surface);
                }
           }

           Surface.Update();
           resync = false;

           dst.Blit(this);
   }

   public  void Suspend()
   {
           idle = true;
   }

   public  void Resume()
   {
           idle = false;
           resync = true;
   }
}

/* namespace Game.Ui.Controls */ }
