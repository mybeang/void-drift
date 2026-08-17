using R3;                      // R3 코어 참조 검증 (R3.dll)
using Cysharp.Threading.Tasks; // UniTask 참조 검증
using UnityEngine.InputSystem; // New Input System 참조 검증 (Unity.InputSystem)

namespace VD.Core
{
    /// <summary>
    /// M0-4 골격 확인용 플레이스홀더. VD.Runtime 어셈블리가
    /// R3 / UniTask를 정상 참조하는지 컴파일로 검증한다.
    /// 실제 코드가 들어오면 삭제 예정.
    /// </summary>
    public static class VDRuntimeMarker
    {
        public const string Assembly = "VD.Runtime";

        // using이 실제로 필요하도록 최소 참조를 남긴다(미사용 경고 방지).
        public static readonly System.Type R3Marker = typeof(Observable);
        public static readonly System.Type UniTaskMarker = typeof(UniTask);
        public static readonly System.Type InputSystemMarker = typeof(Keyboard);
    }
}
