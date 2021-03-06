using NUnit.Framework;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SpecBecause.NUnit.Tests")]

namespace SpecBecause.NUnit
{
    public class SpecBecauseBase : IEngine
    {
        private IEngine Engine { get; set; }

        public SpecBecauseBase()
        {

        }

        internal SpecBecauseBase(IEngine engine = null)
        {

        }

        [SetUp]
        public void SetUp() { }

        [TearDown]
        public void TearDown() { }

        public void Because(Action act)
        {
            throw new NotImplementedException();
        }

        public TResult Because<TResult>(Func<TResult> act)
        {
            throw new NotImplementedException();
        }

        public TException BecauseThrows<TException>(Action act) where TException : Exception
        {
            throw new NotImplementedException();
        }

        public void It(string assertionMessage, Action assertion)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
