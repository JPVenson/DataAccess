using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Pager.Contracts;


namespace JPB.DataAccess.AdoWrapper.MsSql
{
    public class MsSqlUntypedDataPager<T> : IDataPager<T>
    {
        public MsSqlUntypedDataPager()
        {
            CurrentPage = 0;
            PageSize = 10;
            AppendedComands = new List<IDbCommand>();
            CurrentPageItems = new ObservableCollection<T>();

            SyncHelper = action => action();
        }

        public bool Cache
        {
            get { return _cache; }
            set
            {
                if (value)
                    throw new Exception("To be supported ... sory");
                _cache = value;
            }
        }

        public bool RaiseEvents { get; set; }

        /// <summary>
        /// Raised if new Page is loading
        /// </summary>
        public event Action NewPageLoading;

        /// <summary>
        /// Raised if new page is Loaded
        /// </summary>
        public event Action NewPageLoaded;

        protected virtual void RaiseNewPageLoaded()
        {
            if (!RaiseEvents)
                return;
            var handler = NewPageLoaded;
            if (handler != null) handler();
        }

        public List<IDbCommand> AppendedComands { get; set; }

        protected virtual void RaiseNewPageLoading()
        {
            if (!RaiseEvents)
                return;
            var handler = NewPageLoading;
            if (handler != null) handler();
        }

        public long CurrentPage
        {
            get { return _currentPage; }
            set
            {
                if (value >= 0)
                    _currentPage = value;
            }
        }

        public long MaxPage { get; private set; }

        protected string SqlVersion;

        public int PageSize { get; set; }

        public ICollection<T> CurrentPageItems { get; protected set; }

        public Type TargetType { get; set; }

        public void LoadPage(DbAccessLayer dbAccess)
        {
            List<dynamic> selectWhere = null;
            dbAccess.Database.RunInTransaction(s =>
            {
                var finalCOmmands = new List<IDbCommand>();
                IDbCommand finalAppendCommand = null;
                if (AppendedComands.Any())
                {
                    finalAppendCommand = AppendedComands.Aggregate(DbAccessLayer.CreateCommand(s, "WHERE"), (current, comand) => DbAccessLayer.ConcatCommands(s, current, comand));
                }

                if (string.IsNullOrEmpty(SqlVersion))
                {
                    SqlVersion = dbAccess.RunPrimetivSelect<string>("SELECT SERVERPROPERTY('productversion')").FirstOrDefault();
                }

                SyncHelper(CurrentPageItems.Clear);

                var pk = TargetType.GetPK();

                var selectMaxCommand = DbAccessLayer.CreateCommand(s, "SELECT COUNT( * ) AS NR FROM " + TargetType.GetTableName());

                if (finalAppendCommand != null)
                    selectMaxCommand = DbAccessLayer.ConcatCommands(s, selectMaxCommand, finalAppendCommand);

                var maxItems = dbAccess.RunPrimetivSelect(typeof(long), selectMaxCommand).FirstOrDefault();
                if (maxItems != null)
                {
                    long parsedCount;
                    long.TryParse(maxItems.ToString(), out parsedCount);
                    MaxPage = ((long)parsedCount) / PageSize;
                }

                //Check select strategy
                //IF version is or higher then 11.0.2100.60 we can use OFFSET and FETCH
                //esle we need to do it the old way

                RaiseNewPageLoading();
                IDbCommand command;

                if (CheckVersionForFetch())
                {
                    command = DbAccessLayer.CreateSelect(TargetType, s,
                        "ORDER BY " + pk + " ASC OFFSET @PagedRows ROWS FETCH NEXT @PageSize ROWS ONLY", new IQueryParameter[]
                    {
                        new QueryParameter("PagedRows", CurrentPage*PageSize),
                        new QueryParameter("PageSize", PageSize),
                    });
                }
                else
                {
                    //Write as CTE to filter on it
                    var queryBuilde = new StringBuilder();
                    queryBuilde.Append("WITH CTE AS (SELECT ");
                    //queryBuilde.Append(DbAccessLayer.CreatePropertyCSV(TargetType));
                    queryBuilde.Append("*");
                    queryBuilde.Append(" FROM (");
                    queryBuilde.Append("SELECT ROW_NUMBER() OVER (ORDER BY @Pk)");
                    queryBuilde.Append(" AS NUMBER, *");
                    queryBuilde.Append(" FROM ");
                    queryBuilde.Append(TargetType.GetTableName());
                    queryBuilde.Append(") AS TBL ");
                    queryBuilde.Append("WHERE NUMBER BETWEEN ((@PagedRows) * @PageSize + 1) AND ((@PagedRows + 1) * @PageSize)");
                    queryBuilde.Append(") ");

                    queryBuilde.Append("SELECT * FROM CTE ");

                    var parameters = new List<IQueryParameter>(new[]
                    {
                        new QueryParameter("Pk", pk),
                        new QueryParameter("PagedRows", CurrentPage),
                        new QueryParameter("PageSize", PageSize),
                    });

                    command = DbAccessLayer.CreateCommandWithParameterValues(queryBuilde.ToString(), s, parameters);
                }

                if (finalAppendCommand != null)
                    command = DbAccessLayer.ConcatCommands(s, command, finalAppendCommand);

                selectWhere = DbAccessLayer.SelectNative(TargetType, s, command);
            });

            foreach (var item in selectWhere)
            {
                dynamic item1 = item;
                SyncHelper(() => CurrentPageItems.Add(item1));
            }

            if (CurrentPage > MaxPage)
                CurrentPage = MaxPage;

            RaiseNewPageLoaded();
        }

        ICollection IDataPager.CurrentPageItems
        {
            get { return new ArrayList(this.CurrentPageItems.ToArray()); }
        }

        private bool? _checkRun;
        private Action<Action> _syncHelper;
        private long _currentPage;
        private bool _cache;

        private bool CheckVersionForFetch()
        {
            if (_checkRun != null)
                return _checkRun.Value;


            var versionParts = SqlVersion.Split('.');

            //Target 11.0.2100.60 or higher
            //      Major Version
            //      Minor Version 
            //      Build Number
            //      Revision

            var major = int.Parse(versionParts[0]);
            var minor = int.Parse(versionParts[1]);
            var build = int.Parse(versionParts[2]);
            var revision = int.Parse(versionParts[3]);

            if (major > 11)
            {
                _checkRun = true;
            }
            else if (major == 11)
            {
                if (minor > 0)
                {
                    _checkRun = true;
                }
                else if (minor == 0)
                {
                    if (build > 2100)
                    {
                        _checkRun = true;
                    }
                    else if (build == 2100)
                    {
                        if (revision >= 60)
                        {
                            _checkRun = true;
                        }
                    }
                }
            }
            else
            {
                _checkRun = false;
            }

            return _checkRun != null && _checkRun.Value;
        }

        public Action<Action> SyncHelper
        {
            get { return _syncHelper; }
            set
            {
                if (value != null)
                    _syncHelper = value;
            }
        }
    }
}