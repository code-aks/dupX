using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace dupfiles
{
    public class FileModel : IEquatable<FileModel>
    {
        const int bufferSize = int.MaxValue; // 1024 * sizeof(Int64) * 1000;

        public string FilePath { get; set; }

        public string FileName { get; set; }
        public uint CRC { get; set; }
        public string Hash { get; set; }
        public long Size { get; set; }
        public long Group { get; set; }

        public bool IsDelete { get; set; }

        public DateTime DateLastModified { get; set; }
        public DateTime DateCreated { get; set; }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + Size.GetHashCode();
            //hash = (hash * 7) + CRC.GetHashCode();
            return hash;
        }


        public override bool Equals(object obj)
        {
            var other = obj as FileModel;
            return this.Equals(other);
        }

        public bool Equals(FileModel other)
        {
            return FilesAreEqual(this, other).Result;
        }


        static async Task<bool> FilesAreEqual(FileModel firstFile, FileModel secondFile)
        {
            if (firstFile.Group != 0 && secondFile.Group != 0)
            {
                return firstFile.Group == secondFile.Group;
            }
            else
            {
                if (firstFile.Size == secondFile.Size && firstFile.Size == 0)
                    return true;

                if (firstFile.Size != secondFile.Size)
                    return false;

                //if (firstFile.CRC != secondFile.CRC)
                //   return false;
                //if (string.Equals(first.FullName, second.FullName, StringComparison.OrdinalIgnoreCase))
                //    return true;

                var fileInfo1 = new FileInfo(firstFile.FilePath);
                var fileInfo2 = new FileInfo(secondFile.FilePath);

                using (var file1 = fileInfo1.OpenRead())
                {
                    using (var file2 = fileInfo2.OpenRead())
                    {
                        return await StreamsContentsAreEqualAsync(file1, file2).ConfigureAwait(false);
                    }
                }

                // int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ);

                //return await StreamsContentsAreEqualAsync(file1, file2).ConfigureAwait(false);

            }
        }

        private static async Task<int> ReadFullBufferAsync(Stream stream, byte[] buffer)
        {
            int bytesRead = 0;
            while (bytesRead < buffer.Length)
            {
                int read = await stream.ReadAsync(buffer, bytesRead, buffer.Length - bytesRead).ConfigureAwait(false);
                if (read == 0)
                {
                    // Reached end of stream.
                    return bytesRead;
                }

                bytesRead += read;
            }

            return bytesRead;
        }

        private static async Task<bool> StreamsContentsAreEqualAsync(Stream stream1, Stream stream2)
        {
            const int bufferSize = 1024 * sizeof(Int64) * 100;
            var buffer1 = new byte[bufferSize];
            var buffer2 = new byte[bufferSize];

            while (true)
            {
                int count1 = await ReadFullBufferAsync(stream1, buffer1).ConfigureAwait(false);
                int count2 = await ReadFullBufferAsync(stream2, buffer2).ConfigureAwait(false);

                if (count1 != count2)
                {
                    return false;
                }

                if (count1 == 0)
                {
                    return true;
                }

                int iterations = (int)Math.Ceiling((double)count1 / sizeof(Int64));
                for (int i = 0; i < iterations; i++)
                {
                    if (BitConverter.ToInt64(buffer1, i * sizeof(Int64)) != BitConverter.ToInt64(buffer2, i * sizeof(Int64)))
                    {
                        return false;
                    }
                }
            }
        }
    }
}