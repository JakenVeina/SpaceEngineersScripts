using System;

namespace IngameScript
{
    public partial class Program
    {
        public interface IEchoProvider
        {
            void Echo(string text);
        }

        public partial class EchoProvider : IEchoProvider
        {
            public EchoProvider(Action<string> echoAction)
            {
                _echoAction = echoAction;
            }

            public void Echo(string text)
                => _echoAction.Invoke(text);

            private readonly Action<string> _echoAction;
        }
    }
}
