using NUnit.Framework;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SpecBecause.NUnit.Tests")]

namespace SpecBecause.NUnit
{
    public class SpecBecauseBase : IEngine
    {
        private IEngine Engine { get; set; }

        internal SpecBecauseBase(IEngine engine = null)
        {
            Engine = engine ?? new Engine();
        }

        [SetUp]
        public void SetUp() { }

        [TearDown]
        public void TearDown() { }

        public void Because(Action act)
        {
            Engine.Because(act);
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
