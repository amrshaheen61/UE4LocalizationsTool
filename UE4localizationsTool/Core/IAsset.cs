using System.Collections.Generic;
using System.Windows.Forms;

namespace AssetParser
{
    public interface IAsset
    {
        bool IsGood { get; }
        //     List<List<string>> Strings { get; }
        void AddItemsToDataGridView(DataGridView dataGrid);
        void LoadFromDataGridView(DataGridView dataGrid);
        void SaveFile(string FilPath);
        List<List<string>> ExtractTexts();
        void ImportTexts(List<List<string>> strings);


    }
}
