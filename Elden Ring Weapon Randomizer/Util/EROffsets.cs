using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elden_Ring_Weapon_Randomizer
{
    internal class EROffsets
    {
        public const int BasePtrOffset1 = 0x3;
        public const int BasePtrOffset2 = 0x7;

        public const string GameDataManSetupAoB = "48 8B 05 ? ? ? ? 48 85 C0 74 05 48 8B 40 58 C3 C3";

        public const int PlayerGameData = 0x8;
        public enum Weapons
        {
            ArmStyle = 0x324,
            CurrWepSlotOffsetLeft = 0x328,
            CurrWepSlotOffsetRight = 0x32C,
            LHandWeapon1 = 0x398,
            LHandWeapon1_ = 0x5F8,
            LHandWeapon2 = 0x3A0,
            LHandWeapon2_ = 0x600,
            LHandWeapon3 = 0x3A8,
            LHandWeapon3_ = 0x608,
            RHandWeapon1 = 0x39C,
            RHandWeapon1_ = 0x5FC,
            RHandWeapon2 = 0x3A4,
            RHandWeapon2_ = 0x604,
            RHandWeapon3 = 0x3AC,
            RHandWeapon3_ = 0x60C,
            Arrow1 = 0x3B0,
            Arrow1_ = 0x610,
            Bolt1 = 0x3B4,
            Bolt1_ = 0x614,
            Arrow2 = 0x3B8,
            Arrow2_ = 0x618,
            Bolt2 = 0x3BC,
            Bolt2_ = 0x61C
        }

        public enum Magic : int
        {
            BasePtr = 0x530,
            Slot0 = 0x10,
            Slot1 = 0x18,
            Slot2 = 0x20,
            Slot3 = 0x28,
            Slot4 = 0x30,
            Slot5 = 0x38,
            Slot6 = 0x40,
            Slot7 = 0x48,
            Slot8 = 0x50,
            Slot9 = 0x58,
            Slot10 = 0x60,
            Slot11 = 0x68,
            Slot12 = 0x70,
            Slot13 = 0x78,
            SelectedSlot = 0x80
        }

        public enum Player
        {
            Level = 0x68
        }

        public const string SoloParamRepositorySetupAoB = "48 8B 0D ? ? ? ? 48 85 C9 0F 84 ? ? ? ? 45 33 C0 BA 8E 00 00 00";

        public enum Param
        {
            TotalParamLength = 0x0,
            NameOffset = 0x10,
            TableLength = 0x30
        }

        public const int EquipParamWeaponOffset1 = 0x88;
        public const int EquipParamWeaponOffset2 = 0x80;
        public const int EquipParamWeaponOffset3 = 0x80;

        public enum EquipParamWeapon
        {
            SortID = 0x8,
            MaterialSetID = 0x5C,
            OriginEquipWep = 0x60,
            IconID = 0xBE,
            ReinforceTypeID = 0xDA,
            SwordArtsParamId = 0x198,
            WepType = 0x1A6,
            OriginEquipWep16 = 0x250
        }

        public const int EquipParamGemOffset1 = 0x2BD8;
        public const int EquipParamGemOffset2 = 0x80;
        public const int EquipParamGemOffset3 = 0x80;

        public enum EquipParamGem
        {
            SwordArtsParamId = 0x18,
            CanMountWep_Dagger = 0x38,
            CanMountWep_SwordPierce = 0x39,
            CanMountWep_SpearLarge = 0x3A,
            CanMountWep_BowSmall = 0x3B,
            CanMountWep_ShieldSmall = 0x3C,
            Default_WepAttr = 0x35,
            
        }


        public const string WorldChrManAOB = "48 8B 05 ? ? ? ? 48 85 C0 74 0F 48 39 88";
    }
}
