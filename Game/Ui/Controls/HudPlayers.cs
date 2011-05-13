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
using SdlDotNet.Input;
using SdlDotNet.Audio;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Sprites;
using SdlDotNet.Graphics.Primitives;
using Core.Cortex;
using Core.Scene;
using Core.Net;

namespace Game.Ui.Controls {

public class HudTable :Widget
{
           Cortex  cortex;

           List<Gamer> players;

           bool enabled = true;

   public HudTable(Widget parent, int x, int y)
   :base(parent, x, y, 192, 128, true)
   {
           this.players = new List<Gamer>();
           
           int  g = 1;
           while (g <= Proto.MaxGamers)
           {
                  Add(g);
                  g++;
           }
   }

   private bool Add(int team)
   {
           lock(players) {

               players.Add(new Gamer(this, team));
               resync = true;

           /* lock */ }

              return true;
   }

   private void Clear()
   {
           lock(players) {

               players.Clear();
               resync = true;

           /* lock */ }
   }

   public Gamer Get(int team)
   {
          return players[team];
   }

   public void  MarkAll()
   {
           lock(players) {

           foreach (Gamer player in players)
           {
                   player.Marked = true;
           }

           /* lock */ }
   }

   public void  DisableMarked()
   {
           lock(players) {

           foreach (Gamer player in players)
           {
              if   (player.Marked)
                    player.SetEnabled(false);
           }

           /* lock */ }
   }

   protected bool idle = false;
   protected bool resync = true;

   public  override bool Sync(float dt)
   {
       if (idle)
           return false;

           lock(players) {

           foreach (Gamer player in players)
           {
                   resync |= player.Sync(dt);
           }

           /* lock */ }
            return resync;
   }

   public  override void Draw(Surface dst)
   {
       if (idle)
           return;

       if (resync)
       {
           lock(players) {

           Surface.Fill(Color.FromArgb(0, 0, 0, 0));

           int  y = 4;

           foreach (Gamer player in players)
           {
               if  (player.Enabled)
               {
                    player.X = 4;
                    player.Y = y;
                    player.Draw(Surface);
                    y += player.Height + 2;
               }
           }

           /* lock */ }
           Surface.Update();
           resync = false;
       }

           dst.Blit(this);
   }
}

public class Gamer :Widget
{
           int    team = 0;
           int    user = 0;
           int    score = 0;
           string name = "<>";

           Surface sfIcon;
           Surface sfName;
           Surface sfScore;

           SdlDotNet.Graphics.Font NameFont;
           SdlDotNet.Graphics.Font ScoreFont;

           bool   enabled;

   public Gamer(Widget parent, int team)
   :base(parent, 0, 0, 128, 32, true)
   {
           Surface.Alpha = 252;

           sfIcon = new Surface(32, 32, 32, true);
           sfIcon.Draw(new Box((short)0, (short)0, (short)32, (short)32), Color.Red, true, true);
           sfIcon.Blit(new Surface("res/score-overlay.png"));

           NameFont = new SdlDotNet.Graphics.Font("res/xscale.ttf", 14);
           SetName("");

           ScoreFont = new SdlDotNet.Graphics.Font("res/xscale.ttf", 20);
           SetScore(0);
   }

             bool resync = true;
             bool marked;

   public  override bool Sync(float dt)
   {
           return resync;
   }

   public  override void Draw(Surface dst)
   {
       if (resync)
       {
           Surface.Fill(Color.FromArgb(0, 0, 0, 0));
           Surface.Blit(sfIcon, new Point(0, 0));
           Surface.Blit(sfName, new Point(36, 0));
           Surface.Blit(sfScore, new Point(36, 14));
           Surface.Update();
           resync = false;
       }
 
           dst.Blit(this);
   }

   public  void SetName(string name)
   {
           lock(this)
           {
                sfName = NameFont.Render(name, Color.White, false);
                this.name = name;
                this.resync = true;
           }
   }

   public  void SetScore(int score)
   {
           lock(this)
           {
                sfScore = ScoreFont.Render(score.ToString(), Color.Purple, false);
                this.score = score;
                this.resync = true;
           }
   }

   public  void SetEnabled(bool enabled)
   {
           lock(this)
           {
                if (this.enabled ^ enabled)
                {
                    this.enabled = enabled;
                    this.resync  = true;
                }
           }
   }

   public  int User
   {
           get{return user;}
           set{user = value;}
   }

   public  int Team
   {
           get {return team;}
   }

   public  bool Enabled
   {
           get {return enabled;}
   }

   public  bool Marked
   {
           get {return marked;}
           set {marked = value;}
   }
}

/* namespace Game.Ui.Controls */ }
