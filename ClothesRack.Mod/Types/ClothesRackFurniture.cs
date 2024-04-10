using ClothesRack.Patches;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static StardewValley.Minigames.MineCart;
using static StardewValley.Objects.BedFurniture;

namespace ClothesRack.Types
{
    [XmlInclude(typeof(ClothesRackFurniture))]
    public class ClothesRackFurniture : Furniture
    {
        public const string clothingRackItemId = "ClothesRack";
        public const int clothingRackFurnitureId = 9713;

        protected Texture2D Texture { get; private set; }

        public NetRef<Hat> HatSlot { get; } = new NetRef<Hat>();
        public NetRef<Clothing> ShirtSlot { get; } = new NetRef<Clothing>();
        public NetRef<Clothing> PantsSlot { get; } = new NetRef<Clothing>();

        public bool IsEmpty => HatSlot.Value == null && ShirtSlot.Value == null && PantsSlot.Value == null;

        protected bool playHatHangAnimation = false;
        protected float hatAnimFrame = 0;

        public ClothesRackFurniture(Vector2 tile)
            : base("1305", tile) // derive from the chicken statue, type = decor
        {
            Init();
        }

        public static Rectangle getDefaultSourceRect()
        {
            return new Rectangle(0, 0, 16, 32);
        }

        /// <summary>
        /// Override the originbal furniture's data with custom data
        /// </summary>
        private void Init()
        {
            Texture = ClothesRackEntry.ModTextures.ClothesRack;

            base.ItemId = clothingRackItemId;
            this.furniture_type.Value = clothingRackFurnitureId;
            this.Name = "Clothes Rack";

            int which = 0;
            string[] data = this.getData();

            defaultSourceRect.Value = new Rectangle(which * 16 % Texture.Width, which * 16 / Texture.Width * 16, 1, 1);
            defaultSourceRect.Width = Convert.ToInt32(data[2].Split(' ')[0]);
            defaultSourceRect.Height = Convert.ToInt32(data[2].Split(' ')[1]);
            sourceRect.Value = new Rectangle(which * 16 % Texture.Width, which * 16 / Texture.Width * 16, defaultSourceRect.Width * 16, defaultSourceRect.Height * 16);
            defaultSourceRect.Value = sourceRect.Value;            
        }

        public ClothesRackFurniture()
            : base()
        {
            Init();
        }        

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddField(HatSlot);
            NetFields.AddField(ShirtSlot);
            NetFields.AddField(PantsSlot);            
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            Rectangle sourceRect = defaultSourceRect.Value;       

