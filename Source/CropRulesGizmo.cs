using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace StopFarmingItsEnough
{
    /// <summary>收获图标加问号的规则入口，使用叠绘避免生成或复制游戏贴图。</summary>
    internal sealed class Command_CropRules : Command_Action
    {
        /// <summary>建立可在多选时合并的通用入口；所有设施打开同一个全存档规则窗口。</summary>
        public Command_CropRules()
        {
            defaultLabel = "SFIE_RulesTitle".Translate();
            defaultDesc = "SFIE_RulesLocation".Translate();
            icon = ContentFinder<Texture2D>.Get("UI/Designators/Harvest");
            action = Open;
        }

        /// <summary>避免重复窗口，并在打开时重新确认当前游戏。</summary>
        private static void Open()
        {
            if (Current.Game != null && !Find.WindowStack.IsOpen<Window_CropRules>())
                Find.WindowStack.Add(new Window_CropRules());
        }

        /// <summary>在原版按钮图标右上角叠绘问号，保持标准按钮交互、标签与提示。</summary>
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
            GameFont font = Text.Font;
            TextAnchor anchor = Text.Anchor;
            Color color = GUI.color;
            Text.Font = GameFont.Medium; Text.Anchor = TextAnchor.MiddleCenter;
            float width = parms.shrunk ? 36f : GetWidth(maxWidth);
            Rect question = new Rect(topLeft.x + width - 27f, topLeft.y + 3f, 24f, 28f);
            GUI.color = Color.black;
            Widgets.Label(new Rect(question.x + 1f, question.y + 1f, question.width, question.height), "?");
            GUI.color = Color.white; Widgets.Label(question, "?");
            Text.Font = font; Text.Anchor = anchor; GUI.color = color;
            return result;
        }
    }

    /// <summary>向种植区和种植设施的底部按钮栏追加入口，不改动原有按钮或种植模式。</summary>
    [HarmonyPatch]
    internal static class CropRulesGizmoPatch
    {
        /// <summary>原版设施及沿用基类 GetGizmos 的模组设施均可使用此入口。</summary>
        private static IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Zone_Growing), "GetGizmos");
            yield return AccessTools.Method(typeof(Building_PlantGrower), "GetGizmos");
        }
        /// <summary>仅对玩家管理的设施追加按钮，关闭自动控制时仍允许编辑规则。</summary>
        private static void Postfix(IPlantToGrowSettable __instance, ref IEnumerable<Gizmo> __result)
        {
            if (FarmingGate.Managed(__instance)) __result = Append(__result);
        }
        /// <summary>惰性保留上游按钮，然后追加规则入口。</summary>
        private static IEnumerable<Gizmo> Append(IEnumerable<Gizmo> original)
        {
            foreach (Gizmo gizmo in original) yield return gizmo;
            yield return new Command_CropRules();
        }
    }
}
