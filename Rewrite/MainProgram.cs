using FFMpegCore;
using Newtonsoft.Json;
using Octokit;
using PPMLib;
using PPMLib.Encoders;
using Rewrite.Encoder;
using Rewrite.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Rewrite
{
    internal class MainProgram
    {
        //Github Update Checks
        private static bool Update = false;
        private static async Task UpdateCheck()
        {
            //Get all releases from GitHub
            //Source: https://octokitnet.readthedocs.io/en/latest/getting-started/
            GitHubClient client = new GitHubClient(new ProductHeaderValue("Update-Check"));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("RinLovesYou", "Flipnote-Encoder");

            //Setup the versions
            Version latestGitHubVersion = new Version(releases[0].TagName);
            Version localVersion = new Version("5.0.1");
            // weed release

            //Compare the Versions
            //Source: https://stackoverflow.com/questions/7568147/compare-version-numbers-without-using-split-function
            int versionComparison = localVersion.CompareTo(latestGitHubVersion);
            if (versionComparison < 0)
            {
                Update = true;
            }
            else if (versionComparison > 0)
            {
                //not gonna happen
            }
            else
            {
                Update = false;
            }
        }

        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        private static bool Running { get; set; }
        public static EncodeConfig FlipnoteConfig { get; set; }

        private static void Main(string[] args)
        {
            try
            {
                var task = UpdateCheck();
                task.Wait();
            }
            catch (Exception e)
            {

            }

            if (Update)
            {
                Console.WriteLine("A Newer Version is available! would you like to update? y/n");
                var answer = Console.ReadKey(true);
                if (answer.Key == ConsoleKey.Y)
                {
                        OpenBrowser("http://www.github.com/RinLovesYou/Flipnote-Encoder/releases/latest");          
                    return;
                }
            }


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
            if (encoded != null)
            {
                Directory.CreateDirectory("tmp");
                encoded.Save($"tmp/{encoded.CurrentFilename}.ppm");

                if (FlipnoteConfig.Split)
                {
                    double bytelength = new FileInfo($"tmp/{encoded.CurrentFilename}.ppm").Length;
                    bytelength = bytelength / 1024;
                    Console.WriteLine(bytelength);
                    var MB = bytelength / 1024;
                    if (MB >= 1)
                    {
                        List<PPMFile> files = new List<PPMFile>();
                        Console.WriteLine(MB);
                        var framesframes = encoded.Frames.ToArray().Split((int)(encoded.Frames.Length / MB + 1));
                        var audioaudio = encoded.Audio.SoundData.RawBGM.ToArray().Split((int)(encoded.Audio.SoundData.RawBGM.Length / MB + 1));
                        if (MB > 1.3)
                        {
                            framesframes = encoded.Frames.ToArray().Split((int)(encoded.Frames.Length / MB + 2));
                            audioaudio = encoded.Audio.SoundData.RawBGM.ToArray().Split((int)(encoded.Audio.SoundData.RawBGM.Length / MB + 2));
                        }

                        for (int i = 0; i < framesframes.Count(); i++)
                        {
                            var aaa = PPMFile.Create(encoded.CurrentAuthor, framesframes.ToList()[i].ToList(), audioaudio.ToList()[i].ToArray());
                            aaa.Save($"out/{aaa.CurrentFilename}_{i}.ppm");
                        }
                    }
                    else
                    {
                        encoded.Save($"out/{encoded.CurrentFilename}.ppm");
                    }
                }
                else
                {
                    encoded.Save($"out/{encoded.CurrentFilename}.ppm");
                }
                var mp4try = new PPMFile();
                mp4try.LoadFrom($"tmp/{encoded.CurrentFilename}.ppm");
                Mp4Encoder mp4 = new Mp4Encoder(mp4try);
                var a = mp4.EncodeMp4("out", 2);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was a problem creating the flipnote.");
                Console.WriteLine("Please join the support server for further assistance with this issue");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Press any Key to continue...");
                Console.ReadKey();
            }
            Cleanup();
        }

        private static void Cleanup()
        {
            if (Directory.Exists("tmp"))
            {
                try
                {
                    string[] files = Directory.EnumerateFiles("tmp").ToArray();
                    files.ToList().ForEach(f =>
                    {
                        File.Delete(f);
                    });
                    Directory.Delete("tmp");
                }
                catch (Exception e)
                {

                }
            }
            if (Directory.Exists($"out/temp"))
            {
                try
                {
                    string[] files = Directory.EnumerateFiles("out/temp").ToArray();
                    files.ToList().ForEach(f =>
                    {
                        File.Delete(f);
                    });
                    Directory.Delete("out/temp");
                }
                catch (Exception e)
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
            }
            catch (Exception e)
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
                newEncodeConfig.SplitAmount = 2;
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