using System;

namespace NukeFromOrbit
{
    public class DefaultConsole : IConsole
    {
        public void WriteLine(string line) => Console.WriteLine(line);
    }
}