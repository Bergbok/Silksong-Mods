## Showcase

### [Voicelines](./Voicelines)

Plays sound effects when Hornet takes certain actions.  
Comes with [some sound effects from the first game](./Voicelines/SFX/).

Supports custom sound effects, simply add them to the SFX folder after installing.  
Any [file format supported by Unity](https://docs.unity3d.com/6000.2/Documentation/Manual/AudioFiles-compatibility.html) should work.

<details>
<summary>Supported Events</summary>

- Attack
- Bind / Heal
- Clawline / Harpoon
- Cross Stitch / Parry
- Dash / Run Attack
- Death
- Drifter's Cloak / Float
- Faydown Cloak / Double Jump
- Hurt
- Jump
- Lava Bell Hit
- Nail Art / Charge Attack
- Needolin
- Pale Nails
- Ring Taunt / Poshanka
- Rune Rage
- Sharpdart
- Silk Soar / Super Jump
- Silkspear
- Swift Step
- Taunt
- Thread Storm
- Warding Bell Hit

</details>

Inspired by [something Skurry said](https://youtu.be/KoL2oD1TQuo?t=2930).

## Installing

1. [Install BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html#installing-bepinex-1)
2. If running through Steam on [*nix](https://en.wikipedia.org/wiki/Unix-like) systems, [set up launch options](https://docs.bepinex.dev/articles/advanced/steam_interop.html#2-set-up-permissions)

> [!TIP]  
> If `./run_bepinex.sh %command%` doesn't work, try:
> ```bash
> eval $(echo "%command%" | ./run_bepinex.sh "Hollow Knight Silksong")
> ```

3. Download the mod you want to install from [Releases](github.com/Bergbok/Silksong-Mods/releases)
4. Extract the downloaded archive to `silksong_path/BepInEx/plugins`
5. Launch the game
6. [Edit mod configs](https://docs.bepinex.dev/articles/user_guide/configuration.html#configuring-plugins) by using BepInEx's configuration manager (F1 by default), or by editing files in `silksong_path/BepInEx/plugins`

## Building

Requires [.NET](https://dotnet.microsoft.com/en-us/download) to be installed.

```bash
git clone https://github.com/Bergbok/Silksong-Mods.git
cd Silksong-Mods

# optional, for automatically installing mod after building
nano SilksongPath.props
find . -mindepth 1 -maxdepth 1 -type d ! -name ".github" -exec cp SilksongPath.props {}/SilksongPath.props \;

cd <mod folder>
dotnet build
```

<div align=center>
	<picture>
		<img src='https://i.imgur.com/ACl8IP3.png' alt='Hornet sitting' width=100 />
	</picture>
</div>

<!--
## Ideas

- [ ] GunZ mod (make all animations cancellable)
-->
