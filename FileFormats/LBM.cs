using System;
using Godot;
using Hexa.NET.SDL3;
using Hexa.NET.SDL3.Image;


namespace mtw3dviewer.FileFormats
{
    public unsafe class LBM
    {
        /// <summary>
        /// Loads an LBM file as Godot image
        /// </summary>
        public static Image LoadImage(string path)
        {
            SDLSurface* surface = SDLImage.Load(path);

            if (surface == null)
            {
                throw new FormatException($"{path} load failed");
            }
            var image = Image.CreateEmpty(surface->W, surface->H, false, Image.Format.Rgba8);
            for (int x = 0; x < surface->W; x++)
            {
                for (int y = 0; y < surface->H; y++)
                {
                    byte r = 0, g = 0, b = 0, a = 0;
                    SDL.ReadSurfacePixel(surface, x, y, ref r, ref g, ref b, ref a);
                    var intColor = (r << 24) | (g << 16) | (b << 8) | (a);
                    image.SetPixel(x, y, new Color((uint)intColor));
                }
            }
            SDL.Free(surface);
            return image;
        }
    
    }
}