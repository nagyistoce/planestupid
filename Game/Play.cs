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
using SdlDotNet.Graphics.Primitives;
using Core;
using Core.Net;
using Core.World;
using Core.Cortex;
using Core.Scene;
using Game.Ui;
using Game.Ui.Controls;
using Game.Mods;

namespace Game {

public class Play :App, Cortex.iDynamics
{
           Settings     Settings;
           World        World;
           Cortex       Cortex;

           GameBox      mGameBox;

           Space        space;

           HudTable     hud;
           List<Trap>   traps;
           SortedDictionary<int, LandingSpot> spots;

   public  delegate void FOnGameOver();

   public  FOnGameOver OnGameOver;

   public Play()
   {
           Settings = Desktop.Settings;

           World = Core.World.World.Instance();

           Cortex = Core.Cortex.Cortex.Instance();

           hud = new HudTable(this, 0, 0);

           traps = new List<Trap>();

           spots = new SortedDictionary<int, LandingSpot>();

           bgimage = new Surface("scene/map.png");
   }

   ~Play()
   {
   }

           bool ready;
           bool freeze;
           float countdown;

   internal  void Prepare()
   {
           lock(this) {
           Console.WriteLine("Ohai");

           space = new Space(new Size(Desktop.Width, Desktop.Width), 
                             new Rectangle(0, 160, Desktop.Width, Desktop.Height));

           
           space.Background = bgimage;

           /* load your crap here */
           spots.Add(Vehicle.TYPE_PLANE0, new Strip(this, 256, 71));
           spots.Add(Vehicle.TYPE_PLANE1, new Strip45(this, 338, 334));
           spots.Add(Vehicle.TYPE_COPTER, new Helipad(this, 560, 224));

           countdown = 10.0f;
           mGameBox = new GameBox(this);
           mGameBox.ResetEnabled(this, false);
           Desktop.Jukebox.Mode = Jukebox.GAME;

           /* reset time */
           evo = 0;

           /* reset score */
           foreach (User u in Cortex.Names)
           {
               u.score = 0;
           }

           Cortex.psReady();
           freeze = false;
           ready = true;
           /*lock*/}
   }

   internal  void Run()
   {
           lock(this) {
           /*lock*/}
   }

   internal  void End()
   {
           lock(this) {
           ready = false;

           /* free your crap here */
           traps.Clear();
           spots.Clear();
           widgets.Clear();
           space = null;
           Console.WriteLine("Bye");
           /*lock*/}
   }

   public  void dyStrobe(Cortex sender, byte[] data, ref int r)
   {
           lock(this) {

                lock(space.things) {

                     ClearList();
                     //FetchUpdates(data, ref r);

                /*lock*/ }

           /*lock*/ }
   }

           int evo;

   public  void dySync(Cortex sender, byte[] data, ref int r)
   {
           lock(this) 
           {

                      int r_evo;

                    //get server time
                       Proto.getn(ref data, ref r, out r_evo);

                   if (r_evo < evo)
                       return;

                       byte id;
                       User user;
                       Gamer gamer;

                    //get scores
                       hud.MarkAll();

                       while((id = data[r++]) != Proto.EONAMES)
                       {
                          if ((user = Cortex.Names.getUser(id)) != null)
                          {
                              Proto.getn(ref data, ref r, out user.score);
                              
                              gamer = hud.Get((id & Proto._F_GID) >> 4);
                            
                              if (!gamer.Enabled)
                              {
                                  gamer.User = id;
                                  gamer.SetName(user.nick);
                                  gamer.SetEnabled(true);
                              }

                                  gamer.SetScore(user.score);
                                  gamer.Marked = false;
                          }
                       }

                       hud.DisableMarked();

                lock(space.things) {

                     try
                     {
                           //get updates
                           FetchUpdates(data, ref r);

                           //get drops
                           FetchDrops(data, ref r);

                           //done
                           space.Snap(Proto.dtRsTime);
                           evo = r_evo; 
                     }
                     catch(Exception e)
                     {
                           Console.WriteLine("Sync failed: {0}", e.ToString());
                           space.things.Clear();
                     }

               }
           }
   }

   public  void dyOver(Cortex sender, byte[] data, ref int r)
   {
           lock(this) {

                freeze = true;
                mGameBox.ResetEnabled(this, true);
           }
   }


   private void FetchUpdates(byte[] data, ref int r)
   {
           int  id;
           int  type;
           Thing thing;

           while (r != data.Length)
           {
              if (!Proto.getn(ref data, ref r, out id))
                  throw new Exception("Parse error.");

              if (id == Proto.EOLIST)
                  break;

              if (!Proto.getn(ref data, ref r, out type))
                  throw new Exception("Parse error.");

              if (type == Thing.TYPE_UNDEF)
                  throw new Exception("Inconsistent type.");
                       
              if (!space.things.TryGetValue(id, out thing))
              {
                  Vehicle vehicle = null;

                  switch(type)
                  {
                    case Thing.TYPE_TERRAIN: thing = new Terrain(space, id); break;
                    case Thing.TYPE_PLANE0: thing = vehicle = new Combat(space, id); break;
                    case Thing.TYPE_PLANE1: thing = vehicle = new Airliner(space, id); break;
                    case Thing.TYPE_COPTER: thing = vehicle = new Helicopter(space, id); break;
                  }

                  if (vehicle != null)
                      AddTrap(vehicle);
              }

              if (thing != null)
                  if (!thing.Deserialize(ref data, ref r))
                      throw new Exception("Deserialize error");
           }
   }

