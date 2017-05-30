LOADING SCREEN

Version: 1.4

Description 
-----------
A customizable loading screen for Daggerfall Unity.

Prerequisites
-------------
Daggerfall Unity 0.4.12 or higher.

Installation
------------
1. Move 'LoadingScreen' folder inside 'StreamingAssets/Mods'.
2. Pick the correct 'loadingscreen.dfmod' for your OS and place it inside the 'LoadingScreen' folder.

Customization
-------------
This mod randomly picks any .png image found inside this folder and uses it as splash screen:

- \StreamingAssets\Mods\LoadingScreen\Images

Alternatively, enable 'UseSeason' inside the mod settings and place textures in the appropriate folders:

- Images\Desert
- Images\Summer
- Images\Winter

You can also enable 'UseLocation' to use 'Images\Building' and 'Images\Dungeon' when entering one.

Settings
--------

- LoadingLabel
A loading label with three dots that show a rough progress of the loading.
You can customize size and color, other than the text itself.

- PressAnyKey
Pause the game until you press a key after the loading screen.

- ShowForMinimum
To avoid an unpleasant blinking when the loading is too fast, you can configure a minimun time,
in seconds, for wich the splash screen will be displayed. Set to 0 to disable this feature.

- DeathScreen
Show a death screen wich was present in the prototype demo but eventually removed from the final game.
By default it appears after the death video, but you can disable the latter with 'DisableVideo'.
To customize the death screen, place 'DIE_00I0.IMG.png' inside 'StreamingAssets\Textures\img\'.

Known Issues
------------
The loading screen doesn't work on the first save you load.

Changelog
---------
* 1.4
- Improvements
- (optional) Show tips
- (optional) Use Location

* 1.3
- Minor improvements.
- (optional) Death screen.

* 1.2
- Support for Daggerfall Unity 0.4.12
- More customization
- As suggested by Lypyl, every loading screen will last for a minimum of x.y seconds
  to avoid unpleasant blinking. Set to 0 to disable this feature.
- (optional) Press-any-key-to-continue after loading.

* 1.1
- Added animated Loading label.
- Use ini file for customization.
- (optional) Use different images for Summer, Winter and Desert. 

Credits
-------
TheLacus (TheLacus@yandex.com)

