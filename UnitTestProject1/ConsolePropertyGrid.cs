using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using testing;

namespace UnitTestProject1
{
    public class ConsolePropertyGrid
    {
        public ConsolePropertyGrid()
        {
            ExpandConsole = true;
            ClearConsole = true;
        }
        public StringBuilder ExtraInfos = new StringBuilder();

        public Type Target { get; set; }

        public ObservableCollection<object> SourceList { get; set; }

        public bool ExpandConsole { get; set; }

        public bool ClearConsole { get; set; }

        public void RenderGrid()
        {
            var stream = new StringBuilder();
            SourceList.CollectionChanged += SourceListOnCollectionChanged;

            stream.Append("o ");

            var size = 0;

            var FirstItem = SourceList.FirstOrDefault();

            if (FirstItem == null)
            {
                return;
            }

            Target = FirstItem.GetType();

            var props = Target.GetProperties();

            var propNames = new Dictionary<string, int>();

            foreach (var propertyInfo in props)
            {
                var headerLength = propertyInfo.Name.Length + propertyInfo.PropertyType.ToString().Length;
                var maxContentLength = headerLength;

                var propertyIsLargerThenValues = true;
                foreach (var item in SourceList)
                {
                    var value = propertyInfo.GetValue(item);
                    if (value == null)
                        value = DBNull.Value;
                    var s = value.ToString();
                    if (maxContentLength < s.Length)
                    {
                        propertyIsLargerThenValues = false;
                        maxContentLength = s.Length;
                    }
                }

                //check for Modulo

                int left = (maxContentLength - headerLength) / 2;
                int right = left;

                if (maxContentLength % 2 != 0)
                {
                    left += 1;
                }

                var name = "";

                if (!propertyIsLargerThenValues)
                    for (int i = 0; i < left; i++)
                    {
                        name += " ";
                    }

                name += string.Format(@"{0} {{ {1} }}", propertyInfo.Name, propertyInfo.PropertyType);

                if (!propertyIsLargerThenValues)
                    for (int r = 0; r < right; r++)
                    {
                        name += " ";
                    }

                name += " | ";

                propNames.Add(name, maxContentLength);
                size += name.Length;
            }
            var enumerable = propNames.Select(s => s.Key.Length).Aggregate((e, f) => e + f) - 2;

            for (int i = 0; i < enumerable - 1; i++)
            {
                stream.Append("-");
            }

            stream.Append(" o");

            stream.AppendLine();

            stream.Append("| ");

            foreach (var propName in propNames.Select(s => s.Key))
            {
                stream.Append(propName);
            }

            stream.AppendLine();

            stream.Append("| ");

            for (int i = 0; i < enumerable; i++)
            {
                stream.Append("-");
            }

            stream.Append("|");

            stream.AppendLine();

            if (Console.WindowWidth < size)
                Console.WindowWidth = size + 30;

            for (int i = 0; i < SourceList.Count; i++)
            {
                stream.Append("| ");
                var item = SourceList[i];
                for (int index = 0; index < props.Length; index++)
                {
                    var propertyInfo = props[index];
                    var propLength = propNames.FirstOrDefault(s => s.Key.Contains(propertyInfo.Name));

                    var value = propertyInfo.GetValue(item);

                    if (value == null)
                        value = "{Null}";

                    var items = value.ToString();

                    var placeLeft = propLength.Key.Length - 3 - items.Length;

                    int left = placeLeft / 2;
                    int right = placeLeft / 2;

                    if (placeLeft % 2 != 0)
                    {
                        left += 1;
                    }

                    var name = "";

                    for (int j = 0; j < left; j++)
                    {
                        name += " ";
                    }

                    name += items;

                    for (int r = 0; r < right; r++)
                    {
                        name += " ";
                    }

                    stream.Append(name + " | ");
                }
                stream.AppendLine();
            }

            stream.Append("o ");

            for (int i = 0; i < enumerable - 1; i++)
            {
                stream.Append("-");
            }

            stream.Append(" o");
            stream.AppendLine();

            stream.Append(SourceList.Count + " items");

            stream.AppendLine();

            stream.Append(ExtraInfos.ToString());
            if (ClearConsole)
                Console.Clear();

            ExtraInfos.Clear();
            Console.WriteLine(stream.ToString());
        }

        private void SourceListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RenderGrid();
        }

        public static void RenderList<T>(IEnumerable<T> @select)
        {
            var grid = new ConsolePropertyGrid();
            grid.SourceList = new ObservableCollection<object>(@select.Cast<object>());
            grid.RenderGrid();
        }
    }
}