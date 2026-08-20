using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VD.Editor
{
    /// <summary>
    /// 3choice 강화 오서링 창 (M3-4) — 재사용 베이스 <see cref="UpgradeTableEditorView"/>를 호스팅.
    /// 적 오서링 창과 같은 패턴(EditorWindow 얇게 + VisualElement 뷰) · 같은 USS 공유.
    /// </summary>
    public sealed class UpgradeAuthoringWindow : EditorWindow
    {
        [MenuItem("Window/Void Drift/Upgrade Authoring")]
        public static void ShowWindow()
        {
            var win = GetWindow<UpgradeAuthoringWindow>();
            win.titleContent = new GUIContent("Upgrade Authoring");
            win.minSize = new Vector2(520f, 300f);
        }

        void CreateGUI()
        {
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/Authoring/SoTableEditor.uss");
            if (uss != null) rootVisualElement.styleSheets.Add(uss);
            rootVisualElement.Add(new UpgradeTableEditorView());
        }
    }
}
