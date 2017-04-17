using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xx_udp_server
{
    public class xxLogManager
    {

        public static xxLogger Logger { get; set; }

        public static xxLogManager GetLogger(Type type)
        {
            return Logger != null ? Logger.CreateLogManager(type) : new xxLogManager();
        }

        public virtual void Debug(object msg) { }

        public virtual void DebugFormat(string msg, params object[] args)
        {
        }
        public virtual void Debug(object msg, Exception exception)
        {
        }

        public virtual void Info(object msg)
        {
        }

        public virtual void InfoFormat(string msg,params object[] args)
        {
        }
        public virtual void Info(object msg, Exception exception)
        {
        }

        public virtual void Warn(object msg)
        {
        }
        public virtual void Warn(object msg, Exception exception)
        {
        }

        public virtual void WarnFormat(string msg,params object[] args) { }

        public virtual void Error(object msg) { }

        public virtual void Error(object msg, Exception exception)
        {
        }

        public virtual void ErrorFormat(string msg, params object[] args)
        {
        }
        
    }
}
