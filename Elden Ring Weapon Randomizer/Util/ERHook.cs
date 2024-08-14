using PropertyHook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;
using Infusion = Elden_Ring_Weapon_Randomizer.ERWeapon.Infusion;
using WeaponType = Elden_Ring_Weapon_Randomizer.ERWeapon.WeaponType;

namespace Elden_Ring_Weapon_Randomizer
{
    internal class ERHook : PHook, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private PHPointer GameDataManSetup { get; set; }
        private PHPointer GameDataMan { get; set; }
        private PHPointer PlayerGameData { get; set; }
        private PHPointer EquipMagicData { get; set; }
        private PHPointer SoloParamRepositorySetup { get; set; }
        private PHPointer WorldChrManSetup { get; set; }
        private PHPointer WorldChrMan { get; set; }
        private PHPointer SoloParamRepository { get; set; }
        private PHPointer EquipParamWeapon { get; set; }
        private PHPointer EquipParamGem { get; set; }
        private PHPointer FlagByte0 { get; set; }

        //private PHPointer DurabilityAddr { get; set; }
        //private PHPointer DurabilitySpecialAddr { get; set; }
        public bool Loaded => PlayerGameData != null ? PlayerGameData.Resolve() != IntPtr.Zero : false;
        public ERHook(int refreshInterval, int minLifetime, Func<Process, bool> processSelector)
            : base(refreshInterval, minLifetime, processSelector)
        {
            OnHooked += ERHook_OnHooked;
            GameDataManSetup = RegisterAbsoluteAOB(EROffsets.GameDataManSetupAoB);
            SoloParamRepositorySetup = RegisterAbsoluteAOB(EROffsets.SoloParamRepositorySetupAoB);
            WorldChrManSetup = RegisterAbsoluteAOB(EROffsets.WorldChrManAOB);

            RHandTimer.Elapsed += RHandTimer_Elapsed;
            RHandTimer.AutoReset = true;

            LHandTimer.Elapsed += LHandTimer_Elapsed;
            LHandTimer.AutoReset = true;
        }
        private void ERHook_OnHooked(object? sender, PHEventArgs e)
        {
            GameDataMan = CreateBasePointer(BasePointerFromSetupPointer(GameDataManSetup));
            PlayerGameData = CreateChildPointer(GameDataMan, EROffsets.PlayerGameData);
            EquipMagicData = CreateChildPointer(PlayerGameData, (int)EROffsets.Magic.BasePtr);

            SoloParamRepository = CreateBasePointer(BasePointerFromSetupPointer(SoloParamRepositorySetup));
            EquipParamWeapon = CreateChildPointer(SoloParamRepository, EROffsets.EquipParamWeaponOffset1, EROffsets.EquipParamWeaponOffset2, EROffsets.EquipParamWeaponOffset3);
            EquipParamGem = CreateChildPointer(SoloParamRepository, EROffsets.EquipParamGemOffset1, EROffsets.EquipParamGemOffset2, EROffsets.EquipParamGemOffset3);
            var bytes = new byte[0];
            EquipParamWeaponOffsetDict = BuildOffsetDictionary(EquipParamWeapon, "EQUIP_PARAM_WEAPON_ST", ref bytes);
            EquipParamWeaponBytes = bytes;
            EquipParamGemOffsetDict = BuildOffsetDictionary(EquipParamGem, "EQUIP_PARAM_GEM_ST", ref bytes);
            EquipParamGemBytes = bytes;

            WorldChrMan = CreateBasePointer(BasePointerFromSetupPointer(WorldChrManSetup));
            FlagByte0 = CreateChildPointer(WorldChrMan, 0x10EF8, 0x0, 0x190, 0x0);

            GetParams();
        }
        private void GetParams()
        {
            foreach (var category in ERItemCategory.All)
            {
                GetWeaponProperties(category);
            }

            foreach (var gem in ERGem.Gems)
            {
                GetGemProperties(gem);
                GetWeapons(gem);
            }
        }
        private void GetWeaponProperties(ERItemCategory category)
        {
            foreach (var weapon in category.Weapons)
            {
                if (!EquipParamWeaponOffsetDict.ContainsKey(weapon.ID))
                {
                    Debug.WriteLine($"{weapon.ID} {weapon.Name}");
                    continue;
                }
                weapon.Unique = BitConverter.ToInt32(EquipParamWeaponBytes, (int)EquipParamWeaponOffsetDict[weapon.ID] + (int)EROffsets.EquipParamWeapon.MaterialSetID) == 2200;

                if (weapon.Unique)
                    weapon.Infusible = false;

                weapon.SortID = BitConverter.ToUInt32(EquipParamWeaponBytes, (int)EquipParamWeaponOffsetDict[weapon.ID] + (int)EROffsets.EquipParamWeapon.SortID);
                Array.Copy(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[weapon.ID] + (int)EROffsets.EquipParamWeapon.OriginEquipWep, weapon.OriginEquipWep, 0x0, weapon.OriginEquipWep.Length);
                weapon.IconID = BitConverter.ToInt16(EquipParamWeaponBytes, (int)EquipParamWeaponOffsetDict[weapon.ID] + (int)EROffsets.EquipParamWeapon.IconID);
                weapon.SwordArtId = BitConverter.ToUInt32(EquipParamWeaponBytes, (int)EquipParamWeaponOffsetDict[weapon.ID] + (int)EROffsets.EquipParamWeapon.SwordArtsParamId);
                weapon.Type = (ERWeapon.WeaponType)BitConverter.ToUInt32(EquipParamWeaponBytes, (int)EquipParamWeaponOffsetDict[weapon.ID] + (int)EROffsets.EquipParamWeapon.WepType);
                Array.Copy(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[weapon.ID] + (int)EROffsets.EquipParamWeapon.OriginEquipWep16, weapon.OriginEquipWep16, 0x0, weapon.OriginEquipWep16.Length);
            }
        }
        private void GetGemProperties(ERGem gem)
        {
            gem.DefaultWeaponAttr = (byte)BitConverter.ToChar(EquipParamGemBytes, (int)EquipParamGemOffsetDict[gem.ID] + (int)EROffsets.EquipParamGem.Default_WepAttr);
            gem.SwordArtID = BitConverter.ToUInt32(EquipParamGemBytes, (int)EquipParamGemOffsetDict[gem.ID] + (int)EROffsets.EquipParamGem.SwordArtsParamId);
        }
        private void GetWeapons(ERGem gem)
        {
            gem.WeaponTypes = new List<WeaponType>();
            var bitField = BitConverter.ToInt64(EquipParamGemBytes, (int)EquipParamGemOffsetDict[gem.ID] + (int)EROffsets.EquipParamGem.CanMountWep_Dagger);
            if (bitField == 0)
                return;

            for (int i = 0; i < ERGem.Weapons.Count; i++)
            {
                if ((bitField & (1L << i)) != 0)
                    gem.WeaponTypes.Add(ERGem.Weapons[i]);
            }
        }

