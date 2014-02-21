using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenCvSharp;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace FLib
{
    class TemplateMatching
    {
        public const float MatchingThreshold = 0.75f;
        public static Point lastMatchingPoint = Point.Empty;

        public static Rectangle ROI
        {
            get
            {
                return Screen.PrimaryScreen.Bounds;
            }
        }


        /// <summary>
        /// スクリーン画像とimageNameで指定した画像とのマッチング。Cv.MatchingTemplate()を利用
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static Rectangle findTemplate(IplImage tmpl, double threshold = -1, bool showNotFoundDialog = true)
        {
            // thresholdが不当な値だったらthresholdUpDownの数値を使う
            if (threshold < 0 || 100 < threshold) threshold = (double)80;

            // 0 ~ 1.0にスケールを合わせる
            threshold *= 0.01;

            // 時間計測開始
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            // 画像マッチング
            double min_val = 0, max_val = 0;
            CvPoint min_loc = CvPoint.Empty, max_loc = CvPoint.Empty;

            var invratio = 1.0;

            // 異なる解像度の画像を用意し小さい方から順に試していく
            // 期待値的に高速になるはず
            foreach (double ratio in new[] { /* 0.5, 0.75,*/ 1.0 })
            {
#if DEBUG
                Console.Write("[ratio = " + ratio + "] ");
                var stopwatch2 = new System.Diagnostics.Stopwatch();
                stopwatch2.Start();
#endif

                Matching(tmpl, ratio, ref min_val, ref min_loc, ref max_val, ref max_loc);

#if DEBUG
                stopwatch2.Stop();
                Console.WriteLine("max_val = " + max_val + "(" + stopwatch2.Elapsed.TotalSeconds + " s)");
#endif
                invratio = 1 / ratio;

                // 閾値以上のマッチ率が得られたら打ち切る
                if (threshold <= max_val) break;
            }

            // 時間計測終了
            stopwatch.Stop();
#if DEBUG
            Console.WriteLine("Ellapsed Time = " + stopwatch.Elapsed.TotalSeconds + " s");
            //Console.WriteLine("min_val = " + min_val);
            //Console.WriteLine("min_loc = " + min_loc);
            Console.WriteLine("max_val = " + max_val);
            //Console.WriteLine("max_loc = " + max_loc);
            Console.WriteLine("");
#endif

            // max_valがthresholdを超えなければマッチしなかったと判断する
            if (threshold > max_val)
            {
                if (showNotFoundDialog) MessageBox.Show("画像 はスクリーン内で見つかりませんでした。");
                return Rectangle.Empty;
            }

            return new Rectangle((int)(max_loc.X * invratio), (int)(max_loc.Y * invratio), tmpl.Size.Width, tmpl.Size.Height);
        }

        public static float Matching(IplImage tmpl, double ratio, ref double min_val, ref CvPoint min_loc, ref double max_val, ref CvPoint max_loc)
        {
            using (var bmp = TakeScreenshot(ROI))
            using (var target = BitmapConverter.ToIplImage(bmp))
            using (var small_target = new IplImage((int)(target.Size.Width * ratio), (int)(target.Size.Height * ratio), target.Depth, target.NChannels))
            using (var small_tmpl = new IplImage((int)(tmpl.Size.Width * ratio), (int)(tmpl.Size.Height * ratio), tmpl.Depth, tmpl.NChannels))
            {
                // 小さすぎたらマッチングは行わない
                if (small_tmpl.Width <= 0 || small_tmpl.Height <= 0) return 0;

                // 縮小した画像を用意
                target.Resize(small_target);
                tmpl.Resize(small_tmpl);

                // 画像マッチング
                var dstSize = new CvSize(small_target.Width - small_tmpl.Width + 1, small_target.Height - small_tmpl.Height + 1);
                using (var dst = Cv.CreateImage(dstSize, BitDepth.F32, 1))
                {
                    Cv.MatchTemplate(small_target, small_tmpl, dst, MatchTemplateMethod.CCoeffNormed);
                    Cv.MinMaxLoc(dst, out min_val, out max_val, out min_loc, out max_loc, null);

                    if (max_val >= MatchingThreshold)
                    {
                        lastMatchingPoint = new Point((int)(max_loc.X / ratio + tmpl.Width / 2), (int)(max_loc.Y / ratio + tmpl.Height / 2));
                    }
                    Console.WriteLine("matching: " + max_val);

                    return (float)max_val;
                }
            }
        }

        unsafe public static bool IsMatch(IplImage img1, IplImage target, float threshold)
        {
            try
            {
                if (target.Width < img1.Width) return false;
                if (target.Height < img1.Height) return false;

                Console.WriteLine("img1:" + img1.GetElemType() + ", " + img1.Depth);
                Console.WriteLine("target:" + target.GetElemType() + ", " + target.Depth);

                // 画像マッチング
                var dstSize = new CvSize(target.Width - img1.Width + 1, target.Height - img1.Height + 1);
                using (var dst = Cv.CreateImage(dstSize, BitDepth.F32, 1))
                {
                    double min_val = 0, max_val = 0;
                    CvPoint min_loc = CvPoint.Empty, max_loc = CvPoint.Empty;
                    Cv.MatchTemplate(target, img1, dst, MatchTemplateMethod.CCoeffNormed);
                    Cv.MinMaxLoc(dst, out min_val, out max_val, out min_loc, out max_loc, null);
                    Console.WriteLine("matching: " + max_val);
                    return max_val >= threshold;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }
        }

        public static Bitmap TakeScreenshot(Rectangle rect)
        {
            var size = new Size(rect.Width, rect.Height);
            var bmp = new Bitmap(size.Width, size.Height);

            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(rect.X, rect.Y, 0, 0, size, CopyPixelOperation.SourceCopy);
            }

            return bmp;
        }

    }
}
