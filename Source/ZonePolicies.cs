using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace StopFarmingItsEnough
{
    /// <summary>单个种植区的手动限制，默认全部关闭，独立于库存总开关。</summary>
    public sealed class ZonePolicy : IExposable
    {
        public bool NoSow, NoHarvest, MatureOnly;
        /// <summary>随地图保存三个布尔值，旧档缺失字段时默认关闭。</summary>
        public void ExposeData()
        {
            Scribe_Values.Look(ref NoSow, "noSow");
            Scribe_Values.Look(ref NoHarvest, "noHarvest");
            Scribe_Values.Look(ref MatureOnly, "matureOnly");
        }
    }
    /// <summary>常数时间按区域引用查找，只有玩家操作过的区域分配状态；不使用 tick 轮询。</summary>
    public sealed class MapComponent_ZonePolicies : MapComponent
    {
        private static readonly ConditionalWeakTable<Map, MapComponent_ZonePolicies> cache = new ConditionalWeakTable<Map, MapComponent_ZonePolicies>();
        private Dictionary<Zone_Growing, ZonePolicy> policies = new Dictionary<Zone_Growing, ZonePolicy>();
        private List<Zone_Growing> keys;
        private List<ZonePolicy> values;
        /// <summary>建立弱引用索引，不阻止已卸载地图被回收。</summary>
        public MapComponent_ZonePolicies(Map map) : base(map) { cache.Remove(map); cache.Add(map, this); }
        /// <summary>查询区域状态；只有按钮创建状态时传 create=true。</summary>
        public static ZonePolicy For(Zone_Growing zone, bool create = false)
        {
            MapComponent_ZonePolicies component;
            if (zone?.Map == null || !cache.TryGetValue(zone.Map, out component)) return null;
            ZonePolicy policy;
            if (!component.policies.TryGetValue(zone, out policy) && create)
                component.policies.Add(zone, policy = new ZonePolicy());
            return policy;
        }
        /// <summary>只查询目标格的区域；不遍历区域格子或植物。</summary>
        public static ZonePolicy At(Map map, IntVec3 cell) => map == null || !cell.InBounds(map) ? null : For(cell.GetZone(map) as Zone_Growing);
        /// <summary>按区域引用序列化，读档不会依赖区域名称或当前选择植物。</summary>
        public override void ExposeData()
        {
            Scribe_Collections.Look(ref policies, "sfieZonePolicies", LookMode.Reference, LookMode.Deep, ref keys, ref values);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && policies == null) policies = new Dictionary<Zone_Growing, ZonePolicy>();
        }
        /// <summary>删除区域时移除状态，避免失效引用累积。</summary>
        public static void Forget(Zone_Growing zone)
        {
            MapComponent_ZonePolicies component;
            if (zone?.Map != null && cache.TryGetValue(zone.Map, out component)) component.policies.Remove(zone);
        }
        /// <summary>仅在提供自定义状态贴图时绘制；每区域只取首格作为锚点，无中心计算或地块扫描。</summary>
        public override void MapComponentOnGUI()
        {
            if (map != Find.CurrentMap || !RimWorld.Planet.WorldRendererUtility.DrawingMap || !ZonePolicyIcons.HasOverlays) return;
            foreach (var pair in policies)
            {
                Zone_Growing zone = pair.Key;
                if (zone == null || zone.Map != map || zone.Cells.Count == 0) continue;
                IntVec3 cell = zone.Cells[0];
                if (!Find.CameraDriver.CurrentViewRect.Contains(cell) || cell.Fogged(map)) continue;
                Vector2 position = GenMapUI.LabelDrawPosFor(cell);
                position.y += 34f; // 与原库存暂停标记分行，三个手动状态横向并排。
                int slot = 0;
                for (int i = 0; i < 3; i++)
                    if (ZonePolicyIcons.Active(pair.Value, i) && ZonePolicyIcons.Overlays[i] != null)
                        GUI.DrawTexture(new Rect(position.x + slot++ * 32f, position.y, 28f, 28f), ZonePolicyIcons.Overlays[i]);
            }
        }
    }
    /// <summary>贴图只在启动读取一次；未提供自定义按钮图标时使用原版绿勾／红叉。</summary>
    [StaticConstructorOnStartup]
    internal static class ZonePolicyIcons
    {
        internal static readonly string[] Names = { "NoSow", "NoHarvest", "MatureOnly" };
        internal static readonly Texture2D[] Buttons = new Texture2D[3], Overlays = new Texture2D[3];
        internal static bool HasOverlays;
        static ZonePolicyIcons()
        {
            for (int i = 0; i < 3; i++)
            {
                Buttons[i] = ContentFinder<Texture2D>.Get("UI/ZonePolicies/Buttons/" + Names[i], false);
                Overlays[i] = ContentFinder<Texture2D>.Get("UI/ZonePolicies/Overlays/" + Names[i], false);
                HasOverlays |= Overlays[i] != null;
            }
        }
        /// <summary>返回按钮的当前启用状态。</summary>
        internal static bool Active(ZonePolicy p, int index) => index == 0 ? p.NoSow : index == 1 ? p.NoHarvest : p.MatureOnly;
        /// <summary>只对选中的种植区生成三个标准切换按钮。</summary>
        internal static IEnumerable<Gizmo> Commands(Zone_Growing zone)
        {
            var p = MapComponent_ZonePolicies.For(zone, true);
            if (p == null) yield break;
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                yield return new Command_Toggle {
                    defaultLabel = ("SFIE_Zone" + Names[i]).Translate(),
                    defaultDesc = ("SFIE_Zone" + Names[i] + "Tip").Translate(),
                    icon = Buttons[i] ?? (Active(p, i) ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex),
                    isActive = () => Active(p, index),
                    toggleAction = () => { if (index == 0) p.NoSow = !p.NoSow; else if (index == 1) p.NoHarvest = !p.NoHarvest; else p.MatureOnly = !p.MatureOnly; }
                };
            }
        }
    }
    /// <summary>目标级割除保护；原版可播种植物视为作物，不可播种的野生植物视为杂草。</summary>
    public static class ZonePolicyGate
    {
        /// <summary>枯萎病和杂草直接放行；只保护生长进度未到 100% 的可播种作物。</summary>
        public static bool BlockCut(Plant plant)
        {
            if (plant == null || !plant.Spawned) return false;
            var policy = MapComponent_ZonePolicies.At(plant.Map, plant.Position);
            return policy != null && policy.MatureOnly && !plant.Blighted && plant.def.plant.Sowable && plant.Growth < 1f;
        }
        /// <summary>区分真正割除任务，避免误拦截收获、树木移植等其他工作。</summary>
        public static bool IsCut(Job job) => job != null && job.def != null && job.def.defName == "CutPlant";
        /// <summary>所有原版植物作业队列在切换目标后也检查当前限制。</summary>
        public static JobCondition End(JobDriver driver)
        {
            Plant plant = driver.job?.targetA.Thing as Plant;
            return IsCut(driver.job) && BlockCut(plant) ? JobCondition.Incompletable : JobCondition.Ongoing;
        }
    }
    [HarmonyPatch(typeof(Zone), "Deregister")]
    internal static class ZonePolicyDeletePatch
    {
        /// <summary>在区域注销清空地图引用之前删除设置。</summary>
        private static void Prefix(Zone __instance) { if (__instance is Zone_Growing zone) MapComponent_ZonePolicies.Forget(zone); }
    }
}
