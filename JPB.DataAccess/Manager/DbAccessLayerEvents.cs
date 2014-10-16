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
    /// <summary>
    /// A database operation is done
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void DatabaseActionHandler(object sender, DatabaseActionEvent e);

    partial class DbAccessLayer
    {
        /// <summary>
        /// Should raise Instance bound Events
        /// </summary>
        public bool RaiseEvents { get; set; }

        /// <summary>
        /// Should raise non Instance bound Events
        /// </summary>
        public static bool RaiseStaticEvents { get; set; }

        public static event DatabaseActionHandler OnUnknownDelete;
        public event DatabaseActionHandler OnDelete;
        public static event DatabaseActionHandler OnUnknownSelect;
        public event DatabaseActionHandler OnKnownSelect;
        public static event DatabaseActionHandler OnUnknownUpdate;
        public event DatabaseActionHandler OnKnownUpdate;
        public static event DatabaseActionHandler OnUnknownInsert;
        public event DatabaseActionHandler OnKnownUInsert;

        protected static void RaiseUnknownDelete(IDbCommand query)
        {
            if (!RaiseStaticEvents)
                return;

            var handler = OnUnknownDelete;
            if (handler != null)
                handler.BeginInvoke(null, new DatabaseActionEvent(new QueryDebugger(query)), s => { }, null);
        }

        protected virtual void RaiseKnownDelete(IDbCommand query)
        {
            RaiseUnknownDelete(query);
            if (!RaiseEvents)
                return;
            //Async invoke
            var handler = OnDelete;
            if (handler != null)
                handler.BeginInvoke(this, new DatabaseActionEvent(new QueryDebugger(query)), s => { }, null);
        }

        protected static void RaiseUnknownSelect(IDbCommand query)
        {
            if (!RaiseStaticEvents)
                return;
            var handler = OnUnknownSelect;
            if (handler != null)
                handler.BeginInvoke(null, new DatabaseActionEvent(new QueryDebugger(query)), s => { }, null);
        }

        protected virtual void RaiseKnownSelect(IDbCommand query)
        {
            RaiseUnknownSelect(query);
            if (!RaiseEvents)
                return;
            //Async invoke
            var handler = OnKnownSelect;
            if (handler != null)
                handler.BeginInvoke(this, new DatabaseActionEvent(new QueryDebugger(query)), s => { }, null);
        }

        protected static void RaiseUnknwonUpdate(IDbCommand query)
        {
            if (!RaiseStaticEvents)
                return;

            var handler = OnUnknownUpdate;
            if (handler != null)
                handler.BeginInvoke(null, new DatabaseActionEvent(new QueryDebugger(query)), s => { }, null);
        }

        protected virtual void RaiseKnownUpdate(IDbCommand query)
        {
            RaiseUnknwonUpdate(query);
            if (!RaiseEvents)
                return;
            //Async invoke
            var handler = OnKnownUpdate;
            if (handler != null)
                handler.BeginInvoke(this, new DatabaseActionEvent(new QueryDebugger(query)), s => { }, null);
        }

        protected static void RaiseUnknwonInsert(IDbCommand query)
        {
            if (!RaiseStaticEvents)
                return;

            var handler = OnUnknownInsert;
            if (handler != null)
                handler.BeginInvoke(null, new DatabaseActionEvent(new QueryDebugger(query)), s => { }, null);
        }

        protected virtual void RaiseKnownInsert(IDbCommand query)
        {
            RaiseUnknwonInsert(query);
            if (!RaiseEvents)
                return;
            //Async invoke
            var handler = OnKnownUInsert;
            if (handler != null)
                handler.BeginInvoke(this, new DatabaseActionEvent(new QueryDebugger(query)), s => { }, null);
        }
    }
}
