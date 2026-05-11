# Options Menu
Code sample from [Cor Draconis](https://wolverinesoft-studio.itch.io/cor-draconis), a game made with WolverineSoft Studio. View the demo [here](https://www.youtube.com/watch?v=flxr_Fu5tl4).

**OptionsMenu.cs** handles UI interactions for the options menu with sliders and dropdowns for music volume, SFX volume, camera sensitivity, zoom sensitivity, render quality, display mode, and dialogue speed. Changes update both the UI labels and the underlying systems in real time, and fire UnityEvents so other gameplay systems can react (e.g. dialogue speed affecting in-game text behavior).

**Settings.cs** is a persistent singleton that stores the player's current settings via `DontDestroyOnLoad` and fires an event on each scene load so the options menu UI stays in sync.