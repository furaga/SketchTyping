using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FLib
{
    /// <summary>
    /// 使い方：
    ///     LazyBrush lb = new LazyBrush(edgeBmp, preBrushedBmp);
    ///     lb.Execute();
    ///     pictureBox1.Image = lb.brushedImage;
    /// edgeBmpは輪郭線が黒いモノクロ画像
    /// preBrushBmpはユーザが指定したストロークが塗られたビットマップ
    /// </summary>
    public class LazyBrush : IDisposable
    {
        [DllImport("LBGraphCut.dll")]
        static extern IntPtr CreateGraph(int w, int h);
        [DllImport("LBGraphCut.dll")]
        static extern void SetEdgeWeights(IntPtr grid, int w, int h, double[] Vv, double[] Vh);
        [DllImport("LBGraphCut.dll")]
        static extern void Execute(IntPtr grid, int labelID, int w, int h, double[] D, int[] B, int[] L);
        [DllImport("LBGraphCut.dll")]
        static extern void DeleteGraph(IntPtr grid);

        // 入出力
        public Bitmap edgeImage;
        public Bitmap preBrushedImage;
        public Bitmap brushedImage;

        // パラメータ
        double K;
        double hardness; // softなら0.05, hardなら1
        double gamma;
        double colorScale = 1.0 / 255;

        // 途中計算用
        double[] edgeHorizon;  // 横方向のエッジの重み。論文中のV_pqの一部
        double[] edgeVertical;  // 縦方向のエッジの重み。論文中のV_pqの一部
        double[] denseMap;  // 論文中のD
        int[] strokeMap;  // 論文中のD
        int[] labelMap;     // ラベル付。論文中のL

        public LazyBrush(Bitmap edgeImage, Bitmap preBrushedImage, double preBrushHardness = 1, double gamma = 8)
        {
            int w = edgeImage.Width;
            int h = edgeImage.Height;
            int n = w * h;
            this.edgeImage = new Bitmap(edgeImage);
            this.preBrushedImage = new Bitmap(preBrushedImage);
            this.brushedImage = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            this.edgeHorizon = new double[n];
            this.edgeVertical = new double[n];
            this.denseMap = new double[n];
            this.strokeMap = new int[n];
            this.labelMap = new int[n];
            this.K = 2 * (w + h);
            this.hardness = preBrushHardness;
            this.gamma = gamma;
        }

        public void Dispose()
        {
            if (edgeImage != null) edgeImage.Dispose();
            if (preBrushedImage != null) preBrushedImage.Dispose();
            if (brushedImage != null) brushedImage.Dispose();
        }

        unsafe public void Execute()
        {
            HashSet<int> colors = CreateLabelColorMapping();
            CalcEdgeWeights();
            for (int i = 0; i < labelMap.Length; i++) labelMap[i] = -1;

            foreach (int color in colors.Take(colors.Count - 1))
            {
                UpdateEdgeWeights();
                IntPtr grid = CreateGraph(edgeImage.Width, edgeImage.Height);
                SetEdgeWeights(grid, edgeImage.Width, edgeImage.Height, edgeHorizon, edgeVertical);
                Execute(grid, color, edgeImage.Width, edgeImage.Height, denseMap, strokeMap, labelMap);
                DeleteGraph(grid);
            }

            using (BitmapIterator brushIter = new BitmapIterator(brushedImage, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb))
            //            using (BitmapIterator preBrushIter = new BitmapIterator(preBrushedImage, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb))
            using (BitmapIterator intIter = new BitmapIterator(edgeImage, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed))
            {
                int* data = (int*)brushIter.PixelData;
                //                int* predata = (int*)preBrushIter.PixelData;
                byte* intData = (byte*)intIter.PixelData;
                for (int y = 0; y < brushedImage.Height; y++)
                {
                    for (int x = 0; x < brushedImage.Width; x++)
                    {
                        int idx = x + y * brushedImage.Width;
                        int intIdx = x + y * intIter.Stride;
                        data[idx] =
                             labelMap[idx];
//                            intData[intIdx] >= 10 ? intData[intIdx] : labelMap[idx];
                    }
                }
            }
        }

        unsafe HashSet<int> CreateLabelColorMapping()
        {
            HashSet<int> colors = new HashSet<int>();

            for (int i = 0; i < denseMap.Length; i++)
            {
                denseMap[i] = 0;
                strokeMap[i] = -1;
            }

            using (BitmapIterator iter = new BitmapIterator(preBrushedImage, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb))
            {
                int* data = (int*)iter.PixelData;
                for (int y = 0; y < preBrushedImage.Height; y++)
                {
                    for (int x = 0; x < preBrushedImage.Width; x++)
                    {
                        int idx = x + y * preBrushedImage.Width;
                        int color = data[idx];
                        if (color != 0)
                        {
                            denseMap[idx] = hardness * K;
                            strokeMap[idx] = color;
                            colors.Add(color);
                        }
                    }
                }
            }

            return colors;
        }

        unsafe void CalcEdgeWeights()
        {
            using (BitmapIterator iter = new BitmapIterator(edgeImage, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed))
            {
                byte* data = (byte*)iter.PixelData;
                for (int y = 0; y < edgeImage.Height; y++)
                    for (int x = 0; x < edgeImage.Width - 1; x++)
                        edgeHorizon[x + y * edgeImage.Width] =
                            1 + K * Math.Pow(data[x + y * iter.Stride] * colorScale, gamma);
                for (int x = 0; x < edgeImage.Width; x++)
                    for (int y = 0; y < edgeImage.Height - 1; y++)
                        edgeVertical[x + y * edgeImage.Width] =
                            1 + K * Math.Pow(data[x + y * iter.Stride] * colorScale, gamma);
            }
        }

        void UpdateEdgeWeights()
        {
            // TODO: C++側で現在のステップで彩色されたノードを列挙しておいて高速化

            for (int y = 0; y < edgeImage.Height; y++)
            {
                for (int x = 0; x < edgeImage.Width; x++)
                {
                    int i = x + y * edgeImage.Width;
                    if (labelMap[i] >= 0)
                    {
                        // 彩色済み。なかったコトにする。つまり重みをにする
                        edgeHorizon[x + y * edgeImage.Width] = 0;
                        edgeVertical[x + y * edgeImage.Width] = 0;
                        if (x >= 1) edgeHorizon[x - 1 + y * edgeImage.Width] = 0;
                        if (y >= 1) edgeVertical[x + (y - 1) * edgeImage.Width] = 0;
                    }
                }
            }
        }
    }
}