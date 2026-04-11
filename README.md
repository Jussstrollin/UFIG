# Unsuspecting Factory Incremental Game (UFIG)

A terminal-based "game" — although by the looks of it, I wouldn't consider it one. But it's still a game.

Currently, it's about balancing Alpha, Beta, and Gamma factories, where each uses the previous as input. Start with Essence (base production of 1 per tick), feed it to the Alpha Factory (consumes 1 Essence, outputs Alpha). Beta Factory does the same. Gamma Factory consumes Alpha, Beta, and 2 Essence. It's supposed to be hard to mass produce — cool upgrade paths maybe later. *Shrug*

I'd like to call these Greek letters **Arcospheres**.

## What It Currently Has
- Production Chain (Essence → Alpha → Beta → Gamma)
- 7 Upgrades (buy factories with Essence + Arcospheres for Essence upgrades)
- Save / Load System
- Cool UI (maybe)

## How to Run
- Install .NET 6.0
- [.NET 6.0 SDK or higher For Windoe](https://dotnet.microsoft.com/download)

**Arch-based systems:**
```Bash
sudo pacman -S dotnet-sdk-6.0
```
(Higher versions may also work)

### Compile from source
```Bash
git clone [this-repo-link]
cd UFIG/
dotnet run
```
### Download from release
```Bash
cd ~/path/to/downloaded/folder
dotnet run
```

## Controls
- `S` — Go to shop
- `Q` — Quit
- `1-5` — Open upgrade description (in shop)
- `ENTER` — Buy (note: you may need to select the upgrade again before buying another)
- `B` — Go back (from upgrade description)

## Mentions
- The game is inspired by a certain factory game's core loop... its factorio okay,also I tried to capture its addicting nature. I did try, okay?
- The name "Arcospheres" is inspired by the Factorio mod **Space Exploration**. I was fascinated by the Arcosphere puzzle and the balancing aspect — I plan to turn that into gameplay later.  
To the Space Exploration modder(s): Pls dont sue meeee.  
Check it out: https://mods.factorio.com/mod/space-exploration
- This is my first proper "game" that's actually "finished." I'm fully open to advice or feedback — on gameplay or code. It would be much appreciated!

## Future Plans (if I don't drop this)
- Switch to **SadConsole** for even cooler UI, plus a narrator to guide the player
- Make Essence be mined (miners) instead of Magically produced and factories time-based instead of tick-based, with progress bars! Very cool UI yes
- Add a **Research Feature** to unlock cooler stuff and the main progression path. Likely using Arcospheres as cost. Will take time, but I'm excited!
- Way later: add pixel art sprites so you can actually see the beautiful spheres, factories, and miners. Will definitely take time lol.

### My Notes
If you like this, thank you! But I don't want to get your hopes up. I'm not a good programmer, nor a consistent one, but I do want to put work into this game of mine.

The game window is only accounting 1280x720 screens.
