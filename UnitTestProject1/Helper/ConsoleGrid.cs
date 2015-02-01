using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace UnitTestProject1
{
    public class ConsoleGrid
    {
        public ConsoleGrid()
        {
            ExpandConsole = true;
            ClearConsole = true;
            ObserveList = true;
            SelectedItems = new ObservableCollection<object>();
            SourceList = new ObservableCollection<object>();
            ConsolePropertyGridStyle = new DefaultConsolePropertyGridStyle();
            _extraInfos = new StringBuilder();
            Null = "{NULL}";
            RenderTypeName = true;
        }

        public Type Target { get; set; }
        public ObservableCollection<object> SourceList { get; set; }
        public ObservableCollection<object> SelectedItems { get; set; }

        public object FocusedItem
        {
            get { return _focusedItem; }
            set
            {
                _focusedItem = value;
                if(value != null)
                    this.RenderGrid();
            }
        }

        private StringBuilder _extraInfos;
        public StringBuilder ExtraInfos
        {
            get { return _extraInfos; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _extraInfos = value;
            }
        }

        private IConsolePropertyGridStyle _consolePropertyGridStyle;
        private object _focusedItem;

        public IConsolePropertyGridStyle ConsolePropertyGridStyle
        {
            get { return _consolePropertyGridStyle; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _consolePropertyGridStyle = value;
            }
        }

        /// <summary>
        /// If enabled, it will be tried to expand the console's size to its complete width
        /// If this is not possible UI Bugs will be visibile ... WIP
        /// </summary>
        public bool ExpandConsole { get; set; }

        /// <summary>
        /// Clear the console bevor drawing
        /// </summary>
        public bool ClearConsole { get; set; }

        /// <summary>
        /// Attach to the Source list and ReRender the grid when the items change
        /// </summary>
        public bool ObserveList { get; set; }

        /// <summary>
        /// The text render object for null
        /// </summary>
        public string Null { get; set; }

        /// <summary>
        /// Render a Sum text at bottom
        /// </summary>
        public bool RenderSum { get; set; }

        /// <summary>
        /// Clear the Additional Infos Builder after use
        /// </summary>
        public bool PersistendAdditionalInfos { get; set; }

        /// <summary>
        /// Add a Auto column with the Row number
        /// </summary>
        public bool RenderRowNumber { get; set; }

        public bool RenderTypeName { get; set; }

        public void RenderGrid()
        {
            var stream = new ColloredStringBuilder();

            SourceList.CollectionChanged -= SourceListOnCollectionChanged;

            if (ObserveList)
                SourceList.CollectionChanged += SourceListOnCollectionChanged;

            var size = 0;
            var fod = SourceList.FirstOrDefault();
            var length = SourceList.Count;

            if (fod == null)
            {
                return;
            }

            Target = fod.GetType();

            var props =
                Target.GetProperties().Select(s =>
                {
                    var name = s.Name;
                    if (RenderTypeName)
                    {
                        name = string.Format("{0} {{{1}}}", name, s.PropertyType.ToString());
                    }

                    var valueInformations = new ValueInformations()
                    {
                        GetValue = s.GetValue,
                        MaxContentSize = SourceList.Max(e =>
                        {
                            var value = s.GetValue(e);
                            if (value != null)
                            {
                                return value.ToString().Length;
                            }
                            return Null.ToString().Length;
                        }),
                        Name = name,
                    };

                    return valueInformations;
                }).ToList();


            if (RenderRowNumber)
            {
                int fakeId = 0;
                //fake new Column
                props.Insert(0, new ValueInformations()
                {
                    Name = "Nr",
                    MaxContentSize = length.ToString().Length,
                    GetValue = o => fakeId++
                });
            }

            foreach (var valueInfo in props)
            {
                valueInfo.Name = AlignValueToSize(valueInfo.Name, valueInfo.MaxSize);
                size += valueInfo.Name.Length;
            }

            var headerInfos = props.Select(s => s.Name).ToList();
            this.ConsolePropertyGridStyle.RenderHeader(stream, headerInfos);

            stream.AppendLine();

            if (Console.WindowWidth < size)
                Console.WindowWidth = size + 30;
            
            for (int i = 0; i < length; i++)
            {
                var item = SourceList[i];

                var selected = SelectedItems != null && SelectedItems.Contains(item);
                var focused = FocusedItem == item;

                this.ConsolePropertyGridStyle.BeginRenderProperty(stream, i, length.ToString().Length, selected, focused);

                for (int index = 0; index < props.Count; index++)
                {
                    var propertyInfo = props[index];
                    var value = propertyInfo.GetValue(item) ?? Null;
                    var norm = AlignValueToSize(value.ToString(), propertyInfo.MaxSize);
                    this.ConsolePropertyGridStyle.RenderNextProperty(stream, norm, index, selected, focused);
                }
                this.ConsolePropertyGridStyle.EndRenderProperty(stream, i, selected, focused);
                stream.AppendLine();
            }


            this.ConsolePropertyGridStyle.RenderFooter(stream, headerInfos);
            stream.AppendLine();

            if (RenderSum)
            {
                this.ConsolePropertyGridStyle.RenderSummary(stream, length);
                stream.AppendLine();
            }

            if (_extraInfos.Length > 0)
            {
                this.ConsolePropertyGridStyle.RenderAdditionalInfos(stream, _extraInfos);
                stream.AppendLine();
            }
            
            if (!PersistendAdditionalInfos)
                _extraInfos.Clear();
            
            if (ClearConsole)
                Console.Clear();
            stream.Render();
        }


        public static string AlignValueToSize(string source, int max)
        {
            var placeLeft = max - source.Length;

            int left = placeLeft / 2;
            int right = placeLeft / 2;

            if (placeLeft > 0 && placeLeft % 2 != 0)
            {
                left += 1;
            }

            var name = "";

            for (int j = 0; j < left; j++)
            {
                name += " ";
            }

            name += source;

            for (int r = 0; r < right; r++)
            {
                name += " ";
            }

            return name;
        }

        private void SourceListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RenderGrid();
        }

        public static void RenderList<T>(IEnumerable<T> @select)
        {
            var grid = new ConsoleGrid();
            grid.SourceList = new ObservableCollection<object>(@select.Cast<object>());
            grid.RenderGrid();
        }
    }
}