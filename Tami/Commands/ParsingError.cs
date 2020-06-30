using System;

namespace Tami.Commands
{
    public class ParsingError : Exception
    {
        public ParsingError(string msg) : base(msg)
        { }
    }
}
