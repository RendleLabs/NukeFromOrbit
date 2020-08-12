using System;
using System.IO;
using System.Threading.Tasks;

namespace NukeFromOrbit
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var directory = Environment.CurrentDirectory;
            if (args.Length > 0 && Directory.Exists(args[0]))
            {
                directory = args[0];
            }

            var nuker = await Nuker.CreateAsync(directory);
            
            nuker.Nuke();
        }
    }
}
