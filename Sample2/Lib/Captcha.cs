using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Web;

using Microsoft.Extensions.Caching.Distributed;

namespace Sample2.Lib
{
    // http://www.codeproject.com/Articles/169371/Captcha-Image-using-C-in-ASP-NET
    // http://www.codeproject.com/Articles/99148/Simple-CAPTCHA-Create-your-own-in-C
    // https://stackoverflow.com/questions/44150247/how-to-get-the-font-name-from-a-ttf-file-in-net-core
    public class Captcha : IDisposable
    {
        //Private Variable
        private const string _cachePrefixKey = "_captcha_";
        private string _text;
        private int _width;
        private int _height;
        private Bitmap _image;
        private string _fontFileName;
        private readonly IDistributedCache _distributedCache;
        private string _cacheKey;

        //Public Properties
        public string Text
        {
            get { return this._text; }
        }

        public Bitmap Image
        {
            get { return this._image; }
        }

        public int Width
        {
            get { return this._width; }
        }

        public int Height
        {
            get { return this._height; }
        }

        //Constructor 
        public Captcha(IDistributedCache distributedCache, string cacheKey)
        {
            _distributedCache = distributedCache;
            _cacheKey = cacheKey;
        }

        public Captcha(IDistributedCache distributedCache, string cacheKey, int width, int height)
        {
            _distributedCache = distributedCache;
            _cacheKey = cacheKey;
            this._text = GetRandomCode();
            this.SetDimensions(width, height);
            this.GenerateImage();
        }

        public Captcha(IDistributedCache distributedCache, string cacheKey, string s, int width, int height, string fontFileName)
        {
            _distributedCache = distributedCache;
            _cacheKey = cacheKey;
            this._fontFileName = fontFileName;
            this._text = !string.IsNullOrEmpty(s) ? s : GetRandomCode();
            this.SetDimensions(width, height);
            this.GenerateImage();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._image != null)
                {
                    this._image.Dispose();
                    this._image = null;
                }
            }
        }

        private void SetDimensions(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", width,
                    "Argument out of range, must be greater than zero.");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height", height,
                    "Argument out of range, must be greater than zero.");
            this._width = width;
            this._height = height;
        }

        private void GenerateImage()
        {
            _distributedCache.SetString($"{_cachePrefixKey}{_cacheKey}", _text, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                SlidingExpiration = TimeSpan.FromMinutes(15),
            });

            Bitmap bitmap = new Bitmap(this._width, this._height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, this._width, this._height);
            HatchBrush hatchBrush = new HatchBrush(HatchStyle.SmallConfetti, Color.LightGray, Color.White);
            g.FillRectangle(hatchBrush, rect);

            SizeF size;
            float fontSize = rect.Height + 1;
            Font font;

            do
            {
                fontSize--;

                if (!string.IsNullOrWhiteSpace(_fontFileName) && File.Exists(_fontFileName))
                {
                    FontFamily fontFamily = LoadFontFamily(_fontFileName);
                    //font = new Font(fontFamily, fontSize, FontStyle.Strikeout);
                    font = new Font(fontFamily, fontSize, FontStyle.Bold);
                }
                else
                    font = new Font("Arial", fontSize, FontStyle.Italic);

                //font = new Font(FontFamily.GenericSansSerif, fontSize, FontStyle.Bold);
                size = g.MeasureString(this._text, font);
            } while (size.Width > rect.Width);

            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            GraphicsPath path = new GraphicsPath();
            //path.AddString(this.text, font.FontFamily, (int)font.Style, font.Size, rect, format);
            path.AddString(this._text, font.FontFamily, (int)font.Style, 75, rect, format);
            float v = 4F;
            Random random = new Random();
            PointF[] points =
            {
                new PointF(random.Next(rect.Width) / v, random.Next(rect.Height) / v),
                new PointF(rect.Width - random.Next(rect.Width) / v, random.Next(rect.Height) / v),
                new PointF(random.Next(rect.Width) / v, rect.Height - random.Next(rect.Height) / v),
                new PointF(rect.Width - random.Next(rect.Width) / v, rect.Height - random.Next(rect.Height) / v)
            };
            Matrix matrix = new Matrix();
            matrix.Translate(0F, 0F);
            path.Warp(points, rect, matrix, WarpMode.Perspective, 0F);
            hatchBrush = new HatchBrush(HatchStyle.Percent70, Color.Black, Color.Black);
            g.FillPath(hatchBrush, path);
            int m = Math.Max(rect.Width, rect.Height);

            var _hatchBrush = new HatchBrush(HatchStyle.Percent70, Color.White, Color.White);
            for (int i = 0; i < (int)(rect.Width * rect.Height / 30F); i++)
            {
                int x = random.Next(rect.Width);
                int y = random.Next(rect.Height);
                int w = random.Next(m / 50);
                int h = random.Next(m / 50);
                g.FillEllipse(_hatchBrush, x, y, w, h);
            }
            font.Dispose();
            hatchBrush.Dispose();
            _hatchBrush.Dispose();
            g.Dispose();

            this._image = bitmap;
        }

        public bool ValidKey(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            string currentKey = _distributedCache.GetString($"{_cachePrefixKey}{_cacheKey}");
            if (string.IsNullOrWhiteSpace(currentKey))
                return false;

            var valid = string.Equals(password, currentKey, StringComparison.OrdinalIgnoreCase);
            if (valid)
                Clear();

            return valid;
        }

        private string GetRandomCode()
        {
            Random r = new Random();
            string s = "";
            for (int j = 0; j < 5; j++)
            {
                int i = r.Next(3);
                int ch;
                switch (i)
                {
                    case 1:
                        ch = r.Next(0, 9);
                        s = s + ch.ToString();
                        break;
                    case 2:
                        ch = r.Next(65, 90);
                        s = s + Convert.ToChar(ch).ToString();
                        break;
                    case 3:
                        ch = r.Next(97, 122);
                        s = s + Convert.ToChar(ch).ToString();
                        break;
                    default:
                        ch = r.Next(97, 122);
                        s = s + Convert.ToChar(ch).ToString();
                        break;
                }
                r.NextDouble();
                r.Next(100, 1999);
            }

            return s;
        }

        static FontFamily LoadFontFamily(string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            PrivateFontCollection fontCollection = new PrivateFontCollection();
            fontCollection.AddFontFile(fileName);
            return fontCollection.Families[0];
        }

        static FontFamily LoadFontFamily(byte[] buffer)
        {
            // pin array so we can get its address
            var handle = System.Runtime.InteropServices.GCHandle.Alloc(buffer, System.Runtime.InteropServices.GCHandleType.Pinned);
            PrivateFontCollection fontCollection = null;
            try
            {
                var ptr = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
                fontCollection = new PrivateFontCollection();
                fontCollection.AddMemoryFont(ptr, buffer.Length);
                return fontCollection.Families[0];
            }
            catch
            {
                return null;
            }
            finally
            {
                // don't forget to unpin the array!
                handle.Free();
            }
        }

        public void Clear()
        {
            _distributedCache.Remove($"{_cachePrefixKey}{_cacheKey}");
        }
    }
}