   private void FetchDrops(byte[] data, ref int r)
   {
           int  id;
           Thing thing;

           while (r != data.Length)
           {
              if (!Proto.getn(ref data, ref r, out id))
                  throw new Exception("Parse error.");

              if (id == Proto.EOLIST)
                  break;

                  space.things.Remove(id);
                  RemTrapId(id);
           }
   }

   private void ClearList()
   {
           space.things.Clear();
   }

   private Trap AddTrap(Vehicle vehicle)
   {
           Trap  trap = new Trap(null, vehicle, spots[vehicle.Type]);

           lock(traps) {

                traps.Add(trap);
           }

           return trap;
           
   }

   private Trap FindTrapId(int id)
   {
           lock(traps) {

                foreach(Trap trap in traps)
                    if (trap.Id == id)
                        return trap;
           }
           return null;
   }

   private void RemTrap(Trap trap)
   {
           lock(traps) {

                traps.Remove(trap);
           }
   }

   private void RemTrapId(int id)
   {
           RemTrap(FindTrapId(id));
   }

   public  override bool Sync(float dt)
   {
           lock(this) {

                 if (space != null)
                 {
                     if (!freeze)
                         space.Step(dt);
                 }

                 lock(spots) {

                      foreach(KeyValuePair<int, LandingSpot> Kvp in spots)
                      {
                              Kvp.Value.Sync(dt);
                      }
                 }

                 lock(traps) {

                    lock(space.things) {

                         foreach(Trap trap in traps)
                         {
                                 trap.Sync(dt);

                             if (trap.HasRouteInfo())
                                 Cortex.psRoute(trap.Id, trap.GetRouteInfo());

                             if (trap.HasStatusInfo())
                                 Cortex.psFlags(trap.Id, trap.GetStatusInfo());
                         }
                    }
                 }


                 if (freeze)
                 {
                     countdown -= dt;

                     if (countdown >= 0.0f)
                         mGameBox.SetTimeout(countdown);
                         else
                         if (OnGameOver != null)
                             OnGameOver();
                 }

                 hud.Sync(dt);
                 mGameBox.Sync(dt);
           }

           if (Cortex.Instance() != null)
               Cortex.Sync(Proto.msRsTime);

           return true;
   }

           Color[] colors = new Color[]{Color.Blue, Color.LightGreen, Color.Yellow, Color.Red,
                                        Color.Aqua, Color.AliceBlue, Color.Azure, Color.BlueViolet,
                                        Color.Bisque, Color.Coral, Color.Cyan, Color.Crimson,
                                        Color.DeepPink, Color.Firebrick, Color.ForestGreen, Color.GreenYellow};


   public  override void Draw(Surface dst)
   {
           lock(this)       
           {
                space.Draw(dst, 255);

                for (int x = widgets.Count - 1; x >= 0; x--)
                {
                        widgets[x].Draw(dst);
                }

                if (!freeze)
                {
                    lock(traps) {

                      lock(space.things) {

                         foreach(Trap trap in traps)
                         {
                                 Vehicle vehicle = trap.Vehicle;
                                 Color  color = colors[vehicle.Id & 0x0f];

                                 Point   px = new Point(0, 0);
                                 Color[,] box = new Color[,]{ {color, color}, {color, color} };

                                 int     cnt = 0;

                                 dst.Lock();

                                 if (vehicle.Route != null)
                                 {
                                     foreach(PointF sp in vehicle.Route)
                                     {
                                         if ((cnt % 4) == 0)
                                         { 
                                             px = Trap.SpaceToDesktop(sp);
                                             //dst.Draw(px, color);
                                             dst.SetPixels(px, box);
                                          }
                                             cnt++;
                                     }
                                 }

                                 dst.Unlock();

                                 trap.Draw(dst);
                         }
                      /*lock*/}
                    /*lock*/}
                }

                        hud.Draw(dst);
                /*
                      //highlight points on the map
                        Point p   = space.SpaceToDesktop(-.36f, -.36f);
                        Box   hp  = new Box((short)(p.X - 2), (short)(p.Y - 160 - 2),
                                    (short)(p.X + 2), (short)(p.Y - 160 + 2));

                        dst.Draw(hp, Color.Red, false, true);
                */
           }

           dst.Update();
   }

}

class GameBox :Window
{
           Play          play;
           Label         mMessageLabel;
           Label         mTimeoutLabel;

           int time;

   public GameBox(Play owner)
   :base(owner, 400, 75, 400, 48)
   {
           bgcolor = Color.Black;

           mMessageLabel = new Label(this, 16, 2, 296, 32);
           mMessageLabel.setFont("res/xscale.ttf", 0);
           mMessageLabel.setColor(Color.FromArgb(255, 255, 255, 255));
           mMessageLabel.setText("Game Over");
           mMessageLabel.setAnimated(true);

           mTimeoutLabel = new Label(this, 16, 22, 296, 20);
           mTimeoutLabel.setFont("res/xscale.ttf", 0);
           mTimeoutLabel.setColor(Color.LightGreen);
           mTimeoutLabel.setText("Rebooting in 10 seconds");
           mTimeoutLabel.setAnimated(false);
   }

   public  void SetTimeout(float timeout)
   {
       if (time != (int)timeout)
       {
           mTimeoutLabel.setText(string.Format("Rebooting in {0} seconds", time));
           time = (int)timeout;
       }
   }
}


/*namespace Game*/ }
