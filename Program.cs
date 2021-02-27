using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace du {
    class Program {
        static void Main(string[] args) {
            
            if (args.Length != 2) {
                Console.WriteLine("Usage: dotnet run --  [-s] [-p] [-b] <path>");
                Environment.Exit(1);
            }
            var command = args[0];
            var path = args[1];
            DirectoryInfo di;
            
            di = new DirectoryInfo(path);

            if (!di.Exists){
                Console.WriteLine("Error: path does not exists");
                Environment.Exit(1);
            }
            
            Console.WriteLine("Directory: '{0}'\n", di.FullName);

            switch (command) {
                case "-s":
                    sRun(path);
                    break;
                case "-p":
                    pRun(path);
                    break;
                case "-b":
                    pRun(path);
                    Console.WriteLine();
                    sRun(path);
                    break;
                default:
                    Console.WriteLine("Error: {0} is not a parameter", command);
                    break;
            }
        }

        public static void sRun(string path) {
            var start = DateTime.Now;
            var s = new SequentialRun();
            s.seqCounter(path);
            var end = DateTime.Now;
            s.Results(end-start);
        }
        
        public static void pRun(string path) {
            var start = DateTime.Now;
            var p = new ParallelRun();
            p.parCounter(path);
            var end = DateTime.Now;
            p.Results(end-start);
        }
        
        /// <summary>
        /// Sequential run
        /// </summary>
        public class SequentialRun
        {
            private int folders, files;
            private long bytes;
            public SequentialRun() {
                folders = 0;
                files = 0;
                bytes = 0;
            }

            public void seqCounter(string path) {
                
                string[] fileList = {};
                string[] directoryList = {};
                try
                {
                    fileList = Directory.GetFiles(path);
                }catch{}

                files += fileList.Length;
                
                foreach (var file in fileList) {
                    try
                    {
                        var fi = new FileInfo(file);
                        bytes += fi.Length;
                    }catch{}
                }

                try
                {
                    directoryList = Directory.GetDirectories(path);
                }catch{}

                foreach (var dir in directoryList) {
                    folders++;
                    seqCounter(dir);
                }
            }

            public void Results(TimeSpan ts) {
                Console.WriteLine("Sequential Calculated in: {0}s\n{1:n0} folders, " +
                                  "{2:n0} files, {3:n0} bytes", ts.TotalSeconds, folders, files, bytes);
            }
        }

        public class ParallelRun {
            private int folders, files;
            private long bytes;

            public ParallelRun() {
                folders = 0;
                files = 0;
                bytes = 0;
            }

            public void parCounter(string path) {
                string[] fileList = {};
                string[] directoryList = {};

                try
                {
                    fileList = Directory.GetFiles(path);
                }catch{}

                Interlocked.Add(ref files, fileList.Length);
                Parallel.ForEach(fileList, file => {
                    try
                    {
                        var fi = new FileInfo(file);
                        Interlocked.Add(ref bytes, fi.Length);
                    }catch{}
                });

                try
                {
                    directoryList = Directory.GetDirectories(path);
                }catch{}

                Parallel.ForEach(directoryList, dir =>
                {
                    Interlocked.Increment(ref folders);
                    parCounter(dir);
                });
            }

            public void Results(TimeSpan ts) {
                Console.WriteLine("Parallel Calculated in: {0}s\n{1:n0} folders, " +
                                  "{2:n0} files, {3:n0} bytes", ts.TotalSeconds, folders, files, bytes);
            }
        }
    }
}