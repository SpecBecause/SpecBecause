using System;

namespace SpecBecause
{
    public class Engine
    {
        public void Because(Action act) => act();
        public TResult Because<TResult>(Func<TResult> act) => act();
        public void It(string assertionMessage, Action assertion) => assertion();
    }
}
