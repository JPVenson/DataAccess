using System;

namespace UnitTestProject1
{
    public interface IGridControlerCommand
    {
        bool HandleKey { get; }
        bool HandleString { get; }

        bool Handle(string key);
    }

    public class DelegateCommand : IGridControlerCommand
    {
        private readonly string _text;
        private readonly Func<string, bool> _delegateer;

        public DelegateCommand(string text, Func<string, bool> delegateer)
        {
            _text = text;
            _delegateer = delegateer;

            if (text.Length == 1)
            {
                HandleKey = true;
            }
            else
            {
                HandleString = true;
            }
        }

        public DelegateCommand(string text, Action<string> delegateer)
        {
            _text = text;
            _delegateer = s =>
            {
                delegateer(s);
                return true;
            };

            if (text.Length == 1)
            {
                HandleKey = true;
            }
            else
            {
                HandleString = true;
            }
        }

        public bool HandleKey { get; set; }
        public bool HandleString { get; set; }
        public bool Handle(string key)
        {
            if (key.StartsWith(_text))
            {
                return _delegateer(key.Substring(_text.Length));
            }
            return false;
        }
    }
}