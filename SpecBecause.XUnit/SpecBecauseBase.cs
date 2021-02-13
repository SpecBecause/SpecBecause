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
            throw new NotImplementedException();
        }

        public void It(string assertionMessage, Action assertion)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            
        }
    }
}
