using System;
using System.Collections.Generic;
using JPB.DataAccess.DbInfoConfig.DbInfo;
#pragma warning disable 1591

namespace JPB.DataAccess.Anonymous
{
    public class AnonymousClass : IDisposable
    {
        public AnonymousClass(DbClassInfoCache classInfo, WeakReference reference)
        {
            ClassInfo = classInfo;
            Reference = reference;
            Objects = new HashSet<AnonymousObject>();
        }

        public WeakReference Reference { get; private set; }
        public HashSet<AnonymousObject> Objects { get; private set; }
        public DbClassInfoCache ClassInfo { get; private set; }

        public void Dispose()
        {
            foreach (var anonymousObject in Objects)
            {
                anonymousObject.Dispose();
            }
            Objects.Clear();
            ClassInfo = null;
            Reference = null;
        }
    }
}