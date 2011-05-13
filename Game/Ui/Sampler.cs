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

namespace Game.Ui {

public class Sampler
{
           SoundDictionary sounds;

           int volume = 16;

   public Sampler()
   {
           sounds = new SoundDictionary();
   }

   public  Sound  Load(string name, string src)
   {
           Sound  sound;

       if (!sounds.TryGetValue(name, out sound))
       {   sounds.Add(name, sound = new Sound(src));
           sound.Volume = volume;
       }
           return sound;
   }

   public  void   Free(string name)
   {
           sounds.Remove(name);
   }

   public  void   Play(string name)
   {
           Sound  sound;
       if (sounds.TryGetValue(name, out sound))
           sound.Play();
   }

   public  int    Volume
   {
           get   {return 128;}
   }

   public  void   Drop()
   {
   }
}

/*namespace Game.Ui*/ }
