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
using Core.Net;

namespace Core.Cortex {

public class User
{
      public int    flags;
      public int    perms;
      public string nick;

      public int    score;

      public string getNick()
      {
             return nick;
      }

      public int getSID()
      {
             return flags & Proto._F_SID;
      }

      public int getGID()
      {
             return (flags & Proto._F_GID) >> 4;
      }

      public override string ToString()
      {
             return String.Format("{0} {1} {2}", flags, perms, nick);
      }
}

public class NameList :List<User>
{
   public NameList()
   {
   }

   public User getSelf()
   {
          foreach(User u in this)
          {
              if ((u.flags & Proto.F_SELF) != 0)
                     return u;
          }

          return null;
   }

   public int  getUserIndex(int flags)
   {
          int x = 0;
          User u;

          while(x != Count)
          {
                u = this[x];
             if (u.getSID() == (flags & Proto._F_SID))
                 return x;
                x++;
          } 

          return -1;
   } 

   public User getUser(int flags, bool create)
   {
           int index = getUserIndex(flags);

       if (index != -1)
           return this[index];

           User ret = null;

       if (create)
           Add(ret = new User());

          return ret;
   }

   public User getUser(int flags)
   {
          return getUser(flags, false);
   }
}

/*namespace Core.Cortex*/ }
