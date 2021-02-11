using System;

namespace SpecBecause
{
    public class Engine
    {
        public void Because(Action act) => act();
        public TResult Because<TResult>(Func<TResult> act) => act();
        public TException BecauseThrows<TException>(Action act) where TException : Exception
        {
            try
            {
                act();
            }
            catch (TException ex)
            {
                return ex;
            }

            return null;
        }
        public void It(string assertionMessage, Action assertion) => assertion();
    }
}
