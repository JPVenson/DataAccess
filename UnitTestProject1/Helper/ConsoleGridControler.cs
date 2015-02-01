using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UnitTestProject1
{
    public class ConsoleGridControler
    {
        public ConsoleGridControler()
        {
            ConsoleGrid = new ConsoleGrid();
            Commands = new List<IGridControlerCommand>();
        }

        public bool StopDispatcherLoop { get; set; }
        public object FocusedRow { get; set; }
        public int FocusedRowIndex { get; set; }
        public ConsoleGrid ConsoleGrid { get; set; }

        public List<IGridControlerCommand> Commands { get; private set; }

        public void Run()
        {
            var changed = true;
            while (!StopDispatcherLoop)
            {

                if (changed)
                    ConsoleGrid.RenderGrid();

                changed = false;

                var input = Console.ReadKey(false);
                var gridControlerCommand = Commands.Where(s => s.HandleKey).FirstOrDefault(s => s.Handle(input.Key.ToString().ToLower()));

                if (gridControlerCommand == null)
                {
                    switch (input.Key)
                    {
                        case ConsoleKey.DownArrow:
                            if (FocusedRowIndex < ConsoleGrid.SourceList.Count)
                            {
                                FocusedRowIndex++;
                                ConsoleGrid.FocusedItem = ConsoleGrid.SourceList[FocusedRowIndex - 1];
                                changed = true;
                            }
                            break;
                        case ConsoleKey.UpArrow:
                            if (FocusedRowIndex > 1)
                            {
                                FocusedRowIndex--;
                                ConsoleGrid.FocusedItem = ConsoleGrid.SourceList[FocusedRowIndex - 1];
                                changed = true;
                            }
                            break;
                        case ConsoleKey.Enter:

                            if (input.Modifiers == ConsoleModifiers.Shift)
                            {
                                var max = ConsoleGrid.SelectedItems.Max(s => ConsoleGrid.SourceList.IndexOf(s));
                                var min = ConsoleGrid.SelectedItems.Min(s => ConsoleGrid.SourceList.IndexOf(s));

                                if (max != -1 || min != -1)
                                {
                                    var orderAsc = FocusedRowIndex > max;

                                    for (int i = 0; i < ConsoleGrid.SourceList.Count; i++)
                                    {
                                        var source = ConsoleGrid.SourceList[i];
                                        if (ConsoleGrid.SelectedItems.Contains(source))
                                            continue;

                                        if (orderAsc)
                                        {
                                            if (i >= max && i < FocusedRowIndex)
                                            {
                                                ConsoleGrid.SelectedItems.Add(source);
                                            }
                                        }
                                        else
                                        {
                                            if (i >= FocusedRowIndex - 1 && i <= min)
                                            {
                                                ConsoleGrid.SelectedItems.Add(source);
                                            }
                                        }
                                    }
                                    changed = true;
                                    break;
                                }
                            }

                            if (input.Modifiers != ConsoleModifiers.Control)
                            {
                                ConsoleGrid.SelectedItems.Clear();
                            }
                            var val = ConsoleGrid.SourceList[FocusedRowIndex - 1];

                            if (ConsoleGrid.SelectedItems.Contains(val))
                            {
                                ConsoleGrid.SelectedItems.Remove(val);
                            }
                            else
                            {
                                ConsoleGrid.SelectedItems.Add(val);
                            }

                            changed = true;
                            break;
                        case ConsoleKey.Delete:
                            if (ConsoleGrid.SourceList.Any())
                            {
                                ConsoleGrid.SourceList.Remove(ConsoleGrid.SourceList[FocusedRowIndex - 1]);
                                if (FocusedRowIndex > 1)
                                {
                                    FocusedRowIndex--;
                                    ConsoleGrid.FocusedItem = ConsoleGrid.SourceList[FocusedRowIndex - 1];
                                }
                                else if (FocusedRowIndex < ConsoleGrid.SourceList.Count)
                                {
                                    FocusedRowIndex++;
                                    ConsoleGrid.FocusedItem = ConsoleGrid.SourceList[FocusedRowIndex - 1];
                                }
                            }
                            changed = true;
                            break;
                        default:
                            var fullInput = input.Key + Console.ReadLine();
                            changed = Commands.Where(s => s.HandleString).FirstOrDefault(s => s.Handle(input.Key.ToString())) != null;
                            break;
                    }
                }
                else
                {
                    changed = true;
                }
            }
        }
    }
}