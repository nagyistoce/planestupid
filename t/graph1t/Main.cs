using System;
using SdlDotNet;
using SdlDotNet.Core;
using SdlDotNet.Audio;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Sprites;
using SdlDotNet.Input;
using Core.Scene;
using Tao.OpenGl;

/* this test's primary aim was to determine whether the
   impossibly slow Gl scenes when adding textures was somehow
   related to the blend function i used to combine it with standard
   SDL.
   Conclusion: regardless of whether i'm doing textures wrong - which i doubt, 
   i merely followed the tutorials, or something's screwy within SdlDotNet,
   test failed miserably: even with no Gl blending, with a single tex assigned to 
   a single quad, framerate dropped to about 1FPS.
*/

namespace graph1t
{
    class Graph
    {
       public const int width = 800;
       public const int height = 480;
       public const float ratio = (float)width / (float)height;

               Space Space;

       public Graph()
       {
               Video.WindowCaption = "SdlDotNet Test";
               Video.SetVideoMode(width, height, 32, false, true, false, true, true);

               Space = new Space();

               Events.Fps = 60;

               InitGl();          
               InitScene();

           Events.KeyboardDown += delegate(object sender, KeyboardEventArgs e) {

               if (e.Key == Key.Escape)
                   Events.QuitApplication();
           };

           Events.Tick += delegate(object sender, TickEventArgs e) {

                  lock(this) {
                       Space.Step(e.SecondsElapsed);
                       Space.DrawGl(0);
                       Video.GLSwapBuffers();
                  }
           };

           Events.Quit += delegate {

                   Events.QuitApplication();
           };
       }

               Floor floor;
               Cube cube;

       private void InitScene()
       {
               floor = new Floor(Space);
               cube = new Cube(Space);
       }

       private void InitGl()
       {
               ResetGl();
               Gl.glShadeModel(Gl.GL_FLAT);

           //3d
               //Gl.glClearDepth(1.0f);
               //Gl.glEnable(Gl.GL_DEPTH_TEST);
               //Gl.glDepthFunc(Gl.GL_LEQUAL);
               //Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_NICEST);

               //Gl.glEnable(Gl.GL_CULL_FACE);
               Gl.glEnable(Gl.GL_TEXTURE_2D);
               Gl.glEnable(Gl.GL_SHADE_MODEL);

               Gl.glEnable(Gl.GL_LIGHTING);
               Gl.glEnable(Gl.GL_COLOR_MATERIAL);

               float[] ambient = new float[]{0.0f, 1.0f, 1.0f, 1.0f};
               float[] diffuse = new float[]{1.0f, 1.0f, 1.0f, 1.0f};
               float[] specular = new float[]{1.0f, 1.0f, 1.0f, 1.0f};

               Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT,  new float[]{0.1f, 0.1f, 0.1f, 1.0f});
               Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE,  new float[]{0.3f, 0.3f, 0.3f, 1.0f});
               Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPECULAR, new float[]{0.4f, 0.4f, 0.4f, 1.0f});
               //Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, new float[]{0.0f, 0.0f, 2.0f, 0.5f});
               Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, new float[]{1.0f, 1.0f, 2.0f, 0.1f});
               Gl.glEnable(Gl.GL_LIGHT0);
   
               Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_AMBIENT, ambient);
               Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_DIFFUSE, diffuse);
               Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_POSITION, new float[]{0.0f, 0.0f, 2.0f, 1.0f});
               Gl.glEnable(Gl.GL_LIGHT1);

               Gl.glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
               //Gl.glEnable(Gl.GL_BLEND);
               //Gl.glBlendFunc(Gl.GL_DST_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
       }

       private void ResetGl()
       {
               Gl.glViewport(0, 0, width, height);
               Gl.glMatrixMode(Gl.GL_PROJECTION);
               Gl.glLoadIdentity();   
               Gl.glOrtho(-1.0f, +1.0f, -0.6f, +0.6f, -1.0f, +1.0f);
               Gl.glMatrixMode(Gl.GL_MODELVIEW);
               Gl.glLoadIdentity();   
       }

       public void Run()
       {
              Events.Run();
       }


              static Graph graph;

       public static void Main (string[] args)
       {
               graph = new Graph();
               graph.Run();
       }
    }
}
