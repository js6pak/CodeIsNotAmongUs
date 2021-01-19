using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;

namespace CodeIsNotAmongUs.Patches
{
    internal static class CustomRegion
    {
        private static List<RegionInfo> _defaultRegions;

        private static readonly PropertyInfo _orphanedEntriesProperty = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
        private const string Section = "Custom regions";

        public static void Initialize(CodeIsNotAmongUsPlugin plugin)
        {
            _defaultRegions = ServerManager.DefaultRegions.ToList();

            var config = plugin.Config;

            config.ConfigReloaded += (_, _) => Reload(config);
            Reload(config);
        }

        public static void Reload(ConfigFile config)
        {
            var orphanedEntries = (Dictionary<ConfigDefinition, string>) _orphanedEntriesProperty.GetValue(config);

            var regions = orphanedEntries.Where(x => x.Key.Section == Section).ToList();

            if (!regions.Any())
            {
                orphanedEntries.Add(new ConfigDefinition(Section, "localhost"), "127.0.0.1:22023");
                config.Save();

                Reload(config);
                return;
            }

            var newRegions = _defaultRegions.ToList();

            foreach (var pair in regions)
            {
                newRegions.Add(pair.Key.Key, pair.Value);
            }

            ServerManager.DefaultRegions = newRegions.ToArray();
        }

        private static void Add(this IList<RegionInfo> regions, string name, string rawIp)
        {
            var split = rawIp.Split(':');
            var ip = split[0];
            var port = ushort.TryParse(split.ElementAtOrDefault(1), out var p) ? p : (ushort) 22023;

            regions.Insert(0, new RegionInfo(
                name, ip, new[]
                {
                    new ServerInfo($"{name}-Master-1", ip, port)
                })
            );
        }
    }
}
