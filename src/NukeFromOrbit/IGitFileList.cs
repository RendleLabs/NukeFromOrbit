using System.Collections.Generic;
using System.Threading.Tasks;

namespace NukeFromOrbit
{
    public interface IGitFileList
    {
        Task<HashSet<string>> GetAsync();
    }
}