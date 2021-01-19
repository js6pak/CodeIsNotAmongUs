using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace CodeIsNotAmongUs.Patches.RemovePlayerLimit
{
    internal static class ColorPatches
    {
        public static void Initialize()
        {
            AddColor("Cyan", Color32(0, 150, 136), Color32(0, 121, 107));
            AddColor("Gray", Color32(117, 117, 117), Color32(97, 97, 97));
            AddColor("Tan", Color32(145, 137, 119), Color32(81, 66, 62));
        }

        private static Color32 Color32(byte r, byte g, byte b)
        {
            return new Color32(r, g, b, 255);
        }

        private static void AddColor(string name, Color32 color, Color32 shadow)
        {
            Telemetry.ColorNames = Telemetry.ColorNames.AddItem(name).ToArray();
            MedScanMinigame.ColorNames = MedScanMinigame.ColorNames.AddItem(name).ToArray();

            Palette.PlayerColors = Palette.PlayerColors.AddItem(color).ToArray();
            Palette.ShadowColors = Palette.ShadowColors.AddItem(shadow).ToArray();
        }

        [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.UpdateAvailableColors))]
        public static class UpdateAvailableColorsPatch
        {
            public static bool Prefix(PlayerTab __instance)
            {
                PlayerControl.SetPlayerMaterialColors(PlayerControl.LocalPlayer.Data.ColorId, __instance.DemoImage);
                for (var i = 0; i < Palette.PlayerColors.Length; i++)
                {
                    __instance.AvailableColors.Add(i);
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerTab.OnEnable__0), nameof(PlayerTab.OnEnable__0.Method_Internal_Void_0))] // inlined SelectColor
        public static class SelectColorPatch
        {
            public static bool Prefix(PlayerTab.OnEnable__0 __instance)
            {
                var colorId = __instance.j;

                SaveManager.BodyColor = (byte) colorId;
                if (PlayerControl.LocalPlayer)
                {
                    PlayerControl.LocalPlayer.CmdCheckColor((byte) colorId);
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor))]
        public static class CheckColorPatch
        {
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte bodyColor)
            {
                __instance.RpcSetColor(bodyColor);
                return false;
            }
        }
    }
}
