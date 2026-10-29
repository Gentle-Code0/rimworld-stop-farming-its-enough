using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace StopFarmingItsEnough
{
    /// <summary>模组入口与设置界面；采用原版 GUI，不需要额外设置框架。</summary>
    public sealed class FarmingMod : Mod
    {
        public const string ModTitle = "Stop farming, it's enough!";
        public const string HarmonyId = "GentleCode.StopFarmingItsEnough";
        public static FarmingSettings Settings;
        /// <summary>读取全局设置并延迟安装补丁；逐作物目录在游戏初始化时建立。</summary>
        public FarmingMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<FarmingSettings>();
            LongEventHandler.ExecuteWhenFinished(Initialize);
        }

        /// <summary>安装原版与可选兼容补丁；只运行一次，不修改其他模组的文件或存档字段。</summary>
        private static void Initialize()
        {
            var harmony = new Harmony(HarmonyId);
            harmony.PatchAll(typeof(FarmingMod).Assembly);
            OptionalCompatibility.Install(harmony);
            Log.Message("[Stop farming, it's enough!] Initialized; crop/product rules will be bound to each game save.");
        }

        /// <summary>返回 Mod 设置列表中显示的名称。</summary>
        public override string SettingsCategory()
        {
            return ModTitle;
        }

        /// <summary>仅绘制全局开关和独立窗口说明。</summary>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            GameFont oldFont = Text.Font;
            Text.Font = GameFont.Small;
            float y = inRect.y;
            DrawToggle(inRect, ref y, "SFIE_Enabled", ref Settings.Enabled);
            DrawToggle(inRect, ref y, "SFIE_ControlSowing", ref Settings.ControlSowing);
            DrawToggle(inRect, ref y, "SFIE_ControlHarvest", ref Settings.ControlHarvest);
            float toggleY = y;
            DrawToggle(new Rect(inRect.x, y, inRect.width * .49f, 28f), ref toggleY,
                "SFIE_HideSowingIcon", ref Settings.HideSowingIcon);
            DrawToggle(new Rect(inRect.x + inRect.width * .51f, y, inRect.width * .49f, 28f), ref y,
                "SFIE_HideHarvestIcon", ref Settings.HideHarvestIcon);
            string explanation = "SFIE_Explanation".Translate();
            float explanationHeight = Mathf.Max(54f, Text.CalcHeight(explanation, inRect.width));
            Widgets.Label(new Rect(inRect.x, y, inRect.width, explanationHeight), explanation);
            y += explanationHeight + 4f;
            y += 32f;
            Widgets.Label(new Rect(inRect.x, y, inRect.width, 90f), "SFIE_RulesLocation".Translate());
            Text.Font = oldFont;
        }

        /// <summary>绘制一个设置开关；改变时更新配置版本，不在 GUI 帧内检查库存。</summary>
        private static void DrawToggle(Rect area, ref float y, string key, ref bool value)
        {
            bool before = value;
            Rect row = new Rect(area.x, y, area.width, 28f);
            Widgets.CheckboxLabeled(row, key.Translate(), ref value);
            TooltipHandler.TipRegion(row, (key + "Tip").Translate());
            if (before != value) Settings.Changed();
            y += 30f;
        }

    }
}
