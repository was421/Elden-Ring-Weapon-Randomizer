﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Elden_Ring_Weapon_Randomizer
{
    class ERItemCategory
    {
        public List<ERWeapon> Weapons;

        private static Regex categoryEntryRx = new Regex(@"^(?<list>.+) (?<infusable>\S+)$");
        private ERItemCategory(string itemList, bool infusible)
        {
            Weapons = new List<ERWeapon>();
            foreach (string line in itemList.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!line.Contains("/")) //determine if line is a valid resource or not
                    Weapons.Add(new ERWeapon(line, infusible));
            };
        }
        public static void GetItemCategories()
        {
            string result = Util.GetTxtResource("Resources/ERItemCategories.txt");
            All = new List<ERItemCategory>();

            foreach (string line in result.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!line.Contains("//")) //determine if line is a valid resource or not
                {
                    Match itemEntry = categoryEntryRx.Match(line);
                    var name = itemEntry.Groups["list"].Value;
                    var infusible = Convert.ToBoolean(itemEntry.Groups["infusable"].Value);
                    All.Add(new ERItemCategory(Util.GetTxtResource($"Resources/{name}"), infusible));
                }
            };

            GreatArrows = new ERItemCategory(Util.GetTxtResource("Resources/Weapons_DLC/GreatArrows.txt"), false);
            GreatBolts = new ERItemCategory(Util.GetTxtResource("Resources/Weapons_DLC/GreatBolts.txt"), false);
            Arrows = new ERItemCategory(Util.GetTxtResource("Resources/Weapons_DLC/Arrows.txt"), false);
            Bolts = new ERItemCategory(Util.GetTxtResource("Resources/Weapons_DLC/Bolts.txt"), false);
            Sorceries = new ERItemCategory(Util.GetTxtResource("Resources/Weapons_DLC/Sorceries.txt"), false);
            Incantations = new ERItemCategory(Util.GetTxtResource("Resources/Weapons_DLC/Incantations.txt"), false);
        }
        public static List<ERItemCategory> All = new List<ERItemCategory>();
        public static ERItemCategory GreatArrows;
        public static ERItemCategory GreatBolts;
        public static ERItemCategory Arrows;
        public static ERItemCategory Bolts;
        public static ERItemCategory Sorceries;
        public static ERItemCategory Incantations;
        public static ERItemCategory Test;
    }
}
