using EncodeAndSign.Data;
using EncodeAndSign.Encoder;
using Newtonsoft.Json;
using Octokit;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


/* this code sucks. this code sucks. this code sucks. this code sucks.
 * this.
 * code.
 * sucks.
 * but it sucks a bit less now :)
*/

namespace EncodeAndSign
{
    class Program
    {
        private static bool Update = false;

        private static async Task UpdateCheck()
        {
            //Get all releases from GitHub
            //Source: https://octokitnet.readthedocs.io/en/latest/getting-started/
            GitHubClient client = new GitHubClient(new ProductHeaderValue("Update-Check"));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("RinLovesYou", "Flipnote-Encoder");

            //Setup the versions
            Version latestGitHubVersion = new Version(releases[0].TagName);
            Version localVersion = new Version("4.1.1");

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

        //shit that i need because the program might get stuck executing in the background, this is due to the use of console commands.
        //todo: internalize ffmpeg and imagemagick use
        private static bool _quitRequested = false;
        private static object _syncLock = new object();
        private static AutoResetEvent _waitHandle = new AutoResetEvent(false);

        private static Config config = null;

        static void Main(string[] args)
        {
            try
            {
                var task = UpdateCheck();
                task.Wait();
            } catch (Exception e)
            {

            }

            
            if (!File.Exists("config.json"))
            {
                var newConfig = new Config();
                newConfig.Accurate = true;
                newConfig.DitheringMode = 1;
                newConfig.Contrast = 0;
                newConfig.Split = false;
                config = newConfig;


                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.NullValueHandling = NullValueHandling.Ignore;

                using (StreamWriter sw = new StreamWriter("config.json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, newConfig);
                    // {"ExpiryDate":new Date(1230375600000),"Price":0}
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
                    var read = serializer.Deserialize<Config>(writer);
                    config = read;
                    // {"ExpiryDate":new Date(1230375600000),"Price":0}
                }
            }
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            // start the message pumping thread
            Thread msgThread = new Thread(MessagePump);
            msgThread.Start();
            // read input to detect "quit" command
            string command = string.Empty;
            do
            {

                if (Update)
                {
                    Console.WriteLine("A Newer Version is available! would you like to update? y/n");
                    var answer = Console.ReadKey(true);
                    if(answer.Key == ConsoleKey.Y)
                    {
                        Process.Start("http://www.github.com/RinLovesYou/Flipnote-Encoder/releases/latest");
                        return;
                    } else if (answer.Key == ConsoleKey.N)
                    {
                        CreateAndSignFlipnote();
                    }
                } else
                {
                    CreateAndSignFlipnote();
                }

                


                SetQuitRequested();
            } while (!_quitRequested);

            
            // signal that we want to quit
            SetQuitRequested();
            // wait until the message pump says it's done
            _waitHandle.WaitOne();
            // perform any additional cleanup, logging or whatever
        }

        private static void SetQuitRequested()
        {
            lock (_syncLock)
            {
                _quitRequested = true;
            }
        }

        private static void MessagePump()
        {
            do
            {
                // act on messages
            } while (!_quitRequested);
            _waitHandle.Set();
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            //close any background tasks associated with this program
            Process.GetProcessesByName("EncodeAndSign").ToList().ForEach(p =>
            {
                p.Kill();
                p.Dispose();
            });
            Console.WriteLine("exit");
        }

        private static void CreateAndSignFlipnote()
        {
            //no input, stop
            if (!File.Exists("frames/input.mp4")) return;

            try
            {
                //progressbar stuff
                const int totalTicks = 10;
                var options = new ProgressBarOptions
                {
                    ForegroundColor = ConsoleColor.Red,
                    ForegroundColorDone = ConsoleColor.Green,
                    BackgroundColor = ConsoleColor.DarkGray,
                    BackgroundCharacter = '\u2593',
                    ProgressBarOnBottom = true
                };
                var childOptions = new ProgressBarOptions
                {
                    ForegroundColor = ConsoleColor.Green,
                    BackgroundColor = ConsoleColor.DarkGreen,
                    CollapseWhenFinished = false,
                    ProgressCharacter = '─'
                };
                Bitmap[] bitmaps = null;
                using (var pbar = new ProgressBar(totalTicks, "Flipnote Progress", options))
                {

                    //frame stuff
                    if (!File.Exists("frames/frame_0.png"))
                    {

                        //todo: Stop running console commands
                        //Turn video into 30FPS, fixed sound sync issues (https://github.com/khang06/dsiflipencode/pull/5)
                        if (config.Accurate)
                        {
                            using (var child = pbar.Spawn(2, "Mode: Accurate, Splitting Frames...", childOptions))
                            {
                                #region Create30FPSframes
                                Process framerate = new Process();
                                ProcessStartInfo frameinfo = new ProcessStartInfo();
                                frameinfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                frameinfo.FileName = "cmd.exe";
                                frameinfo.Arguments = @"/C cd ffmpeg/bin && ffmpeg -i ..\..\frames\input.mp4 -filter:v fps=30 ..\..\frames\frame_%d.png";
                                frameinfo.RedirectStandardOutput = false;
                                framerate.StartInfo = frameinfo;
                                framerate.Start();
                                framerate.WaitForExit();
                                framerate.Dispose();
                                #endregion
                                child.Tick(1, "Mode: Accurate, Resizing Frames...");
                                #region Scale30FPSFrames
                                Process framesplit = new Process();
                                ProcessStartInfo startInfo = new ProcessStartInfo();

                                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                startInfo.FileName = "cmd.exe";
                                startInfo.Arguments = @"/C cd ffmpeg/bin && ffmpeg -i ..\..\frames\frame_%d.png -vf scale=256:192 ..\..\frames\frame_%d.png";
                                startInfo.RedirectStandardOutput = false;
                                framesplit.StartInfo = startInfo;
                                framesplit.Start();
                                framesplit.WaitForExit();
                                framesplit.Dispose();
                                #endregion
                                child.Tick(2, "Mode: Accurate, Frames Split!");
                            }
                            pbar.Tick(1);
                        }
                        else
                        {
                            using (var child = pbar.Spawn(2, "Mode: Fast", childOptions))
                            {
                                child.Tick(1, "Mode: Fast, Splitting Frames...");
                                #region SplitFrames
                                Process framerate = new Process();
                                ProcessStartInfo frameinfo = new ProcessStartInfo();
                                frameinfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                frameinfo.FileName = "cmd.exe";
                                frameinfo.Arguments = @"/C cd ffmpeg/bin && ffmpeg -i ..\..\frames\input.mp4 -vf scale=256:192 ..\..\frames\frame_%d.png";
                                frameinfo.RedirectStandardOutput = false;
                                framerate.StartInfo = frameinfo;
                                framerate.Start();
                                framerate.WaitForExit();
                                framerate.Dispose();
                                #endregion
                                child.Tick(2, "Mode: Fast, Frames Split!...");
                            }
                            pbar.Tick(1);
                        }

                        File.Copy("frames/frame_1.png", "frames/frame_0.png");

                        #region Dithering
                        string[] Bitmapframes = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "/frames", "*.png");
                        NumericalSort(Bitmapframes);
                        var imageEncoder = new ImageEncoder();
                        var mode = config.DitheringMode;
                        string modestring = String.Empty;
                        using (var child = pbar.Spawn(2, "Dithering...", childOptions))
                        {
                            switch (mode)
                            {
                                case 0:
                                    child.Tick(1, "Dithering Mode: None.");
                                    bitmaps = null;
                                    child.Tick(2, "Dithering Mode: None.");
                                    break;
                                case 14:
                                    child.Tick(1, "Dithering Mode: imagemagick bilevel...");
                                    bitmaps = null;
                                    #region mogrify

                                    Process dither = new Process();
                                    ProcessStartInfo ditherInfo = new ProcessStartInfo();

                                    ditherInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                    ditherInfo.FileName = "cmd.exe";
                                    ditherInfo.Arguments = "/C mogrify -format png -colors 2 -type bilevel frames/*.png";
                                    ditherInfo.RedirectStandardOutput = false;
                                    dither.StartInfo = ditherInfo;
                                    dither.Start();
                                    dither.WaitForExit();
                                    dither.Dispose();
                                    #endregion
                                    child.Tick(2, "Dithering Mode: imagemagick bilevel, Done!");
                                    break;
                                default:
                                    child.Tick(1, $"Dithering mode: {mode}");
                                    imageEncoder.DoRinDithering(Bitmapframes, mode, config.Contrast);
                                    child.Tick(2, $"Dithering mode: {mode}, Done!");
                                    break;
                            }
                            
                            pbar.Tick(2);
                        }
                        #endregion


                    }

                    pbar.Tick(2);

                    if (!File.Exists("frames/audio.wav"))
                    {
                        using (var child = pbar.Spawn(2, "Writing Audio...", childOptions))
                        {
                            child.Tick(1);
                            #region audio
                            Process audio = new Process();
                            ProcessStartInfo audioInfo = new ProcessStartInfo();

                            audioInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                            audioInfo.FileName = "cmd.exe";
                            audioInfo.Arguments = @"/C cd ffmpeg/bin && ffmpeg -i ..\..\frames\input.mp4 -ac 1 -ar 8192 ..\..\frames\audio.wav";
                            audioInfo.RedirectStandardOutput = false;
                            audio.StartInfo = audioInfo;
                            audio.Start();
                            audio.WaitForExit();
                            audio.Dispose();
                            #endregion
                            child.Tick(2, "Audio Written!");
                        }
                    }
                    pbar.Tick(3);


                    //ppm stuff

                    //Get dummy flipnote
                    //File can be replaced by user to automatically embed their own account information.
                    using (var child = pbar.Spawn(4, "Generating Flipnote...", childOptions))
                    {
                        child.Tick(1, "Getting Dummy Information...");
                        string[] dummyPath = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "/DummyFlipnote", "*.ppm");
                        Flipnote dummy = new Flipnote(dummyPath[0]);

                        pbar.Tick(4);

                        child.Tick(2, "Getting Frame Data...");
                        //Get Frames and sort them
                        Encoder.Encoder encoder = null;
                        pbar.Tick(5);
                        if (bitmaps == null)
                        {
                            string[] frames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "/frames", "*.png");
                            NumericalSort(frames);
                            child.Tick(3, "Encoding...");
                            encoder = new Encoder.Encoder(frames, dummy);
                        }
                        else
                        {
                            child.Tick(3, "Encoding...");
                            encoder = new Encoder.Encoder(bitmaps.ToList(), dummy);
                        }
                        pbar.Tick(6);



                        encoder.ResultNote.Save("out/" + encoder.ResultNote.Filename);

                        child.Tick(4, "Encoded!");

                        if (encoder.ResultNote.Signed)
                        {
                            pbar.Tick(10, "Finished! Flipnote: Signed. Press any key to exit.");
                            Console.ReadKey();
                            pbar.Dispose();
                        }
                        else
                        {
                            pbar.Tick(10, "Finished! Flipnote: Unsigned. Press any key to exit.");
                            Console.ReadKey();
                            pbar.Dispose();
                        }
                    }

                }
                
                return;




            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Process.GetProcessesByName("EncodeAndSign").ToList().ForEach(p =>
                {
                    p.Kill();
                    p.Dispose();
                });
            }


        }

        public static void NumericalSort(string[] ar)
        {
            Regex rgx = new Regex("([^0-9]*)([0-9]+)");
            Array.Sort(ar, (a, b) =>
            {
                var ma = rgx.Matches(a);
                var mb = rgx.Matches(b);
                for (int i = 0; i < ma.Count; ++i)
                {
                    int ret = ma[i].Groups[1].Value.CompareTo(mb[i].Groups[1].Value);
                    if (ret != 0)
                        return ret;

                    ret = int.Parse(ma[i].Groups[2].Value) - int.Parse(mb[i].Groups[2].Value);
                    if (ret != 0)
                        return ret;
                }

                return 0;
            });
        }

    }

}
