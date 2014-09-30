using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.DbEventArgs;
using JPB.DataAccess.DebuggerHelper;

namespace JPB.DataAccess.Manager
{
    public delegate void DatabaseActionHandler(object sender, DatabaseActionEvent e);

    partial class DbAccessLayer
    {
        public bool RaiseEvents { get; set; }
        public static bool RaiseStaticEvents { get; set; }

        public static event DatabaseActionHandler OnUnknownDelete;
        protected static void RaiseUnknownDelete(IDbCommand query)
        {
            if (!RaiseStaticEvents)
                return;

            var handler = OnUnknownDelete;
            if (handler != null)
                handler.BeginInvoke(null, new DatabaseActionEvent(new QueryDebugger(query)), s => { }, null);
        }

        public event DatabaseActionHandler OnDelete;
        protected virtual void RaiseKnownDelete(IDbCommand query)
        {
            if (!RaiseEvents)
                return;
            //Async invoke
            var handler = OnDelete;
            if (handler != null)
                handler.BeginInvoke(this, new DatabaseActionEvent(new QueryDebugger(query)), s => { }, null);
        }

        public static event DatabaseActionHandler OnUnknownSelect;
        protected static void RaiseUnknownSelect(IDbCommand query)
        {
            if (!RaiseStaticEvents)
                return;
            var handler = OnUnknownSelect;
            if (handler != null)
                handler.BeginInvoke(null, new DatabaseActionEvent(new QueryDebugger(query)), s => { }, null);
        }

        public event DatabaseActionHandler OnKnownSelect;
        protected virtual void RaiseKnownSelect(IDbCommand query)
        {
            if (!RaiseEvents)
                return;
            //Async invoke
            var handler = OnKnownSelect;
            if (handler != null)
                handler.BeginInvoke(this, new DatabaseActionEvent(new QueryDebugger(query)), s => { }, null);
        }

        public static event DatabaseActionHandler OnUnknownUpdate;
        protected static void RaiseUnknwonUpdate(IDbCommand query)
        {
            if (!RaiseStaticEvents)
                return;

            var handler = OnUnknownUpdate;
            if (handler != null)
                handler.BeginInvoke(null, new DatabaseActionEvent(new QueryDebugger(query)), s => { }, null);
        }

        public event DatabaseActionHandler OnKnownUpdate;
        protected virtual void RaiseKnownUpdate(IDbCommand query)
        {
            if (!RaiseEvents)
                return;
            //Async invoke
            var handler = OnKnownUpdate;
            if (handler != null)
                handler.BeginInvoke(this, new DatabaseActionEvent(new QueryDebugger(query)), s => { }, null);
        }


        public static event DatabaseActionHandler OnUnknownInsert;
        protected static void RaiseUnknwonInsert(IDbCommand query)
        {
            if (!RaiseStaticEvents)
                return;

            var handler = OnUnknownInsert;
            if (handler != null)
                handler.BeginInvoke(null, new DatabaseActionEvent(new QueryDebugger(query)), s => { }, null);
        }

        public event DatabaseActionHandler OnKnownUInsert;
        protected virtual void RaiseKnownInsert(IDbCommand query)
        {
            if (!RaiseEvents)
                return;
            //Async invoke
            var handler = OnKnownUInsert;
            if (handler != null)
                handler.BeginInvoke(this, new DatabaseActionEvent(new QueryDebugger(query)), s => { }, null);
        }
    }
}
