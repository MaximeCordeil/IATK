MatCap Shaders Pack, version 1.362
2015/10/01
(c) 2015 - Jean Moreno
===========================================

This is a pack of MatCap-like shaders, used in popular 3d software such as ZBrush or Sculptris.
They allow to determine the whole shading model of an object with a single texture without using any lights.
As such the shaders are really fast and should work very well on mobile!

They are based on the MatCap shader by Daniel Brauer, from the Unify Community: http://wiki.unity3d.com/index.php/MatCap
Optimizations have been made to make them much faster, and to add some features such as adding a regular texture on top of the matcap one.

"Vertex" are the defaults shader, where most of the calculations are done per-vertex, so they are very fast.

"Bumped" are shaders with normap map support, with more calculations being made per-pixel, and as such are slower.
"Accurate Calculation" can help with non-uniform scaling and skinned mesh renderers in the bump shader, but it is slower.

========================

Check out more free and commercial Unity plugins at:
http://jeanmoreno.com/unity

========================

Release notes:
--------------
v1.362
- updated Welcome Screen

v1.361
- added Welcome Screen

v1.36 (Unity 5)
- fixed tangents normalization in non-accurate bumped shaders (should fix scaling issues)

v1.351 (Unity 5)
- rewrote how the matcap is calculated, fixing scaling and rotation issues with Unity 5 (for real hopefully!)

v1.35 (Unity 5)
- fixed scaling issues with Unity 5

v1.34 (Unity 5)
- updated compatibility with Unity 5

v1.33
- updated "JMO Assets" menu

v1.32
- fixed compilation errors and warnings in d3d11

v1.31
- fixed shadows casting/receiving support

v1.3
- added JMO Assets menu (Window -> JMO Assets), to check for updates or get support

v1.2:
- Added shader "Plain Additive Z" (avoid X-Ray like effect)

v1.11:
 - Removed Asset Store license agreement, only applying to some textures

v1.1:
 - Added Bumped Shaders
 - Added more MatCap textures