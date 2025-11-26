# Showcase

## [Voicelines](./Voicelines)

[![Thunderstore Link](https://img.shields.io/thunderstore/dt/Bergbok/Voicelines?style=flat&logo=thunderstore&logoColor=de1f31&labelColor=fafafa&color=de1f31)](https://thunderstore.io/c/hollow-knight-silksong/p/Bergbok/Voicelines)

Plays sound effects when Hornet takes certain actions.  
Comes with [some sound effects from the first game](./Voicelines/SFX/).

Supports custom sound effects, simply add them to the SFX folder after installing.  
Any [file format supported by Unity](https://docs.unity3d.com/6000.2/Documentation/Manual/AudioFiles-compatibility.html) should work.

Multiple sound effects can be selected and it'll pick a random one.  
You can manipulate odds by editing the configuration file.  
Enter sounds multiple times to increase odds or sounds that don't exist for a chance of not playing anything.

<details>
<summary><strong>Videos</strong></summary>

<br>

https://github.com/user-attachments/assets/3e3813d6-4924-4982-8823-86eb9576f808

https://github.com/user-attachments/assets/c6c63ed8-a299-40be-aace-94eba34b1a71

https://github.com/user-attachments/assets/cd6d4045-beca-4860-a592-76896960c859

https://github.com/user-attachments/assets/f8e58b28-b378-4999-8f3e-3baa8fb070e5

</details>

<details>
<summary><strong>Supported Events</strong></summary>

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

## [Scuttlebraced](./Scuttlebraced)

[![Thunderstore Link](https://img.shields.io/thunderstore/dt/Bergbok/Scuttlebraced?style=flat&logo=thunderstore&logoColor=de1f31&labelColor=fafafa&color=de1f31)](https://thunderstore.io/c/hollow-knight-silksong/p/Bergbok/Scuttlebraced)

Always-on [Scuttlebrace](https://hollowknight.wiki/w/Scuttlebrace).

Barely works at the moment. Work in progress.

<details>
<summary><strong>Video</strong></summary>

https://github.com/user-attachments/assets/df9e905e-c588-4f1f-bec6-4104c828a1c3

</details>

Also inspired by [something Skurry said](https://youtu.be/h8JBDvC4JlI?t=1948).

## [Guts](./Guts)

[![Thunderstore Link](https://img.shields.io/thunderstore/dt/Bergbok/Guts?style=flat&logo=thunderstore&logoColor=de1f31&labelColor=fafafa&color=de1f31)](https://thunderstore.io/c/hollow-knight-silksong/p/Bergbok/Guts)

> It was much too big to be called a sword. Massive, thick, heavy and far too rough. Indeed, it was like a heap of raw iron.

Lets you resize your needle.

<details>
<summary><strong>Screenshot</strong></summary>

<div align=center>
	<picture>
		<img src='https://i.imgur.com/Q16mf36.jpeg' alt='Hunter crest nail slash with 3x scale modifier' />
	</picture>
</div>

</details>

## [GunZ](./GunZ)

[![Thunderstore Link](https://img.shields.io/thunderstore/dt/Bergbok/GunZ?style=flat&logo=thunderstore&logoColor=de1f31&labelColor=fafafa&color=de1f31)](https://thunderstore.io/c/hollow-knight-silksong/p/Bergbok/GunZ)

Lets you animation cancel most actions. Work in progress.

# Installing

> If you don't want to manually install you could use a Thunderstore mod manager like [r2modman](https://github.com/ebkr/r2modmanPlus) or [Gale](https://github.com/Kesomannen/gale)

1. [Install BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html#installing-bepinex-1)
2. If running through Steam on [*nix](https://en.wikipedia.org/wiki/Unix-like) systems, [set up launch options](https://docs.bepinex.dev/articles/advanced/steam_interop.html#2-set-up-permissions)

> [!TIP]  
> If `./run_bepinex.sh %command%` doesn't work, try:
> ```bash
> eval $(echo "%command%" | ./run_bepinex.sh "Hollow Knight Silksong")
> ```

3. Download the mod you want to install from [GitHub releases](github.com/Bergbok/Silksong-Mods/releases) or [Thunderstore](https://thunderstore.io/c/hollow-knight-silksong/p/Bergbok/?section=mods)
4. Extract the downloaded archive to `silksong_path/BepInEx/plugins`
5. Launch the game
6. [Edit mod configs](https://docs.bepinex.dev/articles/user_guide/configuration.html#configuring-plugins) by using BepInEx's configuration manager (F1 by default), or by editing files in `silksong_path/BepInEx/plugins`

# Building

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
https://thunderstore.io/api/experimental/community/hollow-knight-silksong/category/
-->
