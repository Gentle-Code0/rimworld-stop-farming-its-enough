using System;
using System.Collections.Generic;
using Verse;

namespace StopFarmingItsEnough
{
    /// <summary>来源不明用于兼容旧设置，不能假定它是手工关联。</summary>
    public enum ProductLinkSource { Unknown, Automatic, Manual }

    /// <summary>单个作物＋产物的存档配置；同产物的不同作物互不覆盖。</summary>
    public sealed class CropRule : IExposable
    {
        public string PlantDefName;
        public string ProductDefName;
        public ProductLinkSource Source;
        public string StateId;

        /// <summary>手工关联每次新建都有独立状态键，删除后重建不会继承旧存档中的暂停记忆。</summary>
        public string StateKey => FarmingSettings.Key(PlantDefName, ProductDefName)
            + (string.IsNullOrEmpty(StateId) ? "" : ":" + StateId);
        public bool IgnoreSowing = true;
        public bool IgnoreHarvest = true;
        public int Lower = 500;
        public int Upper = 1500;
        // 输入缓冲只在设置界面使用，不序列化；保留输入中的暂时无效文本。
        [NonSerialized] public string LowerBuffer;
        [NonSerialized] public string UpperBuffer;

        /// <summary>保存植物键、忽略开关及阈值；读取设置时不依赖 DefDatabase 已完成加载。</summary>
        public void ExposeData()
        {
            Scribe_Values.Look(ref PlantDefName, "plantDefName");
            Scribe_Values.Look(ref ProductDefName, "productDefName");
            Scribe_Values.Look(ref Source, "source", ProductLinkSource.Unknown);
            Scribe_Values.Look(ref StateId, "stateId");
                        // 只读旧字段用于迁移；新字段显式写入，允许两个方向独立且不会被旧值覆盖。
            bool legacyIgnore = true;
            if (Scribe.mode == LoadSaveMode.LoadingVars)
                Scribe_Values.Look(ref legacyIgnore, "ignore", true);
            Scribe_Values.Look(ref IgnoreSowing, "ignoreSowing", legacyIgnore, forceSave: true);
            Scribe_Values.Look(ref IgnoreHarvest, "ignoreHarvest", legacyIgnore, forceSave: true);
            Scribe_Values.Look(ref Lower, "lower", 500);
            Scribe_Values.Look(ref Upper, "upper", 1500);
        }
    }

    /// <summary>游戏 Mod 设置；控制开关即时生效，阈值和新规则在下一个 600-tick 检查生效。</summary>
    public sealed class FarmingSettings : ModSettings
    {
        public bool Enabled = true;
        public bool ControlSowing = true;
        public bool ControlHarvest;
        public bool HideSowingIcon;
        public bool HideHarvestIcon;
        // 只用于旧存档升级；当前游戏编辑不写入迁移模板。
        public List<CropRule> LegacyRules = new List<CropRule>();
        public int Revision { get; private set; }
        /// <summary>保存全局开关和只读迁移模板，当前规则随游戏保存。</summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref Enabled, "enabled", true);
            Scribe_Values.Look(ref ControlSowing, "controlSowing", true);
            Scribe_Values.Look(ref ControlHarvest, "controlHarvest", false);
            Scribe_Values.Look(ref HideSowingIcon, "hideSowingIcon", false);
            Scribe_Values.Look(ref HideHarvestIcon, "hideHarvestIcon", false);
            Scribe_Collections.Look(ref LegacyRules, "rules", LookMode.Deep);
            if (LegacyRules == null) LegacyRules = new List<CropRule>();
        }
        /// <summary>保持原有存档键格式。</summary>
        public static string Key(string plant, string product) => CropRuleSettings.Key(plant, product);
        /// <summary>通知地图在下一检查点更新全局配置。</summary>
        public void Changed() { unchecked { Revision++; } }
    }
}
