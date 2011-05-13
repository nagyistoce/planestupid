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
using SdlDotNet.Core;
using SdlDotNet.Audio;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Sprites;
using SdlDotNet.Input;
using Core;
using Core.Scene;

namespace Game.Ui {

public class Desktop
{
   public  const int width = 800;
   public  const int height = 480;
   public  const float ratio = (float)width / (float)height;

           Surface root;
  
           App app;

           SpriteCollection sprites;

           IFocusable      focus;

           Scheduler       scheduler;

           Sampler         sampler;

           Jukebox         jukebox;

           Settings        settings;

           Statistics      statistics;

           bool            idle;

   public  const int EV_SYNC = 0x0001;

   Desktop()
   {
           Video.WindowCaption = ".: PlaneStupid :.";
           root = Video.SetVideoMode(Desktop.Width, Desktop.Height, 32, false, false, false, true, true);

           sprites = new SpriteCollection();

           sprites.EnableMouseButtonEvent();
           sprites.EnableMouseMotionEvent();
           sprites.EnableKeyboardEvent();

           scheduler = new Scheduler();
           sampler = new Sampler();
           jukebox = new Jukebox();
           settings = new Settings();
           statistics = new Statistics();

           Events.Fps = 60;

           Events.KeyboardDown += delegate(object sender, KeyboardEventArgs e) {

               if (e.Key == Key.Escape)
                   Events.QuitApplication();
           };

           Events.Tick += delegate(object sender, TickEventArgs e) {

                   lock(this) {

                     if (idle)
                         return;

                     if (app != null)
                     {
                         if (app.Sync(e.SecondsElapsed))
                         {
                             app.Draw(root);
                         }
                     }

                   /*lock*/ }
           };

           Events.Quit += delegate {

                   Events.QuitApplication();
           };

                   idle = false;
   }

   ~Desktop()
   {
   }


   private void  InitGl()
   {
   /*
           Gl.glShadeModel(Gl.GL_SMOOTH);

           //3d
           Gl.glClearDepth(1.0f);
           Gl.glEnable(Gl.GL_DEPTH_TEST);
           Gl.glDepthFunc(Gl.GL_LEQUAL);
           Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_NICEST);

           Gl.glEnable(Gl.GL_CULL_FACE);

           //light
           //Gl.glEnable(Gl.GL_LIGHTING);
           Gl.glEnable(Gl.GL_COLOR_MATERIAL);

           float[] ambient = new float[]{1.0f, 1.0f, 1.0f, 1.0f};
           float[] diffuse = new float[]{1.0f, 1.0f, 1.0f, 1.0f};
           float[] specular = new float[]{1.0f, 1.0f, 1.0f, 1.0f};

           //Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, ambient);
           //Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, diffuse);
           //Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPECULAR, specular);
           //Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, new float[]{0.0f, 0.0f, 2.0f, 0.5f});
           Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, new float[]{0.0f, 0.0f, 5.0f, 0.5f});
           Gl.glEnable(Gl.GL_LIGHT0);
   
           Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_AMBIENT, ambient);
           Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_DIFFUSE, diffuse);
           Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_POSITION, new float[]{0.0f, 0.0f, 2.0f, 1.0f});
           //Gl.glEnable(Gl.GL_LIGHT1);

           Gl.glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
           Gl.glEnable(Gl.GL_BLEND);
           Gl.glBlendFunc(Gl.GL_DST_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
   */
   }

   private void  ResetGl()
   {
   /*
           Gl.glViewport(0, 0, width, height);
           Gl.glMatrixMode(Gl.GL_PROJECTION);
           Gl.glLoadIdentity();   
           Gl.glOrtho(-1.0f, +1.0f, -0.6f, +0.6f, -1.0f, +1.0f);
           //Glu.gluPerspective(45.0f, 1.666666f, 0.1f, 100.0f);
           //Gl.glFrustum(-1.0f, 1.0f, -1.0f, 1.0f, 1.5f, 20.0f); 

           Gl.glMatrixMode(Gl.GL_MODELVIEW);
           Gl.glLoadIdentity();   
   */
   }

   private static Desktop instance;

   public  static Desktop New()
   {
       if (instance == null)
           instance = new Desktop();

           return instance;
   }

   public  static void Run()
   {
       if (instance != null)
           Events.Run();
   }

   public  static void AppSelect(App app)
   {
           lock(instance) {

           instance.idle = true;

       if (instance.app != app)
       {
           if (instance.app != null)
               instance.app.ResetEnabled(null, false);

           instance.app = app;
           WidgetWantFocus(null);
       }

           instance.idle = false;

           /*lock*/ }
   }

   public  static void WidgetWantEvents(Sprite widget)
   {
           instance.sprites.Add(widget);
   }

   public  static void WidgetNoEvents(Sprite widget)
   {
           instance.sprites.Remove(widget);
   }

   public  static void WidgetWantFocus(IFocusable widget)
   {
       if (instance.focus == widget)
           return;

       if (instance.focus != null)
           instance.focus.LostFocus();

           instance.focus = widget;

       if (instance.focus != null)
           instance.focus.GainedFocus();
   }

   public  static Scheduler Jobs
   {
           get {return instance.scheduler;}
   }

   public  static Sampler Sampler
   {
           get {return instance.sampler;}
   }

   public  static Jukebox Jukebox
   {
           get {return instance.jukebox;}
   }

   public  static Settings Settings
   {
           get {return instance.settings;}
   }

   public  static Statistics Statistics
   {
           get {return instance.statistics;}
   }

   public  static Surface Surface
   {
           get {return instance.root;}
   }

   public  static int  Width
   {
           get {return width;}
   }

   public  static int  Height
   {
           get {return height;}
   }

   public  static Desktop Instance
   {
          get {return instance;}
   }
}

/*namespace Game.Ui*/}
