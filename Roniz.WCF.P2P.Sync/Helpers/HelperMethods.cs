using System;
using System.Diagnostics.Contracts;
using Roniz.Diagnostics.Logging;

namespace Roniz.WCF.P2P.Sync.Helpers
{
    static class HelperMethods
    {
        /// <summary>
        /// helper method to handle every business logic exception
        /// </summary>
        /// <param name="exception">the exception from the business logic layer</param>
        public static void HandleBusinessLogicException(Exception exception)
        {
            Contract.Requires<ArgumentNullException>(exception != null);
            LogManager.GetCurrentClassLogger().Error(exception, ExceptionLoggingOptions.Full, Properties.Resources.ExceptionCatchFromBusinessLogic);
        }
    }
}
