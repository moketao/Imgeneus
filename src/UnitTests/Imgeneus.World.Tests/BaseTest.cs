﻿using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.DatabaseBackgroundService;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Chat;
using Imgeneus.World.Game.Dyeing;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.MapConfig;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;

namespace Imgeneus.World.Tests
{
    public abstract class BaseTest
    {
        protected Mock<ILogger<Character>> loggerMock = new Mock<ILogger<Character>>();
        protected Mock<IGameWorld> gameWorldMock = new Mock<IGameWorld>();
        protected Mock<IBackgroundTaskQueue> taskQueuMock = new Mock<IBackgroundTaskQueue>();
        protected Mock<IDatabasePreloader> databasePreloader = new Mock<IDatabasePreloader>();
        protected Mock<ICharacterConfiguration> config = new Mock<ICharacterConfiguration>();
        protected Mock<ILogger<Map>> mapLoggerMock = new Mock<ILogger<Map>>();
        protected Mock<ILogger<Mob>> mobLoggerMock = new Mock<ILogger<Mob>>();
        protected Mock<IChatManager> chatMock = new Mock<IChatManager>();
        protected Mock<ILinkingManager> linkingMock = new Mock<ILinkingManager>();
        protected Mock<IDyeingManager> dyeingMock = new Mock<IDyeingManager>();
        protected Mock<IWorldClient> worldClientMock = new Mock<IWorldClient>();

        protected Map testMap => new Map(
                    Map.TEST_MAP_ID,
                    new MapDefinition(),
                    new MapConfiguration() { Size = 100, CellSize = 100 },
                    mapLoggerMock.Object,
                    databasePreloader.Object);

        public BaseTest()
        {
            config.Setup((conf) => conf.GetConfig(It.IsAny<int>()))
                  .Returns(new Character_HP_SP_MP() { HP = 100, MP = 200, SP = 300 });

            config.Setup((conf) => conf.DefaultStats)
                  .Returns(new DefaultStat[1] {
                      new DefaultStat()
                      {
                          Job = CharacterProfession.Fighter,
                          Str = 12,
                          Dex = 11,
                          Rec = 10,
                          Int = 8,
                          Wis = 9,
                          Luc = 10,
                          MainStat = 0
                      }
                  });

            databasePreloader
                .SetupGet((preloader) => preloader.Mobs)
                .Returns(new Dictionary<ushort, DbMob>()
                {
                    { 1, Wolf },
                    { 3041, CrypticImmortal }
                });

            databasePreloader
                .SetupGet((preloader) => preloader.Skills)
                .Returns(new Dictionary<(ushort SkillId, byte SkillLevel), DbSkill>()
                {
                    { (1, 1) , StrengthTraining },
                    { (14, 1), ManaTraining },
                    { (15, 1), SharpenWeaponMastery_Lvl1 },
                    { (15, 2), SharpenWeaponMastery_Lvl2 },
                    { (35, 1), MagicRoots_Lvl1 },
                    { (273, 100), AttributeRemove },
                    { (735, 1), EarthWeapon },
                    { (765, 1), EarthSkin },
                    { (672, 1), Panic_Lvl1 },
                    { (787, 1), Dispel }
                });
            databasePreloader
                .SetupGet((preloader) => preloader.Items)
                .Returns(new Dictionary<(byte Type, byte TypeId), DbItem>()
                {
                    { (17, 2), WaterArmor },
                    { (2, 92), FireSword },
                    { (100, 192), PerfectLinkingHammer },
                    { (44, 237), PerfectExtractingHammer },
                    { (100, 139), LuckyCharm },
                    { (17, 59), JustiaArmor },
                    { (30, 1), Gem_Str_Level_1 },
                    { (30, 2), Gem_Str_Level_2 },
                    { (30, 3), Gem_Str_Level_3 },
                    { (30, 7), Gem_Str_Level_7 }

                });

            databasePreloader
                .SetupGet((preloader) => preloader.MobItems)
                .Returns(new Dictionary<(ushort MobId, byte ItemOrder), DbMobItems>());
        }

        #region Test mobs

        protected DbMob Wolf = new DbMob()
        {
            Id = 1,
            MobName = "Small Ruined Wolf",
            AI = MobAI.Combative,
            Level = 38,
            HP = 2765,
            Element = Element.Wind1,
            AttackSpecial3 = MobRespawnTime.TestEnv,
            NormalTime = 1
        };

        protected DbMob CrypticImmortal = new DbMob()
        {
            Id = 3041,
            MobName = "Cryptic the Immortal",
            AI = MobAI.CrypticImmortal,
            Level = 75,
            HP = 35350000,
            AttackOk1 = 1,
            Attack1 = 8822,
            AttackPlus1 = 3222,
            AttackRange1 = 5,
            AttackTime1 = 2500,
            NormalTime = 1,
            ChaseTime = 1
        };

        #endregion

        #region Skills

        protected DbSkill StrengthTraining = new DbSkill()
        {
            SkillId = 1,
            SkillLevel = 1,
            TypeDetail = TypeDetail.PassiveDefence,
            SkillName = "Strength Training Lv1",
            TypeAttack = TypeAttack.Passive,
            AbilityType1 = AbilityType.PhysicalAttackPower,
            AbilityValue1 = 18
        };

