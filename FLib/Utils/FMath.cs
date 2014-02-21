using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace FLib
{
    public class FMath
    {
        public static float MahalanobisDistance(Color[] pixels1, Color[] pixels2)
        {
            float sqdist = 0;

            foreach (Func<Color, float> getter in new Func<Color, float>[] {
                col => col.R,
                col => col.G,
                col => col.B
            })
            {
                float avg1, vrc1;
                float avg2, vrc2;
                GetAverageVariance(pixels1, getter, out avg1, out vrc1);
                GetAverageVariance(pixels2, getter, out avg2, out vrc2);
                sqdist += (avg1 - avg2) * (avg1 - avg2);
            }

            return (float)Math.Sqrt(sqdist);
        }
        public static void Swap<T>(ref T x, ref T y)
        {
            T t = x;
            x = y;
            y = t;
        }

        public static void GetAverageVariance(Color[] pixels, Func<Color, float> getter, out float avg, out float vrc)
        {
            float sum = 0;
            float sqsum = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                float val = getter(pixels[i]);
                sum += val;
                sqsum += val * val;
            }
            avg = sum / pixels.Length;
            vrc = sqsum / pixels.Length - avg * avg;
        }

    }
}
