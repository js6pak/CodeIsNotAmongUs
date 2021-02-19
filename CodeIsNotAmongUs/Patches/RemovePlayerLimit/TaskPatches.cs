using HarmonyLib;
using UnityEngine;

namespace CodeIsNotAmongUs.Patches.RemovePlayerLimit
{
    internal static class TaskPatches
    {
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.GetSpawnLocation))]
        public static class GetSpawnLocationPatch
        {
            public static void Prefix([HarmonyArgument(0)] ref int playerId, [HarmonyArgument(1)] ref int numPlayer)
            {
                playerId %= 10;
                numPlayer = Mathf.Max(numPlayer, 10);
            }
        }

        [HarmonyPatch(typeof(KeyMinigame), nameof(KeyMinigame.Start))]
        public static class KeyMinigamePatch
        {
            public static void Postfix(KeyMinigame __instance)
            {
                var localPlayer = PlayerControl.LocalPlayer;
                __instance.targetSlotId = localPlayer != null ? localPlayer.PlayerId % 10 : 0;
            }
        }
    }
}