        protected DbSkill ManaTraining = new DbSkill()
        {
            SkillId = 14,
            SkillLevel = 1,
            TypeDetail = TypeDetail.PassiveDefence,
            SkillName = "Mana Training",
            TypeAttack = TypeAttack.Passive,
            AbilityType1 = AbilityType.MP,
            AbilityValue1 = 110
        };

        protected DbSkill SharpenWeaponMastery_Lvl1 = new DbSkill()
        {
            SkillId = 15,
            SkillLevel = 1,
            TypeDetail = TypeDetail.WeaponMastery,
            SkillName = "Sharpen Weapon Mastery Lvl 1",
            TypeAttack = TypeAttack.Passive,
            Weapon1 = 1,
            Weapon2 = 3,
            Weaponvalue = 1
        };

        protected DbSkill SharpenWeaponMastery_Lvl2 = new DbSkill()
        {
            SkillId = 15,
            SkillLevel = 2,
            TypeDetail = TypeDetail.WeaponMastery,
            SkillName = "Sharpen Weapon Mastery Lvl 2",
            TypeAttack = TypeAttack.Passive,
            Weapon1 = 1,
            Weapon2 = 3,
            Weaponvalue = 2
        };

        protected DbSkill MagicRoots_Lvl1 = new DbSkill()
        {
            SkillId = 35,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Immobilize,
            SkillName = "Magic Roots",
            DamageHP = 42,
            TypeAttack = TypeAttack.MagicAttack,
            ResetTime = 10,
            KeepTime = 5,
            DamageType = DamageType.PlusExtraDamage,
        };

        protected DbSkill AttributeRemove = new DbSkill()
        {
            SkillId = 273,
            SkillLevel = 100,
            TypeDetail = TypeDetail.RemoveAttribute,
            SkillName = "Attribute Remove",
            TypeAttack = TypeAttack.MagicAttack,
            DamageType = DamageType.FixedDamage
        };

        protected DbSkill EarthWeapon = new DbSkill()
        {
            SkillId = 735,
            SkillLevel = 1,
            SkillName = "Earth Weapon",
            TypeDetail = TypeDetail.ElementalAttack,
            Element = Element.Earth1
        };

        protected DbSkill EarthSkin = new DbSkill()
        {
            SkillId = 765,
            SkillLevel = 1,
            SkillName = "Earth Skin",
            TypeDetail = TypeDetail.ElementalProtection,
            Element = Element.Earth1
        };

        protected DbSkill Panic_Lvl1 = new DbSkill()
        {
            SkillId = 672,
            SkillLevel = 1,
            SkillName = "Panic",
            TypeDetail = TypeDetail.SubtractingDebuff,
            AbilityType1 = AbilityType.PhysicalDefense,
            AbilityValue1 = 119,
            TypeAttack = TypeAttack.MagicAttack,
        };

        protected DbSkill Dispel = new DbSkill()
        {
            SkillId = 787,
            SkillLevel = 1,
            SkillName = "Dispel",
            TypeDetail = TypeDetail.Dispel,
            TypeAttack = TypeAttack.MagicAttack,
        };

        #endregion

        #region Items

        protected DbItem WaterArmor = new DbItem()
        {
            Type = 17,
            TypeId = 2,
            ItemName = "Water armor",
            Element = Element.Water1
        };

        protected DbItem FireSword = new DbItem()
        {
            Type = 2,
            TypeId = 92,
            ItemName = "Thane Breaker of Fire",
            Element = Element.Fire1
        };

        protected DbItem PerfectLinkingHammer = new DbItem()
        {
            Type = 100,
            TypeId = 192,
            ItemName = "Perfect Linking Hammer",
            Special = SpecialEffect.PerfectLinkingHammer
        };

        protected DbItem PerfectExtractingHammer = new DbItem()
        {
            Type = 44,
            TypeId = 237,
            ItemName = "GM Extraction Hammer",
            Special = SpecialEffect.PerfectExtractionHammer
        };

        protected DbItem LuckyCharm = new DbItem()
        {
            Type = 100,
            TypeId = 139,
            ItemName = "Lucky Charm",
            Special = SpecialEffect.LuckyCharm
        };

        protected DbItem JustiaArmor = new DbItem()
        {
            Type = 17,
            TypeId = 59,
            ItemName = "Justia Armor",
            ConstStr = 30,
            ConstDex = 30,
            ConstRec = 30,
            ConstHP = 1800,
            ConstSP = 600,
            Slot = 6,
            Quality = 1200,
            Attackfighter = 1,
            Defensefighter = 1,
            ReqWis = 20
        };

        protected DbItem Gem_Str_Level_1 = new DbItem()
        {
            Type = 30,
            TypeId = 1,
            ConstStr = 3,
            ReqIg = 0 // always fail linking or extracting, unless hammer is used
        };

        protected DbItem Gem_Str_Level_2 = new DbItem()
        {
            Type = 30,
            TypeId = 2,
            ConstStr = 5,
            ReqIg = 255 // always sucess linking or extracting.
        };

        protected DbItem Gem_Str_Level_3 = new DbItem()
        {
            Type = 30,
            TypeId = 3,
            ConstStr = 7,
            ReqIg = 255 // always sucess linking or extracting.
        };

        protected DbItem Gem_Str_Level_7 = new DbItem()
        {
            Type = 30,
            TypeId = 7,
            ConstStr = 50,
            ReqVg = 1, // Will break item if linking/extracting fails
            ReqIg = 0 // always fail linking or extracting, unless hammer is used
        };

        #endregion
    }
}
