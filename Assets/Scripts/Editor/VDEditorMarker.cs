using UnityEditor;  // Editor 전용 어셈블리(includePlatforms: Editor) 검증
using VD.Core;      // VD.Editor → VD.Runtime 참조 검증

namespace VD.Editor
{
    /// <summary>
    /// asmdef 구조 유지 검증용 잔존 마커. VD.Editor가 Editor 전용으로 컴파일되고
    /// VD.Runtime을 참조하는지(에디터→런타임 단방향)를 실제 런타임 타입으로 확인한다.
    /// 실제 에디터 툴 코드(M2)가 들어오면 삭제 예정.
    /// (M1-1에서 VDRuntimeMarker는 실코드 안착으로 제거 → 참조를 GameManager로 교체.)
    /// </summary>
    [InitializeOnLoad]
    public static class VDEditorMarker
    {
        // VD.Runtime의 실제 타입을 참조 → Editor→Runtime 의존 방향 검증. 값은 "VD.Runtime".
        public static readonly string RuntimeRef = typeof(GameManager).Assembly.GetName().Name;
    }
}
