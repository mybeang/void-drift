using UnityEngine;

namespace VoidDrift.Smoke
{
    /// <summary>
    /// M0-2 스모크: 큐브를 물리 엔진(Rigidbody)으로 Z축을 중심으로 회전.
    /// 방식 = Dynamic Rigidbody + angularVelocity (사용자 지정).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CubeSpinner : MonoBehaviour
    {
        public enum SpinDirection { CounterClockwise, Clockwise } // +Z / -Z

        [Header("회전")]
        [Tooltip("Z축 회전 속도(도/초).")]
        [SerializeField] private float rotationSpeedDegPerSec = 90f;
        [Tooltip("회전 방향.")]
        [SerializeField] private SpinDirection direction = SpinDirection.CounterClockwise;

        [Header("큐브")]
        [Tooltip("큐브 크기(균일 스케일).")]
        [SerializeField] private float cubeSize = 1f;

        private Rigidbody _rb;

        private void Awake()
        {
            transform.localScale = Vector3.one * cubeSize;
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.angularDamping = 0f; // Unity 6: angularDrag → angularDamping
            _rb.constraints = RigidbodyConstraints.FreezePosition
                            | RigidbodyConstraints.FreezeRotationX
                            | RigidbodyConstraints.FreezeRotationY;
        }

        private void FixedUpdate()
        {
            float sign = direction == SpinDirection.CounterClockwise ? 1f : -1f;
            _rb.angularVelocity = new Vector3(0f, 0f, rotationSpeedDegPerSec * Mathf.Deg2Rad * sign);
        }

#if UNITY_EDITOR
        private void OnValidate() => transform.localScale = Vector3.one * cubeSize;
#endif
    }
}
