using System.Linq;
using HarmonyLib;
using Hazel;
using UnhollowerBaseLib;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace CodeIsNotAmongUs.Patches.RemovePlayerLimit
{
    public static class MeetingPatches
    {
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
        public static class CheckForEndVotingPatch
        {
            // "api" for all mayors out there :)
            public static byte GetVotePower(PlayerVoteArea playerVoteArea)
            {
                return 1;
            }

            public static int IndexOfMax<T>(T[] self, out bool tie)
            {
                var max = self.Max();

                if (self.Count(x => x.Equals(max)) > 1)
                {
                    tie = true;
                    return -1;
                }

                tie = false;
                return self.ToList().IndexOf(max);
            }

            public static bool Prefix(MeetingHud __instance)
            {
                var playerStates = __instance.playerStates;
                if (playerStates.All(ps => ps != null && (ps.isDead || ps.didVote)))
                {
                    var self = new byte[playerStates.Max(x => x.TargetPlayerId) + 2];
                    foreach (var playerVoteArea in playerStates)
                    {
                        if (playerVoteArea.didVote && !playerVoteArea.isDead)
                        {
                            // -2 none, -1 skip, 0+ players
                            if (playerVoteArea.votedFor == -2)
                                continue;

                            var votedFor = playerVoteArea.votedFor + 1;
                            if (votedFor >= 0 && votedFor < self.Length)
                            {
                                self[votedFor] += GetVotePower(playerVoteArea);
                            }
                        }
                    }

                    var maxIdx = IndexOfMax(self, out var tie) - 1;
                    var exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(v => v != null && v.PlayerId == maxIdx);

                    var states = playerStates.Select(ps => ps != null ? ps.GetState() : (byte) 0).ToArray();
                    var votes = playerStates.Select(s => (byte) s.votedFor).ToArray();

                    var messageWriter = AmongUsClient.Instance.StartRpc(__instance.NetId, 23, SendOption.Reliable);
                    messageWriter.WriteBytesAndSize(states);
                    messageWriter.WriteBytesAndSize(votes);
                    messageWriter.Write(exiled != null && exiled.Object != null ? exiled.Object.PlayerId : byte.MaxValue);
                    messageWriter.Write(tie);
                    messageWriter.EndMessage();

                    VotingComplete(__instance, states, votes, exiled, tie);
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.HandleRpc))]
        public static class MeetingHudHandleRpcPatch
        {
            public static bool Prefix(MeetingHud __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
            {
                if (callId == (int) RpcCalls.VotingComplete)
                {
                    var states = reader.ReadBytesAndSize();
                    var votes = reader.ReadBytesAndSize();
                    var playerById = GameData.Instance.GetPlayerById(reader.ReadByte());
                    var tie = reader.ReadBoolean();
                    VotingComplete(__instance, states, votes, playerById, tie);

                    return false;
                }

                return true;
            }
        }

        private static void VotingComplete(MeetingHud __instance, byte[] states, byte[] votes, GameData.PlayerInfo exiled, bool tie)
        {
            if (__instance.state == MeetingHud.VoteStates.Results)
            {
                return;
            }

            __instance.state = MeetingHud.VoteStates.Results;
            __instance.resultsStartedAt = __instance.discussionTimer;
            __instance.exiledPlayer = exiled;
            __instance.wasTie = tie;
            __instance.SkipVoteButton.gameObject.SetActive(false);
            __instance.SkippedVoting.gameObject.SetActive(true);
            AmongUsClient.Instance.DisconnectHandlers.Remove(__instance.Cast<IDisconnectHandler>());
            PopulateResults(__instance, states, votes);
            __instance.SetupProceedButton();
        }

        private static void PopulateResults(MeetingHud __instance, byte[] states, byte[] votes)
        {
            __instance.TitleText.Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingResults, new Il2CppReferenceArray<Object>(0));
            var skippedCount = 0;
            for (var i = 0; i < __instance.playerStates.Length; i++)
            {
                var playerVoteArea = __instance.playerStates[i];
                playerVoteArea.ClearForResults();
                var votedCount = 0;
                for (var j = 0; j < states.Length; j++)
                {
                    if ((states[j] & 128) == 0)
                    {
                        {
                            if (j > __instance.playerStates.Length)
                            {
                                break;
                            }

                            var playerById = GameData.Instance.GetPlayerById((byte) __instance.playerStates[j].TargetPlayerId);
                            var votedFor = (int) votes[j];

                            var voted = votedFor == playerVoteArea.TargetPlayerId;
                            var skipped = i == 0 && (votedFor == -1 || votedFor == 255);

                            if (voted || skipped)
                            {
                                var spriteRenderer = UnityEngine.Object.Instantiate(__instance.PlayerVotePrefab, skipped ? __instance.SkippedVoting.transform : playerVoteArea.transform, true);
                                if (PlayerControl.GameOptions.AnonymousVotes)
                                {
                                    PlayerControl.SetPlayerMaterialColors(Palette.Black, spriteRenderer);
                                }
                                else
                                {
                                    PlayerControl.SetPlayerMaterialColors(playerById.ColorId, spriteRenderer);
                                }

                                var transform = spriteRenderer.transform;
                                transform.localPosition = __instance.CounterOrigin + new Vector3(__instance.CounterOffsets.x * (skipped ? skippedCount : votedCount), 0f, 0f);

                                if (__instance.playerStates.Length <= 15)
                                {
                                    transform.localScale = Vector3.zero;
                                    __instance.StartCoroutine(Effects.Bloop(Mathf.Min(skipped ? skippedCount : votedCount, 10), transform, 1, 0.5f));
                                }

                                if (skipped)
                                {
                                    skippedCount++;
                                }
                                else
                                {
                                    votedCount++;
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.GetState))]
        public static class GetStatePatch
        {
            public static bool Prefix(PlayerVoteArea __instance, out byte __result)
            {
                __result = (byte) ((__instance.isDead ? 128 : 0) | (__instance.didVote ? 64 : 0) | (__instance.didReport ? 32 : 0));
                return false;
            }
        }
    }
}
