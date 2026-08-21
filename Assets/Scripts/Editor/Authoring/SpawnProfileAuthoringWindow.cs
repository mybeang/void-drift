using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VD.Editor
{
    /// <summary>
    /// 스폰 프로파일 오서링 창 (M4-6) — 재사용 베이스 <see cref="SpawnProfileTableEditorView"/>를 호스팅.
    /// 적·강화·난이도 오서링 창과 같은 패턴(EditorWindow 얇게 + VisualElement 뷰) · 같은 USS 공유.
    /// enemy-design §5 3층(스폰 풀 시간축) — 페이즈별 적 조합·밀도를 데이터로 큐레이션.
    /// </summary>
    public sealed class SpawnProfileAuthoringWindow : EditorWindow
    {
        [MenuItem("Window/Void Drift/Spawn Profile Authoring")]
        public static void ShowWindow()
        {
            var win = GetWindow<SpawnProfileAuthoringWindow>();
            win.titleContent = new GUIContent("Spawn Profile Authoring");
            win.minSize = new Vector2(520f, 300f);
        }

        void CreateGUI()
        {
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/Authoring/SoTableEditor.uss");
            if (uss != null) rootVisualElement.styleSheets.Add(uss);
            rootVisualElement.Add(new SpawnProfileTableEditorView());
        }
    }
}
