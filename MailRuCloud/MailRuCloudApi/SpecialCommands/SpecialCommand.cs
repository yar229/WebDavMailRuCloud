using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace YaR.MailRuCloud.Api.SpecialCommands
{
    public abstract class SpecialCommand
    {
        protected readonly MailRuCloud Cloud;
        protected readonly string Path;
        protected readonly IList<string> Parames;

        protected abstract MinMax<int> MinMaxParamsCount { get; }

        protected SpecialCommand(MailRuCloud cloud, string path, IList<string> parames)
        {
            Cloud = cloud;
            Path = path;
            Parames = parames;

            CheckParams();
        }

        public virtual Task<SpecialCommandResult> Execute()
        {
            if (Parames.Count < MinMaxParamsCount.Min || Parames.Count > MinMaxParamsCount.Max)
                return Task.FromResult(SpecialCommandResult.Fail);

            return Task.FromResult(SpecialCommandResult.Success);
        }

        private void CheckParams()
        {
            if (Parames.Count < MinMaxParamsCount.Min || Parames.Count > MinMaxParamsCount.Max)
                throw new ArgumentException("Invalid pameters count");
        }

    }

    public struct MinMax<T> where T : IComparable<T>
    {
        public MinMax(T min, T max)
        {
            if (min.CompareTo(max) > 0) throw new ArgumentException("min > max");
            Min = min;
            Max = max;
        }

        public MinMax(T one) : this(one, one)
        {
        }

        public T Min { get; }
        public T Max { get; }
    }
}
