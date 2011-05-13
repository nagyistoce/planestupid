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

namespace Core.Scene {

public class vector
{
   public  float x;
   public  float y;
   public  float z;

   public  vector(float x, float y, float z)
   {
           this.x = x;
           this.y = y;
           this.z = z;
   }

   public  static vector operator+(vector lhs, vector rhs)
   {
           return new vector(lhs.x + rhs.x, 
                  lhs.y + rhs.y, 
                  lhs.z + rhs.z);
   }

   public  static vector operator-(vector lhs, vector rhs)
   {
           return new vector(lhs.x - rhs.x, 
                  lhs.y - rhs.y, 
                  lhs.z - rhs.z);
   }

   public  static vector operator*(vector lhs, float rhs)
   {
           return new vector(lhs.x * rhs,
                  lhs.y * rhs, 
                  lhs.z * rhs);
   }

   public  static vector operator/(vector lhs, float rhs)
   {
           return new vector(lhs.x / rhs,
                  lhs.y / rhs, 
                  lhs.z / rhs);
   }

   public  static vector Null
   {
           get {return new vector(0.0f, 0.0f, 0.0f);}
   }
}

/*namespace Core.Scene*/ }