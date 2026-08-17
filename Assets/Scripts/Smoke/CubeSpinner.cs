using UnityEngine;

namespace VoidDrift.Smoke
{
    /// <summary>
    /// M0-2 스모크 테스트: 큐브를 제자리에서 회전시키되 Z 위치를 고정한다.
    /// 게임 컨셉의 'Z축 고정'(자유 XY 이동, Z 잠금) 파이프라인 확인용 — 실게임 로직 아님.
    /// </summary>
    public class CubeSpinner : MonoBehaviour
    {
        [Tooltip("초당 회전 각도(도). x/y/z 축별.")]
        [SerializeField] private Vector3 spinDegreesPerSecond = new Vector3(30f, 60f, 0f);

        private float _lockedZ;

        private void Awake()
        {
            _lockedZ = transform.position.z;
            Debug.Log($"[CubeSpinner] Awake — Z locked at {_lockedZ}");
        }

        private void Update()
        {
            transform.Rotate(spinDegreesPerSecond * Time.deltaTime, Space.Self);

            // Z축 고정: 회전/외력으로 Z가 흔들려도 초기값으로 되돌린다.
            var p = transform.position;
            if (!Mathf.Approximately(p.z, _lockedZ))
            {
                p.z = _lockedZ;
                transform.position = p;
            }
        }
    }
}
