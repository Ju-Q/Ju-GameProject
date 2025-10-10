using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MagicArsenal
{
    public class MagicRotatingBeam : MonoBehaviour
    {
        [Header("Center Settings")]
        public Transform bossCenter;      // Boss 中心
        public float radius = 10f;        // 距离中心半径
        public float rotationSpeed = 30f; // 每秒旋转角度

        [System.Serializable]
        public class FloorSettings
        {
            public float height = 3f;   // 该层高度
            public int beamCount = 3;   // 该层发射器数量
        }

        [Header("Floor Settings")]
        public List<FloorSettings> floors = new List<FloorSettings>(); // 每层配置

        [Header("Prefabs")]
        public GameObject beamLineRendererPrefab;
        public GameObject beamStartPrefab;
        public GameObject beamEndPrefab;

        [Header("Beam Settings")]
        public float sweepDistance = 30f;
        public float beamEndOffset = 1f;
        public float textureScrollSpeed = 8f;
        public float textureLengthScale = 3;

        [Header("Collision Settings")]
        public LayerMask collisionMask = ~0; // 碰撞层
        public float hitCheckRadius = 0.3f;

        [Header("Charge Settings")]
        public float chargeTime = 2f;               // 蓄力时间
        public GameObject chargeEffectPrefab;       // 蓄力特效

        private class BeamInstance
        {
            public GameObject beamStart;
            public GameObject beamEnd;
            public GameObject beam;
            public LineRenderer line;
            public float currentAngle;
            public bool hasHitPlayer;
            public float height;
        }

        private List<BeamInstance> beams = new List<BeamInstance>();
        private bool isActive = false;

        /// <summary>
        /// 外部调用，蓄力后激活旋转光束
        /// </summary>
        public void ActivateBeams()
        {
            if (bossCenter == null)
            {
                Debug.LogWarning("请设置 Boss 中心点！");
                return;
            }
            if (isActive) return;

            StartCoroutine(ChargeAndActivate());
        }

        private IEnumerator ChargeAndActivate()
        {
            isActive = true;
            beams.Clear();

            // 蓄力特效
            GameObject chargeFX = null;
            if (chargeEffectPrefab != null && bossCenter != null)
            {
                chargeFX = Instantiate(chargeEffectPrefab, bossCenter.position, bossCenter.rotation);
            }

            yield return new WaitForSeconds(chargeTime);

            if (chargeFX) Destroy(chargeFX);

            // 根据楼层配置生成光束
            foreach (var floor in floors)
            {
                float angleStep = 360f / floor.beamCount;
                for (int i = 0; i < floor.beamCount; i++)
                {
                    float angle = i * angleStep;
                    Vector3 pos = bossCenter.position + Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;
                    pos.y = bossCenter.position.y + floor.height;

                    BeamInstance b = new BeamInstance();
                    b.beamStart = Instantiate(beamStartPrefab, pos, Quaternion.identity);
                    b.beamEnd = Instantiate(beamEndPrefab, pos + b.beamStart.transform.forward * sweepDistance, Quaternion.identity);
                    b.beam = Instantiate(beamLineRendererPrefab, pos, Quaternion.identity);
                    b.line = b.beam.GetComponent<LineRenderer>();
                    b.currentAngle = angle;
                    b.hasHitPlayer = false;
                    b.height = floor.height;

                    beams.Add(b);
                }
            }
        }

        /// <summary>
        /// 外部调用，停止旋转光束
        /// </summary>
        public void DeactivateBeams()
        {
            Debug.Log("[MagicRotatingBeam] ❌ DeactivateBeams() called — cleaning up beams");
            isActive = false;

            foreach (var b in beams)
            {
                if (b.beamStart) Destroy(b.beamStart);
                if (b.beamEnd) Destroy(b.beamEnd);
                if (b.beam) Destroy(b.beam);
            }
            beams.Clear();
        }

        public void ForceStopAndClear()
        {
            Debug.Log("[MagicRotatingBeam] ⚠️ ForceStopAndClear() called — 停止旋转并清理所有特效");

            isActive = false; // 停止 Update

            // 销毁所有生成的光束对象
            foreach (var b in beams)
            {
                if (b.beamStart) Destroy(b.beamStart);
                if (b.beamEnd) Destroy(b.beamEnd);
                if (b.beam) Destroy(b.beam);
            }

            beams.Clear();
        }

        private void Update()
        {
            if (!isActive || beams.Count == 0) return;

            for (int i = 0; i < beams.Count; i++)
            {
                UpdateBeamInstance(beams[i]);
            }
        }

        private void UpdateBeamInstance(BeamInstance b)
        {
            // 更新角度
            b.currentAngle += rotationSpeed * Time.deltaTime;
            if (b.currentAngle > 360f) b.currentAngle -= 360f;

            Vector3 startPos = bossCenter.position + Quaternion.Euler(0, b.currentAngle, 0) * Vector3.forward * radius;
            startPos.y = bossCenter.position.y + b.height;

            Vector3 dir = (b.beamEnd.transform.position - b.beamStart.transform.position).normalized;
            dir = Quaternion.Euler(0, rotationSpeed * Time.deltaTime, 0) * dir;

            Vector3 endPos = startPos + (Quaternion.Euler(0, b.currentAngle, 0) * Vector3.forward) * sweepDistance;

            // 碰撞检测
            if (Physics.Raycast(startPos, (endPos - startPos).normalized, out RaycastHit hit, sweepDistance, collisionMask))
            {
                endPos = hit.point;
            }

            UpdateBeamVisual(b, startPos, endPos);

            // 检测玩家受伤
            CheckHitPlayer(b, startPos, (endPos - startPos).normalized, Vector3.Distance(startPos, endPos));
        }

        private void UpdateBeamVisual(BeamInstance b, Vector3 start, Vector3 end)
        {
#if UNITY_5_5_OR_NEWER
            b.line.positionCount = 2;
#else
            b.line.SetVertexCount(2);
#endif
            b.line.SetPosition(0, start);
            b.beamStart.transform.position = start;

            Vector3 offsetEnd = end - (Vector3.up * beamEndOffset);
            b.beamEnd.transform.position = offsetEnd;
            b.line.SetPosition(1, offsetEnd);

            b.beamStart.transform.LookAt(b.beamEnd.transform.position);
            b.beamEnd.transform.LookAt(b.beamStart.transform.position);

            float distance = Vector3.Distance(start, offsetEnd);
            b.line.material.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
            b.line.material.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);
        }

        private void CheckHitPlayer(BeamInstance b, Vector3 origin, Vector3 dir, float distance)
        {
            if (b.hasHitPlayer) return;

            if (Physics.SphereCast(origin, hitCheckRadius, dir, out RaycastHit hit, distance, collisionMask))
            {
                GameObject target = hit.collider.gameObject;
                if (target.CompareTag("Player"))
                {
                    ProjectileHitHandler hitHandler = GetComponent<ProjectileHitHandler>();
                    if (hitHandler != null)
                    {
                        hitHandler.HandleHit(target);
                    }
                    else
                    {
                        var ph = target.GetComponent<PlayerHealth>();
                        if (ph != null)
                            ph.TakeHit();
                    }

                    b.hasHitPlayer = true;
                }
            }
        }

        public void ResetAttack()
        {
            DeactivateBeams();  // 停止旋转并销毁光束
        }

    }
}
