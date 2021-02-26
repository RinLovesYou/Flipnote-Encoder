using EncodeAndSign.Extensions;
using System;

namespace EncodeAndSign.Encoder
{

    //Encodes the audio in a way that Flipnote likes
    //you *could* just pass the raw bytes from the wav but it'll sound a bit weird
    public class AdpcmEncoder
    {
        private int prev_sample { get; set; }
        private int step_index { get; set; }
        public AdpcmEncoder()
        {
            this.prev_sample = 0;
            this.step_index = 0;
        }

        public int[] IndexTable = new int[16]
        {
            -1, -1, -1, -1, 2, 4, 6, 8,
            -1, -1, -1, -1, 2, 4, 6, 8,
        };

        public int[] StepTable = new int[89]
        {
            7,     8,     9,    10,    11,    12,    13,    14,    16,    17,
            19,    21,    23,    25,    28,    31,    34,    37,    41,    45,
            50,    55,    60,    66,    73,    80,    88,    97,   107,   118,
            130,   143,   157,   173,   190,   209,   230,   253,   279,   307,
            337,   371,   408,   449,   494,   544,   598,   658,   724,   796,
            876,   963,  1060,  1166,  1282,  1411,  1552,  1707,  1878,  2066,
            2272,  2499,  2749,  3024,  3327,  3660,  4026,  4428,  4871,  5358,
            5894,  6484,  7132,  7845,  8630,  9493, 10442, 11487, 12635, 13899,
            15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794, 32767
        };

        static int swapNibbles(int x)
        {
            return ((x & 0x0F) << 4 |
                    (x & 0xF0) >> 4);
        }

        public int Encode(short sample)
        {

            int nib = swapNibbles(sample);

            int delta = sample - prev_sample;
            int enc_sample = 0;

            if (delta < 0)
            {
                enc_sample = 8;
                delta = -delta;
            }

            enc_sample += Math.Min(7, (delta * 4 / StepTable[step_index]));
            prev_sample = sample;
            step_index = Utils.Clamp(step_index + IndexTable[enc_sample], 0, 79);

            return enc_sample;

        }
    }
}
