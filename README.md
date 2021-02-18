# Flipnote-Signer
Shitty C# console application that runs a bunch of console commands to sign fipnotes

Install python 3, ffmpeg and imagemagick before running this.

try running ppmSignature.py yourself first, there might be an error that you're missing a dependency, you can install that using pip or npm i think

# Folder Structure
in order for this to not kill itself here's what you need to do

Create a folder called whatever you want

Fill the folder with the following files:

* folder called `frames` (place the video called `input.mp4` here)
* the latest release of [dsiflipencode](https://github.com/khang06/dsiflipencode), including the exe and dependency DLLs
* The Flipnote Studio Private Key (good luck getting that one) called `fnkey.pem`
* The `ppmSignature.py` script, that handles signing (good luck getting that one too lol)
* and the latest EXE found in [releases](https://github.com/RinLovesYou/Flipnote-Signer/releases) 

As long as you have your `input.mp4` located in `frames` you can just double click `EncodeAndSign.exe`
This program will do the following

* If no frames exist, it will split the video into frames, and dither them
* If no audio exists, it will create `audio.wav`
* Create a random thumbnail.bin, giving funny random thumbnails
* Create the generated Flipnote as a .pmm file
* Unlock that ppm, in case you want to upload it to freenote/edit it in flipnote
* sign the ppm so it can be used on the original Flipnote Studio

# This is made for a very small number of people in possession of the flipnote key and the signing script
do not ask for the key or the script
you won't get it
if you don't have it this is not for you
