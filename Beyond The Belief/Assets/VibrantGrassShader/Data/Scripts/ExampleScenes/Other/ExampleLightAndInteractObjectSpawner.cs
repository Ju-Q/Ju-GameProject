using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VibrantGrassShaderTools;

namespace VibrantGrassShader
{
    public class ExampleLightAndInteractObjectSpawner : MonoBehaviour
    {
        [Foldout("Controls", true)]
        [SerializeField] private bool EnableLineExample, EnableSpreadExample, EnableLineOnStart, EnableSpreadOnStart;
        [SerializeField] private int AmountOfObjects = 100;
        [Foldout("Data (Don't Touch)", true)]
        [SerializeField] private GameObject VibrantGrassShaderMainObject;
        [SerializeField] private Object MovingLightPrefab;
        [SerializeField, ColorUsage(true, true)] private Color FirstColor;
        [SerializeField] private Vector3 FirstPos, LastPos;
        private MainControls VGSMC;
        private List<GameObject> GOAddedInLine, GOAddedSpread;
        private string PrefabPath;
        private bool LineExampleOld, SpreadExampleOld;

        private void Start()
        {
            LineExampleOld = EnableLineExample;
            SpreadExampleOld = EnableSpreadExample;
            if (EnableLineOnStart == true) EnableLineExample = true;
            if (EnableSpreadOnStart == true) EnableSpreadExample = true;
            VGSMC = VibrantGrassShaderMainObject.GetComponent<MainControls>();
        }

        private void Update()
        {
            if (EnableLineExample == true)
            {
                if (LineExampleOld == false) SpawnLine();
            }
            else
            {
                if (LineExampleOld == true) DestroyLine();
            }
            LineExampleOld = EnableLineExample;
            if (EnableSpreadExample == true)
            {
                if (SpreadExampleOld == false) SpawnSpread();
            }
            else
            {
                if (SpreadExampleOld == true) DestroySpread();
            }
            SpreadExampleOld = EnableSpreadExample;
        }

        private void SpawnLine()
        {
            float PosXStep = (LastPos.x - FirstPos.x) / (float)AmountOfObjects;
            float PosZStep = (LastPos.z - FirstPos.z) / (float)AmountOfObjects;
            Vector3 HSVCol = Vector3.zero;
            Color.RGBToHSV(FirstColor, out HSVCol.x, out HSVCol.y, out HSVCol.z);
            float HueSteps0To1 = 1.0f / (float)AmountOfObjects;
            GOAddedInLine = new List<GameObject>();
            for (int i = 0; i < AmountOfObjects; i++)
            {
                GameObject objectSpawned = null;
#if UNITY_EDITOR
                objectSpawned = PrefabUtility.InstantiatePrefab(MovingLightPrefab, transform) as GameObject;
#endif
                if (objectSpawned == null) objectSpawned = Instantiate(MovingLightPrefab, transform) as GameObject;
                DynamicLightAndInteract VGSLI = objectSpawned.GetComponent<DynamicLightAndInteract>();
                Vector3 NewHSV = HSVCol;
                NewHSV.x += HueSteps0To1 * i;
                Color col = Color.HSVToRGB(NewHSV.x, NewHSV.y, NewHSV.z, true);
                VGSLI.LightColor = col;
                objectSpawned.transform.position = FirstPos + new Vector3(PosXStep * i, 0.0f, PosZStep * i);
                if (VGSMC.LightAndInteractObjectsList.Contains(objectSpawned) == false) VGSMC.LightAndInteractObjectsList.Add(objectSpawned);
                GOAddedInLine.Add(objectSpawned);
            }
        }

        private void SpawnSpread()
        {
            Vector3 HSVCol = Vector3.zero;
            Color.RGBToHSV(FirstColor, out HSVCol.x, out HSVCol.y, out HSVCol.z);
            float HueSteps0To1 = 1.0f / (float)AmountOfObjects;
            GOAddedSpread = new List<GameObject>();
            int LineAmounts = Mathf.FloorToInt(Mathf.Sqrt((float)AmountOfObjects));
            float PosXStep = (LastPos.x - FirstPos.x) / ((float)LineAmounts - 1.0f);
            float PosZStep = (LastPos.z - FirstPos.z) / ((float)LineAmounts - 1.0f);
            for (int i = 0; i < LineAmounts; i++)
            {
                for (int j = 0; j < LineAmounts; j++)
                {
                    bool IsEven = false;
                    int IntToCheck = j + i;
                    if (IntToCheck % 2 == 0) IsEven = true;
                    GameObject objectSpawned = null;
#if UNITY_EDITOR
                    objectSpawned = PrefabUtility.InstantiatePrefab(MovingLightPrefab, transform) as GameObject;
#endif
                    if (objectSpawned == null) objectSpawned = Instantiate(MovingLightPrefab, transform) as GameObject;
                    DynamicLightAndInteract VGSLI = objectSpawned.GetComponent<DynamicLightAndInteract>();
                    Vector3 NewHSV = HSVCol;
                    int CurrentSpotIndex = i * LineAmounts + i;
                    NewHSV.x += HueSteps0To1 * (j + (CurrentSpotIndex));
                    Color col = Color.HSVToRGB(NewHSV.x, NewHSV.y, NewHSV.z, true);
                    VGSLI.LightColor = col;
                    objectSpawned.transform.position = FirstPos + new Vector3(PosXStep * j, 0.0f, PosZStep * i);
                    if (IsEven == true) objectSpawned.transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
                    if (VGSMC.LightAndInteractObjectsList.Contains(objectSpawned) == false) VGSMC.LightAndInteractObjectsList.Add(objectSpawned);
                    GOAddedSpread.Add(objectSpawned);
                }
            }
        }


        private void DestroyLine()
        {
            for (int i = 0; i < GOAddedInLine.Count; i++)
            {
                if (VGSMC.LightAndInteractObjectsList.Contains(GOAddedInLine[i]) == true) VGSMC.LightAndInteractObjectsList.Remove(GOAddedInLine[i]);
                Destroy(GOAddedInLine[i]);
            }
        }

        private void DestroySpread()
        {
            for (int i = 0; i < GOAddedSpread.Count; i++)
            {
                if (VGSMC.LightAndInteractObjectsList.Contains(GOAddedSpread[i]) == true) VGSMC.LightAndInteractObjectsList.Remove(GOAddedSpread[i]);
                Destroy(GOAddedSpread[i]);
            }
        }
    }
}