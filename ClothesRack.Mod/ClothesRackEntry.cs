using ClothesRack.Patches;
using ClothesRack.Types;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ClothesRack
{
    public class ClothesRackEntry : Mod
    {

        public static ModTextures ModTextures = new ModTextures();

        ShopExtension shopExtension = new ShopExtension();
        SavegamePatch savegamePatch = new SavegamePatch();
        HarmonyPatches harmonyPatches = new HarmonyPatches();

        public override void Entry(IModHelper helper)
        {
            Logger.Init(Monitor, true);
            ModTextures.Init();
            shopExtension.Init(helper);            
            harmonyPatches.Apply(ModManifest.UniqueID);

            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;

            helper.Events.Content.AssetRequested += Content_AssetRequested;


        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo("data/furniture", true))
            {
                e.Edit((data) =>
                {
                    data.AsDictionary<string, string>().Data.Add(ClothesRackFurniture.clothingRackItemId, "Clothes Rack/decor/1 2/1 1/1/500");
                });
            }
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {            
            savegamePatch.AfterLoad();
        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            savegamePatch.BeforeSave();
        }
    }
}
