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
using SdlDotNet.Core;
using SdlDotNet.Audio;

namespace Game.Ui {

public class Jukebox
{
           List<Music> tunes;

           int         mode = NO;

   public Jukebox()
   {
           tunes = new List<Music>();

           Volume = 96;

           Events.MusicFinished += delegate(object sender, MusicFinishedEventArgs e) {

                  playnext();
           };
   }

   public  const int NO = 0;
   public  const int MENU = 1;
   public  const int GAME = 2;

   private void setMode(int umode)
   {
       if (mode != umode)
       {
           tunes.Clear();

           switch(umode)
           {
             case MENU:
                  tunes.Add(new Music("music/08.ogg"));
                  tunes.Add(new Music("music/07.ogg"));
                  tunes.Add(new Music("music/06.ogg"));
                  tunes.Add(new Music("music/05.ogg"));
                  tunes.Add(new Music("music/04.ogg"));
                  tunes.Add(new Music("music/03.ogg"));
                  tunes.Add(new Music("music/02.ogg"));
                  tunes.Add(new Music("music/01.ogg"));
                  break;

             case GAME:
                  tunes.Add(new Music("music/01.ogg"));
                  tunes.Add(new Music("music/02.ogg"));
                  tunes.Add(new Music("music/03.ogg"));
                  tunes.Add(new Music("music/04.ogg"));
                  tunes.Add(new Music("music/05.ogg"));
                  tunes.Add(new Music("music/06.ogg"));
                  tunes.Add(new Music("music/07.ogg"));
                  tunes.Add(new Music("music/08.ogg"));
                  break;
           }

           if (umode != NO)
           {
               playnext();
           }
       }
   }

   private void playnext()
   {
       foreach (Music tune in tunes)
       {
           if (MusicPlayer.CurrentMusic == tune)
           {
               if (MusicPlayer.IsPlaying)
                   MusicPlayer.Stop();
                   MusicPlayer.CurrentMusic = null;
           }   else
           if (MusicPlayer.CurrentMusic == null)
           {   MusicPlayer.CurrentMusic = tune;
               MusicPlayer.EnableMusicFinishedCallback();              
               MusicPlayer.Play();
               break;
           }
       }
   }

   private void stop()
   {
       if (MusicPlayer.CurrentMusic != null)
       {   MusicPlayer.Stop();
           MusicPlayer.CurrentMusic = null;
       }
   }

   public void Drop()
   {
   }

   public int Mode
   {
           get {return mode;}
           set {setMode(value);}
   }

   public int Volume
   {
           get {return MusicPlayer.Volume;}
           set {MusicPlayer.Volume = value;}
   }
}

/*namespace Game.Ui*/ }
