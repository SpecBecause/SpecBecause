using System;
using System.Collections.Generic;

namespace SpecBecause
{
    public class Engine : IDisposable
    {
        private List<Exception> CapturedExceptions { get; } = new List<Exception>();
        private bool BecauseWasCalled { get; set; }
        private bool ItWasCalled { get; set; }


        public void Because(Action act)
        {
            if (ItWasCalled)
            {
                throw new Exception($"{nameof(Engine.Because)} cannot be called after {nameof(Engine.It)}.");
            }

            BecauseWasCalled = true;

            act();
        }

        public TResult Because<TResult>(Func<TResult> act)
        {
            if (ItWasCalled)
            {
                throw new Exception($"{nameof(Engine.Because)} cannot be called after {nameof(Engine.It)}.");
            }

            BecauseWasCalled = true;

            return act();
        }

        public TException? BecauseThrows<TException>(Action act) where TException : Exception
        {
            if (ItWasCalled)
            {
                throw new Exception($"{nameof(Engine.Because)} cannot be called after {nameof(Engine.It)}.");
            }

            BecauseWasCalled = true;

            try
            {
                act();
            }
            catch (TException ex)
            {
                return ex;
            }
            catch (Exception ex)
            {
                throw new Exception("Act threw an unexpected exception.", ex);
            }

            return null;
        }
        public void It(string assertionMessage, Action assertion)
        {
            if (!BecauseWasCalled)
            {
                throw new Exception($"{nameof(Engine.Because)} must be called before {nameof(Engine.It)}.");
            }

            ItWasCalled = true;

            try
            {
                assertion();
            }
            catch (Exception ex)
            {
                CapturedExceptions.Add(ex);
            }
        }

        public void Dispose()
        {
            if (!BecauseWasCalled || !ItWasCalled)
            {
                throw new Exception("Friendly reminder when using Engine you must call Because and It methods before disposing.");
            }

            if (CapturedExceptions.Count == 1)
            {
                throw CapturedExceptions[0];
            }
            else if (CapturedExceptions.Count > 1)
            {
                throw new AggregateException(CapturedExceptions);
            }
        }
    }
}
