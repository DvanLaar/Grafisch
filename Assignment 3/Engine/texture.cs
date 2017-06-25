using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using System.IO;

namespace rasterizer
{
    public class Texture
    {
        /// <summary>
        /// The texture ID belonging to this texture.
        /// </summary>
        public int id;

        /// <summary>
        /// Creates an empty texture
        /// </summary>
        public Texture()
        {
            id = -1;
        }

        /// <summary>
        /// Creates a texture from the image at 'fileName'
        /// </summary>
        /// <param name="fileName"></param>
        public Texture(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                throw new ArgumentException(fileName);

            id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            // We will not upload mipmaps, so disable mipmapping (otherwise the texture will not appear).
            // We can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
            // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            Bitmap bitmap;
            using (Stream imageStream = File.Open(fileName, FileMode.Open))
            {
                Image image = Image.FromStream(imageStream);
                bitmap = new Bitmap(image);
            }

            LoadBitmap(bitmap, TextureTarget.Texture2D);
        }

        protected void LoadBitmap(Bitmap bitmap, TextureTarget target)
        {
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(target, 0, PixelInternalFormat.Rgba, bitmapData.Width, bitmapData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
            bitmap.UnlockBits(bitmapData);
        }

        /// <summary>
        /// Loads a texture from the expected location for a texture.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static Texture Load(string filename)
        {
            return new Texture("../../assets/" + filename);
        }
    }

    public class CubeTexture : Texture
    {
        public CubeTexture(string fileNameRight, string fileNameLeft, string fileNameTop, string fileNameBottom, string fileNameBack, string fileNameFront) : base()
        {
            id = GL.GenTexture();
            // Bind and set parameters for the cubemap
            GL.BindTexture(TextureTarget.TextureCubeMap, id);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            // Load the six sides of the cube
            LoadBitmap(new Bitmap(fileNameRight), TextureTarget.TextureCubeMapPositiveX);
            LoadBitmap(new Bitmap(fileNameLeft), TextureTarget.TextureCubeMapNegativeX);
            LoadBitmap(new Bitmap(fileNameTop), TextureTarget.TextureCubeMapPositiveY);
            LoadBitmap(new Bitmap(fileNameBottom), TextureTarget.TextureCubeMapNegativeY);
            LoadBitmap(new Bitmap(fileNameBack), TextureTarget.TextureCubeMapPositiveZ);
            LoadBitmap(new Bitmap(fileNameFront), TextureTarget.TextureCubeMapNegativeZ);
        }
    }

} // namespace Template_P3
