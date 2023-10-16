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
using static StardewValley.Objects.BedFurniture;

namespace ClothesRack.Types
{
   public class ClothesRackFurniture : Furniture
    {
        public const int clothingRackId = 9713;
        protected Texture2D Texture { get; }

        protected NetRef<Hat> HatSlot { get; } = new NetRef<Hat>();
        protected NetRef<Clothing> ShirtSlot { get; } = new NetRef<Clothing>();
        protected NetRef<Clothing> PantsSlot { get; } = new NetRef<Clothing>();

        public ClothesRackFurniture()
            : base()
        {
            Texture = ClothesRackEntry.ModTextures.ClothesRack;

            base.ParentSheetIndex = clothingRackId;
            this.furniture_type.Value = clothingRackId;
            this.Name = "Clothes Rack";
            this.boundingBox.Value = new Rectangle(0, 0, 1, 2);

            int which = 0;
            string[] data = this.getData();

            defaultSourceRect.Value = new Rectangle(which * 16 % furnitureTexture.Width, which * 16 / furnitureTexture.Width * 16, 1, 1);
            defaultSourceRect.Width = Convert.ToInt32(data[2].Split(' ')[0]);
            defaultSourceRect.Height = Convert.ToInt32(data[2].Split(' ')[1]);
            sourceRect.Value = new Rectangle(which * 16 % furnitureTexture.Width, which * 16 / furnitureTexture.Width * 16, defaultSourceRect.Width * 16, defaultSourceRect.Height * 16);
            defaultSourceRect.Value = sourceRect.Value;

            defaultBoundingBox.Value = new Rectangle((int)tileLocation.X, (int)tileLocation.Y, 1, 1);
            defaultBoundingBox.Width = Convert.ToInt32(data[3].Split(' ')[0]);
            defaultBoundingBox.Height = Convert.ToInt32(data[3].Split(' ')[1]);
            boundingBox.Value = new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, defaultBoundingBox.Width * 64, defaultBoundingBox.Height * 64);
            defaultBoundingBox.Value = boundingBox;

            updateDrawPosition();
            rotations.Value = Convert.ToInt32(data[4]);
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            base.NetFields.AddFields(HatSlot, ShirtSlot, PantsSlot);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(Texture, location + new Vector2(32f, 32f), defaultSourceRect, color * transparency, 0f, new Vector2(defaultSourceRect.Width / 2, defaultSourceRect.Height / 2), 1f * getScaleSize() * scaleSize, SpriteEffects.None, layerDepth);
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
            Rectangle drawn_source_rect = sourceRect.Value;
            float layerDepth;
            if (isDrawingLocationFurniture)
            {
                layerDepth = ((int)furniture_type == 12) ? (2E-09f + tileLocation.Y / 100000f) : ((float)(boundingBox.Value.Bottom - (((int)furniture_type == 6 || (int)furniture_type == 17 || (int)furniture_type == 13) ? 48 : 8)) / 10000f);
                spriteBatch.Draw(Texture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), drawn_source_rect, Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
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
                var which =hat.which.Value;
                float scaleSize = 0.9f;
                bool isPrismatic = hat.isPrismatic.Value;                
                spriteBatch.Draw(FarmerRenderer.hatsTexture, location + new Vector2(32f, 32f), new Rectangle((int)which * 20 % FarmerRenderer.hatsTexture.Width, (int)which * 20 / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20), isPrismatic ? (Utility.GetPrismaticColor() * alpha) : (Color.White * alpha), MathHelper.ToRadians(20), new Vector2(10f, 10f), 4f * scaleSize, SpriteEffects.None, layerDepth);
            }
            if (ShirtSlot.Value != null)
            {
                ShirtSlot.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, drawPosition + new Vector2(0, 14)), 1.1f, alpha, layerDepth + 2/10000f, StackDrawType.Hide, Color.White, false);
            }
            if (PantsSlot.Value != null)
            {
                PantsSlot.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, drawPosition + new Vector2(0, 48)), 1.1f, alpha, layerDepth + 1/10000f, StackDrawType.Hide, Color.White, false);
            }

            /*
            if (heldObject.Value != null)
            {
                if (heldObject.Value is Furniture)
                {
                    (heldObject.Value as Furniture).drawAtNonTileSpot(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2(boundingBox.Center.X - 32, boundingBox.Center.Y - (heldObject.Value as Furniture).sourceRect.Height * 4 - (drawHeldObjectLow ? (-16) : 16))), (float)(boundingBox.Bottom - 7) / 10000f, alpha);
                }
                else
                {
                    spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(boundingBox.Center.X - 32, boundingBox.Center.Y - (drawHeldObjectLow ? 32 : 85))) + new Vector2(32f, 53f), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)boundingBox.Bottom / 10000f);
                    spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(boundingBox.Center.X - 32, boundingBox.Center.Y - (drawHeldObjectLow ? 32 : 85))), GameLocation.getSourceRectForObject(heldObject.Value.ParentSheetIndex), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(boundingBox.Bottom + 1) / 10000f);
                }
            }*/
        }

        public override void drawAtNonTileSpot(SpriteBatch spriteBatch, Vector2 location, float layerDepth, float alpha = 1f)
        {
            Rectangle drawn_source_rect = sourceRect.Value;
            //drawn_source_rect.X += drawn_source_rect.Width * (int)sourceIndexOffset;
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

            // swap outfits
            var tmpHat = Game1.player.hat.Value;

            SwapClothingItem(HatSlot, Game1.player.hat);
            SwapClothingItem(ShirtSlot, Game1.player.shirtItem);
            SwapClothingItem(PantsSlot, Game1.player.pantsItem);

            return true;
        }
    }
}
