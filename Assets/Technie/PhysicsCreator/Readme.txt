
Technie Physics Creator
=======================

Quick Start
-----------

As Technie Physics Creator is an editor tool, all you need to do is import the package into your project. Then the `Hull Painter` window can be opened via the `Window` menu. Dock this somewhere handy as you go through the quick start guide.

Now select a single object in your scene that you want to paint physics colliders for. The object must have a MeshRenderer component on it, this is what we'll paint onto. In the hull painter window press `Generate`, this will do two things:

 - Assets for painting and hull data will be created in your project in `Assets/Physics Hulls/`. These store the data about the painted surfaces and also the generated collision meshes.
 
 - A `HullPainter` component will be added to the object. This is what connects the assets to this particular object so you can paint on it.
 
After the assets are generated the window will show you have a single hull ready for painting. Click and drag on the surface of the object to paint faces to include in the collider. To remove faces click or drag on a face that is already marked. You can also set the name, colour and type of the hull in the window.

More hulls can be added by clicking `Add Hull`, if you have multiple hulls on an object then choose which to edit by pressing the `Paint` button.

Once you're happy with your painting, hit `Generate Colliders` and colliders will be created on the object.


Hull Types
----------

When creating hulls, you have a few types:

BOX - generates BoxColliders. Very efficient, but can only be axis-aligned so may not be a good fit. Use this where possible.

SPHERE - generates the smallest possible SphereCollider that encloses the hull. Very efficient, use this when you need a smoothly curved surface.

FACE - takes the selected faces, and gives them a thickness (from the 'face thickness' setting). Useful for floors, walls, ceilings, etc.

CONVEX HULL - creates a convex hull around the selected faces. Highest accuracy and still fast and allows for rigidbodies. Use for the most awkward objects.


Generate Colliders From Selection
---------------------------------

Window->Technie Collider Creator->Generate Colliders From Selection

A useful starting point when you're authoring physics for a level. Clones the current selection and wraps colliders around any object with a MeshRenderer on it. Use this to quickly fill in physics before hand adjusting the trickier areas.


Span Physics
------------

Window->Techie Collider Creator->Span From Selection

Tool for guided creation of colliders, especially for level geometry. Select several objects (such as the corners of a wall) and this will try and wrap the tightest possible box collider around them. Very useful for rapidly authoring collision for walls, floors, ceilings, etc.


Disconnecting From Editor Data
------------------------------

Physics Creator has been deliberately written to have zero CPU overhead when your game is actually running. The HullPainter component itself does no work, and only uses a tiny amount of memory for the painting data. Colliders are all created at editor-time and not game startup. So while building your game you can leave the `HullPainter` component on your objects for when you want to further edit your hulls.

If you want zero CPU and memory overhead (such as when you ship your game) you can remove the `HullPainter` component. This means that the painting data asset will no longer be loaded, but all colliders will still work as before.


Reconnecting Editor Data
------------------------

If you disconnect an object from the painting data you can later reconnect it by dragging the painting data asset into the hull painter window. This will let you paint the hulls again.


Install Location
----------------

If you need to tidy your `Assets` directory, then the physics creator can be moved by moving the whole directory. You will then need to open `HullPainterWindow.cs` and update `installPath` as appropriate for icons to load correctly.


Tips
----

If your model is small or contains small, difficult to select triangles then scale the whole model up while hull painting by scaling the transform. Your mouse / camera movement will be more precise. Then just return to normal scale at the end and generate the colliders again.

Use the `Box` hull type where possible, as this will generate BoxColliders rather than convex hulls. These are quicker for the physics engine to process and do not require an external asset. Use the `Convex Hull` type for tricky shapes or for when it would save you having multiple box colliders.

Since colliders are generated to always fully enclose painted faces, you do not have to paint every face. Often you only need to paint the faces at the far extremes of the shape you want. This can be quicker to edit in some situations.

The `Generate Colliders` button will update existing colliders after the first usage, so use it often.

If you edit a generated collider manually then the painter will decide you have taken ownership of it and will no longer update it. Delete the collider and press `Generate Colliders` to return ownership.


Cavets
------

Since painting data is stored using triangle indices for efficiency, painting data will be lost if the original mesh changes. You will need to repaint your hulls for the new mesh.


Problems? Feature Requests? Bugs?
---------------------------------

Send an email to `technie@triangularpixels.com` for support and feature suggestions.

Please include your Unity version, and OS in any support emails. If reporting a bug then if you include a (small!) reproduction project with instructions on how to reproduce your bug then we'll be able to fix things *much* quicker. Thanks!
