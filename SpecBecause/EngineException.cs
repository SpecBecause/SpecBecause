using System;
using System.Collections.Generic;

namespace SpecBecause
{
    public class EngineException : Exception
    {
        public EngineException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
