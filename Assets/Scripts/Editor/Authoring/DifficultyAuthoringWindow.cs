using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VD.Editor
{
    /// <summary>
    /// 난이도 페이즈 오서링 창 (M4-5) — 재사용 베이스 <see cref="DifficultyTableEditorView"/>를 호스팅.
    /// 적·강화 오서링 창과 같은 패턴(EditorWindow 얇게 + VisualElement 뷰) · 같은 USS 공유.
    /// 배율 곡선 데이터(페이즈 길이·상승률·경계 점프·안내문구)를 편집 — progression-design §2, "통합 튜닝 창" 계열.
    /// </summary>
    public sealed class DifficultyAuthoringWindow : EditorWindow
    {
        [MenuItem("Window/Void Drift/Difficulty Authoring")]
        public static void ShowWindow()
        {
            var win = GetWindow<DifficultyAuthoringWindow>();
            win.titleContent = new GUIContent("Difficulty Authoring");
            win.minSize = new Vector2(520f, 300f);
        }

        void CreateGUI()
        {
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/Authoring/SoTableEditor.uss");
            if (uss != null) rootVisualElement.styleSheets.Add(uss);
            rootVisualElement.Add(new DifficultyTableEditorView());
        }
    }
}
