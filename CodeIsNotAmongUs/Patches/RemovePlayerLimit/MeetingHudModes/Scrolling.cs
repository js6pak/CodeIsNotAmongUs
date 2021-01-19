using System.Linq;
using HarmonyLib;
using Reactor;
using UnityEngine;

namespace CodeIsNotAmongUs.Patches.RemovePlayerLimit.MeetingHudModes
{
    internal static class Scrolling
    {
        private static bool Enabled => PluginSingleton<CodeIsNotAmongUsPlugin>.Instance.MeetingHudMode.Value == MeetingHudMode.Scrolling;

        public static float Scroll { get; set; }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        public static class StartPatch
        {
            public static void Postfix(MeetingHud __instance)
            {
                if (!Enabled)
                    return;

                var original = __instance.transform.FindChild("Background").FindChild("baseGlass").gameObject;
                var mask = Object.Instantiate(original);
                mask.name = "Scrolling mask";
                mask.transform.position = original.transform.position;
                // mask.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
                var spriteRenderer = mask.GetComponent<SpriteRenderer>();
                spriteRenderer.color = Color.red;
                var spriteMask = mask.AddComponent<SpriteMask>();
                spriteMask.sprite = spriteRenderer.sprite;
                // Object.Destroy(spriteRenderer);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        public static class ButtonsPatch
        {
            public static void Postfix(MeetingHud __instance)
            {
                if (!Enabled)
                    return;

                var maxPages = (int) Mathf.Ceil(__instance.playerStates.Count / 10f);

                Scroll = Mathf.Clamp(Scroll + Input.mouseScrollDelta.y, -maxPages, 0);
                System.Console.WriteLine("maxPages " + maxPages);
                System.Console.WriteLine("scroll " + Scroll);

                var i = 0;

                foreach (var playerVoteArea in __instance.playerStates.ToArray().OrderBy(x => x.isDead).ThenBy(x => x.NameText.Text))
                {
                    var x = i % 2;
                    var y = i / 2 + Scroll / 2f;
                    playerVoteArea.transform.localPosition = __instance.VoteOrigin + new Vector3(__instance.VoteButtonOffsets.x * x, __instance.VoteButtonOffsets.y * y, -1f);

                    foreach (var renderer in playerVoteArea.GetComponentsInChildren<SpriteRenderer>())
                    {
                        renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                    }

                    i++;
                }
            }
        }
    }
}