        public Dictionary<uint, uint> EquipParamWeaponOffsetDict { get; private set; }
        private byte[] EquipParamWeaponBytes { get; set; }
        public Dictionary<uint, uint> EquipParamGemOffsetDict { get; private set; }
        private byte[] EquipParamGemBytes { get; set; }
        private Dictionary<uint, uint> BuildOffsetDictionary(PHPointer pointer, string expectedParamName, ref byte[] paramBytes)
        {
            var dictionary = new Dictionary<uint, uint>();
            var nameOffset = pointer.ReadInt32((int)EROffsets.Param.NameOffset);
            var paramName = pointer.ReadString(nameOffset, Encoding.UTF8, 0x18);
            if (paramName != expectedParamName)
                throw new InvalidOperationException($"Incorrect Param Pointer: {paramName} should be {expectedParamName}");

            paramBytes = pointer.ReadBytes((int)EROffsets.Param.TotalParamLength, (uint)nameOffset);
            var tableLength = pointer.ReadInt32((int)EROffsets.Param.TableLength);
            var param = 0x40;
            var paramID = 0x0;
            var paramOffset = 0x8;
            var nextParam = 0x18;

            while (param < tableLength)
            {
                var itemID = pointer.ReadUInt32(param + paramID);
                var itemParamOffset = pointer.ReadUInt32(param + paramOffset);
                dictionary.Add(itemID, itemParamOffset);

                param += nextParam;
            }

            return dictionary;
        }

