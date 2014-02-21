using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using OpenCvSharp;

namespace FLib
{
    public class TrappedBallSegmentation : IDisposable
    {
        bool randColorMode = true;
        // 入出力

        const int edgeThreshold = 10;
        public Bitmap edgeImage;
        public Bitmap brushedImage;
        public Bitmap orgImage;
        float sqColorDistThreshold = 100;


        public TrappedBallSegmentation(Bitmap orgImage, Bitmap edgeImage)
        {
            this.orgImage = new Bitmap(orgImage);
            this.edgeImage = new Bitmap(edgeImage);
            this.brushedImage = new Bitmap(orgImage.Width, orgImage.Height, PixelFormat.Format32bppArgb);
        }

        public void Dispose()
        {
            if (edgeImage != null) orgImage.Dispose();
            if (edgeImage != null) edgeImage.Dispose();
            if (brushedImage != null) brushedImage.Dispose();
        }

        // 各ピクセルを左上端としてエッジ画像に充填できる正方形の大きさ
        // DPで求める
        unsafe int[] CalcBallSize(Bitmap mono, out int maxSize, out int minSize)
        {
            // 各ピクセルを左上端としてエッジ画像に充填できる正方形の大きさ
            // DPで求める
            maxSize = 0;
            minSize = int.MaxValue;
            int[] ballSize = new int[mono.Width * mono.Height];
            using (BitmapIterator iter = new BitmapIterator(mono, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed))
            {
                byte* data = (byte*)iter.PixelData;
                // 右辺
                for (int y = mono.Height - 1; y >= 0; y--)
                {
                    int pixelIdx = mono.Width - 1 + y * iter.Stride;
                    byte val = data[pixelIdx];
                    if (val <= edgeThreshold)
                    {
                        int ballIdx = mono.Width - 1 + y * mono.Width;
                        ballSize[ballIdx] = 1;
                    }
                }
                // 下辺
                for (int x = mono.Width - 1; x >= 0; x--)
                {
                    int pixelIdx = x + (mono.Height - 1) * iter.Stride;
                    byte val = data[pixelIdx];
                    if (val <= edgeThreshold)
                    {
                        int ballIdx = x + (mono.Height - 1) * mono.Width;
                        ballSize[ballIdx] = 1;
                    }
                }
                // それ以外
                for (int y = mono.Height - 2; y >= 0; y--)
                {
                    for (int x = mono.Width - 2; x >= 0; x--)
                    {
                        int idx = x + y * iter.Stride;
                        byte val = data[idx];
                        if (val <= 10)
                        {
                            int ballIdx = x + y * mono.Width;
                            int ballIdxR = (x + 1) + y * mono.Width;
                            int ballIdxB = x + (y + 1) * mono.Width;
                            int ballIdxD = (x + 1) + (y + 1) * mono.Width;
                            ballSize[ballIdx] = 1 + Math.Min(ballSize[ballIdxR], Math.Min(ballSize[ballIdxB], ballSize[ballIdxD]));
                            maxSize = Math.Max(maxSize, ballSize[ballIdx]);
                            minSize = Math.Min(minSize, ballSize[ballIdx]);
                        }
                    }
                }
            }

            return ballSize;
        }

