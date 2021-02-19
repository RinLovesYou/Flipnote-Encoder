# Flipnote-Signer
Shitty C# console application that runs a bunch of console commands to sign fipnotes

Most of the work was done by khang06 and his [dsiflipencoder](https://github.com/khang06/dsiflipencode), I only worked on automating the creation of flipnotes, and made the signing logic.

Install ffmpeg and imagemagick before running this.

[![ForTheBadge built-with-swag](http://ForTheBadge.com/images/badges/built-with-swag.svg)](https://GitHub.com/Naereen/) 

## Todo
* gui maybe
* idk

# How to use
in order for this to not kill itself here's what you need to do

* download and unzip the latest [release](https://github.com/RinLovesYou/Flipnote-Signer/releases) 
* place the video called `input.mp4` in the `frames` folder)
* (OPTIONAL) place the Flipnote Studio Private Key (good luck getting that one) called `fnkey.pem` in the same folder as the exe

As long as you have your `input.mp4` located in `frames` you can just double click `EncodeAndSign.exe`
This program will do the following

### DELETE OLD `frames` CONTENT BEFORE DOING A NEW ONE

* If no frames exist, it will split the video into frames, and dither them
* If no audio exists, it will create `audio.wav`
* Create a random thumbnail.bin, giving funny random thumbnails
* Create the generated Flipnote as a .pmm file
* Unlock that ppm, in case you want to upload it to freenote/edit it in flipnote
* sign the ppm so it can be used on the original Flipnote Studio

# FAQ
* Q: why won't it play on my dsi? A: You don't have the flipnote private key
* Q: Can you give it to me? A: no good luck googling for it
* Q: Can you add x? A: Yes! Maybe! suggest in [issues](https://github.com/RinLovesYou/Flipnote-Signer/issues)

discord: Rin#6969
twitter: @does_rin

[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/K3K61YCS7)
