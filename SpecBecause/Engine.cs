using System;
using System.Collections.Generic;

namespace SpecBecause
{
    public class Engine : IEngine
    {
        private CallState State { get; } = new CallState();
        private List<Exception> CapturedExceptions { get; } = new List<Exception>();


        public void Because(Action act)
        {
            State.SetBecauseWasCalled();

            act();
        }

        public TResult Because<TResult>(Func<TResult> act)
        {
            State.SetBecauseWasCalled();

            return act();
        }

        public TException? BecauseThrows<TException>(Action act) where TException : Exception
        {
            State.SetBecauseWasCalled();

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
                throw new EngineException("Act threw an unexpected exception.", ex);
            }

            return null;
        }
        public void It(string assertionMessage, Action assertion)
        {
            State.SetItWasCalled();

            try
            {
                assertion();
            }
            catch (Exception ex)
            {
                CapturedExceptions.Add(new EngineException(assertionMessage, ex));
            }
        }

        public void Dispose()
        {
            if (!State.IsReadyForDisposal)
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


        private class CallState
        {
            private bool BecauseWasCalled { get; set; }
            private bool ItWasCalled { get; set; }
            public bool IsReadyForDisposal => BecauseWasCalled && ItWasCalled;

            public void SetBecauseWasCalled()
            {
                if (ItWasCalled)
                {
                    throw new Exception($"{nameof(Engine.Because)} cannot be called after {nameof(Engine.It)}.");
                }

                BecauseWasCalled = true;
            }

            public void SetItWasCalled()
            {
                if (!BecauseWasCalled)
                {
                    throw new Exception($"{nameof(Engine.Because)} must be called before {nameof(Engine.It)}.");
                }

                ItWasCalled = true;
            }
        }
    }
}
