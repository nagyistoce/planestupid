PlaneStupid
-----------

PlaneStupid is a multiplayer air traffic control game similar to FlightControl.
Written in c# and originally developed under the Mono platform.
Available under zlib/libpng licence.

Dependencies (on all platforms)
libsdl (v1.2 or later)
libsdl-gfx
libsdl-image
libsdl-ttf2
libsdl-sound
libsdl-mixer
You also need the corresponding -dev packages in order to compile from source

Play on GNU/Linux and MAC
1. get and install libsdl [[ http://www.libsdl.org/ ]]
2. untar
3. run "mono Game.exe"

Play on Windows
1. get and install sdldotnet sdk [[ http://sourceforge.net/projects/cs-sdl/files/ ]]
2. ignore step 3
3. restart the computer
4. unzip the game archive
5. launch Game.exe

Notes:
The whole Game.Ui namespace is sort of a mess, i will clean it up in due time; if you'd
like to use that part of the source, just be careful and don't borrow it with my mistakes.

Bugs && peculiarities:
1. game just dies for no apparent reason [rare, i'll hopefuly find out why soon]
2. game just freezes [kinda happens on windows]
3. planes drift out of control [rare]
4. strips stay highlighted [rare, no ideea why that happens]

Credits:
spaceape [ spaceape99@gmail.com ] - coding and graphics
k2 - ideeas, graphics, testing

Enjoy.
spaceape