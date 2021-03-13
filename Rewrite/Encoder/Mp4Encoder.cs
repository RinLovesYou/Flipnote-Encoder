using FFMpegCore;
using FFMpegCore.Enums;
using PPMLib;
using PPMLib.Winforms;
using Rewrite.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rewrite.Encoder
{
    public class Mp4Encoder
    {
        private PPMFile Flipnote { get; set; }

        /// <summary>
        /// Simple Mp4 Encoder. Requires FFMpeg to be installed in path.
        /// </summary>
        /// <param name="flipnote"></param>
        public Mp4Encoder(PPMFile flipnote)
        {
            this.Flipnote = flipnote;
        }

        /// <summary>
        /// Encode the Mp4.
        /// </summary>
        /// <returns>Mp4 byte array</returns>
        public byte[] EncodeMp4()
        {
            return Encode();
        }

        /// <summary>
        /// Encode the Mp4 and save it to the specified path
        /// </summary>
        /// <param name="path">Creates path if it doesn't exist. Doesn't save if path is "temp"</param>
        /// <returns>Mp4 byte array</returns>
        public byte[] EncodeMp4(string path)
        {
            return Encode(path);
        }

        /// <summary>
        /// Encode the Mp4 with the specified scale multiplier
        /// </summary>
        /// <param name="scale">Scale Multiplier</param>
        /// <returns>Mp4 byte array</returns>
        public byte[] EncodeMp4(int scale)
        {
            return Encode("out", scale);
        }

        /// <summary>
        /// Encode the Mp4 with the specified scale and save it to the given path.
        /// </summary>
        /// <param name="path">Creates path if it doesn't exist. Doesn't save if path is "temp"</param>
        /// <param name="scale">Scale Multiplier</param>
        /// <returns>Mp4 byte array</returns>
        public byte[] EncodeMp4(string path, int scale)
        {
            return Encode(path, scale);
        }

        private byte[] Encode(string path = "temp", int scale = 1)
        {
            try
            {
                if (!Directory.Exists("temp"))
                {
                    Directory.CreateDirectory("temp");
                }
                else
                {
                    Cleanup();
                }

                for (int i = 0; i < Flipnote.FrameCount; i++)
                {
                    PPMRenderer.GetFrameBitmap(Flipnote.Frames[i]).Save($"temp/frame_{i}.png");
                }

                var frames = Directory.EnumerateFiles("temp").ToArray();
                MathUtils.NumericalSort(frames);

                File.WriteAllBytes("temp/audio.wav", Flipnote.Audio.GetWavBGM(Flipnote));

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var a = FFMpegArguments
                        .FromConcatInput(frames, options => options
                        .WithFramerate(Flipnote.Framerate))
                        .AddFileInput("temp/audio.wav", false)
                        .OutputToFile($"{path}/{Flipnote.CurrentFilename}.mp4", true, o =>
                        {
                            o.Resize(256 * scale, 192 * scale)
                            .WithVideoCodec(VideoCodec.LibX264)
                            .ForcePixelFormat("yuv420p")
                            .ForceFormat("mp4");
                        });

                a.ProcessSynchronously();

                var mp4 = File.ReadAllBytes($"{path}/{Flipnote.CurrentFilename}.mp4");
                

                Cleanup();

                return mp4;


            }
            catch (Exception e)
            {
                Cleanup();
                return null;
            }
        }

        private void Cleanup()
        {
            if (!Directory.Exists("temp"))
            {
                return;
            }
            var files = Directory.EnumerateFiles("temp", "*.png");

            files.ToList().ForEach(file =>
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    // idk yet
                }

            });
            Directory.Delete("temp");
        }
    }
}
