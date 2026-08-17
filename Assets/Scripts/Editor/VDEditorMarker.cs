using UnityEditor;  // Editor 전용 어셈블리(includePlatforms: Editor) 검증
using VD.Core;      // VD.Editor → VD.Runtime 참조 검증

namespace VD.Editor
{
    /// <summary>
    /// M0-4 골격 확인용 플레이스홀더. VD.Editor 어셈블리가
    /// Editor 전용으로 컴파일되고 VD.Runtime을 참조하는지 검증한다.
    /// 실제 에디터 툴 코드가 들어오면 삭제 예정.
    /// </summary>
    [InitializeOnLoad]
    public static class VDEditorMarker
    {
        // VD.Runtime 심볼을 참조 → Editor→Runtime 의존 방향 검증.
        public const string RuntimeRef = VDRuntimeMarker.Assembly;
    }
}
