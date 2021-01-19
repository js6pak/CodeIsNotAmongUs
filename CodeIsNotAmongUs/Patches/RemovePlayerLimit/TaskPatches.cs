using HarmonyLib;

namespace CodeIsNotAmongUs.Patches.RemovePlayerLimit
{
    internal static class TaskPatches
    {
        [HarmonyPatch(typeof(KeyMinigame), nameof(KeyMinigame.Start))]
        public static class KeyMinigamePatch
        {
            public static void Postfix(KeyMinigame __instance)
            {
                var localPlayer = PlayerControl.LocalPlayer;
                __instance.Field_6 = localPlayer != null ? localPlayer.PlayerId % 10 : 0;
            }
        }
    }
}
