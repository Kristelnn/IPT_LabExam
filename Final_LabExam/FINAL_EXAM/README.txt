Fantasy RPG Battle Simulator

Description:

This application simulates a turn-based battle between mystical champions in a fantasy world — but with a twist. 
The champions are inspired by the real personalities of our group members from our  classroom.
Just like in class, each character brings their own unique strengths and quirks into the battlefield. 
The Fantasy RPG Battle Simulator transforms our typical school dynamic into a magical combat arena, where everyday 
traits like being dramatic, calm, or humorous become powerful combat abilities. Designed in C# using Windows Forms, 
this project blends object-oriented programming with creativity, humor, and a touch of school spirit — turning our 
classroom into a fantasy adventure!


Meet the Champions:

- The OA Sorcerer
 Inspired by Kristel, known in class for being "OA" (overacting) in the most entertaining ways. This
flashy sorcerer deals magical burst damage and has a flair for drama and power. Capable of critical
spellcasting with unpredictable flair.

- Ante Maloi Crusader
 Inspired by Irene, who reminds everyone of BINI Maloi. A graceful yet deadly warrior of divine
justice. She uses heavenly strikes and charged attacks every third turn. Though her health is lower,
her impact is divine.

- The Tao Lang Adventurer
 Based on Keece, who always insists she's just "tao lang" (just a normal person). But in battle?
She's a balanced, witty, and surprising fighter. She alternates between fast combos and strong hits,
showing that even "normal" people are epic in their own way.

Features:

-Beautiful pixel art character sprites
-Smooth battle animations with visual effects
-Dark fantasy themed interface (because that is how PSU looks like for us during hell week)
-Dynamic health bars and damage numbers
-Detailed battle log with emojis

OOP Principles Applied:

Abstraction:
-Used an abstract class ClassFighter to define basic fighter structure like Name, Health, and the Attack() method.

Inheritance:
-Each character class (e.g., OASorcerer, AnteMaloiCrusader, TaoLangAdventurer) inherits from ClassFighter.

Encapsulation:
-Fighter properties like health are protected and accessed only through class methods.

Polymorphism:
-Each class overrides the Attack() method to deliver unique combat behaviors (e.g., crits, combos).

Challenges Faced:

1. Designing fantasy characters while still keeping the members' real-life personalities.
2. Managing image assets in Windows Forms with proper loading and disposal.
3. Creating smooth, flicker-free animations using double buffering.
4. Syncing visual effects and log output with real-time combat actions.

Installation:

*Make sure you have .NET 8.0 SDK installed*

1. Ensure all character images are in the characters folder:
   oa_sorcerer.png
   antemaloi_crusader.png
   taolangadventurer.png
   bgbg.png

2. Run the application

3. Select your champions (characters) and enter player names

4. Click "Start Battle" to begin!
