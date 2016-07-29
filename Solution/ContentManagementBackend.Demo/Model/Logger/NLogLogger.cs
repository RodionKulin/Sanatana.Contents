using ContentManagementBackend.Demo.App_Resources;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.Demo
{
    public class NLogLogger : ICommonLogger
    {
        //поля
        private NLog.Logger _logger;

        
        //инициализация
        public NLogLogger()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
        }


        //Debug
        public void Debug(string message)
        {
            _logger.Debug(message);
        }
        public void Debug(string message, params object[] parameters)
        {
            _logger.Debug(message, parameters);
        }


        //Info
        public void Info(string message)
        {
            _logger.Info(message);
        }
        public void Info(string message, params object[] parameters)
        {
            _logger.Info(message, parameters);
        }

        
        //Error
        public void Error(string message)
        {
            _logger.Error(message);
        }
        public void Error(string message, params object[] parameters)
        {
            message = string.Format(message, parameters);
            _logger.Error(message);
        }

        

        //Exception
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Exception(Exception exception)
        {
            StackFrame callingMethodFrame = new StackFrame(1);
            MethodBase callingMethod = callingMethodFrame.GetMethod();
            string message = string.Format(InnerMessages.NLogLogger_ExceptionFormat
                , callingMethod.Name, callingMethod.DeclaringType.FullName);
            
            _logger.Error(exception, message);
        }
        public void Exception(Exception exception, string message)
        {
            _logger.Error(exception, message);
        }
        public void Exception(Exception exception, string message, params object[] parameters)
        {
            message = string.Format(message, parameters);
            _logger.Error(exception, message);
        }


        //IDisposable
        public void Dispose()
        {
        }
    }
}
