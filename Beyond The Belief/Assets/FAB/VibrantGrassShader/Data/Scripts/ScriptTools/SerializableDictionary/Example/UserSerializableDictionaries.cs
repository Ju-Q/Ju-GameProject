using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using VibrantGrassShader;

namespace VibrantGrassShaderTools
{
    [Serializable]
    public class StringStringDictionary : SerializableDictionary<string, string> { }

    [Serializable]
    public class RenderTextureAndColorSerializableDictionary : SerializableDictionary<RenderTexture, Color> { }
    //Grass
    [Serializable]
    public class Texture2DAndColorSerializableDictionary : SerializableDictionary<Texture2D, Color> { }
    [Serializable]
    public class GOAndVGSDistValuesSerializableDictionary : SerializableDictionary<GameObject, VibrantGrassShaderDistanceFadeOutValues> { }
    [Serializable]
    public class GOAndVect2IntSerializableDict : SerializableDictionary<GameObject, Vector2Int> { }
    [Serializable]
    public class ObjectListStorage : SerializableDictionary.Storage<List<UnityEngine.Object>> { }
    [Serializable]
    public class GOAndListOfObjectsSerializableDict : SerializableDictionary<GameObject, List<UnityEngine.Object>, ObjectListStorage> { }
    //EndGrass

    [Serializable]
    public class GOAndObjectSerializableDict : SerializableDictionary<GameObject, UnityEngine.Object> { }

    [Serializable]
    public class ObjectColorDictionary : SerializableDictionary<UnityEngine.Object, Color> { }

    [Serializable]
    public class ColorArrayStorage : SerializableDictionary.Storage<Color[]> { }

    [Serializable]
    public class StringColorArrayDictionary : SerializableDictionary<string, Color[], ColorArrayStorage> { }

    [Serializable]
    public class MyClass
    {
        public int i;
        public string str;
    }

    [Serializable]
    public class QuaternionMyClassDictionary : SerializableDictionary<Quaternion, MyClass> { }
}