        unsafe void FlashArrayToBitmap(int[] array, int w, int h, Bitmap bmp)
        {
            using (var iter = new BitmapIterator(bmp, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb))
            {
                byte* data = (byte*)iter.PixelData;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int idx = 4 * x + y * iter.Stride;
                        int arIdx = x + y * w;
                        data[idx + 0] = (byte)array[arIdx];
                        data[idx + 1] = (byte)array[arIdx];
                        data[idx + 2] = (byte)array[arIdx];
                        data[idx + 3] = (byte)255;
                    }
                }
            }
        }

        unsafe List<IplImage> MoveBall(HashSet<int> orgUncoloredPixels, int[] labelMap, int radius, BitmapIterator edgeIter, BitmapIterator orgIter, int[] ballSizeList)
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            System.Diagnostics.Stopwatch sw2 = new Stopwatch();

            List<IplImage> segments = new List<IplImage>();
            byte* edgeData = (byte*)edgeIter.PixelData;
            int ballSize = 1 + 2 * radius;
            HashSet<int> uncoloredPixels = new HashSet<int>(orgUncoloredPixels);
            Queue<int> uncoloredPixelList = new Queue<int>(orgUncoloredPixels);

            while (uncoloredPixels.Count >= 1)
            {
                int idx = uncoloredPixelList.Dequeue();
                if (!uncoloredPixels.Contains(idx)) continue;
                uncoloredPixels.Remove(idx);

                int x, y;
                y = Math.DivRem(idx, edgeImage.Width, out x);

                // ボールが入るか
                if (ballSizeList[idx] < ballSize) continue;

                // 彩色済みなら塗らない
                bool colored = false;
                for (int yy = y; yy < y + ballSize; yy++)
                    for (int xx = x; xx < x + ballSize; xx++)
                        if (!(y == yy && x == xx) && !uncoloredPixels.Contains(xx + yy * edgeImage.Width))
                        {
                            colored = true;
                            goto COLORED_DECIDED;
                        }

            COLORED_DECIDED:
            
                if (colored) continue;

                // 新しいセグメント
                IplImage segmentImage = new IplImage(edgeImage.Width, edgeImage.Height, BitDepth.U8, 4);
                Cv.Set(segmentImage, new CvScalar(0, 0, 0, 0));

                Random rand = new Random();
                Color c = Color.FromArgb(rand.Next(255) + 1, rand.Next(255) + 1, rand.Next(255) + 1);

                Cv.Rectangle(segmentImage, x + 1, y + 1, x + ballSize - 1, y + ballSize - 1, Cv.RGB(c.R, c.G, c.B));

                // 新しい色でFlood fill
                HashSet<int> initPoints = new HashSet<int>();
                for (int yy = y; yy < y + ballSize - 1; yy++)
                {
                    initPoints.Add(x + yy * edgeImage.Width);
                    initPoints.Add((x + ballSize - 1) + yy * edgeImage.Width);
                }
                for (int xx = x; xx < x + ballSize - 1; xx++)
                {
                    initPoints.Add(xx + y * edgeImage.Width);
                    initPoints.Add(xx + (y + ballSize - 1) * edgeImage.Width);
                }

                CvRect roi = FloodFill(initPoints, segmentImage, c, uncoloredPixels, orgIter, sw2);



                sw2.Start();

                Cv.SetImageROI(segmentImage, roi);
                {
                    IplConvKernel kernel = new IplConvKernel(2 * radius + 1, 2 * radius + 1, radius, radius, ElementShape.Ellipse);
                    Cv.Erode(segmentImage, segmentImage, kernel, 1);
                    Cv.Dilate(segmentImage, segmentImage, kernel, 1);
                }
                Cv.ResetImageROI(segmentImage);


                for (int yy = roi.Y; yy < roi.Bottom; yy++)
                    for (int xx = roi.X; xx < roi.Right; xx++)
                        if (segmentImage.ImageDataPtr[4 * xx + yy * segmentImage.WidthStep + 3] != 0)
                        {
                            int idx2 = xx + yy * edgeImage.Width;
                            labelMap[idx2] = segmentCnt;
                            uncoloredPixels.Remove(idx2);
                            orgUncoloredPixels.Remove(idx2);
                        }
                segmentCnt++;

                segments.Add(segmentImage);

                sw2.Stop();
            }
            //if (segments.Count >= 1) segments.First().Save(radius + ".png");

