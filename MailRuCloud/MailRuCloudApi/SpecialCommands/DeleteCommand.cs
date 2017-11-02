using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.SpecialCommands
{
    public class DeleteCommand : SpecialCommand
    {
        private readonly MailRuCloud _cloud;
        private readonly string _path;
        private readonly string _param;

        public DeleteCommand(MailRuCloud cloud, string path, string param)
        {
            _cloud = cloud;
            _path = WebDavPath.Clean(path);
            _param = param.Replace('\\', '/');
        }

        public override async Task<SpecialCommandResult> Execute()
        {
            string path;
            if (string.IsNullOrWhiteSpace(_param))
                path = _path;
            else if (_param.StartsWith("/"))
                path = _param;
            else
                path = WebDavPath.Combine(_path, _param);

            var entry = await _cloud.GetItem(path);
            if (null == entry)
                return SpecialCommandResult.Fail;

            var res = await _cloud.Remove(entry);
            return new SpecialCommandResult { IsSuccess = res };
        }
    }
}