# Gameplay Scripts

Code sample from [Dream Synthesizer](https://superdude11235.itch.io/dream-synthesizer), a game made with WolverineSoft Studio.

**EquipMenu.cs** handles the equipment menu UI, toggling it open and closed with the "E" key and syncing displayed item slots to the player's current inventory via a callback on `onEquipChangedCallback`.

**PlayerHealth.cs** manages the player's health bar UI and health boosts from armor. `UpdateHealthPoints()` refreshes the health bar display whenever health or max health changes. `HandleArmorHealth()` adjusts max health based on the equipped armor type, and `RestoreHealth()` refills health to the current maximum, both calling `UpdateHealthPoints()` to keep the UI in sync.

**EnemyMovement.cs** controls enemy pathfinding. Enemies patrol between two points and switch to chasing the player when within a set distance, stopping the chase if the player moves far enough away. Sprite direction flips based on movement direction.

**EnemyDamage.cs** handles enemy attacks. On contact with the player, it determines knockback direction, calls the player's `TakeDamage()` and `Knockback()` functions, and deals counter-damage back to the enemy if the player has a counter equipped.

**EnemyHurt.cs** handles enemy death. Takes damage from weapon and slide collisions based on the player's equipped weapon strength, plays a hurt sound, and on death spawns a random item drop, fires an `EnemyDied` event, and destroys the enemy.