//            if (sw.ElapsedMilliseconds >= 1000)
  //          {
    //            Console.WriteLine("[radius=" + radius + "]");
      //          Console.WriteLine("total= " + sw.ElapsedMilliseconds + " ms");
        //        Console.WriteLine("total= " + sw2.ElapsedMilliseconds + " ms");
          //  }
            return segments;
        }

        int segmentCnt = 1;

        unsafe public void Execute()
        {
            const int deltaRadius = 3;
            segmentCnt = 1;

            // 充填できるボールサイズを事前計算
            int maxBallSize, minBallSize;
            int[] ballSizeList = CalcBallSize(edgeImage, out maxBallSize, out minBallSize);
            int maxRadius = ((maxBallSize / 2) / deltaRadius - 1) * deltaRadius;
            int minRadius = ((minBallSize / 2) / deltaRadius + 1) * deltaRadius;
//            FlashArrayToBitmap(ballSizeList, edgeImage.Width, edgeImage.Height, brushedImage);

            // 出力
            Dictionary<IplImage, int> segmentList = new Dictionary<IplImage, int>();
            int[] labelMap = new int[edgeImage.Width * edgeImage.Height];

            // 塗るべきピクセルを追加
            HashSet<int> uncoloredPixels = new HashSet<int>();
            using (BitmapIterator edgeIter = new BitmapIterator(edgeImage, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed))
            {
                byte* edgeData = (byte*)edgeIter.PixelData;
                int i = 0;
                for (int y = 0; y < edgeImage.Height; y++)
                {
                    for (int x = 0; x < edgeImage.Width; x++)
                    {
                        if (edgeData[x + y * edgeIter.Stride] <= edgeThreshold)
                        {
                            uncoloredPixels.Add(i);
                        }
                        else
                        {
                            // エッジは取り除く
                            labelMap[i] = -1;
                        }
                        i++;
                    }
                }
            }

            // エッジの間に大雑把に色を塗っていく
            using (BitmapIterator edgeIter = new BitmapIterator(edgeImage, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed))
            using (BitmapIterator orgIter = new BitmapIterator(orgImage, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb))
            {
                byte* edgeData = (byte*)edgeIter.PixelData;
                byte* orgData = (byte*)orgIter.PixelData;

                // ボールを徐々に小さくしながらTrappedBallSegmentation
                for (int radius = maxRadius; radius >= minRadius; radius -= deltaRadius)
                {
                    List<IplImage> segments = MoveBall(uncoloredPixels, labelMap, radius, edgeIter, orgIter, ballSizeList);
                    for (int i = 0; i < segments.Count; i++)
                    {
                        segmentList[segments[i]] = radius;
                    }
                }

                Console.WriteLine("move ball: done");

                int cnt = labelMap.Count(val => val == 0);
                Debug.Assert(cnt == uncoloredPixels.Count);


                RegionGrowing(uncoloredPixels, labelMap, orgIter);

                Dictionary<int, Color> colors = new Dictionary<int, Color>();

                colors[-1] = Color.Black;
                colors[0] = Color.Black;

                List<byte>[] r = new List<byte>[segmentList.Count];
                List<byte>[] g = new List<byte>[segmentList.Count];
                List<byte>[] b = new List<byte>[segmentList.Count];
                int[] c = new int[segmentList.Count];

                for (int y = 0; y < edgeImage.Height; y++)
                {
                    for (int x = 0; x < edgeImage.Width; x++)
                    {
                        int label = labelMap[x + y * edgeImage.Width];
                        if (1 <= label && label < segmentList.Count)
                        {
                            if (r[label] == null)
                            {
                                r[label] = new List<byte>();
                                g[label] = new List<byte>();
                                b[label] = new List<byte>();
                            }
                            r[label].Add(orgData[4 * x + y * orgIter.Stride + 0]);
                            g[label].Add(orgData[4 * x + y * orgIter.Stride + 1]);
                            b[label].Add(orgData[4 * x + y * orgIter.Stride + 2]);
                            c[label]++;
                        }
                    }
                }

                Random rand = new Random();

                for (int i = 1; i < segmentList.Count; i++)
                {
                    if (c[i] >= 1)
                    {
                        r[i].Sort();
                        g[i].Sort();
                        b[i].Sort();
                        if (randColorMode)
                        {
                            colors[i] = Color.FromArgb(
                                rand.Next(155) + 100,
                                rand.Next(155) + 100,
                                rand.Next(155) + 100);
                        }
                        else
                        {
                            colors[i] = Color.FromArgb(
                                r[i][r[i].Count / 2],
                                g[i][g[i].Count / 2],
                                b[i][b[i].Count / 2]);
                        }
                    }
                }

                using (BitmapIterator brushedIter = new BitmapIterator(brushedImage, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb))
                {
                    byte* brushedData = (byte*)brushedIter.PixelData;
                    for (int y = 0; y < edgeImage.Height; y++)
                    {
                        for (int x = 0; x < edgeImage.Width; x++)
                        {
                            int label = labelMap[x + y * edgeImage.Width];
                            if (!colors.ContainsKey(label))
                            {
                                colors[label] = Color.FromArgb(
                                    orgData[4 * x + y * orgIter.Stride + 0],
                                    orgData[4 * x + y * orgIter.Stride + 1],
                                    orgData[4 * x + y * orgIter.Stride + 2]);
                            }
                            brushedData[4 * x + y * brushedIter.Stride + 0] = colors[label].R;
                            brushedData[4 * x + y * brushedIter.Stride + 1] = colors[label].G;
                            brushedData[4 * x + y * brushedIter.Stride + 2] = colors[label].B;
                            brushedData[4 * x + y * brushedIter.Stride + 3] = 255;
                        }
                    }
                }
            }
        }


        unsafe void RegionGrowing(HashSet<int> uncoloredPixels, int[] labelMap, BitmapIterator orgIter)
        {
            float threshold = sqColorDistThreshold;
            PriorityQueue<ContourPixel> contour = new PriorityQueue<ContourPixel>();
            while (uncoloredPixels.Count >= 1 && threshold <= float.MaxValue / 4)
            {
                foreach (int idx in uncoloredPixels)
                {
                    int x, y;
                    y = Math.DivRem(idx, edgeImage.Width, out x);
                    if (0 <= x - 1 && !uncoloredPixels.Contains(idx - 1) && labelMap[idx - 1] >= 1)
                        contour.Push(new ContourPixel(x - 1, y, 0));
                    if (0 <= y - 1 && !uncoloredPixels.Contains(idx - edgeImage.Width) && labelMap[idx - edgeImage.Width] >= 1)
                        contour.Push(new ContourPixel(x, y - 1, 0));
                    if (edgeImage.Width > x + 1 && !uncoloredPixels.Contains(idx + 1) && labelMap[idx + 1] >= 1)
                        contour.Push(new ContourPixel(x + 1, y, 0));
                    if (edgeImage.Height > y + 1 && !uncoloredPixels.Contains(idx + edgeImage.Width) && labelMap[idx + edgeImage.Width] >= 1)
                        contour.Push(new ContourPixel(x, y + 1, 0));
                }

                Console.WriteLine("init contour: done");

                while (contour.Count >= 1)
                {
                    ContourPixel cpt = contour.Top;
                    contour.Pop();
                    if (cpt.error >= sqColorDistThreshold)
                    {
                        break;
                    }
                    AddToContour(uncoloredPixels, contour, labelMap, cpt.x - 1, cpt.y, cpt, orgIter, threshold);
                    AddToContour(uncoloredPixels, contour, labelMap, cpt.x + 1, cpt.y, cpt, orgIter, threshold);
                    AddToContour(uncoloredPixels, contour, labelMap, cpt.x, cpt.y - 1, cpt, orgIter, threshold);
                    AddToContour(uncoloredPixels, contour, labelMap, cpt.x, cpt.y + 1, cpt, orgIter, threshold);
                }
                Console.WriteLine("region growing: done (" + uncoloredPixels.Count + ", " + threshold + ")");
                threshold *= 2;
            }

            List<byte>[] r = new List<byte>[segmentCnt];
            List<byte>[] g = new List<byte>[segmentCnt];
            List<byte>[] b = new List<byte>[segmentCnt];
            int[] c = new int[segmentCnt];
            byte* orgData = (byte*)orgIter.PixelData;

            for (int y = 0; y < edgeImage.Height; y++)
            {
                for (int x = 0; x < edgeImage.Width; x++)
                {
                    int label = labelMap[x + y * edgeImage.Width];
                    if (1 <= label && label < segmentCnt)
                    {
                        if (r[label] == null)
                        {
                            r[label] = new List<byte>();
                            g[label] = new List<byte>();
                            b[label] = new List<byte>();
                        }
                        r[label].Add(orgData[4 * x + y * orgIter.Stride + 0]);
                        g[label].Add(orgData[4 * x + y * orgIter.Stride + 1]);
                        b[label].Add(orgData[4 * x + y * orgIter.Stride + 2]);
                        c[label]++;
                    }
                }
            }

            for (int i = 1; i < r.Length; i++)
            {
                if (c[i] >= 1)
                {
                    r[i].Sort();
                    g[i].Sort();
                    b[i].Sort();
                }
            }

            // 隣接している似た色の領域をまとめる
            Dictionary<int, int> segmentCorrespondence = new Dictionary<int, int>();

            int id = 0;
            for (int y = 0; y < edgeImage.Height; y++)
            {
                for (int x = 0; x < edgeImage.Width; x++)
                {
                    if (x + 1 < edgeImage.Width && SameRegion(labelMap[id], labelMap[id + 1], r, g, b))
                    {
                        segmentCorrespondence[Math.Max(labelMap[id], labelMap[id + 1])] = Math.Min(labelMap[id], labelMap[id + 1]);
                    }
                    if (y + 1 < edgeImage.Height && SameRegion(labelMap[id], labelMap[id + edgeImage.Width], r, g, b))
                    {
                        segmentCorrespondence[Math.Max(labelMap[id], labelMap[id + edgeImage.Width])] = Math.Min(labelMap[id], labelMap[id + edgeImage.Width]);
                    }
                    id++;
                }
            }

            for (int i = 0; i < labelMap.Length; i++)
                while (segmentCorrespondence.ContainsKey(labelMap[i]))
                    labelMap[i] = segmentCorrespondence[labelMap[i]];
        }

        void AddToContour(HashSet<int> uncoloredPixels, PriorityQueue<ContourPixel> contour, int[] labelMap, int x, int y, ContourPixel src, BitmapIterator orgIter, float threshold)
        {
            int idx = x + y * edgeImage.Width;
            if (uncoloredPixels.Contains(idx) && SqColorDist(orgIter, src.x, src.y, x, y) <= threshold)
            {
                contour.Push(new ContourPixel(x, y, 0));
                labelMap[idx] = labelMap[src.x + src.y * edgeImage.Width];
                uncoloredPixels.Remove(idx);
            }
        }

        bool SameRegion(int label1, int label2, List<byte>[] r, List<byte>[] g, List<byte>[] b)
        {
            if (label1 == label2) return false;
            if (label1 <= 0 || label2 <= 0) return false;
            if (label1 >= segmentCnt || label2 >= segmentCnt) return false;
            float r1 = r[label1][r[label1].Count / 2];
            float g1 = g[label1][g[label1].Count / 2];
            float b1 = b[label1][b[label1].Count / 2];
            float r2 = r[label2][r[label2].Count / 2];
            float g2 = g[label2][g[label2].Count / 2];
            float b2 = b[label2][b[label2].Count / 2];
            float dr = r1 - r2;
            float dg = g1 - g2;
            float db = b1 - b2;
            float sqDist = dr *dr + dg * dg + db*db;
            return sqDist <= sqColorDistThreshold;
        }

        public class ContourPixel : IComparable<ContourPixel>
        {
            public float error = 0;
            public int x, y;
            public ContourPixel(int x, int y, float error)
            {
                this.error = error;
                this.x = x;
                this.y = y;
            }
            public int CompareTo(ContourPixel obj)
            {
                if (error == obj.error) return 0;
                return error < obj.error ? -1 : 1;
            }
        }
   
        unsafe float SqColorDist(BitmapIterator iter, int x0, int y0, int x1, int y1)
        {
            byte* data = (byte*)iter.PixelData;
            int offset0 = 4* x0 + y0 * iter.Stride;
            int offset1 = 4 * x1 + y1 * iter.Stride;
            byte r0 = data[offset0 + 2];
            byte g0 = data[offset0 + 1];
            byte b0 = data[offset0 + 0];
            byte r1 = data[offset1 + 2];
            byte g1 = data[offset1 + 1];
            byte b1 = data[offset1 + 0];
            float dr = r1 - r0;
            float dg = g1 - g0;
            float db = b1 - b0;
            float sqDist = dr * dr + dg * dg + db * db;
            return sqDist;
        }

        unsafe bool CanFill(int idx, int idxSeg, int ptX, int ptY, int srcX, int srcY, HashSet<int> filled, HashSet<int> uncoloredPixels, IplImage segmentImage, BitmapIterator orgIter, Stopwatch sw2 = null)
        {
//            sw2.Start();
            if (!uncoloredPixels.Contains(idx))
            {
  //              sw2.Stop();
                return false;
            }
            if (filled.Contains(idx))
            {
    //            sw2.Stop();
                return false;
            }
            if (ptX < 0 || edgeImage.Width <= ptX)
            {
      //          sw2.Stop();
                return false;
            }
            if (ptY < 0 || edgeImage.Height <= ptY)
            {
        //        sw2.Stop();
                return false;
            }
            if (segmentImage.ImageDataPtr[idxSeg] != 0) 
            {
          //      sw2.Stop();
                return false;
            }
            if (SqColorDist(orgIter, srcX, srcY, ptX, ptY) >= sqColorDistThreshold)
            {
            //    sw2.Stop();
                return false;
            }
//            sw2.Stop();
            return true;
        }

        unsafe public CvRect FloodFill(HashSet<int> initPoints, IplImage segmentImage, Color c, HashSet<int> uncoloredPixels, BitmapIterator orgIter, Stopwatch sw2 = null)
        {
            int roiMinX = int.MaxValue;
            int roiMinY = int.MaxValue;
            int roiMaxX = int.MinValue;
            int roiMaxY = int.MinValue; 
            List<int> curs = initPoints.ToList();
            List<int> nexts = new List<int>();
            HashSet<int> filled = new HashSet<int>(initPoints);
            byte* partData = segmentImage.ImageDataPtr;
            while (curs.Count > 0)
            {
                for (int i = 0; i < curs.Count; i++)
                {
                    int src = curs[i];
                    int x, y;
                    y = Math.DivRem(src, edgeImage.Width, out x);
                    if (x < roiMinX) roiMinX = x;
                    if (x > roiMaxX) roiMaxX = x;
                    if (y < roiMinY) roiMinY = y;
                    if (y > roiMaxY) roiMaxY = y;
                    int offset = 4 * x + y * segmentImage.WidthStep;
                    if (CanFill(src - 1, offset - 4 + 3, x - 1, y, x, y, filled, uncoloredPixels, segmentImage, orgIter, sw2))
                    {
                        nexts.Add(src - 1);
                        filled.Add(src - 1);
                    }
                    if (CanFill(src + 1, offset + 4 + 3, x + 1, y, x, y, filled, uncoloredPixels, segmentImage, orgIter, sw2))
                    {
                        nexts.Add(src + 1);
                        filled.Add(src + 1);
                    }
                    if (CanFill(src - edgeImage.Width, offset - segmentImage.WidthStep + 3, x, y - 1, x, y, filled, uncoloredPixels, segmentImage, orgIter, sw2))
                    {
                        nexts.Add(src - segmentImage.Width);
                        filled.Add(src - segmentImage.Width);
                    }
                    if (CanFill(src + edgeImage.Width, offset + segmentImage.WidthStep + 3, x, y + 1, x, y, filled, uncoloredPixels, segmentImage, orgIter, sw2))
                    {
                        nexts.Add(src + segmentImage.Width);
                        filled.Add(src + segmentImage.Width);
                    }
                    partData[offset + 0] = c.B;
                    partData[offset + 1] = c.G;
                    partData[offset + 2] = c.R;
                    partData[offset + 3] = 255;
                }
                curs.Clear();
                FMath.Swap(ref curs, ref nexts);
            }

            return new CvRect(
                Math.Max(0, roiMinX - 1),
                Math.Max(0, roiMinY - 1),
                Math.Min(edgeImage.Width, roiMaxX - roiMinX + 2),
                Math.Min(edgeImage.Height, roiMaxY - roiMinY + 2));
        }

    }
}
