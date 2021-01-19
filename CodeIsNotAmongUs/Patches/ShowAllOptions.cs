using HarmonyLib;
using Reactor;
using UnhollowerBaseLib;
using UnityEngine;

namespace CodeIsNotAmongUs.Patches
{
    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.OnEnable))]
    internal static class ShowAllOptions
    {
        public static void Prefix(GameSettingMenu __instance)
        {
            if (PluginSingleton<CodeIsNotAmongUsPlugin>.Instance.ShowAllOptions.Value)
            {
                __instance.HideForOnline = new Il2CppReferenceArray<Transform>(0);
            }
        }
    }
}
