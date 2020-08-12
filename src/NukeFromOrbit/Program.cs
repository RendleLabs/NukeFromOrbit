using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

namespace NukeFromOrbit
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var command = CreateRootCommand();
            await command.InvokeAsync(args);
        }

        private static RootCommand CreateRootCommand()
        {
            var command = new RootCommand
            {
                new Option<bool>(new[]{"--yes", "-y"}, () => false, "Don't ask for confirmation, just nuke it."),
                new Option<bool>(new[]{"--dry-run", "-n"}, () => false, "List items that will be nuked but don't nuke them."),
                new Argument<string>("workingDirectory", () => Environment.CurrentDirectory)
            };

            command.Description = "Dust off and nuke bin and obj directories from orbit. It's the only way to be sure.";
            
            command.Handler = CommandHandler.Create<bool, bool, string>(async (yes, dryRun, workingDirectory) =>
            {
                await Nuke(yes, dryRun, workingDirectory);
            });

            return command;
        }

        private static async Task Nuke(bool yes, bool dryRun, string workingDirectory)
        {
            var nuker = await Nuker.CreateAsync(workingDirectory);
            var items = nuker.GetItemsToBeNuked();
            if (dryRun)
            {
                OutputDryRun(items);
            }
            else if (yes || Confirm(items))
            {
                Console.WriteLine();
                nuker.NukeItems(items);
            }
        }

        private static bool Confirm(IReadOnlyCollection<DeleteItem> items)
        {
            OutputDryRun(items);
            Console.Write("Do you want to delete these items? Y/N: ");
            var yn = Console.ReadLine();
            return yn.Equals("Y", StringComparison.CurrentCultureIgnoreCase);
        }

        private static void OutputDryRun(IEnumerable<DeleteItem> items)
        {
            Console.WriteLine();
            foreach (var item in items)
            {
                Console.WriteLine(item.Path);
            }
            Console.WriteLine();
        }
    }
}