            spriteBatch.Draw(Texture, location + new Vector2(32f, 32f), sourceRect, color * transparency, 0f, new Vector2(defaultSourceRect.Width / 2, defaultSourceRect.Height / 2), 1f * getScaleSize() * scaleSize, SpriteEffects.None, layerDepth);
            if (((drawStackNumber == StackDrawType.Draw && maximumStackSize() > 1 && Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && Stack != int.MaxValue)
            {
                Utility.drawTinyDigits(stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
            }
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (isTemporarilyInvisible)
            {
                return;
            }

            Vector2 drawPosition = new Vector2(this.drawPosition.X, this.drawPosition.Y);
            Rectangle sourceRect = defaultSourceRect.Value;

            float layerDepth;
            if (isDrawingLocationFurniture)
            {
                layerDepth = ((int)furniture_type == 12) ? (2E-09f + tileLocation.Y / 100000f) : ((float)(boundingBox.Value.Bottom - (((int)furniture_type == 6 || (int)furniture_type == 17 || (int)furniture_type == 13) ? 48 : 8)) / 10000f);
                spriteBatch.Draw(Texture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), sourceRect, Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
            }
            else
            {
                layerDepth = ((int)furniture_type == 12) ? (2E-09f + tileLocation.Y / 100000f) : ((float)(boundingBox.Value.Bottom - (((int)furniture_type == 6 || (int)furniture_type == 17 || (int)furniture_type == 13) ? 48 : 8)) / 10000f);                
                spriteBatch.Draw(Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 - (sourceRect.Height * 4 - boundingBox.Height) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), sourceRect, Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
            }

            if (HatSlot.Value is Hat hat)
            {
                // HatSlot.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, drawPosition + new Vector2(0, -16)), 1.1f, alpha, layerDepth + 3/10000f, StackDrawType.Hide, Color.White, false);
                // since we want to draw the hat with a slight rotation, we need to draw it on our own
                var location = Game1.GlobalToLocal(Game1.viewport, drawPosition + new Vector2(0, -16));
                int hatId = int.Parse(hat.ItemId);
                float scaleSize = 0.9f;
                bool isPrismatic = hat.isPrismatic.Value;

                float rot = MathHelper.ToRadians(20);
                if (playHatHangAnimation)
                {
                    rot += MathHelper.ToRadians((float)(4 * Math.Sin(hatAnimFrame)));
                }

                spriteBatch.Draw(FarmerRenderer.hatsTexture, location + new Vector2(32f, 32f), new Rectangle((int)hatId * 20 % FarmerRenderer.hatsTexture.Width, (int)hatId * 20 / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20), isPrismatic ? (Utility.GetPrismaticColor() * alpha) : (Color.White * alpha), rot, new Vector2(10f, 10f), 4f * scaleSize, SpriteEffects.None, layerDepth + 3 / 10000f);
            }
            if (ShirtSlot.Value != null)
            {
                ShirtSlot.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, drawPosition + new Vector2(0, 14)), 1.1f, alpha, layerDepth + 2/10000f, StackDrawType.Hide, Color.White, false);
            }
            if (PantsSlot.Value != null)
            {
                PantsSlot.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, drawPosition + new Vector2(0, 48)), 1.1f, alpha, layerDepth + 1/10000f, StackDrawType.Hide, Color.White, false);
            }
        }

        public override void updateWhenCurrentLocation(GameTime time)
        {
            if (playHatHangAnimation)
            {
                hatAnimFrame += (float)(time.ElapsedGameTime.TotalMilliseconds * 0.02);

                if (hatAnimFrame > 10)
                {
                    playHatHangAnimation = false;
                }
            }
        }

        public override void drawAtNonTileSpot(SpriteBatch spriteBatch, Vector2 location, float layerDepth, float alpha = 1f)
        {
            Rectangle drawn_source_rect = sourceRect.Value;            
            spriteBatch.Draw(Texture, location, drawn_source_rect, Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
        }

        protected override string loadDescription()
        {
            return "A clothes rack";
        }

        private static void SwapClothingItem<T>(NetRef<T> slot1, NetRef<T> slot2)
            where T: class, INetObject<INetSerializable>
        {
            T tmp = slot2.Value;
            slot2.Value = slot1.Value;
            slot1.Value = tmp;
        }

        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        {
            if (justCheckingForActivity)
            {
                return true;
            }

            SwapClothingItem(HatSlot, Game1.player.hat);
            SwapClothingItem(ShirtSlot, Game1.player.shirtItem);
            SwapClothingItem(PantsSlot, Game1.player.pantsItem);           

            if (HatSlot.Value != null)
            {
                hatAnimFrame = 0;
                playHatHangAnimation = true;
            }

            return true;
        }        
        
        public override void AttemptRemoval(Action<Furniture> removal_action)
        {
            removal_action?.Invoke(this);
        }

        public override bool canBeRemoved(Farmer who)
        {
            // can only be removed, if it does not hold any clothing items
            return HatSlot.Value == null && ShirtSlot.Value == null && PantsSlot.Value == null;
        }        

        protected override Item GetOneNew()
        {
            return new ClothesRackFurniture();
        }        

        public static bool NeedsReplacementInstance(Item item, out Furniture furniture)
        {
                
            if (item is not ClothesRackFurniture && item is Furniture _furniture && _furniture.ItemId == ClothesRackFurniture.clothingRackItemId)
            {
                furniture = _furniture;
                return true;
            }

            furniture = null;
            return false;
        }
    }
}
