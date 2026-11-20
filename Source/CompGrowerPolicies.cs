using System.Collections.Generic;
using RimWorld;
using Verse;

namespace StopFarmingItsEnough
{
    /// <summary>把设施手动状态保存在建筑本身；无需地图扫描，卸载、重装、跨地图搬迁均保留。</summary>
    public sealed class CompGrowerPolicies : ThingComp
    {
        public ZonePolicy Policy = new ZonePolicy();

        /// <summary>在定义加载完成、生成任何建筑之前，给所有种植接口建筑安装一次无 tick 组件。</summary>
        internal static void InstallDefinitions()
        {
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.thingClass == null || !typeof(Building).IsAssignableFrom(def.thingClass)
                    || !typeof(IPlantToGrowSettable).IsAssignableFrom(def.thingClass)) continue;
                if (def.comps == null) def.comps = new List<CompProperties>();
                if (!def.comps.Exists(c => c.compClass == typeof(CompGrowerPolicies)))
                    def.comps.Add(new CompProperties { compClass = typeof(CompGrowerPolicies) });
            }
        }

        /// <summary>旧建筑读取旧存档缺失节点时自动创建全关闭状态。</summary>
        public override void PostExposeData()
        {
            Scribe_Deep.Look(ref Policy, "sfieGrowerManualPolicy");
            if (Policy == null) Policy = new ZonePolicy();
        }

    }
}
