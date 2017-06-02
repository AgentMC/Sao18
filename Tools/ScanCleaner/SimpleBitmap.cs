using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScanCleaner
{
    class SimpleBitmap
    {
        public SimpleBitmap(int width, int height)
        {
            BaseBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
        }

        public SimpleBitmap(BitmapSource source)
        {
            BaseBitmap = new WriteableBitmap(source);
        }
        
        public WriteableBitmap BaseBitmap { get; private set; }

        public unsafe uint GetPixel(int x, int y)
        {
            var ptr = GetBackBufferPtr(x, y);
            switch (BaseBitmap.Format.BitsPerPixel)
            {
                case 8:
                    return *ptr;
                case 16:
                    return *(ushort*) ptr;
                case 32:
                    return *(uint*)ptr;
                default:
                    throw new DataMisalignedException("Misaligned Bits per pixel. 8, 16 and 32 supported.");
            }
        }

        public void SetPixel(int x, int y, uint c)
        {
            BaseBitmap.Lock();
            SetPixelUnlocked(x, y, c);
            BaseBitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1)); 
            BaseBitmap.Unlock();
        }

        private unsafe void SetPixelUnlocked(int x, int y, uint c)
        {
            var ptr = GetBackBufferPtr(x, y);
            switch (BaseBitmap.Format.BitsPerPixel)
            {
                case 8:
                    *ptr = (byte) c;
                    break;
                case 16:
                    *(ushort*) ptr = (ushort) c;
                    break;
                case 32:
                    *(uint*) ptr = c;
                    break;
                default:
                    throw new DataMisalignedException("Misaligned Bits per pixel. 8, 16 and 32 supported.");
            }
        }

        private unsafe byte* GetBackBufferPtr(int x, int y)
        {
            const int bitsPerByte = 8;
            var byteOffset = BaseBitmap.BackBufferStride * y + x * (BaseBitmap.Format.BitsPerPixel / bitsPerByte);
            return ((byte*) BaseBitmap.BackBuffer.ToPointer()) + byteOffset;
        }

        public Color GetColor(int x, int y)
        {
            var p = BitConverter.GetBytes(GetPixel(x, y));
            return Color.FromArgb(p[3], p[2], p[1], p[0]);
        }

        public void SetColor (int x, int y, Color c, bool perfromLock)
        {
            var color = (uint) c.A << 24 | ((uint) c.R << 16) | ((uint) c.G << 8) | c.B;
            if (perfromLock)
                SetPixel(x, y, color);
            else
                SetPixelUnlocked(x, y, color);
        }
    }
}
