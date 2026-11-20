using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace StopFarmingItsEnough
{
    /// <summary>模组设置内的新游戏模板编辑器；独立建立定义列表，不切换或修改运行中存档的目录。</summary>
    internal sealed class NewGameDefaultsEditor
    {
        private CropRuleSettings owner;
        private readonly List<CropEntry> entries = new List<CropEntry>();
        private readonly List<CropEntry> filtered = new List<CropEntry>();
        private string search = "", previousSearch;
        private Vector2 scroll;

        /// <summary>只在首次显示或模板容器更换时建立列表；读取定义，不读取地图或库存。</summary>
        private void Build(CropRuleSettings settings)
        {
            owner = settings;
            owner.Reindex();
            entries.Clear();
            var pairs = new HashSet<string>();
            var ufpc = UfpcHarvestProducts.Discover();
            foreach (ThingDef plant in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (plant.plant?.sowTags == null || plant.plant.sowTags.Count == 0) continue;
                Add(plant, plant.plant.harvestedThingDef, pairs);
                var extension = plant.GetModExtension<HarvestProductsExtension>();
                if (extension?.products != null)
                    foreach (ThingDef product in extension.products) Add(plant, product, pairs);
                List<ThingDef> additional;
                if (ufpc.TryGetValue(plant, out additional))
                    foreach (ThingDef product in additional) Add(plant, product, pairs);
            }
            // 已记录的副产物以及当前游戏发现的副产物只提供定义；绝不引用其 CropRule。
            foreach (CropRule rule in owner.Rules.ToArray())
                Add(DefDatabase<ThingDef>.GetNamedSilentFail(rule.PlantDefName),
                    DefDatabase<ThingDef>.GetNamedSilentFail(rule.ProductDefName ?? ""), pairs);
            foreach (CropEntry entry in CropCatalog.Entries) Add(entry.Plant, entry.Product, pairs);
            entries.Sort((a, b) => string.Compare(a.Plant.label + a.Product.label,
                b.Plant.label + b.Product.label, StringComparison.CurrentCulture));
            previousSearch = null;
        }

        /// <summary>创建模板自己的规则行；相同产物来自不同植物时保持独立配置。</summary>
        private void Add(ThingDef plant, ThingDef product, HashSet<string> pairs)
        {
            if (plant?.plant == null || product == null || !pairs.Add(FarmingSettings.Key(plant.defName, product.defName))) return;
            CropRule rule = owner.GetOrCreate(plant.defName, product.defName, product == plant.plant.harvestedThingDef);
            entries.Add(new CropEntry { Plant = plant, Product = product, Rule = rule, Supported = product.CountAsResource });
        }

        /// <summary>在默认阈值顶部显示固定说明，并绘制可搜索的虚拟列表；修改仅进入全局模板。</summary>
        internal void Draw(Rect rect)
        {
            if (owner != FarmingMod.Settings.NewGameDefaults) Build(FarmingMod.Settings.NewGameDefaults);
            string notice = "SFIE_DefaultsNotice".Translate();
            float height = Mathf.Max(48f, Text.CalcHeight(notice, rect.width));
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, height), notice);
            float y = rect.y + height + 6f;
            search = Widgets.TextField(new Rect(rect.x, y, rect.width, 28f), search);
            y += 34f;
            if (search != previousSearch)
            {
                filtered.Clear();
                foreach (CropEntry entry in entries)
                    if (string.IsNullOrEmpty(search) || (entry.Plant.label + entry.Plant.defName + entry.Product.label + entry.Product.defName)
                        .IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0) filtered.Add(entry);
                previousSearch = search;
                scroll = Vector2.zero;
            }
            Window_CropRules.DrawHeader(new Rect(rect.x, y, rect.width - 20f, 28f));
            y += 30f;
            Rect outer = new Rect(rect.x, y, rect.width, Mathf.Max(1f, rect.yMax - y));
            Rect view = new Rect(0f, 0f, rect.width - 20f, filtered.Count * 80f);
            Widgets.BeginScrollView(outer, ref scroll, view);
            int first = Mathf.Max(0, Mathf.FloorToInt(scroll.y / 80f));
            int last = Mathf.Min(filtered.Count, Mathf.CeilToInt((scroll.y + outer.height) / 80f) + 1);
            for (int i = first; i < last; i++) Window_CropRules.DrawRow(new Rect(0f, i * 80f, view.width, 80f), filtered[i], i, owner);
            Widgets.EndScrollView();
        }
    }
}
