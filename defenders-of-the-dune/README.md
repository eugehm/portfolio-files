# Skill Tree System

Code sample from [Defenders of the Dune](https://wolverinesoftstudio.notion.site/Defenders-of-the-Dune-147654a5c2b7803897adede2fdc3c217), a game made with WolverineSoft Studio. Full development blog [here](https://eugehm.github.io/devblog/).

**SkillPointSystem.cs** is a singleton that tracks the player's current skill point total. Points are added via `AddPoints()` (e.g. when an enemy burrow is destroyed) and retrieved via `GetPoints()`. Publishes an `OnPointsChange` event whenever the count changes so other systems can react.

**SkillTreeManager.cs** is a singleton that tracks the unlock status of all skills and equipment using a dictionary. `UnlockSkill()` marks a skill as unlocked and publishes an `UnlockSkillEvent`. `Unlocked()` checks whether a given skill is unlocked. Also handles purchasing logic (`Purchase()`, `HasEnough()`, `Spend()`) and save/load support via `SaveData()` and `LoadData()`.

**SkillTreeBranchManager.cs** attaches to branch connectors in the tree UI. Listens for `UnlockSkillEvent` and updates the branch's color when its prerequisite skills are met.

**SkillTreeMenu.cs** controls the skill tree screen. Opens and closes via hotkey (pausing the game while open), displays skill name, cost, and description when a node is selected, and manages the buy button state based on whether the player can afford the selected skill. Subscribes to `OnPointsChange` to keep the displayed point count current.

**SkillUnlockNode.cs** attaches to each skill button in the tree. Manages per-node state (`Locked`, `Available`, `Purchasable`, `Purchased`) based on prerequisite completion and current skill points. Handles the full purchase flow on `OnBuy` events and refreshes visual state (icon color, border) whenever points or unlocks change.