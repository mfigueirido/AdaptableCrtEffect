# Adaptable CRT Effect #

Most CRT effects found in the internet only work right when source images are sized like old TV or arcade screens, which have typical resolutions going from 240i to 480p. This works perfectly for emulators of old systems like the Sega Mega Drive or Super Nintendo.

But what would happen if you were developing a modern pixel art game which outputs to a big resolution like 1080p while retaining the pixel appearance? In this case those shaders won't produce a desirable CRT effect because the effect won't be resized to match the art.

## Why render a pixel art game at a resolution bigger than the art itself? ##

There's one big benefit with this approach and that is everything that moves in your game will look super smooth. Your camera, characters, animations of all kinds, particle effects... they all would benefit from the extra "subpixels" in between and remove all kinds of jitter.

It's true that retro purists might not like this approach because you are allowing some pixels to not align to the grid, and that can be distracting. One way to lessen this effect is by simply snapping to the grid your game entities when they stop moving. If your game is slow paced this would probably look great and you'll have the best of both worlds.
The great Pedro Medeiros has some advice about this topic:
https://saint11.org/blog/consistency/

## About this effect ##

- It's based on the popular Lottes CRT shader but modified to support upscaled art.
- Does a bloom pass before the CRT gets applied.
- Supports the following CRT presets: full scanlines, soft scanlines, no scanlines, no scanlines + no curvature (this only does CRT smoothing) and disabled.
- Includes a subtle pass of chromatic aberration.
- If you disable the CRT effect a subtle border smoothing will still be applied to reduce the harshness of the pixels. This can be disabled as well.

## How to set it up ##

Simply set the resolution properties to match what your game needs.
For example if you are rendering to 1080p and one art pixel occupies five screen pixels, divide the screen resolution by five and put that on the "Base" properties:

```
PostProcessingHelper.BaseWidth = 384;
PostProcessingHelper.BaseHeight = 216;
PostProcessingHelper.PresentationWidth = 1920;
PostProcessingHelper.PresentationHeight = 1080;
```

If you are not rendering at a higher resolution then both "Base" and "Presentation" sizes have to be the same:

```
PostProcessingHelper.BaseWidth = 384;
PostProcessingHelper.BaseHeight = 216;
PostProcessingHelper.PresentationWidth = 384;
PostProcessingHelper.PresentationHeight = 216;
```

## One last thing ##

This sample includes a picure of my team's current game in development: **Deadvivors**. If you're curious about it, take a look here :)
https://twitter.com/EnxebreGames
