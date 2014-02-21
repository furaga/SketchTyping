using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing.Imaging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FLib
{
    public static class XNATexture
    {
        unsafe public static void Load(GraphicsDevice GraphicsDevice, System.Drawing.Bitmap bmp, out Texture2D texture, out Color[] texData, bool reverse = false)
        {
            using (BitmapIterator iter = new BitmapIterator(bmp, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb))
            {
                texData = new Color[bmp.Width * bmp.Height];
                byte* data = (byte*)iter.PixelData;
                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        int idx = !reverse ?
                            4 * x + y * iter.Stride :
                            4 * (bmp.Width - 1 - x) + y * iter.Stride;
                        if (data[idx + 3] == 0)
                        {
                            texData[x + y * bmp.Width] = Color.Transparent;
                        }
                        else
                        {
                            texData[x + y * bmp.Width].R = data[idx + 2];
                            texData[x + y * bmp.Width].G = data[idx + 1];
                            texData[x + y * bmp.Width].B = data[idx + 0];
                            texData[x + y * bmp.Width].A = data[idx + 3];
                        }
                    }
                }
                texture = new Texture2D(GraphicsDevice, bmp.Width, bmp.Height);
                texture.SetData(texData);
            }
        }

        unsafe public static void Load(GraphicsDevice GraphicsDevice, string imgPath, out Texture2D texture, out Color[] texData, bool reverse = false)
        {
            if (!File.Exists(imgPath))
            {
                throw new FileNotFoundException("File not found", imgPath);
            }
            else
            {
                using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(imgPath))
                {
                    Load(GraphicsDevice, bmp, out texture, out texData, reverse);
                }
            }
        }
    }
}
