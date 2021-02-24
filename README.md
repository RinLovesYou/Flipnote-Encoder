# Flipnote-Encoder

A Flipnote encoder written in C#

uses binaries from [ffmpeg](https://ffmpeg.org) and [imagemagick](https://imagemagick.org/index.php) video/image manipulation

[![ForTheBadge built-with-swag](http://ForTheBadge.com/images/badges/built-with-swag.svg)](https://github.com/RinLovesYou) 

# How to use
### DELETE OLD `frames` CONTENT BEFORE DOING A NEW ONE
* download and unzip the latest [release](https://github.com/RinLovesYou/Flipnote-Encoder/releases) 
* place the video called `input.mp4` in the `frames` folder)
* (Optional) Replace the flipnote in DummyFlipnote with one of your own! It'll embed your user information in the encoded Flipnote
* (Optional) place the Flipnote Studio Private Key (good luck getting that one) called `fnkey.pem` in the same folder as the exe

As long as you have your `input.mp4` located in `frames` you can just double click `EncodeAndSign.exe` it should work (Playback on a real dsi with 1mb+ flipnotes is not guaranteed)

Expect bugs and report them in the [issues](https://github.com/RinLovesYou/Flipnote-Encoder/issues) section please

it won't sign a flipnote if no `fnkey.pem` exists, you can still play it back with most online players like [rakujira](https://flipnote.rakujira.jp)

![screenshot](https://media.discordapp.net/attachments/738116823035150356/812439551930007582/unknown.png)

# FAQ
* Q: why won't it play on my dsi? A: You don't have the flipnote private key
* Q: Can you give it to me? A: no good luck googling for it
* Q: Can you add x? A: Yes! Maybe! suggest in [issues](https://github.com/RinLovesYou/Flipnote-Signer/issues)

discord: `Rin#6969`

twitter: `@does_rin`

(side note: current source code does not reflect latest release as it was a hotfix)

## Special Thanks
* [khang06](https://github.com/khang06) For his awesome encoder, being the inspiration, and his help in understanding audio
* [miso-xyz](https://github.com/miso-xyz) For his FlipnoteDesktop program, that provided the grunt work in writing the Flipnote
* guys from [DSiBrew](https://dsibrew.org/wiki/Main_Page) and [Flipnote Collective](https://github.com/Flipnote-Collective) for their awesome documentation on .PPM file format.

# Note
Flipnote Studio is a trademark of Nintendo. This project is not linked to them in any way. It is intended for educational purposes only.

[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/K3K61YCS7)
