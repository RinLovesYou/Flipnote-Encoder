using NAudio.Wave;
using PPMLib;
using PPMLib.Extensions;
using Rewrite.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rewrite.Encoder
{
    class FlipnoteEncoder
    {
        private EncodeConfig Config { get; set; }

        public FlipnoteEncoder()
        {
            Config = MainProgram.FlipnoteConfig;
        }

        public PPMFile Encode()
        {
            PPMFile Dummy = new PPMFile();
            PPMFile encoded = new PPMFile();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Getting Dummy...");
            try
            {
                var DummyPath = Directory.GetFiles("DummyFlipnote", "*.ppm");
                Dummy.LoadFrom(DummyPath[0]);
            } catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not get Dummy!");
                throw e;
            }

            Encoder encoder = new Encoder();
            var audio = encoder.PrepareAudio();
            encoder.PrepareFrames();

            List<PPMFrame> EncodedFrames = new List<PPMFrame>();

            Directory.CreateDirectory("out");

            switch(Config.ColorMode)
            {
                case 1: EncodedFrames = encoder.BlackWhiteFrames(Config.InputFolder).ToList(); break;
                case 2: EncodedFrames = encoder.ThreeColorFrames(Config.InputFolder, PenColor.Red).ToList(); break;
                case 3: EncodedFrames = encoder.ThreeColorFrames(Config.InputFolder, PenColor.Blue).ToList(); break;
                case 4: EncodedFrames = encoder.FullColorFrames(Config.InputFolder).ToList(); break;
                default: EncodedFrames = encoder.BlackWhiteFrames(Config.InputFolder).ToList(); break;
            }

            if (File.Exists($"{Config.InputFolder}/{Config.InputFilename}.wav"))
            {
                using (WaveFileReader reader = new WaveFileReader($"{Config.InputFolder}/{Config.InputFilename}.wav"))
                {

                    byte[] buffer = new byte[reader.Length];
                    int read = reader.Read(buffer, 0, buffer.Length);
                    short[] sampleBuffer = new short[read / 2];
                    Buffer.BlockCopy(buffer, 0, sampleBuffer, 0, read);

                    List<byte> bgm = new List<byte>();
                    AdpcmEncoder aencoder = new AdpcmEncoder();
                    for (int i = 0; i < sampleBuffer.Length; i += 2)
                    {
                        try
                        {
                            bgm.Add((byte)(aencoder.Encode(sampleBuffer[i]) | aencoder.Encode(sampleBuffer[i + 1]) << 4));
                        }
                        catch (Exception e)
                        {

                        }

                    }

                    encoded = PPMFile.Create(Dummy.CurrentAuthor, EncodedFrames, bgm.ToArray());
                }
            } else
            {
                encoded = PPMFile.Create(Dummy.CurrentAuthor, EncodedFrames, new List<byte>().ToArray());
            }

            //encoded.Save($"out/{encoded.CurrentFilename}.ppm");

            return encoded;
            
        }
    }
}
