using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace dupfiles
{
    class Program
    {
        static void Main(string[] args)
        {

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Enter folder paths separated by a semicolon as arguments");
                Console.WriteLine("Windows: dupX.exe \"C:\\Downloads;D:\\Folder Path\"");
                Console.WriteLine("Linux: ./dupX \"C:\\Downloads;D:\\Folder Path\"");
                return;
            }
            var dirs = args[0].Split(";").Select(x => x.Trim('"'));

            Console.WriteLine("Searching for Duplicate files in following paths:");
            foreach (var dir in dirs)
            {
                Console.WriteLine(dir);
            }
            
            Console.WriteLine("Press any key to continue searching files");
            
            Console.ReadLine();

            var dupFinder = new DuplicateFinderConcurrent();
            var allDuplicates = dupFinder.Process(dirs).Result;

            var dupCleaner = new DuplicateCLeaner();
            dupCleaner.CLeanDuplicates(allDuplicates);
        }
    }
}
