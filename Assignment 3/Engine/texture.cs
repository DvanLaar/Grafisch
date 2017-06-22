using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace Template_P3
{

    public class Texture
    {
        // data members
        public int id;

        // constructor
        public Texture(string filename)
        {
            if (filename == "")
                return;
            if (String.IsNullOrEmpty(filename)) throw new ArgumentException(filename);
            id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            // We will not upload mipmaps, so disable mipmapping (otherwise the texture will not appear).
            // We can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
            // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            Bitmap bmp = new Bitmap(filename);
            BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
            bmp.UnlockBits(bmp_data);
        }

        public static Texture Load(string filename)
        {
            return new Texture("../../assets/" + filename);
        }
    }

    public class CubeTexture
    {
        public int id;

        public CubeTexture(string filename_right, string filename_left, string filename_top, string filename_bottom, string filename_back, string filename_front)
        {
            id = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, id);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            Bitmap bmp = new Bitmap(filename_right);
            BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
            bmp.UnlockBits(bmp_data);

            LoadTexture(filename_right, TextureTarget.TextureCubeMapPositiveX);
            LoadTexture(filename_left, TextureTarget.TextureCubeMapNegativeX);
            LoadTexture(filename_top, TextureTarget.TextureCubeMapPositiveY);
            LoadTexture(filename_bottom, TextureTarget.TextureCubeMapNegativeY);
            LoadTexture(filename_back, TextureTarget.TextureCubeMapPositiveZ);
            LoadTexture(filename_front, TextureTarget.TextureCubeMapNegativeZ);
        }

        private void LoadTexture(string filename, TextureTarget target)
        {

            Bitmap bmp = new Bitmap(filename);
            BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(target, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
            bmp.UnlockBits(bmp_data);
        }

    }

} // namespace Template_P3
