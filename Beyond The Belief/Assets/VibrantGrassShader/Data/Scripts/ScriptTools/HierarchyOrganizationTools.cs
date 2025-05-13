using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VibrantGrassShaderTools
{
    public class HierarchyOrganizationTools
    {
        public int GetHighestNumberInChildren(Transform ParentObject, out Transform transformFound, string Seperater = null)
        {
            string seperaterResult = "_";
            if (Seperater != null) seperaterResult = Seperater;
            //Check the already created Meshes 
            int AddedInt = 1;
            Transform[] ChildrenOfParent = new Transform[ParentObject.childCount];
            transformFound = null;
            for (int i = 0; i < ParentObject.childCount; i++)
            {
                ChildrenOfParent[i] = ParentObject.GetChild(i);
                int EndingNumbers = 0;
                char[] SeperatorChar = new char[] { char.Parse(seperaterResult) };
                string[] NameSplit = ChildrenOfParent[i].gameObject.name.Split(SeperatorChar);
                int.TryParse(NameSplit[NameSplit.Length - 1], out EndingNumbers);
                if (EndingNumbers >= AddedInt)
                {
                    AddedInt = EndingNumbers;
                    transformFound = ChildrenOfParent[i];
                }
            }
            return AddedInt;
        }
    }
}