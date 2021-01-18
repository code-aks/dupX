using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace dupfiles
{
    public class DuplicateCLeaner
    {
        public void CLeanDuplicates(List<FileModel> allDuplicates)
        {

            var stopWatch = new Stopwatch();
            var nonDupSize = allDuplicates.GroupBy(x => x.Group).Select(x => x.First().Size).Sum();
            var totalDupSize = allDuplicates.Sum(x => x.Size);
            var potentialSaving = totalDupSize - nonDupSize;

            Console.WriteLine($"{allDuplicates.Select(x => x.Group).Distinct().Count()} groups found with {allDuplicates.Count()} duplicate files. Total space wasted from duplicate files { FormatBytes(potentialSaving)}. All duplicates grouped in :{stopWatch.Elapsed}");
            stopWatch.Start();

            Console.WriteLine("Duplicate Results:");

            foreach (var item in allDuplicates)
            {
                Console.WriteLine($" Group= {item.Group} , Size= {item.Size}, Path = {item.FilePath}");
            }

            stopWatch.Stop();
            Console.WriteLine($"Time: {stopWatch.Elapsed}");

            if (allDuplicates.Count() > 0)
            {
                Console.WriteLine("Delete Duplicates : Press 1 for intelligent Delete. Press 2 to delete newer and retain the oldest, 3 to delete older and retain newest:");

                var response = Console.ReadLine();

                DeleteDuplicates(allDuplicates, response.Trim());
            }

            Console.ReadLine();
        }

        private static void DeleteDuplicates(IEnumerable<FileModel> allDuplicates, string input, bool reallyDelete = false)
        {
            Console.WriteLine("Processing Duplicates");
            foreach (var group in allDuplicates.GroupBy(x => x.Group))
            {
                FileModel retain = null;
                switch (input)
                {
                    case "1":
                        {
                            retain = group.OrderByDescending(x => x.FileName.Where(Char.IsLetter).Count()).ThenBy(x => x.DateLastModified).FirstOrDefault();
                            break;
                        }
                    case "2":
                        {
                            retain = group.OrderBy(x => x.DateLastModified).First();
                            break;
                        }
                    case "3":
                        {
                            retain = group.OrderByDescending(x => x.DateLastModified).First();
                            break;
                        }
                    default:
                        {
                            return;
                        }
                }

                foreach (var item in group)
                {

                    item.IsDelete = item != retain;
                    var isRetain = !item.IsDelete ? "*" : "";
                    if (!item.IsDelete)
                    {
                        Console.BackgroundColor = ConsoleColor.Green;
                    }
                    Console.WriteLine($" Group= {item.Group} , Size= {item.Size}, Path = {item.FilePath}, DLM = {item.DateLastModified}, {isRetain}");
                    Console.ResetColor();
                }
            }

            if (!reallyDelete)
            {
                Console.WriteLine("Press 0 to confirm deleting!");
                var del = Console.ReadLine();
                if (del.Trim() == "0")
                {
                    DeleteDuplicates(allDuplicates, input, true);
                }
            }
            else
            {
                var toDeleteFiles = allDuplicates.Select(x => x).Where(x => x.IsDelete);
                toDeleteFiles.AsParallel().ToList().ForEach(y =>
                {
                    if (y.IsDelete)
                    {
                        File.Delete(y.FilePath);
                    }
                });
                Console.WriteLine($"Deleted {toDeleteFiles.Count()} files, saved {FormatBytes(toDeleteFiles.Sum(x => x.Size))} !");
            }
        }

        private static string FormatBytes(long bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return String.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }
    }
}