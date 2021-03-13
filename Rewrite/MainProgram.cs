using FFMpegCore;
using Newtonsoft.Json;
using PPMLib;
using Rewrite.Encoder;
using Rewrite.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rewrite
{
    internal class MainProgram
    {
        private static bool Running { get; set; }
        public static EncodeConfig FlipnoteConfig { get; set; }

        private static void Main(string[] args)
        {
            Running = true;

            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "ffmpeg/bin" });

            while (Running)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Clear();

                Console.WriteLine("What would you like to do?");
                Console.WriteLine("1: Encode a video to Flipnote");
                Console.WriteLine("2: Decode a Flipnote");
                var Selection = Console.ReadKey(true);

                switch (Selection.Key)
                {
                    case ConsoleKey.D1: EncodeFlipnote(); break;
                    case ConsoleKey.D2: DecodeFlipnote(); break;
                    default: break;
                }
            }
        }

        public static void EncodeFlipnote()
        {
            CreateEncodeConfig();

            var encoder = new FlipnoteEncoder();

            var encoded = encoder.Encode();
            Mp4Encoder mp4 = new Mp4Encoder(encoded);
            var a = mp4.EncodeMp4("out", 2);
            if (FlipnoteConfig.Split)
            {


                encoded.Save($"tmp/{encoded.CurrentFilename}.ppm");
                var bytelength = new FileInfo($"tmp/{encoded.CurrentFilename}.ppm").Length;
                bytelength = bytelength / 1024;
                Console.WriteLine(bytelength / 1024);
                var MB = bytelength / 1024;
                if (MB >= 1)
                {
                    List<PPMFile> files = new List<PPMFile>();
                    File.Delete($"tmp/{encoded.CurrentFilename}.ppm");
                    var framesframes = encoded.Frames.ToArray().Split((int)(encoded.Frames.Length / (MB + 2)));
                    var audioaudio = encoded.Audio.SoundData.RawBGM.ToArray().Split((int)(encoded.Audio.SoundData.RawBGM.Length / (MB + 2)));
                    int i = 0;
                    framesframes.ToList().ForEach(frames =>
                    {
                        var aaa = PPMFile.Create(encoded.CurrentAuthor, frames.ToList(), audioaudio.ToList()[i].ToArray());
                        aaa.Save($"out/{aaa.CurrentFilename}_{i}.ppm");
                        i++;
                    });
                }
                else
                {
                    encoded.Save($"out/{encoded.CurrentFilename}.ppm");
                }
            } else
            {
                encoded.Save($"out/{encoded.CurrentFilename}.ppm");
            }
            Cleanup();
        }

        private static void Cleanup()
        {
            if(Directory.Exists("tmp"))
            {
                try
                {
                    string[] files = Directory.EnumerateFiles("tmp", "*.png").ToArray();
                    files.ToList().ForEach(f =>
                    {
                        File.Delete(f);
                    });
                    Directory.Delete("tmp");
                } catch(Exception e)
                {

                }
            }

            try
            {
                string[] files = Directory.EnumerateFiles(FlipnoteConfig.InputFolder, "*.png").ToArray();
                files.ToList().ForEach(f =>
                {
                    File.Delete(f);
                    if (File.Exists($"{FlipnoteConfig.InputFolder}/{FlipnoteConfig.InputFilename}.wav")) 
                    {
                        File.Delete($"{FlipnoteConfig.InputFolder}/{FlipnoteConfig.InputFilename}.wav");
                    }
                });
            } catch (Exception e)
            {

            }
        }

        private static void DecodeFlipnote()
        {
        }

        private static void CreateEncodeConfig()
        {
            if (!File.Exists("config.json"))
            {
                var newEncodeConfig = new EncodeConfig();
                newEncodeConfig.Accurate = true;
                newEncodeConfig.DitheringMode = 1;
                newEncodeConfig.ColorMode = 1;
                newEncodeConfig.Contrast = 0;
                newEncodeConfig.InputFilename = "input.mp4";
                newEncodeConfig.InputFolder = "frames";
                newEncodeConfig.Split = false;
                newEncodeConfig.DeleteOnFinish = true;
                FlipnoteConfig = newEncodeConfig;

                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.NullValueHandling = NullValueHandling.Ignore;

                using (StreamWriter sw = new StreamWriter("config.json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, newEncodeConfig);
                }
            }
            else
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.NullValueHandling = NullValueHandling.Ignore;

                using (StreamReader sw = new StreamReader("config.json"))
                using (JsonReader writer = new JsonTextReader(sw))
                {
                    var read = serializer.Deserialize<EncodeConfig>(writer);
                    FlipnoteConfig = read;
                }
            }
        }
    }
}