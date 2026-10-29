using Verse;

namespace StopFarmingItsEnough
{
    /// <summary>将规则保存在游戏而非地图或 ModSettings；同档多地图共享阈值，库存锁仍逐地图保存。</summary>
    public sealed class GameComponent_CropRules : GameComponent
    {
        public CropRuleSettings Settings = new CropRuleSettings();
        private bool hasSavedRules;

        /// <summary>每个 Game 创建独立容器，新游戏不复制上一次游戏或全局迁移模板。</summary>
        public GameComponent_CropRules(Game game) { }

        /// <summary>保存当前游戏的完整规则；缺少节点表示需要在 LoadedGame 中迁移旧全局配置。</summary>
        public override void ExposeData()
        {
            Scribe_Deep.Look(ref Settings, "cropRules");
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                hasSavedRules = Settings != null;
                if (Settings == null) Settings = new CropRuleSettings();
            }
        }

        /// <summary>旧存档首次升级时逐字段复制模板；不共享 CropRule 对象或输入缓冲。</summary>
        public override void LoadedGame()
        {
            if (!hasSavedRules)
            {
                Settings = new CropRuleSettings();
                if (FarmingMod.Settings?.LegacyRules != null)
                    foreach (CropRule old in FarmingMod.Settings.LegacyRules)
                    {
                        if (old == null) continue;
                        Settings.Rules.Add(new CropRule {
                            PlantDefName = old.PlantDefName, ProductDefName = old.ProductDefName,
                            Source = old.Source, StateId = old.StateId, Lower = old.Lower, Upper = old.Upper,
                            IgnoreSowing = old.IgnoreSowing, IgnoreHarvest = old.IgnoreHarvest
                        });
                    }
            }
            BindRules();
        }

        /// <summary>新游戏使用全新的默认忽略规则。</summary>
        public override void StartedNewGame() { BindRules(); }

        /// <summary>在游戏加载结束后绑定目录并重建地图规则引用，保留地图的滞回记忆且不读取库存。</summary>
        private void BindRules()
        {
            hasSavedRules = true;
            Settings.Reindex();
            CropCatalog.EnsureCurrentGame(true);
            foreach (Map map in Current.Game.Maps) MapComponent_FarmingLimits.For(map)?.FinalizeInit();
        }
    }
}
