using System;

namespace PackageStructure.Editor
{
    [Flags]
    internal enum DependencyPrerequisite
    {
        None = 0,
        UniTask = 1,
        VContainer = 2,
        EventBus = 4,
        Dotween = 8
    }

    internal sealed class DependencyPackage
    {
        internal string DisplayName { get; }
        internal string Name { get; }
        internal string Url { get; }
        internal DependencyPrerequisite Prerequisites { get; }

        internal DependencyPackage(string displayName, string name, string url,
            DependencyPrerequisite prerequisites = DependencyPrerequisite.None)
        {
            DisplayName = displayName;
            Name = name;
            Url = url;
            Prerequisites = prerequisites;
        }
    }
}
