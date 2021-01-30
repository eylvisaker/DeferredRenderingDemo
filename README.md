# Deferred Rendering Demo

This package demonstrates the implementation of a deferrerd renderer. This engine is designed to be resusable
for different kinds of scenes.

It has the following features:

* Multiple directional lights with optional shadows.
* Lots and lots of point lights - I still get 60 fps with 1300 point lights + (directional light with cascading shadow maps) on a GTX 1660 at 4k.
* Bump mapping
* Specular mapping
* Geometry instancing
* HDR Rendering with tone mapping and glow
* Sunrises and sunsets. Just for fun.

The shadows are a little unstable when the light source moves. I believe this could be fixed
by implementing PCSS. Maybe I'll do that if I get some free time for it.

This uses an updated version of MonoGame, including 
integrations of the code in the following PRs:

* [Hull, Domain and Geometry Shaders for DirectX and OpenGL](https://github.com/MonoGame/MonoGame/pull/7352)
* [DesktopGL support for Texture2DArray](https://github.com/MonoGame/MonoGame/pull/7361)

## Input Commands

Use a gamepad to navigate. Left and Right triggers will move you up and down. Left stick button resets your
position, right stick button resets your orientation.

Keyboard commands: 

- `Shift-1`, `Shift-2` - Switch scenes. Each scene has its own keyboard commands.
- `Enter` - Pause/Resume the sun cycle

### Lattice Scene (`Shift-2`)

- `+` - Increase the lattice size. This adds more point lights.
- `-` - Decrease the lattice size.


# Credits

Deferred Rendering Engine - Erik Ylvisaker
Cascaded Shadow Maps - [TGJones](https://github.com/tgjones/monogame-samples)

License: Microsoft Public License (MS-PL)
