using System;
using HarmonyLib;
using UnityEngine;

namespace WaterMerchantGraduate
{
    public sealed class Entry : IMod
    {
        public void Init(ModManifest manifest) { L.Configure(manifest); }
        public void OnEnable() { FaucetRules.Enabled = true; Debug.Log("[WaterMerchantGraduate] " + L.Text("mod.description")); }
        public void OnDisable() { FaucetRules.Enabled = false; }
    }

    internal static class FaucetRules
    {
        internal static bool Enabled;
        [ThreadStatic] internal static GameItem Recipient;
        internal static bool IsTarget(GameItem item) { return item != null && WaterHelper.IsFaucet(item); }
        internal static void Wrap(GameItem item)
        {
            var original = item.onThisTargetItemFunc;
            if (original == null) return;
            item.onThisTargetItemFunc = (container, target) =>
            {
                if (!Enabled || !IsTarget(target)) { original(container, target); return; }
                var previous = Recipient;
                Recipient = container;
                try { original(container, target); }
                finally { Recipient = previous; }
            };
        }
        internal static bool Connected()
        {
            if (!PlayerStore.IsInstanceExist()) return false;
            var rust = NetworkUpgrade.GetUpgradeById("RUSTWATER_TAP");
            var gutter = NetworkUpgrade.GetUpgradeById("GUTTERFLOW_TAP");
            return (rust != null && rust.IsUnlocked()) || (gutter != null && gutter.IsUnlocked());
        }
        internal static RichText Tooltip(GameItem faucet)
        {
            var builder = new RichTextBuilder(32);
            builder.AddLine(faucet.name, bold: true);
            builder.AddEmptyLine(8);
            if (Connected())
            {
                builder.AddLine(L.Text("quality"), color: RenderHandler.ColorPalette.LightBlue);
                builder.AddLine(L.Text("composition"), color: RenderHandler.ColorPalette.LightBlue);
                builder.AddEmptyLine(8);
                builder.AddLine(L.Text("instruction"));
                builder.AddLine(L.Text("mod.name"), color: RenderHandler.ColorPalette.LightBlue);
                builder.AddLine(L.Text("preserve"), italic: true);
            }
            else
            {
                builder.AddLine(L.Text("locked"));
                builder.AddLine(L.Text("after.connect"), color: RenderHandler.ColorPalette.LightBlue);
            }
            return builder.Build();
        }
    }

    [HarmonyPatch(typeof(WaterHelper), nameof(WaterHelper.InitLiquidContainerItem))]
    internal static class ContainerInteraction
    {
        [HarmonyPostfix] private static void Postfix(GameItem item) { FaucetRules.Wrap(item); }
    }

    [HarmonyPatch(typeof(WaterHelper), nameof(WaterHelper.AddWater))]
    internal static class FaucetOutput
    {
        [HarmonyPrefix]
        internal static void Prefix(GameItem waterContainer, ref int grade, ref int addAmount, ref bool useRange, ref int lowPara, ref int highPara)
        {
            // This scope is entered ONLY by the native container-to-faucet interaction.
            // Native AddWater(-1) adds to remaining space and preserves existing contents.
            if (!FaucetRules.Enabled || FaucetRules.Recipient == null || !ReferenceEquals(waterContainer, FaucetRules.Recipient)) return;
            grade = 0;
            addAmount = -1;
            useRange = true;
            lowPara = highPara = 10000;
        }
    }

    [HarmonyPatch(typeof(GameItemElement), nameof(GameItemElement.HasTooltip))]
    internal static class FaucetTooltipVisible
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        internal static void Postfix(GameItemElement __instance, ref bool __result)
        { if (FaucetRules.Enabled && FaucetRules.IsTarget(__instance)) __result = true; }
    }
    [HarmonyPatch(typeof(GameItemElement), nameof(GameItemElement.GetTooltip))]
    internal static class FaucetTooltip
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        internal static void Postfix(GameItemElement __instance, ref RichText __result)
        { if (FaucetRules.Enabled && FaucetRules.IsTarget(__instance)) __result = FaucetRules.Tooltip(__instance); }
    }
    [HarmonyPatch(typeof(GameItemElement), nameof(GameItemElement.GetTooltipAdvanced))]
    internal static class FaucetAdvancedTooltip
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        internal static void Postfix(GameItemElement __instance, ref RichText __result)
        { if (FaucetRules.Enabled && FaucetRules.IsTarget(__instance)) __result = FaucetRules.Tooltip(__instance); }
    }
}

