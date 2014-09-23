using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitTestProject1
{
    public class ConsolePropertyGrid
    {
        public StringBuilder ExtraInfos = new StringBuilder();

        public Type Target { get; set; }

        public List<object> SourceList { get; set; }

        public void RenderGrid()
        {
            var stream = new StringBuilder();

            stream.Append("o ");

            var size = 0;

            var props = Target.GetProperties();

            var propNames = new Dictionary<string, int>();

            foreach (var propertyInfo in props)
            {
                var maxContentLength = propertyInfo.Name.Length;

                foreach (var item in SourceList)
                {
                    var value = propertyInfo.GetValue(item);
                    if (value == null)
                        value = DBNull.Value;
                    var s = value.ToString();
                    if (maxContentLength < s.Length)
                    {
                        maxContentLength = s.Length;
                    }
                }

                //check for Modulo

                int left = maxContentLength / 2;
                int right = maxContentLength / 2;

                if (maxContentLength % 2 != 0)
                {
                    left += 1;
                }

                var name = "";

                for (int i = 0; i < left; i++)
                {
                    name += " ";
                }

                name += propertyInfo.Name;

                for (int r = 0; r < right; r++)
                {
                    name += " ";
                }

                name += " | ";

                propNames.Add(name, maxContentLength);
                size += propertyInfo.Name.Length;
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
                        value = DBNull.Value;

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

                    //if (items.Length < propLength.Value)
                    //{
                    //    for (int j = 0; j < propLength.Value / items.Length; j++)
                    //    {
                    //        items += " ";
                    //    }
                    //}

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

            Console.Clear();
            ExtraInfos.Clear();
            Console.WriteLine(stream.ToString());
        }
    }
}