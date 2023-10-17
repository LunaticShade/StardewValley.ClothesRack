using ClothesRack.Types;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StardewValley.Minigames.MineCart;

namespace ClothesRack.Patches
{
    /// <summary>
    /// Handles required savegame changes so that
    /// the game does not crash due to the presence of an unknown, custom object instance
    /// </summary>
    internal class SavegamePatch
    {
        public const string Custom_Type_Field_Uid = "9957505A-2486-40EC-BD6F-5AD7E0FEB1D7";
        public const int ChickenStatueFurnitureId = 1305;
        public const string PlaceholderTypeName = "clothes rack";        

        private StardewValley.Object GetPlaceholderObject(ClothesRackFurniture rack)
        {
            if (rack.IsEmpty)
            {
                // replace with ChickenStatue
                Furniture chickenStatue = new Furniture(ChickenStatueFurnitureId, rack.TileLocation);
                chickenStatue.modData[Custom_Type_Field_Uid] = PlaceholderTypeName;
                return chickenStatue;
            } else
            {
                // replace with chest which holds the items of the racks
                var chest = new Chest(true);
                chest.modData[Custom_Type_Field_Uid] = PlaceholderTypeName;
                chest.addItem(rack.HatSlot.Value);
                chest.addItem(rack.ShirtSlot.Value);
                chest.addItem(rack.PantsSlot.Value);
                return chest;
            }            
        }

        private ClothesRackFurniture FromPlaceholderObject(StardewValley.Object placeholder)
        {
            if (placeholder is Chest chest)
            {
                ClothesRackFurniture rack = new ClothesRackFurniture(placeholder.TileLocation);
                rack.HatSlot.Value = chest.items.OfType<Hat>().FirstOrDefault();
                rack.ShirtSlot.Value = chest.items.OfType<Clothing>().FirstOrDefault(x => x.clothesType.Value == (int)Clothing.ClothesType.SHIRT);
                rack.PantsSlot.Value = chest.items.OfType<Clothing>().FirstOrDefault(x => x.clothesType.Value == (int)Clothing.ClothesType.PANTS);
                return rack;
            } else
            {
                return new ClothesRackFurniture(placeholder.TileLocation);                
            }            
        }

        private bool IsPlaceholderObject(Item item)
        {
            if (item is Furniture furniture && furniture.ParentSheetIndex == ChickenStatueFurnitureId && furniture.modData.TryGetValue(Custom_Type_Field_Uid, out string ctype) && ctype == PlaceholderTypeName)
            {
                // chicken statue placeholder
                return true;
            }

            if (item is Chest chest && chest.modData.TryGetValue(Custom_Type_Field_Uid, out string chtype) && chtype.StartsWith("clothes"))
            {
                return true;
            }

            return false;
        }

        void SaveList<T>(IList<T> lst, GameLocation placedInLocation = null)
            where T : Item
        {
            for (int i = lst.Count - 1; i >= 0; i--)
            {
                if (lst[i] is ClothesRackFurniture rack)
                {
                    // replace the custom object with a palceholder object which is known to the game
                    // so that it can save
                    var placeholder = GetPlaceholderObject(rack);

                    if (placeholder is T)
                    {
                        lst[i] = placeholder as T;
                    }
                    else
                    {
                        // we need to place the object in the location
                        lst.RemoveAt(i);

                        if (placedInLocation != null)
                        {
                            placedInLocation.Objects.Add(rack.TileLocation, placeholder);
                        }
                        else
                        {
                            Logger.Error($"Could not place placeholder of type {placeholder.GetType().FullName} since location was null");
                        }
                    }
                }
            }
        }

        void RestoreInFurniture(IList<Furniture> lst)
        {
            for (int i = lst.Count - 1; i >= 0; i--)
            {
                if (IsPlaceholderObject(lst[i]))
                {
                    lst[i] = FromPlaceholderObject(lst[i]);                    
                }
            }
        }

        void RestoreInList<T>(IList<T> lst)
            where T: Item
        {
            for (int i = lst.Count - 1; i >= 0; i--)
            {
                if (IsPlaceholderObject(lst[i]))
                {
                    var rack = FromPlaceholderObject(lst[i] as StardewValley.Object);
                    lst[i] = rack as T;                    
                }
            }
        }

        void RestoreInLocationObjects(GameLocation location)
        {
            var locationKeys = location.Objects.Keys.ToList();
            for (int i = locationKeys.Count - 1; i >= 0; i--)
            {
                var obj = location.Objects[locationKeys[i]];
                if (IsPlaceholderObject(obj))
                {
                    // fix: for objects, the location is not set correctly
                    obj.TileLocation = locationKeys[i];

                    var rack = FromPlaceholderObject(obj);
                    location.Objects.Remove(locationKeys[i]);
                    location.furniture.Add(rack);
                } else
                {
                    if (obj is Chest chest)
                    {
                        RestoreInList(chest.items);
                    }
                }
            }
        }

        public void PrepareSave()
        {
            SaveList(Game1.player.Items);

            // in order to save, replace every clothes rack with a chicken statue with custom mod data
            foreach(var location in Game1.locations.OfType<DecoratableLocation>())
            {                
                SaveList(location.furniture, location);
                
                foreach(var chest in location.Objects.Values.OfType<Chest>())
                {
                    SaveList(chest.items);
                }
            }
        }

        public void FinishLoad()
        {
            RestoreInList(Game1.player.Items);

            // reload placed game objects            
            foreach (var location in Game1.locations.OfType<DecoratableLocation>())
            {
                RestoreInFurniture(location.furniture);
                RestoreInLocationObjects(location);
            }

            // todo: storage chests
        }
    }
}
