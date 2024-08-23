using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace BannedWords
{
    public partial class BannedWords(ILogger<BannedWords> logger) : BasePlugin, IPluginConfig<BannedWordsConfig>
    {
        public override string ModuleName => "Banned Words";
        public override string ModuleAuthor => "Marchand";
        public override string ModuleVersion => "1.0.0";

        public override void Load(bool hotReload)
        {
            AddCommandListener("say", OnPlayerChatAll);
            AddCommandListener("say_team", OnPlayerChatTeam);
        }

        public void OnConfigParsed(BannedWordsConfig config)
        {
            Config = config;

            if (Config.Version != 1)
            {
                BannedWordsDebug($"Config version mismatch detected: expected 1, but got {Config.Version}.");
            }
        }
        
        private readonly ILogger<BannedWords> _logger = logger;

        public void BannedWordsDebug(string msg)
        {
            _logger.LogInformation($"[DEBUG] {msg}\n");
        }

        private HookResult HandlePlayerChat(CCSPlayerController? player, CommandInfo message, bool isTeamChat)
        {
            if (player == null || !player.IsValid || player.IsBot || string.IsNullOrEmpty(message.GetArg(1)))
                return HookResult.Handled;

            string arg = message.GetArg(1).ToLower();
            if (Config.BannedWords.Any(word => arg.Contains(word)))
            {
                string playerBanType = Config.BanSettings.PlayerBanType?.ToLower() ?? string.Empty;
        
                if (Config.BanSettings.PlayerBanType == "silence")
                {
                    string silenceMessagePlayer = Chat.FormatMessage(Localizer[LocalizerKeys.SilenceMsgPlayer, Config.BanSettings.DurationInMinutes.ToString(), Localizer[LocalizerKeys.BanReason]]);
                    string silenceMessageServer = Chat.FormatMessage(Localizer[LocalizerKeys.SilenceMsgServer, player.PlayerName, Localizer[LocalizerKeys.BanReason]]);

                    Server.ExecuteCommand($"css_silence #{player.UserId} {Config.BanSettings.DurationInMinutes} {Localizer[LocalizerKeys.BanReason]}");
                    player.PrintToChat(silenceMessagePlayer);
                    Server.PrintToChatAll(silenceMessageServer);
                }
                else if (Config.BanSettings.PlayerBanType == "gag")
                {
                    string gagMessagePlayer = Chat.FormatMessage(Localizer[LocalizerKeys.GagMsgPlayer, Config.BanSettings.DurationInMinutes.ToString(), Localizer[LocalizerKeys.BanReason]]);
                    string gagMessageServer = Chat.FormatMessage(Localizer[LocalizerKeys.GagMsgServer, player.PlayerName, Localizer[LocalizerKeys.BanReason]]);

                    Server.ExecuteCommand($"css_gag #{player.UserId} {Config.BanSettings.DurationInMinutes} {Localizer[LocalizerKeys.BanReason]}");
                    player.PrintToChat(gagMessagePlayer);
                    Server.PrintToChatAll(gagMessageServer);
                }
                
                return HookResult.Handled;
            }

            return HookResult.Continue;
        }

        private HookResult OnPlayerChatAll(CCSPlayerController? player, CommandInfo message)
        {
            return HandlePlayerChat(player, message, isTeamChat: false);
        }

        private HookResult OnPlayerChatTeam(CCSPlayerController? player, CommandInfo message)
        {
            return HandlePlayerChat(player, message, isTeamChat: true);
        }

        public static class Chat
        {
            private static readonly Dictionary<string, char> PredefinedColors = 
                typeof(ChatColors)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .ToDictionary(
                    field => $"{{{field.Name}}}", 
                    field => (char)(field.GetValue(null) ?? '\x01')
                );

            public static string FormatMessage(string message) =>
                PredefinedColors.Aggregate(message, (current, color) => current.Replace(color.Key, $"{color.Value}"));
        }
    }

    public static class LocalizerKeys
    {
        public const string SilenceMsgPlayer = "silencemsgplayer";
        public const string SilenceMsgServer = "silencemsgserver";
        public const string GagMsgPlayer = "gagmsgplayer";
        public const string GagMsgServer = "gagmsgserver";
        public const string BanReason = "banreason";
    }
}