        public void RestoreParams()
        {
            if (!Hooked)
                return;

            EquipParamWeapon.WriteBytes((int)EROffsets.Param.TotalParamLength, EquipParamWeaponBytes);
        }
        public IntPtr BasePointerFromSetupPointer(PHPointer pointer)
        {
            var readInt = pointer.ReadInt32(EROffsets.BasePtrOffset1);
            return pointer.ReadIntPtr(readInt + EROffsets.BasePtrOffset2);
        }
        public int Level => PlayerGameData.ReadInt32((int)EROffsets.Player.Level);
        public string LevelString => PlayerGameData?.ReadInt32((int)EROffsets.Player.Level).ToString() ?? "";
        public bool NoFPConsumption
        {
            get => Util.GetBit(FlagByte0.ReadByte(0x19B),2);
            set
            {
                if (!Loaded)
                    return;
                FlagByte0.WriteByte(0x19B, Util.SetBit(FlagByte0.ReadByte(0x19B), 2, value));
            }
        }
        public byte ArmStyle
        {
            get => PlayerGameData.ReadByte((int)EROffsets.Weapons.ArmStyle);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteByte((int)EROffsets.Weapons.ArmStyle, value);
            }
        }
        public uint CurrWepSlotOffsetLeft
        {
            get => PlayerGameData.ReadUInt32((int)EROffsets.Weapons.CurrWepSlotOffsetLeft);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.CurrWepSlotOffsetLeft, value);
            }
        }
        public uint CurrWepSlotOffsetRight
        {
            get => PlayerGameData.ReadUInt32((int)EROffsets.Weapons.CurrWepSlotOffsetRight);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.CurrWepSlotOffsetRight, value);
            }
        }
        public uint RHandWeapon1
        {
            get => PlayerGameData.ReadUInt32((int)EROffsets.Weapons.RHandWeapon1);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.RHandWeapon1, value);
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.RHandWeapon1_, value);
            }
        }
        public uint RHandWeapon2
        {
            get => PlayerGameData.ReadUInt32((int)EROffsets.Weapons.RHandWeapon2);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.RHandWeapon2, value);
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.RHandWeapon2_, value);
            }
        }
        public uint RHandWeapon3
        {
            get => PlayerGameData.ReadUInt32((int)EROffsets.Weapons.RHandWeapon3);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.RHandWeapon3, value);
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.RHandWeapon3_, value);
            }
        }
        public uint LHandWeapon1
        {
            get => PlayerGameData.ReadUInt32((int)EROffsets.Weapons.LHandWeapon1);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.LHandWeapon1, value);
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.LHandWeapon1_, value);
            }
        }
        public uint LHandWeapon2
        {
            get => PlayerGameData.ReadUInt32((int)EROffsets.Weapons.LHandWeapon2);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.LHandWeapon2, value);
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.LHandWeapon2_, value);
            }
        }
        public uint LHandWeapon3
        {
            get => PlayerGameData.ReadUInt32((int)EROffsets.Weapons.LHandWeapon3);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.LHandWeapon3, value);
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.LHandWeapon3_, value);
            }
        }
        public uint Arrow1
        {
            get => PlayerGameData.ReadUInt32((int)EROffsets.Weapons.Arrow1);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.Arrow1, value);
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.Arrow1_, value);
            }
        }
        public uint Arrow2
        {
            get => PlayerGameData.ReadUInt32((int)EROffsets.Weapons.Arrow2);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.Arrow2, value);
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.Arrow2_, value);
            }
        }
        public uint Bolt1
        {
            get => PlayerGameData.ReadUInt32((int)EROffsets.Weapons.Bolt1);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.Bolt1, value);
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.Bolt1_, value);
            }
        }
        public uint Bolt2
        {
            get => PlayerGameData.ReadUInt32((int)EROffsets.Weapons.Bolt2);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.Bolt2, value);
                PlayerGameData.WriteUInt32((int)EROffsets.Weapons.Bolt2_, value);
            }
        }

        private uint OGRHandWeapon1 { get; set; }
        private PHPointer OGRHandWeapon1Param 
        { 
            get => CreateBasePointer(EquipParamWeapon.Resolve() + (int)EquipParamWeaponOffsetDict[OGRHandWeapon1]);
        }

        private uint OGRHandWeapon2 { get; set; }
        private PHPointer OGRHandWeapon2Param
        {
            get => CreateBasePointer(EquipParamWeapon.Resolve() + (int)EquipParamWeaponOffsetDict[OGRHandWeapon2]);
        }

        private uint OGRHandWeapon3 { get; set; }
        private PHPointer OGRHandWeapon3Param
        {
            get => CreateBasePointer(EquipParamWeapon.Resolve() + (int)EquipParamWeaponOffsetDict[OGRHandWeapon3]);
        }

        public bool Randomize 
        { set 
            {
                RHandRandom = value;
                LHandRandom = value;

                if (!value) {
                    NoFPConsumption = value;
                    RestoreParams();
                    //AssignBlots(true);
                    AssignMagic(true);
                }
            } 
        }

        List<uint> UsedWeapons = new List<uint>();

        static Random RRand = new Random((int)DateTime.UtcNow.Ticks);
        static Random LRand = new Random(RRand.Next());
        public bool LevelRestrict { get; set; }
        public bool RandomizeAsh { get; set; }

        Timer RHandTimer = new Timer();
        public int RHandTime { get; set; } = 60;
        private bool _rHandRandom;
        public bool RHand1 { get; set; } = true;
        public bool RHand2 { get; set; } = false;
        public bool RHand3 { get; set; } = false;
        public bool RHandRandom
        {
            get => _rHandRandom;
            set
            {
                _rHandRandom = value;
                if (_rHandRandom)
                {
                    RRand = new Random(RRand.Next());
                    OGRHandWeapon1 = Util.DeleteFromEnd(RHandWeapon1, 2) * 100; //remove the levels from the weapon
                    OGRHandWeapon2 = Util.DeleteFromEnd(RHandWeapon2, 2) * 100; //remove the levels from the weapon
                    OGRHandWeapon3 = Util.DeleteFromEnd(RHandWeapon3, 2) * 100; //remove the levels from the weapon
                    RandomizeRightHand();
                    RHandTimer.Start();
                }
                else
                {
                    RHandTimer.Stop();
                    RHandWeapon1 = OGRHandWeapon1;
                    RHandWeapon2 = OGRHandWeapon2;
                    RHandWeapon3 = OGRHandWeapon3;
                }
            }
        }

        private void RHandTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RandomizeRightHand();
        }

        private void RandomizeRightHand()
        {
            if (RHand1)
            {
                ERWeapon weapon = GetWeapon(RRand);
                RHandWeapon1 = weapon.ID;
                AssignWeapon(OGRHandWeapon1Param, weapon);
            }

            if (RHand2)
            {
                ERWeapon weapon = GetWeapon(RRand);
                RHandWeapon2 = weapon.ID;
                AssignWeapon(OGRHandWeapon2Param, weapon);
            }

            if (RHand3)
            {
                ERWeapon weapon = GetWeapon(RRand);
                RHandWeapon3 = weapon.ID;
                AssignWeapon(OGRHandWeapon3Param, weapon);
            }

            if (RHand1 || RHand2 || RHand3)
            {
                //AssignBlots(false);
                AssignMagic(false);
                NoFPConsumption = true;
            }

            RHandTimer.Interval = RHandTime * 1000;
        }
        private void AssignBlots(bool clear) {
            if (clear) {
                Arrow1 = 0xFF_FF_FF_FF;
                Arrow2 = 0xFF_FF_FF_FF;
                Bolt1 = 0xFF_FF_FF_FF;
                Bolt2 = 0xFF_FF_FF_FF;
                return;
            }
            Arrow1 = ERItemCategory.Arrows.Weapons[RRand.Next(ERItemCategory.Arrows.Weapons.Count)].ID;
            Arrow2 = ERItemCategory.GreatArrows.Weapons[RRand.Next(ERItemCategory.GreatArrows.Weapons.Count)].ID;
            Bolt1 = ERItemCategory.Bolts.Weapons[RRand.Next(ERItemCategory.Bolts.Weapons.Count)].ID;
            Bolt2 = ERItemCategory.Bolts.Weapons[RRand.Next(ERItemCategory.Bolts.Weapons.Count)].ID;
        }
        private void AssignMagic(bool clear)
        {
            if (clear) {
                for (int i = 0; i < 14; i++)
                {
                    EquipMagicData.WriteUInt32((int)EROffsets.Magic.Slot0 + (0x8 * i), 0xFF_FF_FF_FF);
                }
                return;
            }
            List<ERWeapon> Incantations = ERItemCategory.Incantations.Weapons;
            List<ERWeapon> Sorceries = ERItemCategory.Sorceries.Weapons;
            for (int i = 0; i < 14; i++) {
                EquipMagicData.WriteUInt32((int)EROffsets.Magic.Slot0 + (0x8 * i), (i % 2 == 0) ? Incantations[RRand.Next(Incantations.Count)].ID : Sorceries[RRand.Next(Sorceries.Count)].ID);
            }
            
        }
        private void AssignWeapon(PHPointer weaponPointer, ERWeapon weapon)
        {
            weaponPointer.WriteUInt32((int)EROffsets.EquipParamWeapon.SwordArtsParamId, weapon.SwordArtId);
            weaponPointer.WriteInt16((int)EROffsets.EquipParamWeapon.IconID, weapon.IconID);
        }


        private uint OGLHandWeapon1 { get; set; }
        private PHPointer OGLHandWeapon1Param
        {
            get => CreateBasePointer(EquipParamWeapon.Resolve() + (int)EquipParamWeaponOffsetDict[OGLHandWeapon1]);
        }
        private uint OGLHandWeapon2 { get; set; }
        private PHPointer OGLHandWeapon2Param
        {
            get => CreateBasePointer(EquipParamWeapon.Resolve() + (int)EquipParamWeaponOffsetDict[OGLHandWeapon2]);
        }
        private uint OGLHandWeapon3 { get; set; }
        private PHPointer OGLHandWeapon3Param
        {
            get => CreateBasePointer(EquipParamWeapon.Resolve() + (int)EquipParamWeaponOffsetDict[OGLHandWeapon3]);
        }

        Timer LHandTimer = new Timer();
        public int LHandTime { get; set; } = 60;
        public bool LHand1 { get; set; } = true;
        public bool LHand2 { get; set; } = false;
        public bool LHand3 { get; set; } = false;
        private bool _lHandRandom;
        public bool LHandRandom
        {
            get => _lHandRandom;
            set
            {
                _lHandRandom = value;
                if (_lHandRandom)
                {
                    LRand = new Random(LRand.Next());
                    OGLHandWeapon1 = Util.DeleteFromEnd(LHandWeapon1, 2) * 100; //remove the levels from the weapon
                    OGLHandWeapon2 = Util.DeleteFromEnd(LHandWeapon2, 2) * 100; //remove the levels from the weapon
                    OGLHandWeapon3 = Util.DeleteFromEnd(LHandWeapon3, 2) * 100; //remove the levels from the weapon
                    RandomizeLeftHand();
                    LHandTimer.Start();
                }
                else
                {
                    LHandTimer.Stop();
                    LHandWeapon1 = OGLHandWeapon1;
                    LHandWeapon2 = OGLHandWeapon2;
                    LHandWeapon3 = OGLHandWeapon3;
                }
            }
        }

        private void LHandTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RandomizeLeftHand();
        }

        private void RandomizeLeftHand()
        {

            if (LHand1)
            {
                ERWeapon weapon = GetWeapon(LRand);
                LHandWeapon1 = weapon.ID;
                AssignWeapon(OGLHandWeapon1Param, weapon);
            }
            if (LHand2)
            {
                ERWeapon weapon = GetWeapon(LRand);
                LHandWeapon2 = weapon.ID;
                AssignWeapon(OGLHandWeapon2Param, weapon);
            }
            if (LHand3)
            {
                ERWeapon weapon = GetWeapon(LRand);
                LHandWeapon3 = weapon.ID;
                AssignWeapon(OGLHandWeapon3Param, weapon);
            }
            if (LHand1 || LHand2 || LHand3)
            {
                //AssignBlots(false);
                AssignMagic(false);
                NoFPConsumption = true;
            }

            LHandTimer.Interval = LHandTime * 1000;
        }

        private ERWeapon GetWeapon(Random rand)
        {

            ERWeapon newWeapon = new ERWeapon();
            List<ERWeapon> ERWeapons = ERItemCategory.All.SelectMany(x => x.Weapons).ToList();

            var tempWep = ERWeapons.Where(x => !UsedWeapons.Contains(x.RealID)).ToList();
            if(tempWep.Count == 0)
            {
                UsedWeapons.Clear();
                Debug.WriteLine("All weapons used, resetting");
            } else {
               ERWeapons = tempWep;
            }

            Debug.WriteLine($"Weapons left: {ERWeapons.Count} || Weapons Used {UsedWeapons.Count}");

            ERWeapon weapon = ERWeapons[rand.Next(ERWeapons.Count)];

            newWeapon.Clone(weapon);

            var id = newWeapon.ID;

            ERGem? ash;
            var gems = ERGem.Gems.Where(x => x.WeaponTypes.Contains(newWeapon.Type)).ToList();
            if (RandomizeAsh && !newWeapon.Unique && gems.Count > 0)
            {
                ash = gems[rand.Next(gems.Count)];
            }
            else
            {
                ash = ERGem.Gems.FirstOrDefault(x => x.SwordArtID == newWeapon.SwordArtId);
            }

            newWeapon.SwordArtId = ash?.SwordArtID ?? newWeapon.SwordArtId;

            //uint infusion = 0;
            //if (newWeapon.Infusible && ash != null)
            //    infusion = (uint)ash?.Infusions[rand.Next(ash.Infusions.Count)];
            //id += infusion;

            int maxLevel = newWeapon.Unique ? 10 : 25;
            uint upgradeLevel = 0;
            if (LevelRestrict)
                upgradeLevel = (uint)GetLevel(maxLevel);
            else
                upgradeLevel = (uint)maxLevel;

            id += upgradeLevel;

            newWeapon.ID = id;

            UsedWeapons.Add(newWeapon.RealID);

            if(UsedWeapons.Count > 10)
            {
                UsedWeapons.RemoveAt(0);
            }


            Debug.WriteLine($"Weapon: {newWeapon.Name} || ID: {newWeapon.RealID} - {newWeapon.ID} : {upgradeLevel} : Is Unique: {newWeapon.Unique}");

            return newWeapon;
        }

        public int MaxLevel { get; set; } = 80;

        public int GetLevel(int maxLevel)
        {
            if (maxLevel == 1 || Level >= MaxLevel)
                return maxLevel - 1;

            var levels = (float)MaxLevel / (maxLevel - 1);

            return (int)Math.Floor(Level / levels);
        }
    }
}
