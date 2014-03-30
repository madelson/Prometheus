using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus
{
    public sealed class InvalidCreatorException : Exception
    {
        internal InvalidCreatorException(ICreator creator, string message = null, Exception innerException = null)
            : base(BuildMessage(creator, message), innerException)
        {
        }

        private static string BuildMessage(ICreator creator, string message)
        {
            return string.Format("Unable to construct {0}{1}", creator.DisplayName, message.NullSafe(m => ": " + m));
        }

        internal interface ICreator 
        {
            string DisplayName { get; }
        }
    }

    internal static class InvalidCreatorExceptionHelpers
    {
        public static Exception Throw(this InvalidCreatorException.ICreator @this, string message)
        {
            throw new InvalidCreatorException(@this, message);
        }

        public static void ThrowIf(this InvalidCreatorException.ICreator @this, bool condition, string message)
        {
            if (condition)
            {
                throw new InvalidCreatorException(@this, message);
            }
        }

        public static TResult WrapWithException<TResult>(this InvalidCreatorException.ICreator @this, Func<TResult> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                throw new InvalidCreatorException(@this, innerException: ex);
            }
        }

        public static void WrapWithException(this InvalidCreatorException.ICreator @this, Action action)
        {
            @this.WrapWithException(() => { action(); return true; });
        }
    }
}
