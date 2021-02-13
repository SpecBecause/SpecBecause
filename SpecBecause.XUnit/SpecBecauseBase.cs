using System;

namespace SpecBecause.XUnit
{
    public class SpecBecauseBase : IEngine
    {
        private IEngine Engine { get; }

        public SpecBecauseBase(IEngine? engine = null)
        {
            Engine = engine ?? new Engine();
        }

        public void Because(Action act)
        {
            Engine.Because(act);
        }

        public TResult Because<TResult>(Func<TResult> act)
        {
            return Engine.Because(act);
        }

        public TException? BecauseThrows<TException>(Action act) where TException : Exception
        {
            return Engine.BecauseThrows<TException>(act);
        }

        public void It(string assertionMessage, Action assertion)
        {
            Engine.It(assertionMessage, assertion);
        }

        public void Dispose()
        {
            
        }
    }
}
