using System;
using System.Collections.Generic;
using System.Linq;

namespace SpecBecause
{
    public class Engine : IDisposable
    {
        private List<Exception> capturedExceptions = new List<Exception>();

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
            catch (Exception ex)
            {
                throw new Exception("Act threw an unexpected exception.", ex);
            }

            return null;
        }
        public void It(string assertionMessage, Action assertion)
        {
            try
            {
                assertion();
            }
            catch (Exception ex)
            {
                capturedExceptions.Add(ex);
            }
        }

        public void Dispose()
        {
            if (capturedExceptions.Count == 1)
            {
                throw capturedExceptions[0];
            }
            else if (capturedExceptions.Count > 1)
            {
                throw new AggregateException(capturedExceptions);
            }
        }
    }
}
