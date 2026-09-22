using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace StopFarmingItsEnough
{
    /// <summary>读取 UFPC 自定义收获副产物；只在目录初始化时反射，不向派工或 tick 热路径添加操作。</summary>
    internal static class UfpcHarvestProducts
    {
        /// <summary>从已解析的 UFPC 扩展提取可能产出的资源，避免首次收获前缺少规则；未启用时直接返回。</summary>
        internal static Dictionary<ThingDef, List<ThingDef>> Discover()
        {
            var result = new Dictionary<ThingDef, List<ThingDef>>();
            System.Type extensionType = AccessTools.TypeByName("UFPC.PlantWithProductsExtension");
            if (extensionType == null) return result;
            System.Type itemType = AccessTools.TypeByName("UFPC.PlantWithProductsProperties");
            FieldInfo items = AccessTools.Field(extensionType, "plantWithProducts");
            FieldInfo product = itemType == null ? null : AccessTools.Field(itemType, "productDef");
            FieldInfo maximum = itemType == null ? null : AccessTools.Field(itemType, "productMax");
            // 检查已知 API，避免上游改名或改变字段类型后导致整个作物目录初始化失败。
            if (items == null || !typeof(IEnumerable).IsAssignableFrom(items.FieldType)
                || product == null || product.FieldType != typeof(ThingDef)
                || maximum == null || maximum.FieldType != typeof(int))
            {
                Log.Warning("[Stop farming, it's enough!] UFPC product API changed; use manual product links for its additional yields.");
                return result;
            }
            int count = 0;
            foreach (ThingDef plant in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (plant.plant?.sowTags == null || plant.plant.sowTags.Count == 0 || plant.modExtensions == null) continue;
                foreach (DefModExtension extension in plant.modExtensions)
                {
                    if (!extensionType.IsInstanceOfType(extension)) continue;
                    IEnumerable values = items.GetValue(extension) as IEnumerable;
                    if (values == null) continue;
                    foreach (object value in values)
                    {
                        if (value == null || !itemType.IsInstanceOfType(value) || (int)maximum.GetValue(value) <= 0) continue;
                        ThingDef resource = product.GetValue(value) as ThingDef;
                        if (resource == null) continue;
                        List<ThingDef> products;
                        if (!result.TryGetValue(plant, out products)) result.Add(plant, products = new List<ThingDef>());
                        if (products.Contains(resource)) continue;
                        products.Add(resource);
                        count++;
                    }
                }
            }
            Log.Message("[Stop farming, it's enough!] UFPC compatibility: discovered " + count + " additional crop/product links.");
            return result;
        }
    }
}
