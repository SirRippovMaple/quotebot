using System;
using System.Linq;
using System.Text;
using Trumpi.Twitch.Irc;

namespace QuoteBot
{
    public class ParameterParser
    {
        private readonly string[] _parameters;
        private int _position;

        public ParameterParser(string[] parameters)
        {
            _parameters = parameters;
        }

        public ParameterParser(IrcMessage message, string command)
        {
            _parameters = message.Parameters[1]
                .Substring(command.Length)
                .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }

        public int Remaining => _parameters.Length - _position;

        public int ReadInt()
        {
            try
            {
                var s = _parameters[_position];
                var i = int.Parse(s);
                _position++;
                return i;
            }
            catch (IndexOutOfRangeException e)
            {
                throw new Exception("Not enough parameters were provided", e);
            }
            catch (FormatException e)
            {
                throw new Exception("Cannot read integer", e);
            }
        }

        public int? ReadIntOptional()
        {
            try
            {
                if (IsEnd())
                {
                    return null;
                }

                var s = _parameters[_position];
                if (int.TryParse(s, out var i))
                {
                    _position++;
                    return i;
                }

                return null;
            }
            catch (IndexOutOfRangeException e)
            {
                throw new Exception("Not enough parameters were provided", e);
            }
        }

        public string ReadString()
        {
            try
            {
                if (IsEnd()) return null;
                return _parameters[_position++];
            }
            catch (IndexOutOfRangeException e)
            {
                throw new Exception("Not enough parameters were provided", e);
            }
        }

        public string[] ReadCommaSeparatedList()
        {
            var s = String.Join(" ", _parameters.Skip(_position).ToArray());
            var tokens = s.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            return tokens.Select(x => x.Trim()).ToArray();
        }

        public string ReadToEnd()
        {
            var s = String.Join(" ", _parameters.Skip(_position).ToArray());
            return s;
        }

        public string ReadTerminatedString(string terminator)
        {
            StringBuilder builder = new StringBuilder();
            var current = _parameters[_position++];
            builder.Append(current);
            while (_position < _parameters.Length && !current.EndsWith(terminator))
            {
                builder.Append(" ");
                current = _parameters[_position++];
                builder.Append(current);
            }

            return builder.ToString();
        }

        public bool IsEnd()
        {
            return _position >= _parameters.Length;
        }

        public string[] ReadSpaceSeparatedList()
        {
            return _parameters.Skip(_position).ToArray();
        }

        public void Reset()
        {
            _position = 0;
        }
    }
}