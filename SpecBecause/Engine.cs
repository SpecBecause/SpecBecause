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
            if (capturedExceptions.Any())
                throw capturedExceptions[0];
        }
    }
}
