# FSXGliderTow
A super crude and very old program for multiplayer glider towing in FSX (and possibly P3D)

I featured this in a short video (https://www.youtube.com/watch?v=-HG5_0A_yHE) and I kept getting comments and messages about it, so I looked and actually managed to dig up the source code, wow.

# Download
Make sure to read "How to use it" below. Once you did, it should be pretty straightforward.
Download an .exe over at the releases page: https://github.com/mazzn/FSXGliderTow/releases

Or clone the repository and compile it yourself using any version of Visual Studio 2013 and newer. This code is an absolute mess from over 3 years ago, so please don't mind it.

# How to use it
1) Make sure SimConnect is installed. I can't really help you with this because for me it's always been really strange to get it running right, especially with Prepar3D.
2) Start FSX/P3D, load into a multiplayer session in a glider, have everyone connect and position the player which will tow you right in front of you. In your FSX session, make sure that collisions are turned off! Otherwise the towplane will crash the other player.
3) Open FSX Glider Tow.exe and click "Connect to FSX"
4) Now you have to be somewhat quick before the towplane starts taking off. If you're too slow, just disconnect the tow rope and try again.
5) Press CTRL+Shift+Y to spawn the towplane
6) Quickly press the "Request planes" button. The two lists should get populated with a few entries.
7) Select the towplane from the left list (should be a Maule M7 and likely the last entry in the list)
8) Select the player in front of you from the right list
9) Press the "Attach towplane to player" button - the towplane should be set to the other players position and stay there (more or less)

Once you are done towing, simply press CTRL+Y (or whatever button you have set) to release the rope. You can then use the "Detach towplane" button to let it go from the other player.
