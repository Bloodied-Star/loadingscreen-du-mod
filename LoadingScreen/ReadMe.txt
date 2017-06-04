LOADING SCREEN

Version: 1.5

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

Common settings keys
--------------------
- Position
Affects position on screen of the element in question. The first value is the X (left), the second is the Y (up).
Negatives values are subtracted from the size of the screen: -X is right, -Y is bottom. 
The Alignment of the text is determinated automatically.

- Size
The size of the rect that contains the text. The first value is the Width, the second is the Height.
The Width affects where is the endline, meaning that determines the width of the paragraph.

- Font
A numerical index that determines the font used, among the various included in Daggerfall Unity:
* 1-5 are variations of OpenSans, a popular and classical font (ExtraBold, Bold, Semibold, Regular, Light).
* 6-11 are from the TESFonts, a font pack with very interesting styles, excellent for a game like Daggerfall
  (Kingthings Exeter, Kingthings Petrock, Kingthings Petrock light, MorrisRomanBlack, oblivion-font, Planewalker).
* 0, or above 12, means Unity default font (Arial if available, otherwise Liberation Sans).

- FontStyle
A number from 0 to 4 -> 0:Normal, 1:Bold, 2:Italic, 3:BoldAndItalic.

- Color
A color in hexadecimal format. 
The first six digits affects RGB, the last two affect alpha; 00 is full transparency, ff is full opacity.

Changelog
---------
* 1.5
- (optional) Show active quests messages 

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

