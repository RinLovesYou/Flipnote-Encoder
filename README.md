# Flipnote-Encoder

The Cross-Platform Flipnote Encoder and Signer written in .NET

built with [PPMLib](https://github.com/miso-xyz/PPMLib)

uses binaries from [ffmpeg](https://ffmpeg.org) for video manipulation.

[![ForTheBadge built-with-swag](http://ForTheBadge.com/images/badges/built-with-swag.svg)](https://github.com/RinLovesYou) 

[![Support Server](https://img.shields.io/discord/815244291366453259.svg?label=Support_Server&logo=Discord&colorB=7289da&style=for-the-badge)](https://discord.gg/MBM9ZeAjna)

# Quickstart
* download and unzip the latest [release](https://github.com/RinLovesYou/Flipnote-Encoder/releases) 
* place the video called `input.mp4` in the `frames` folder, make sure no other files exist there
* Replace the Dummy Flipnote with one of your own. This will embed your User Information
* (Optional) place the Flipnote Studio Private Key (good luck getting that one) called `fnkey.pem` in the same folder as the exe

[![forthebadge](https://forthebadge.com/images/badges/powered-by-energy-drinks.svg)](https://forthebadge.com)

# Cross-Platform Information
Here are the ways you can run the Flipnote Encoder:

* Windows: Just double click the .exe
* Linux: `./FlipnoteEncoder` in a terminal, or double click the .sh script
* OSX: `./FlipnoteEncoder` in a terminal I'd assume.

# config.json
Located inside the Root folder is a config.json. Should it not exist, a new one will be created by the program.
Here you can see all the config Items with a description of how to use them:

  * "DitheringMode": 1, - refer to [wiki](https://github.com/RinLovesYou/Flipnote-Encoder/wiki/Dithering-Modes)
  * "ColorMode": 1, - refer to [wiki](https://github.com/RinLovesYou/Flipnote-Encoder/wiki/Color-Modes)
  * "Accurate": true, - Wether to force 30FPS or not. Fixes audio sync. 
  * "Contrast": 0, - How much contrast to add
  * "InputFolder": "frames",
  * "InputFilename": "input.mp4",
  * "Split": false, - Wether or not to split the resulting Flipnote.
  * "SplitAmount": 2, - Unused at the moment. Encoder automatically tries to pick the best split amount
  * "DeleteOnFinish": true - I don't even think i added a case for this. Why wouldn't you want this? Deletes all temp frames.

Expect bugs and report them in the [issues](https://github.com/RinLovesYou/Flipnote-Encoder/issues) section please.

it won't sign a flipnote if no `fnkey.pem` exists, you can still play it back with most online players like [rakujira](https://flipnote.rakujira.jp) though.

![screenshot](https://media.discordapp.net/attachments/738116823035150356/812439551930007582/unknown.png)

# FAQ
* Q: why won't it play on my dsi? A: You don't have the flipnote private key
* Q: Can you give it to me? A: no good luck googling for it
* Q: Can you add x? A: Yes! Maybe! suggest in [issues](https://github.com/RinLovesYou/Flipnote-Signer/issues)
* Q: Is this open source? A: Yes! you can find the source code [here](https://github.com/RinLovesYou/LinuxNote).

[discord:](https://discord.gg/MBM9ZeAjna) `Rin#6969`

[twitter:](https://twitter.com/does_rin) `@does_rin`

# Information for building it yourself
* Install the .NET 5 SDK
* Clone the [Source](https://github.com/RinLovesYou/LinuxNote)
* Edit the csproj file to buld for `win-x64`, `linux-x64` or `osx-x64`
* build [PPMLib](https://github.com/RinLovesYou/PPMLib)
* Edit the csproj file to include the path to the built PPMLib.dll
* Open a Terminal
* run `dotnet restore` and then `dotnet build --configuration Release`

## Special Thanks
* [khang06](https://github.com/khang06) For his awesome encoder, being the inspiration, and his help in understanding audio.
* [NotImplementedLife](https://github.com/NotImplementedLife) For their FlipnoteDesktop program, which helped a lot in understanding the structure of a Flipnote.
* [JoshuaDoes](https://github.com/joshuaDoes) For being really. really. really patient.
* guys from [DSiBrew](https://dsibrew.org/wiki/Main_Page) and [Flipnote Collective](https://github.com/Flipnote-Collective) for their awesome documentation on .PPM file format.

## Credits
* [PPMLib](https://github.com/miso-xyz/PPMLib)
* [FFMpegCore](https://github.com/rosenbjerg/FFMpegCore)
* [ImageSharp](https://github.com/SixLabors/ImageSharp)

# Note
Flipnote Studio is a trademark of Nintendo. This project is not linked to them in any way. It is intended for educational purposes only.

I am not responsible for how this tool is used. It is against Sudomemo TOS to upload encoded flipnotes there.
Consider [Freenote](https://discord.gg/jHAgKe2uJs) instead :)

[![forthebadge](https://forthebadge.com/images/badges/mom-made-pizza-rolls.svg)](https://forthebadge.com)
