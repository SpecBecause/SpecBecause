using System;

namespace SpecBecause
{
    public interface IEngine: IDisposable
    {
        void Because(Action act);
        TResult Because<TResult>(Func<TResult> act);
        TException? BecauseThrows<TException>(Action act) where TException : Exception;
        void It(string assertionMessage, Action assertion);
    }
}