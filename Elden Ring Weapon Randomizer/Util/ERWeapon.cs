using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elden_Ring_Weapon_Randomizer
{
    internal class ERWeapon : ERItem
    {
        public enum Infusion
        {
            Standard = 000,
            Heavy = 100,
            Keen = 200,
            Quality = 300,
            Fire = 400,
            FlameArt = 500,
            Lightning = 600,
            Sacred = 700,
            Magic = 800,
            Cold = 900,
            Posion = 1000,
            Blood = 1100,
            Occult = 1200,
        };

        public enum WeaponType
        {
            None = 0,
            Dagger = 1,
            StraightSword = 3,
            Greatsword = 5,
            ColossalSword = 7,
            CurvedSword = 9,
            CurvedGreatsword = 11,
            Katana = 13,
            Twinblade = 14,
            ThrustingSword = 15,
            HeavyThrustingSword = 16,
            Axe = 17,
            Greataxe = 19,
            Hammer = 21,
            GreatHammer = 23,
            Flail = 24,
            Spear = 25,
            HeavySpear = 28,
            Halberd = 29,
            Scythe = 31,
            Fist = 35,
            Claw = 37,
            Whip = 39,
            ColossalWeapon = 41,
            LightBow = 50,
            Bow = 51,
            Greatbow = 53,
            Crossbow = 55,
            Ballista = 56,
            Staff = 57,
            Seal = 61,
            SmallShield = 65,
            MediumShield = 67,
            Greatshield = 69,
            Arrow = 81,
            Greatarrow = 83,
            Bolt = 85,
            BallistaBolt = 86,
            Torch = 87,
            HandToHand = 88,
            PerfumeBottle = 89,
            ThrustingShield = 90,
            ThrowingBlade = 91,
            ReverseHandBlade = 92,
            LightGreatsword = 93,
            GreatKatana = 94,
            BeastClaw = 95
        }

        public enum AmmoType
        {
            Arrow = 81,
            GreatArrow = 83,
            Bolt = 85,
            BallistaBolt = 86,
        }

        public uint SortID { get; set; }
        public uint RealID { get; set; }
        public bool Infusible { get; set; }
        public byte[] OriginEquipWep { get; set; } = new byte[0x40];
        public bool Unique { get; set; }
        public short IconID { get; set; }
        public uint SwordArtId { get; set; }
        public WeaponType Type { get; set; }
        public byte[] OriginEquipWep16 { get; set; } = new byte[0x28];
        public ERWeapon(string config, bool infusible) : base(config) 
        {
            RealID = Util.DeleteFromEnd(ID, 3);
            Infusible = infusible;
        }

        public ERWeapon()
        {
        }

        internal void Clone(ERWeapon source)
        {
           Name = source.Name;
           ID = source.ID;
           RealID = source.RealID;
           Infusible = source.Infusible;
           Array.Copy(source.OriginEquipWep, OriginEquipWep,source.OriginEquipWep.Length);
           IconID = source.IconID;
           Unique = source.Unique;
           SwordArtId = source.SwordArtId;
           Type = source.Type;
           Array.Copy(source.OriginEquipWep16, OriginEquipWep16, source.OriginEquipWep16.Length);
        }
    }
}
