using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using YaR.MailRuCloud.Api.Base;

namespace YaR.MailRuCloud.Api.SpecialCommands
{
    /// <summary>
    /// Обрабатывает командную строку и возвращает нужный объект команды
    /// </summary>
    public class SpecialCommandFabric
    {
        private static readonly List<SpecialCommandContainer> CommandContainers = new List<SpecialCommandContainer>
        {
            new SpecialCommandContainer
            {
                Commands = new [] {"del"},
                CreateFunc = (cloud, path, param) => new DeleteCommand(cloud, path, param)
            },
            new SpecialCommandContainer
            {
                Commands = new [] {"link"},
                CreateFunc = (cloud, path, param) => new SharedFolderLinkCommand(cloud, path, param)
            },
            new SpecialCommandContainer
            {
                Commands = new [] {"link", "check"},
                CreateFunc = (cloud, path, param) => new RemoveBadLinksCommand(cloud, path, param)
            },
            new SpecialCommandContainer
            {
                Commands = new [] {"join"},
                CreateFunc = (cloud, path, param) => new SharedFolderJoinCommand(cloud, path, param)
            },
            new SpecialCommandContainer
            {
                Commands = new [] {"copy"},
                CreateFunc = (cloud, path, param) => new CopyCommand(cloud, path, param)
            },
            new SpecialCommandContainer
            {
                Commands = new [] {"move"},
                CreateFunc = (cloud, path, param) => new MoveCommand(cloud, path, param)
            },
            new SpecialCommandContainer
            {
                Commands = new [] {"fish"},
                CreateFunc = (cloud, path, param) => new FishCommand(cloud, path, param)
            },
            new SpecialCommandContainer
            {
                Commands = new [] {"lcopy"},
                CreateFunc = (cloud, path, param) => new LocalToServerCopyCommand(cloud, path, param)
            }
        };


        public SpecialCommand Build(MailRuCloud cloud, string param)
        {
            if (null == param || !param.Contains("/>>")) return null;

            int pos = param.LastIndexOf("/>>", StringComparison.Ordinal);
            string path = WebDavPath.Clean(param.Substring(0, pos + 1));
            string data = param.Substring(pos + 3);

            var parames = ParseParameters(data);
            var commandContainer = FindCommandContainer(parames);
            if (commandContainer == null) return null;

            parames = parames.Skip(commandContainer.Commands.Length).ToList();
            var cmd = commandContainer.CreateFunc(cloud, path, parames);

            return cmd;
        }

        private SpecialCommandContainer FindCommandContainer(IList<string> parames)
        {
            var commandContainer = CommandContainers
                .Where(cm =>
                    cm.Commands.Length <= parames.Count &&
                    cm.Commands.SequenceEqual(parames.Take(cm.Commands.Length)))
                .Aggregate((agg, next) => next.Commands.Length > agg.Commands.Length ? next : agg);

            return commandContainer;
        }

        private List<string> ParseParameters(string paramString)
        {
            var list = Regex
                .Matches(paramString, @"((""((?<token>.*?)(?<!\\)"")|(?<token>[\S]+))(\s)*)")
                .Cast<Match>()
                .Select(m => m.Groups["token"].Value)
                .ToList();

            return list;
        }



        private class SpecialCommandContainer
        {
            public string[] Commands;
            public Func<MailRuCloud, string, IList<string>, SpecialCommand> CreateFunc;
        }

    }

}