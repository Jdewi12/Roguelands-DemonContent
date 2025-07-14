using GadgetCore.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DemonContent
{
    public static class ItemExtensions
    {
        public static Item Create(int id, int q = 1, int exp = 0, int tier = 0, int corrupted = 0, int[] aspect = null, int[] aspectlvl = null)
        {
            if (aspect == null)
                aspect = new int[3];
            if(aspectlvl == null) 
                aspectlvl = new int[3];
            return new Item(id, q, exp, tier, corrupted, aspect, aspectlvl);
        }

        public static Item Instantiate(this ItemInfo itemInfo, int q = 1, int exp = 0, int tier = 0, int corrupted = 0, int[] aspect = null, int[] aspectlvl = null) 
            => Create(itemInfo.GetID(), q, exp, tier, corrupted, aspect, aspectlvl);
    }
}
