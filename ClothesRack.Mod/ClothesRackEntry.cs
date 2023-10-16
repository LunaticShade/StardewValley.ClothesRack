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

namespace ClothesRack
{
    public class ClothesRackEntry : Mod
    {

        public static ModTextures ModTextures = new ModTextures();

        public override void Entry(IModHelper helper)
        {
            Logger.Init(Monitor, true);
            ModTextures.Init();

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;

            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;

            helper.Events.Content.AssetRequested += Content_AssetRequested;


        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo("data/furniture", true))
            {
                e.Edit((data) =>
                {
                    data.AsDictionary<int, string>().Data.Add(ClothesRackFurniture.clothingRackId, "Clothes Rack/decor/1 2/1 1/1/500");
                });
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            
        }

        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
               
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            Game1.player.addItemToInventory(new ClothesRackFurniture());
        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
           
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;                      
        }
    }
}
