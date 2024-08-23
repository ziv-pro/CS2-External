﻿using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace BannedWords
{
    public class BanSettings
    {
        [JsonPropertyName("PlayerBanType")]
        public string PlayerBanType { get; set; } = "silence";

        [JsonPropertyName("DurationInMinutes")]
        public int DurationInMinutes { get; set; } = 5;
    }

    public class BannedWordsConfig : BasePluginConfig
    {
        [JsonPropertyName("BanSettings")]
        public BanSettings BanSettings { get; set; } = new BanSettings();

        [JsonPropertyName("BannedWords")]
        public string[] BannedWords { get; set; } = new[] { "word1", "word2", "word3" };

        [JsonPropertyName("ConfigVersion")]
        public override int Version { get; set; } = 1;
    }

    public partial class BannedWords : BasePlugin, IPluginConfig<BannedWordsConfig>
    {
        public required BannedWordsConfig Config { get; set; }
    }
}