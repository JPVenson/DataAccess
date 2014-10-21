using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.EntryCreator.MsSql;

namespace JPB.DataAccess.EntryCreator
{
    public class Program
    {
        public Program()
        {

        }

        static void Main(string[] args)
        {
            var prog = new Program();
            if (!args.Any() || args.First().ToLower() == @"\?")
            {
                prog.RenderHelp();
            }
            var outputDir = @"D:\";
            string connectionString = "Data Source=(localdb)\\Projects;Initial Catalog=TestDB;Integrated Security=True;";

#if DEBUG


#else
            Console.WriteLine("Enter output dir");

            outputDir = Console.ReadLine();
            if (string.IsNullOrEmpty(outputDir) || !Directory.Exists(outputDir))
            {
                Console.WriteLine("Invalid Directory ...");
                Console.ReadKey();
                return;
            }
            Console.WriteLine(@"Enter Connection string or type \explore to search for a server [ToBeSupported]");
            
            do
            {
                connectionString = Console.ReadLine();
                if (connectionString == @"\explore")
                    Console.WriteLine("To be supported ... please enter a Connection string");
            } while (string.IsNullOrEmpty(connectionString) || connectionString.ToLower().Contains(@"\explore"));
#endif



            new MsSqlCreator().CreateEntrys(connectionString, outputDir, string.Empty);
            Console.ReadKey();
        }

        private void RenderHelp()
        {
            Console.WriteLine("Creates cs Entrys based on a Database ( currently MsSQL only )");
            Console.WriteLine();
            Console.WriteLine("Usage: output [[/con]] ");
            Console.WriteLine();
            Console.WriteLine("\t output     A existing Directory where all cs classes are written to");
            Console.WriteLine("\t /con       Specifys the Connection property to a existing Database");
        }
    }
}
