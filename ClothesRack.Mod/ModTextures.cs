using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClothesRack
{
    /// <summary>
    /// Manages custom textures of the mod
    /// </summary>
    public class ModTextures
    {
        public Texture2D ClothesRack { get; private set; }        

        public void Init()
        {
            ClothesRack = LoadFromResource("ClothesRack.Textures.clothes_rack.png");
        }   
        
        private Texture2D LoadFromResource(string name)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream(name);
                return Texture2D.FromStream(Game1.graphics.GraphicsDevice, stream);
            } catch (Exception exc)
            {
                Logger.Error($"Could not load mod texture file '{name}': {exc.Message}");
                return null;
            }
        }
    }
}
