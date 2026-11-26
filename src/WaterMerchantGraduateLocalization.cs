
using System;
using System.Collections.Generic;
using System.Globalization;
using HarmonyLib;
namespace WaterMerchantGraduate {
 internal static class L {
  internal const string ModId = "water_merchant_graduate";
  internal static string LanguageOverride = "Auto";
  internal static string Resolve(string code, string choice) {
   if (choice == "简体中文") return "zh";
   if (choice == "繁體中文") return "zh-Hant";
   if (choice == "English") return "en";
   code = (code ?? "").Replace('_','-').ToLowerInvariant();
   if (code == "tchinese" || code.StartsWith("zh-hant") || code == "zh-tw" || code == "zh-hk" || code == "zh-mo") return "zh-Hant";
   if (code == "schinese" || code == "zh" || code.StartsWith("zh-hans") || code == "zh-cn" || code == "zh-sg") return "zh";
   return "en";
  }
  internal static string Code { get { return Resolve(LocHelper.GetCurrentLocaleCode(), LanguageOverride); } }
  internal static string Text(string key, params object[] args) {
   string[] row;
   if (!Rows.TryGetValue(key, out row)) throw new ArgumentException("Missing localization key: " + key);
   string code = Code, value = row[code == "zh" ? 0 : code == "zh-Hant" ? 1 : 2];
   return args == null || args.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, args);
  }
  internal static void Configure(ModManifest manifest, Action refresh = null) {
#if !NATIVE_BOUNDARY
   currentManifest = manifest; changed = refresh;
   if (!configured) {
    configured = true;
    var language = ModConfig.For(ModId).Define("language", "Auto", "Language / 语言 / 語言",
      () => "Language / 语言 / 語言", new[]{"Auto","简体中文","繁體中文","English"},
      () => "Auto: follow game language / 跟随游戏 / 跟隨遊戲", false);
    LanguageOverride = language.Value;
    language.OnChanged += value => { LanguageOverride = value; Refresh(); };
    UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocaleChanged += locale => Refresh();
   }
   Refresh();
#endif
  }
#if !NATIVE_BOUNDARY
  private static bool configured;
  private static ModManifest currentManifest;
  private static Action changed;
  internal static void ApplyManifest(ModManifest manifest) {
   if (manifest == null || manifest.ID != ModId) return;
   manifest.Name = Text("mod.name"); manifest.Description = Text("mod.description");
  }
  private static void Refresh() { ApplyManifest(currentManifest); if (changed != null) changed(); }
#endif
  private static readonly Dictionary<string,string[]> Rows = new Dictionary<string,string[]> {
   { "mod.name", new[]{"水商一键毕业", "水商一鍵畢業", "Pure Water, One Click"} },
   { "mod.description", new[]{"水龙头输出100%纯水，一次接满剩余容量；同步水质提示。独立运行，无需其他模组。", "水龍頭輸出100%純水，一次接滿剩餘容量；同步水質提示。獨立運作，無需其他模組。", "Faucets dispense 100% pure water and fill all remaining capacity. Matching water-quality tooltips. Standalone; no other mods required."} },
   { "quality", new[]{"出水水质：纯水", "出水水質：純水", "Output quality: pure water"} },
   { "composition", new[]{"水                         100.00%", "水                         100.00%", "Water                     100.00%"} },
   { "instruction", new[]{"将容器对水龙头使用，一次接满剩余容量。", "將容器對水龍頭使用，一次接滿剩餘容量。", "Use a container on the faucet to fill its remaining capacity."} },
   { "preserve", new[]{"原有内容物会混合，不会被清空。", "原有內容物會混合，不會被清空。", "Existing contents are mixed with the new water, not removed."} },
   { "locked", new[]{"水管尚未接通。先解锁任意供水管线。", "水管尚未接通。請先解鎖任一供水管線。", "No water supply connected. Unlock a water-supply line first."} },
   { "after.connect", new[]{"接通后出水：100%纯水，一次接满。", "接通後出水：100%純水，一次接滿。", "Once connected: 100% pure water, filled to capacity."} },
  };
 }
#if !NATIVE_BOUNDARY
 [HarmonyPatch(typeof(ModUIManager), "ShowMod")]
 internal static class LocalizedModDescription {
  [HarmonyPrefix] private static void Prefix(LoadedMod __0) { if (__0 != null) L.ApplyManifest(__0.Manifest); }
 }
#endif
}
