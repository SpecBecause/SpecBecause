using NUnit.Framework;
using System;

namespace SpecBecause.NUnit
{
    public class SpecBecauseBase : IEngine
    {
        private IEngine Engine { get; set; }
        private Func<IEngine> EngineProvider { get; set; }

        public SpecBecauseBase(Func<IEngine> engineProvider = null)
        {
            EngineProvider = engineProvider ?? (() => new Engine());
        }

        [SetUp]
        public void SetUp()
        {
            Engine = EngineProvider();
        }

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
