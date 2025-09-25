using UnityEngine;
using System.Collections;

namespace MagicArsenal
{
    public class MagicBeamScript : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject beamLineRendererPrefab;
        public GameObject beamStartPrefab;
        public GameObject beamEndPrefab;
        public GameObject chargeEffectPrefab;

        [Header("Attack Settings")]
        public float chargeTime = 1.5f;
        public float intervalTime = 1f;
        public float predictionTime = 0.5f;

        [Header("Vertical Sweep Settings")]
        public float startAngle = -60f;
        public float endAngle = 60f;
        public float sweepDuration = 2f;
        public float sweepDistance = 30f;

        [Header("Beam Settings")]
        public float beamEndOffset = 1f;
        public float textureScrollSpeed = 8f;
        public float textureLengthScale = 3;

        [Header("Collision Settings")]
        public LayerMask collisionMask = ~0; // 碰撞层（默认全选）
        public float hitCheckRadius = 0.3f;  // 光束检测半径

        private GameObject beamStart;
        private GameObject beamEnd;
        private GameObject beam;
        private LineRenderer line;

        // 🆕 每道光束是否已击中过玩家
        private bool hasHitPlayer;

        /// <summary>
        /// 外部调用的攻击接口
        /// </summary>
        public IEnumerator ExecuteAttack(Transform target)
        {
            Vector3 fixedStartPos = transform.position;
            Quaternion fixedRot = transform.rotation;

            // 计算预判位置
            Vector3 predictedPos = target.position;
            Rigidbody targetRb = target.GetComponent<Rigidbody>();
            if (targetRb != null)
                predictedPos += targetRb.velocity * predictionTime;

            Vector3 baseDir = (predictedPos - fixedStartPos).normalized;

            // 蓄力特效
            GameObject chargeFX = null;
            if (chargeEffectPrefab)
                chargeFX = Instantiate(chargeEffectPrefab, fixedStartPos, fixedRot);

            yield return new WaitForSeconds(chargeTime);

            if (chargeFX) Destroy(chargeFX);

            // 光束
            beamStart = Instantiate(beamStartPrefab, fixedStartPos, Quaternion.identity);
            beamEnd = Instantiate(beamEndPrefab, predictedPos, Quaternion.identity);
            beam = Instantiate(beamLineRendererPrefab, fixedStartPos, Quaternion.identity);
            line = beam.GetComponent<LineRenderer>();

            // 🆕 重置击中标志
            hasHitPlayer = false;

            // 扫射
            yield return StartCoroutine(SweepBeam(fixedStartPos, baseDir));

            // 清理
            Destroy(beamStart);
            Destroy(beamEnd);
            Destroy(beam);

            yield return new WaitForSeconds(intervalTime);
        }

        private IEnumerator SweepBeam(Vector3 fixedStartPos, Vector3 baseDir)
        {
            float elapsed = 0f;
            while (elapsed < sweepDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / sweepDuration;

                float currentAngle = Mathf.Lerp(startAngle, endAngle, t);

                Vector3 rightAxis = Vector3.Cross(Vector3.up, baseDir).normalized;
                Vector3 dir = Quaternion.AngleAxis(currentAngle, rightAxis) * baseDir;

                Vector3 endPos = fixedStartPos + dir * sweepDistance;

                if (Physics.Raycast(fixedStartPos, dir, out RaycastHit hit, sweepDistance, collisionMask))
                {
                    endPos = hit.point;
                }

                UpdateBeam(fixedStartPos, endPos, dir);

                yield return null;
            }

            // 最后一帧
            float finalAngle = endAngle;
            Vector3 rightAxisFinal = Vector3.Cross(Vector3.up, baseDir).normalized;
            Vector3 finalDir = Quaternion.AngleAxis(finalAngle, rightAxisFinal) * baseDir;
            Vector3 finalPos = fixedStartPos + finalDir * sweepDistance;

            if (Physics.Raycast(fixedStartPos, finalDir, out RaycastHit finalHit, sweepDistance, collisionMask))
            {
                finalPos = finalHit.point;
            }
            UpdateBeam(fixedStartPos, finalPos, finalDir);
        }

        private void UpdateBeam(Vector3 start, Vector3 end, Vector3 dir)
        {
            if (!line) return;

#if UNITY_5_5_OR_NEWER
            line.positionCount = 2;
#else
            line.SetVertexCount(2);
#endif
            line.SetPosition(0, start);
            beamStart.transform.position = start;

            Vector3 offsetEnd = end - (Vector3.up * beamEndOffset);
            beamEnd.transform.position = offsetEnd;
            line.SetPosition(1, offsetEnd);

            beamStart.transform.LookAt(beamEnd.transform.position);
            beamEnd.transform.LookAt(beamStart.transform.position);

            float distance = Vector3.Distance(start, offsetEnd);
            line.material.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
            line.material.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);

            // 检测击中玩家
            CheckHitPlayer(start, dir, distance);
        }

        private void CheckHitPlayer(Vector3 origin, Vector3 dir, float distance)
        {
            if (hasHitPlayer) return; // 🆕 已经击中过就不再触发

            if (Physics.SphereCast(origin, hitCheckRadius, dir, out RaycastHit hit, distance, collisionMask))
            {
                GameObject target = hit.collider.gameObject;
                if (target.CompareTag("Player"))
                {
                    // 找ProjectileHitHandler来处理
                    ProjectileHitHandler hitHandler = GetComponent<ProjectileHitHandler>();
                    if (hitHandler != null)
                    {
                        hitHandler.HandleHit(target);
                    }
                    else
                    {
                        // 如果没挂，直接调用逻辑
                        var ph = target.GetComponent<PlayerHealth>();
                        if (ph != null)
                            ph.TakeHit();
                    }

                    hasHitPlayer = true; // 🆕 标记只触发一次
                }
            }
        }
    }
}
