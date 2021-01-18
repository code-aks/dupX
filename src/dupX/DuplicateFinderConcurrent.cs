using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace dupfiles
{
    public class DuplicateFinderConcurrent
    {
        public async Task<List<FileModel>> Process(IEnumerable<string> paths)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            //var models = new List<FileModel>();
            var models = await this.GetFileModels(paths);

            timer.Stop();
            Console.WriteLine($"{models.Length} files found in :{timer.Elapsed}");

            //timer.Restart();


            var candidateGroup = models.OrderByDescending(x => x.Size).GroupBy(x => x.Size).Where(g => g.Count() > 1).ToList();
            //.OrderByDescending(x => x.Key).ToList();

            //timer.Stop();
            //Console.WriteLine($"Found {candidateGroup.Count()}  candidate groups in :{timer.Elapsed}");

            timer.Restart();
            var pGroups = candidateGroup.Select(x => x.AsParallel()
                   .Select(y => y).GroupBy(z => z).Where(g => g.Count() > 1).ToList()).SelectMany(x => x).ToList();

            timer.Stop();
            Console.WriteLine($"Found {pGroups.Count()}  candidate groups in :{timer.Elapsed}");

            timer.Restart();

            //var groups = candidateGroup.SelectMany(x => x).GroupBy(x => x).Where(g => g.Count() > 1);

            var result = this.MarkGroupNumber(pGroups);

            timer.Stop();

            return result;
        }

        private List<FileModel> MarkGroupNumber(IEnumerable<IGrouping<FileModel, FileModel>> fileModels)
        {
            var result = new List<FileModel>();
            int groupCount = 0;
            foreach (var group in fileModels)
            {
                groupCount++;
                foreach (var item in group)
                {
                    item.Group = groupCount;
                    result.Add(item);
                }
            }
            return result;
        }

        public async Task<FileModel[]> GetFileModels(IEnumerable<string> paths)
        {
            var files = paths.SelectMany(x => Directory.EnumerateFiles(x, "*", SearchOption.AllDirectories));

            return await Task.WhenAll(files.Select(item => Task.Run(() => GetFileModel(item))));
        }

        private FileModel GetFileModel(string path)
        {
            var fi = new FileInfo(path);
            var fm = new FileModel();

            fm.FilePath = fi.FullName;
            fm.Size = fi.Length;
            fm.DateCreated = fi.CreationTime;
            fm.DateLastModified = fi.LastWriteTime;
            fm.FileName = fi.Name;
            fm.Hash = string.Format($"{fm.Size}");
            return fm;
        }

    }
}