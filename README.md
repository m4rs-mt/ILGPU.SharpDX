# ILGPU.SharpDX

Many 3D applications often rely on compute shaders to implement computer graphics or advanced simulation algorithms.
Using ILGPU to implement advanced algorithms in 3D applications can simplify these tasks.
This interop library provides access to native Direct3D (11.X) resources (compute buffers or textures) from ILGPU.

# Build instructions

ILGPU.SharpDX requires Visual Studio 2017 (Community edition or higher).

# Build ILGPU.SharpDX and its libraries

Use the provided Visual Studio solution to build the ILGPU.SharpDX libs
in the desired configuration (Debug/Release).

Note: ILGPU.SharpDX uses the build configuration "Any CPU" (which simplifies
an integration into other projects).

# License information

ILGPU.SharpDX is licensed under the University of Illinois/NCSA Open Source License.
Detailed license information can be found in LICENSE.txt.

Copyright (c) 2016-2018 Marcel Koester (www.ilgpu.net). All rights reserved.

## License information of required dependencies

Different parts of ILGPU.SharpDX require different third-party libraries.

* ILGPU.SharpDX Dependencies:
    - ILGPU (http://www.ilgpu.net)
    - SharpDX (http://sharpdx.org/)

Detailed copyright and license information of these dependencies can be found in
LICENSE-3RD-PARTY.txt.
