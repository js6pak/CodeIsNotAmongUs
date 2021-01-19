using HarmonyLib;
using Reactor;

namespace CodeIsNotAmongUs.Patches
{
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    internal static class HideCode
    {
        public static void Postfix(GameStartManager __instance)
        {
            var plugin = PluginSingleton<CodeIsNotAmongUsPlugin>.Instance;

            if (plugin.HideCode.Value)
            {
                __instance.GameRoomName.Text = "Room\r\nhidden";
                plugin.Log.LogInfo($"Room code ({GameCode.IntToGameNameV2(AmongUsClient.Instance.GameId)}) hidden");
            }
        }
    }
}
