using ClothesRack.Types;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClothesRack.Patches
{
    class ShopExtension
    {
        IClickableMenu currentMenu = null;

        public void Init(IModHelper modHelper)
        {
            modHelper.Events.Display.MenuChanged += Display_MenuChanged;         
        }

        private void Display_MenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            if (e.NewMenu is ShopMenu shop)
            {
                ShopOpened(shop);
            }
        }

        protected void ShopOpened(ShopMenu shop)
        {
            if ((shop.ShopId?.ToLower() ?? "") == "carpenter")
            {
                // Add the clothes rack to the carpenter's shop's stack                
                Item saleItem = new ClothesRackFurniture();
                shop.forSale.Insert(0, saleItem);
                shop.itemPriceAndStock.Add(saleItem, new ItemStockInformation(350, int.MaxValue));
            }
        }
    }
}
