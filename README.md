# Coin Quest: The Lost Temple

This Unity 2022.3 project builds a playable third-person island adventure when you press Play in `Assets/Scenes/SampleScene.unity`. Its world, player, camera, menu, HUD, and objectives are created by the runtime bootstrap, so that scene can stay intentionally simple.

## Play

- WASD: move; mouse: look; Space: jump; Left Shift: sprint
- E: interact with runes, the checkpoint, temple door, and chest
- Left mouse: fire magic; Q: Echo Pulse
- Escape: pause or resume

Collect coins for the HUD counter, discover all three runes to unlock the temple door, enter the temple, and interact with the chest to win. Guardians can be stunned by Echo Pulse and defeated with magic. The pulse also reveals the hidden bridge briefly.

## Windows build

In Unity, use **Coin Quest > Build Windows x64**. The build tool writes `Build/Windows/CoinQuest.exe`; keep the adjacent `_Data` folder and Unity player DLLs together with the executable. The project is configured to include `SampleScene` as its only build scene.

The build must be run from a licensed Unity Editor installation. This project copy has not been verified in Play Mode or built in this environment yet.
