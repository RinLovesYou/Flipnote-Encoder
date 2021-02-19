# Flipnote-Signer
Shitty C# console application that runs a bunch of console commands to sign fipnotes

Install ffmpeg and imagemagick before running this.

[![ForTheBadge built-with-swag](http://ForTheBadge.com/images/badges/built-with-swag.svg)](https://GitHub.com/Naereen/) 

## Todo
* Remove external depencencies (internalize ppmSignature.py(✓) and dsiflipencode(x) logic)
* gui maybe
* idk

# Folder Structure
in order for this to not kill itself here's what you need to do

Add the following files to the root folder:

* place the video called `input.mp4` in the `frames` folder)
* the latest release of [dsiflipencode](https://github.com/khang06/dsiflipencode), including the exe and dependency DLLs
* The Flipnote Studio Private Key (good luck getting that one) called `fnkey.pem`
* and the latest [release](https://github.com/RinLovesYou/Flipnote-Signer/releases) 

As long as you have your `input.mp4` located in `frames` you can just double click `EncodeAndSign.exe`
This program will do the following

DELETE OLD `frames` CONTENT BEFORE DOING A NEW ONE

* If no frames exist, it will split the video into frames, and dither them
* If no audio exists, it will create `audio.wav`
* Create a random thumbnail.bin, giving funny random thumbnails
* Create the generated Flipnote as a .pmm file
* Unlock that ppm, in case you want to upload it to freenote/edit it in flipnote
* sign the ppm so it can be used on the original Flipnote Studio

# This is made for a very small number of people in possession of the flipnote key
do not ask for the key

you won't get it

if you don't have it, you can still use this to generate flipnotes, it just won't play back on a real dsi

[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/K3K61YCS7)
