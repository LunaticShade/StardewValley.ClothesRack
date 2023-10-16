using ClothesRack.Types;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClothesRack.Patches
{
    /// <summary>
    /// Handles required savegame changes so that
    /// the game does not crash due to the presence of an unknown, custom object instance
    /// </summary>
    internal class SavegamePatch
    {
        public const string Custom_Type_Field_Uid = "9957505A-2486-40EC-BD6F-5AD7E0FEB1D7";
        public const int PlaceholderItemIndex = 1305; // chicken statue
        
        private Furniture GetPlaceholderObject(ClothesRackFurniture rack)
        {
            Furniture placeholder = new Furniture(PlaceholderItemIndex, rack.TileLocation);
            rack.SaveModData(placeholder.modData);
            return placeholder;
        }

        private ClothesRackFurniture FromPlaceholderObject(Furniture placeholder)
        {
            ClothesRackFurniture rack = new ClothesRackFurniture(placeholder.TileLocation);
            rack.LoadModData(placeholder.modData);
            return rack;
        }

        private bool IsPlaceholder(Item item)
        {
            return item is Furniture furniture && furniture.ParentSheetIndex == PlaceholderItemIndex && furniture.modData.TryGetValue(Custom_Type_Field_Uid, out string ctype) && ctype == ClothesRackFurniture.custom_type_name;
        }

        void SaveList<T>(IList<T> lst)
            where T: Item
        {
            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i] is ClothesRackFurniture rack)
                {
                    var placeholder = GetPlaceholderObject(rack) as T;
                    lst[i] = placeholder;
                }
            }
        }

        void LoadList<T>(IList<T> lst)
           where T : Item
        {
            for (int i = 0; i < lst.Count; i++)
            {
                if (IsPlaceholder(lst[i]))
                {
                    lst[i] = FromPlaceholderObject(lst[i] as Furniture) as T;
                }
            }
        }

        public void PrepareSave()
        {
            SaveList(Game1.player.Items);

            // in order to save, replace every clothes rack with a chicken statue with custom mod data
            foreach(var location in Game1.locations.OfType<DecoratableLocation>())
            {
                SaveList(location.furniture);
            }

            // todo: storage chests
        }

        public void FinishLoad()
        {
            LoadList(Game1.player.Items);

            // reload placed game objects            
            foreach (var location in Game1.locations.OfType<DecoratableLocation>())
            {
                LoadList(location.furniture);
            }

            // todo: storage chests
        }
    }
}
