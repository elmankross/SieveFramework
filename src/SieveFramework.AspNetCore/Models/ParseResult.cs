using System;

namespace SieveFramework.AspNetCore.Models
{
    public class ParseResult<TResource> : ParseResult
    {
        public new TResource Result { get; }

        [Obsolete("hack")]
        internal ParseResult(bool isSuccess = true)
        {
            Successful = isSuccess;
        }

        public ParseResult(TResource result)
            : base(result)
        {
            Result = result;
        }

        public ParseResult(params ParseError[] errors)
            : base(errors)
        {
        }
    }


    public class ParseResult
    {
        public bool Successful { get; protected set; }
        public object Result { get; }
        public ParseError[] Errors { get; }


        internal ParseResult(object result)
        {
            Result = result;
            Successful = true;
            Errors = new ParseError[0];
        }


        internal ParseResult(params ParseError[] errors)
        {
            Errors = errors;
            Successful = false;
            Result = default;
        }
    }


    public struct ParseError
    {
        public string Message { get; }
        public string Context { get; }

        public ParseError(string message, string context = null)
        {
            Message = message;
            Context = context;
        }
    }
}