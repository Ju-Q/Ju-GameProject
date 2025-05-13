using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VibrantGrassShaderTools
{
    public class GeneratedDirections
    {
        public static Vector3[] GenerateSphericalEquidistantDirections(int numViewDirections)
        {
            Vector3[] directions = new Vector3[numViewDirections];

            float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
            float angleIncrement = Mathf.PI * 2 * goldenRatio;

            for (int i = 0; i < numViewDirections; i++)
            {
                float t = (float)i / numViewDirections;
                float alpha = Mathf.Acos(1 - 2 * t);
                float theta = angleIncrement * i;

                float x = Mathf.Sin(alpha) * Mathf.Cos(theta);
                float y = Mathf.Sin(alpha) * Mathf.Sin(theta);
                float z = Mathf.Cos(alpha);
                directions[i] = new Vector3(x, y, z);
            }
            return directions;
        }

        public static Vector3[] GenerateDisksOfDirections(int RaysAmount, int DisksAmount)
        {
            GameObject RayObject = new GameObject();
            Transform RayTransform = RayObject.transform;
            List<Vector3> RaysDirection = new List<Vector3>();

            RayTransform.eulerAngles = Vector3.zero;
            //int RaysAmount = RayAmount;
            //int RaysAmount = 4 * RayAmount;
            float DegreeToAddPerRay = 360.0f / RaysAmount;
            //int DiskAmountResult = DiskAmount;
            //int DiskAmount = 1 * DiskAmountMultiplier;
            float DegreeToAddPerDisk = 180.0f / DisksAmount;
            for (int i = 0; i < DisksAmount; i++)
            {
                float Degrees = DegreeToAddPerDisk * i;
                RayTransform.eulerAngles = new Vector3(0.0f, 0.0f, Degrees);
                for (int i2 = 0; i2 < RaysAmount; i2++)
                {
                    float RayDegrees = DegreeToAddPerRay * i2;
                    float DegreesToRadian = ((RayDegrees - 90.0f) * -1) * Mathf.Deg2Rad;
                    Vector3 VectorResult1 = new Vector3(Mathf.Cos(DegreesToRadian), 0.0f, Mathf.Sin(DegreesToRadian));
                    Vector3 VectorResultLocalized = RayTransform.InverseTransformDirection(VectorResult1);
                    if (RaysDirection.Contains(VectorResultLocalized) == false)
                    { RaysDirection.Add(VectorResultLocalized); }
                }
            }
            Vector3[] NewCustomGeneratedDirection = new Vector3[RaysDirection.Count];
            for (int i = 0; i < RaysDirection.Count; i++)
            {
                NewCustomGeneratedDirection[i] = RaysDirection[i];
            }
            GameObject.Destroy(RayObject);
            return NewCustomGeneratedDirection;
        }

        public static Vector3 GetDiskDirectionWithDegrees(float DegreesToAdd)
        {
            float DegreesToRadian = ((DegreesToAdd - 90) * -1) * Mathf.Deg2Rad;
            Vector3 RotatedDirection = new Vector3(Mathf.Cos(DegreesToRadian), 0.0f, Mathf.Sin(DegreesToRadian)).normalized;
            return RotatedDirection;
        }
    }
}
