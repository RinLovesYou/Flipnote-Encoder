using EncodeAndSign.Data;
using ShellProgressBar;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;


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
        //shit that i need because the program might get stuck executing in the background, this is due to the use of console commands.
        //todo: internalize ffmpeg and imagemagick use
        private static bool _quitRequested = false;
        private static object _syncLock = new object();
        private static AutoResetEvent _waitHandle = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            // start the message pumping thread
            Thread msgThread = new Thread(MessagePump);
            msgThread.Start();
            // read input to detect "quit" command
            string command = string.Empty;
            do
            {
                //do the flipnote stuff
                doShit();
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

        private static void doShit()
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
                using (var pbar = new ProgressBar(totalTicks, "Flipnote Progress", options))
                {


                    if (!File.Exists("frames/frame_0.png"))
                    {

                        //todo: Stop running console commands
                        //this code sucks.

                        //Turn video into 30FPS, fixed sound sync issues (https://github.com/khang06/dsiflipencode/pull/5)
                        Process framerate = new Process();
                        ProcessStartInfo frameinfo = new ProcessStartInfo();

                        frameinfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        frameinfo.FileName = "cmd.exe";
                        frameinfo.Arguments = @"/C cd ffmpeg/bin && ffmpeg -i ..\..\frames\input.mp4 -filter:v fps=30 ..\..\frames\frame_%d.png";
                        frameinfo.RedirectStandardOutput = false;

                        framerate.StartInfo = frameinfo;
                        framerate.Start();

                        pbar.Tick();
                        using (var child = pbar.Spawn(totalTicks, "Changing Framerate...", childOptions))
                        {
                            child.Tick(2);
                            framerate.WaitForExit();
                            framerate.Dispose();


                            
                            Process framesplit = new Process();
                            ProcessStartInfo startInfo = new ProcessStartInfo();

                            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                            startInfo.FileName = "cmd.exe";
                            startInfo.Arguments = @"/C cd ffmpeg/bin && ffmpeg -i ..\..\frames\frame_%d.png -vf scale=256:192 ..\..\frames\frame_%d.png";
                            startInfo.RedirectStandardOutput = false;
                            framesplit.StartInfo = startInfo;
                            framesplit.Start();
                            child.Tick(5, "Splitting Frames...");
                            framesplit.WaitForExit();
                            framesplit.Dispose();
                            child.Tick(10, "Frames Split!");
                        }


                        File.Copy("frames/frame_1.png", "frames/frame_0.png");

                        Process dither = new Process();
                        ProcessStartInfo ditherInfo = new ProcessStartInfo();

                        ditherInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        ditherInfo.FileName = "cmd.exe";
                        ditherInfo.Arguments = @"/C cd magick && magick mogrify -format png -colors 2 -type bilevel ..\frames\*.png";
                        ditherInfo.RedirectStandardOutput = false;

                        dither.StartInfo = ditherInfo;
                        dither.Start();

                        pbar.Tick();
                        using (var child = pbar.Spawn(totalTicks, "Dithering...", childOptions))
                        {
                            child.Tick(5);
                            dither.WaitForExit();
                            dither.Dispose();
                            child.Tick(10, "Dithered!");
                        }

                    }
                    else
                    {
                        pbar.Tick(2);
                    }

                    if (!File.Exists("frames/audio.wav"))
                    {
                        Process audio = new Process();
                        ProcessStartInfo audioInfo = new ProcessStartInfo();

                        audioInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        audioInfo.FileName = "cmd.exe";
                        audioInfo.Arguments = @"/C cd ffmpeg/bin && ffmpeg -i ..\..\frames\input.mp4 -ac 1 -ar 8192 ..\..\frames\audio.wav";
                        audioInfo.RedirectStandardOutput = false;

                        audio.StartInfo = audioInfo;
                        audio.Start();

                        pbar.Tick();
                        using (var child = pbar.Spawn(totalTicks, "Writing Wav...", childOptions))
                        {
                            child.Tick(5);
                            audio.WaitForExit();
                            audio.Dispose();
                            child.Tick(10, "Audio Written!");
                        }

                    }
                    else
                    {
                        pbar.Tick();
                    }

                    pbar.Tick();

                    
                    using (var child = pbar.Spawn(totalTicks, "Generating PPM...", childOptions))
                    {
                        //Get dummy flipnote
                        //File can be replaced by user to automatically embed their own account information.
                        string[] dummyPath = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "/DummyFlipnote", "*.ppm");
                        Flipnote dummy = new Flipnote(dummyPath[0]);

                        //Get Frames and sort them
                        child.Tick(2);
                        string[] frames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "/frames", "*.png");
                        NumericalSort(frames);
                        child.Tick(5);
                        //Create Encoder
                        var encoder = new Encoder.Encoder(frames, dummy);
                        child.Tick(8);
                        encoder.ResultNote.Save("out/" + encoder.ResultNote.Filename);

                        if (encoder.ResultNote.Signed)
                        {
                            pbar.Tick(10, "Finished! Flipnote: Signed! Press any key to exit.");

                        }
                        else
                        {
                            pbar.Tick(10, "Finished! Flipnote: Unsigned. Press any key to exit.");
                        }
                        child.Tick(10, "PPM Written!");
                        Console.ReadKey();
                        return;

                    }





                }

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
