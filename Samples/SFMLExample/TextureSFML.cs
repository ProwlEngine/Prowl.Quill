﻿// TextureSFML.cs
using SFML.Graphics;

namespace SFMLExample
{
    public class TextureSFML : IDisposable
    {
        public Texture Handle { get; private set; }
        public uint Width { get; private set; }
        public uint Height { get; private set; }

        public TextureSFML(Texture texture, uint width, uint height)
        {
            Handle = texture;
            Width = width;
            Height = height;
        }

        public static TextureSFML LoadFromFile(string path)
        {
            // Check if file exists
            if (!File.Exists(path))
                throw new FileNotFoundException($"Texture file not found: {path}");

            // Create texture from file
            Texture texture = new Texture(path);
            var size = texture.Size;
            
            return new TextureSFML(texture, size.X, size.Y);
        }

        public static TextureSFML CreateNew(uint width, uint height)
        {
            // Create empty texture with specified dimensions
            Texture texture = new Texture(width, height);
            
            // Set texture parameters similar to OpenGL version
            texture.Smooth = true;  // Linear filtering
            
            return new TextureSFML(texture, width, height);
        }

        public void SetData(Prowl.Vector.Geometry.IntRect bounds, byte[] data)
        {
            ArgumentNullException.ThrowIfNull(bounds);
            // Make sure we have valid bounds
            if (bounds.Size.X <= 0 || bounds.Size.Y <= 0)
                throw new ArgumentException("Invalid texture bounds");
                
            // Create image from raw data
            Image image = new Image((uint)bounds.Size.X, (uint)bounds.Size.Y, data);
            
            // Update texture with the image at the specified position
            Handle.Update(image, (uint)bounds.Min.X, (uint)bounds.Min.Y);
        }

        public void Use(int unit = 0)
        {
            // SFML handles texture binding differently than OpenGL
            // This method doesn't need to do anything as binding happens
            // when the texture is used in RenderStates
        }

        public void Dispose()
        {
            Handle?.Dispose();
            Handle = null;
        }
    }
}