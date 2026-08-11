using OpenTK.Graphics.OpenGL4;
using StbImageSharp;
using System;
using System.IO;
using System.Reflection.Metadata;

//https://opentk.net/learn/chapter1/5-textures.html?tabs=load-texture-opentk4

namespace _3D_Engine.Classes.UI
{
    public class Texture
    {
        int handle;

        public Texture(string path)
        {
            handle = GL.GenTexture();

            // stb_image loads from the top-left pixel, whereas OpenGL loads from the bottom-left, causing the texture to be flipped vertically.
            // This will correct that, making the texture display properly.
            StbImage.stbi_set_flip_vertically_on_load(1);

            // Load the image.
            ImageResult image = ImageResult.FromStream(File.OpenRead(path), ColorComponents.RedGreenBlueAlpha);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public void Use()
        {
            GL.BindTexture(TextureTarget.Texture2D, handle);
        }

    }
}
