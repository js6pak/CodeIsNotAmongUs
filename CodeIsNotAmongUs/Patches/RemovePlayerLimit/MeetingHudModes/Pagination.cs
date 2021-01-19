using System.Linq;
using HarmonyLib;
using Reactor;
using UnityEngine;

namespace CodeIsNotAmongUs.Patches.RemovePlayerLimit.MeetingHudModes
{
    internal static class Pagination
    {
        private static bool Enabled => PluginSingleton<CodeIsNotAmongUsPlugin>.Instance.MeetingHudMode.Value == MeetingHudMode.Pagination;

        public static int Page { get; set; }

        private static string _lastText;

        private static void UpdatePageText(MeetingHud meetingHud, int maxPages)
        {
            if (meetingHud.TimerText.Text == _lastText)
                return;

            meetingHud.TimerText.Text = _lastText = meetingHud.TimerText.Text + $" ({Page + 1}/{maxPages})";
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        public static class ButtonsPatch
        {
            public static void Postfix(MeetingHud __instance)
            {
                if (!Enabled)
                    return;

                var maxPages = (int) Mathf.Ceil(__instance.playerStates.Count / 10f);

                Page = Input.mouseScrollDelta.y switch
                {
                    > 0 => Mathf.Clamp(Page - 1, 0, maxPages - 1),
                    < 0 => Mathf.Clamp(Page + 1, 0, maxPages - 1),
                    _ => Page
                };

                UpdatePageText(__instance, maxPages);

                var i = 0;

                foreach (var playerVoteArea in __instance.playerStates.ToArray().OrderBy(x => x.isDead).ThenBy(x => x.NameText.Text))
                {
                    var active = i >= 10 * Page && i < 10 * (Page + 1);
                    playerVoteArea.gameObject.SetActive(active);

                    if (active)
                    {
                        var paged = i - Page * 10;
                        var x = paged % 2;
                        var y = paged / 2;
                        playerVoteArea.transform.localPosition = __instance.VoteOrigin + new Vector3(__instance.VoteButtonOffsets.x * x, __instance.VoteButtonOffsets.y * y, -1f);
                    }

                    i++;
                }
            }
        }
    }
}
