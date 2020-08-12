using System.Collections.Generic;

namespace NukeFromOrbit
{
    public class DeleteItem
    {
        private sealed class PathEqualityComparer : IEqualityComparer<DeleteItem>
        {
            public bool Equals(DeleteItem? x, DeleteItem? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Path == y.Path;
            }

            public int GetHashCode(DeleteItem obj)
            {
                return obj.Path.GetHashCode();
            }
        }

        public static IEqualityComparer<DeleteItem> PathComparer { get; } = new PathEqualityComparer();

        public DeleteItem(string path, ItemType type)
        {
            Path = path;
            Type = type;
        }

        public string Path { get; }
        public ItemType Type { get; }
    }
}