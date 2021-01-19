using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using CodeIsNotAmongUs.Patches;
using CodeIsNotAmongUs.Patches.RemovePlayerLimit;
using HarmonyLib;
using Reactor;

namespace CodeIsNotAmongUs
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class CodeIsNotAmongUsPlugin : BasePlugin
    {
        public const string Id = "pl.js6pak.CodeIsNotAmongUs";

        public Harmony Harmony { get; } = new Harmony(Id);

        public ConfigEntry<bool> HideCode { get; private set; }
        public ConfigEntry<bool> ShowAllOptions { get; private set; }
        public ConfigEntry<MeetingHudMode> MeetingHudMode { get; internal set; }

        public override void Load()
        {
            HideCode = Config.Bind("Tweaks", "Hide code", false, "Hides code while in lobby (its printed out in the logs)");
            ShowAllOptions = Config.Bind("Tweaks", "Show all options", true, "Allows changing options like map, impostor count, player max count in lobby");
            MeetingHudMode = Config.Bind("RemovePlayerLimit", "MeetingHud Mode", CodeIsNotAmongUs.MeetingHudMode.Pagination);

            CustomRegion.Initialize(this);
            RemovePlayerLimit.Initialize();
            ColorPatches.Initialize();
            Harmony.PatchAll();
        }
    }
}
