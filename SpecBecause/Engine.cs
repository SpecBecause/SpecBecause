using System;

namespace SpecBecause
{
    public class Engine
    {
        public void Because(Action act) => act();
        public TResult Because<TResult>(Func<TResult> act) => act();
    }
}
