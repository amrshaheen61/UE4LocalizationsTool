using System.Collections.Generic;

namespace AssetParser
{
    public interface IAsset
    {
        bool IsGood { get; }
        List<List<string>> Strings { get; }
        void SaveFile(string FilPath);
    }
}
