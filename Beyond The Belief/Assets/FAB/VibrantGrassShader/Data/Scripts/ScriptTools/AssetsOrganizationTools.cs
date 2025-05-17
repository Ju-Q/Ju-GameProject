using UnityEditor;

namespace VibrantGrassShaderTools
{
#if UNITY_EDITOR
    public class AssetsOrganizationTools
    {
        public int GetHighestNumberInFolder(string FolderPath, string IncludedString, string Seperater, out string objectFound, bool? HasExtensionToIgnore = null)
        {
            System.IO.DirectoryInfo Directory = new System.IO.DirectoryInfo(FolderPath);
            int AddedInt = 0;
            string[] FoldersToSearch = new string[] { FolderPath };
            string[] AllTheAssetsToCheck = AssetDatabase.FindAssets(IncludedString, FoldersToSearch);
            int IndexOfHighestObject = 0;
            //Get the highest numbers + 1
            for (int i = 0; i < AllTheAssetsToCheck.Length; i++)
            {
                string Path = AssetDatabase.GUIDToAssetPath(AllTheAssetsToCheck[i]);
                int EndingNumbers = 0;
                char[] SeperatorChar = new char[] { char.Parse(Seperater) };
                int SplitInt = 1;
                if (HasExtensionToIgnore == true)
                {
                    SeperatorChar = new char[] { char.Parse(Seperater), char.Parse(".") };
                    SplitInt = 2;
                }
                string[] PathSplit = Path.Split(SeperatorChar);
                int.TryParse(PathSplit[PathSplit.Length - SplitInt], out EndingNumbers);
                if (EndingNumbers >= AddedInt)
                {
                    AddedInt = EndingNumbers;
                    IndexOfHighestObject = i;
                }
                
            }
            if (AllTheAssetsToCheck.Length > 0) objectFound = AllTheAssetsToCheck[IndexOfHighestObject];
            else objectFound = null;
            return AddedInt;
        }
    }
#endif
}
