using System;

namespace SieveFramework.Exceptions
{
    public class InvalidFilterFormatException : Exception
    {
        public string Example { get; }

        public InvalidFilterFormatException(string message, string example = null)
            : base(message)
        {
            Example = example;
        }
    }
}
