using ClothesRack.Types;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData.HomeRenovations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClothesRack.Patches
{
    internal class HarmonyPatches
    {

        public void Apply(string modId)
        {
            Harmony harmony = new Harmony(modId);            

            harmony.Patch(
                  original: typeof(Furniture).GetMethod(nameof(Furniture.drawInMenu), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }),
                  prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(furniture_drawInMenu))
            );            

            harmony.Patch(                
                original: typeof(Farmer).GetMethod(nameof(Farmer.addItemToInventory), new Type[] { typeof(Item), typeof(List<Item>) }),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(farmer_addItemToInventory))
            );
        } 

        static bool furniture_drawInMenu(Furniture __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            if (ClothesRackFurniture.NeedsReplacementInstance(__instance, out Furniture furniture))
            {           
                var sourceRect = ClothesRackFurniture.getDefaultSourceRect();
                spriteBatch.Draw(ClothesRackEntry.ModTextures.ClothesRack, location + new Vector2(32f, 32f), sourceRect, color * transparency, 0f, new Vector2(sourceRect.Width / 2, sourceRect.Height / 2), 1f * getScaleSize(sourceRect) * scaleSize, SpriteEffects.None, layerDepth);
                if (((drawStackNumber == StackDrawType.Draw && __instance.maximumStackSize() > 1 && __instance.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && __instance.Stack != int.MaxValue)
                {
                    Utility.drawTinyDigits(__instance.Stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(__instance.Stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
                }

                return false;
            }

            return true;
        }

        public static bool farmer_addItemToInventory(Farmer __instance, Item __result, ref Item item, List<Item> affected_items_list)
        {
            if (ClothesRackFurniture.NeedsReplacementInstance(item, out Furniture furniture))
            {
                item = new ClothesRackFurniture(furniture.TileLocation);                
            }

            return true;
        }


        /// <summary>
        /// Copied from Furniture.getScaleSize()
        /// </summary>        
        static float getScaleSize(Rectangle rect)
        {
            int num = rect.Width / 16;
            int num2 = rect.Height / 16;
            if (num >= 7)
            {
                return 0.5f;
            }

            if (num >= 6)
            {
                return 0.66f;
            }

            if (num >= 5)
            {
                return 0.75f;
            }

            if (num2 >= 5)
            {
                return 0.8f;
            }

            if (num2 >= 3)
            {
                return 1f;
            }

            if (num <= 2)
            {
                return 2f;
            }

            if (num <= 4)
            {
                return 1f;
            }

            return 0.1f;
        }
    }
}
