# Deferred Rendering Demo

This package demonstrates the implementation of a deferrerd renderer. This engine is designed to be resusable
for different kinds of scenes.

It has the following features:

* Multiple directional lights with optional shadows.
* Lots and lots of point lights.
* Bump mapping
* Specular mapping
* Geometry instancing
* HDR Rendering with tone mapping and glow
* Sunrises and sunsets. Just for fun.

The shadows are a little unstable when the light source moves. I believe this could be fixed
by implementing PCSS. Maybe I'll do that if I get some free time for it.

## Building

After checking out to code, you will need to run to following command to pull the
dependencies: 

* `git submodule update --init --recursive`

It should build after that in Visual Studio 2019. I have not tested whether this 
builds on non-Windows operating systems or older versions of Visual Studio yet.

There seems to be some concurrency problems when building the content for both DesktopGL
and WindowsDX at the same time. If you have trouble with this, you can either rebuild a few
times until it passes, or unload either the DesktopGL or WindowsDX project to build the 
other one.

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

License: Microsoft Public License (MS-PL)

* Deferred Rendering Engine - Erik Ylvisaker
* Cascaded Shadow Maps - [TGJones](https://github.com/tgjones/monogame-samples)

* This uses an updated version of MonoGame, including integrations of the code 
  in the following PRs:

  * [Hull, Domain and Geometry Shaders for DirectX and OpenGL](https://github.com/MonoGame/MonoGame/pull/7352)
  * [DesktopGL support for Texture2DArray](https://github.com/MonoGame/MonoGame/pull/7361)
