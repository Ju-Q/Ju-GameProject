using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VibrantGrassShaderTools
{
    [CustomPropertyDrawer(typeof(StringStringDictionary))]
    [CustomPropertyDrawer(typeof(ObjectColorDictionary))]
    [CustomPropertyDrawer(typeof(StringColorArrayDictionary))]
    [CustomPropertyDrawer(typeof(RenderTextureAndColorSerializableDictionary))]
    [CustomPropertyDrawer(typeof(Texture2DAndColorSerializableDictionary))]
    [CustomPropertyDrawer(typeof(GOAndVGSDistValuesSerializableDictionary))]
    [CustomPropertyDrawer(typeof(GOAndVect2IntSerializableDict))]
    [CustomPropertyDrawer(typeof(GOAndObjectSerializableDict))]
    [CustomPropertyDrawer(typeof(GOAndListOfObjectsSerializableDict))]
    [CustomPropertyDrawer(typeof(ObjectListStorage))]
    public class AnySerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }

    [CustomPropertyDrawer(typeof(ColorArrayStorage))]
    public class AnySerializableDictionaryStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }

}