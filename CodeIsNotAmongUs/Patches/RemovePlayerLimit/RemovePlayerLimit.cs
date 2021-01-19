using System.Collections;
using System.Linq;
using HarmonyLib;
using Reactor;
using UnhollowerBaseLib;
using UnityEngine;

namespace CodeIsNotAmongUs.Patches.RemovePlayerLimit
{
    public static class RemovePlayerLimit
    {
        public static void Initialize()
        {
            GameOptionsData.MaxImpostors = GameOptionsData.RecommendedImpostors = Enumerable.Repeat((int) byte.MaxValue, byte.MaxValue).ToArray();
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
        public static class HitBufferPatch
        {
            public static void Postfix(PlayerControl __instance)
            {
                __instance.Field_21 = new Il2CppReferenceArray<Collider2D>(200);
            }
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.GetAvailableId))]
        public static class GetAvailableIdPatch
        {
            public static bool Prefix(GameData __instance, out sbyte __result)
            {
                var i = (sbyte) 0;

                while (__instance.AllPlayers.ToArray().Any(p => p.PlayerId == i))
                {
                    i++;
                }

                __result = i;

                return false;
            }
        }

        public static bool IsInCutscene { get; private set; }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetInfected))]
        public static class SetInfectedPatch
        {
            public static void Postfix()
            {
                if (DestroyableSingleton<TutorialManager>.InstanceExists)
                    return;

                IsInCutscene = true;
                Coroutines.Start(Coroutine());
            }

            public static IEnumerator Coroutine()
            {
                yield return new WaitForSeconds(5);

                IsInCutscene = false;
                HudManager.Instance.SetHudActive(true);
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CanMove), MethodType.Getter)]
        public static class CanMovePatch
        {
            public static bool Prefix(ref bool __result)
            {
                if (IsInCutscene)
                {
                    return __result = false;
                }

                return true;
            }
        }
    }
}
