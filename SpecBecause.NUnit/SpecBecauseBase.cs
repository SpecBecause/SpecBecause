using NUnit.Framework;
using System;

namespace SpecBecause.NUnit
{
    public class SpecBecauseBase : IEngine
    {
        private IEngine Engine { get; set; }

        public SpecBecauseBase(IEngine engine = null)
        {
            Engine = engine ?? new Engine();
        }

        [SetUp]
        public void SetUp() { }

        [TearDown]
        public void TearDown()
        {
            Dispose();
        }

        public void Because(Action act)
        {
            Engine.Because(act);
        }

        public TResult Because<TResult>(Func<TResult> act)
        {
            return Engine.Because(act);
        }

        public TException BecauseThrows<TException>(Action act) where TException : Exception
        {
            return Engine.BecauseThrows<TException>(act);
        }

        public void It(string assertionMessage, Action assertion)
        {
            Engine.It(assertionMessage, assertion);
        }

        public void Dispose()
        {
            Engine.Dispose();
        }
    }
}
