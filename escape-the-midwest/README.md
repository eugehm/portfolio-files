# Escape The Midwest

Final project for EECS 298: 3D Tech Art and Animation at the University of Michigan. [Play it here.](https://eugehm.itch.io/escape-the-midwest)

A 3D platformer assembled in Unity using a course-provided framework as a base. All 3D assets were modeled in Blender, all textures were designed in Piskel, and all voice overs were recorded and integrated by me. Beyond assembly, I added custom scripts and modified existing ones for a snowball zone mechanic, a new gameplay section with its own camera behavior, hazard system, and zone triggers.

**Scripts added for the snowball zone:**
- **SnowballSpawner.cs:** Spawns snowballs at random above the zone at a set interval, activated and deactivated by zone triggers.
- **SnowballMelter.cs:** Shrinks and destroys a snowball over time once it enters a melting trigger zone.
- **SnowballDestroy.cs:** Destroys any snowball still in the zone when the player triggers a game over.
- **MeltingTrigger.cs:** Zone trigger that starts the melt on any snowball that enters it.
- **PlayerTriggerStart.cs/PlayerTriggerStop.cs:** Start and stop all assigned spawners when the player enters or exits the zone.
- **ResetTrigger.cs:** Tracks whether the player is in the game over zone so `SnowballDestroy` knows when to clean up.
- **SnowballCameraTrigger.cs/SnowballCameraExit.cs:** Shift the camera to a tilted front-facing angle when the player enters the snowball zone and restore it on exit.

**Scripts modified for the snowball zone:**
- **GameplayCamera.cs:** Added `TiltCamera()` and `RemoveTiltCamera()` to support zone-specific camera angles via the existing special camera request system, and updated `SpecialRequestMotion()` to handle object-tracked requests.

***NOTE:*** *The repository includes a custom WebGL template provided by the course instructor for building the game as a playable web build. This was not made by me.*