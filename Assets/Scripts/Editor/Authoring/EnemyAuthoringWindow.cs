using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VD.Editor
{
    /// <summary>
    /// 적 조합 오서링 창 (M2-3c) — 포폴 공고 1순위 어필. 재사용 베이스 <see cref="EnemyTableEditorView"/>를 호스팅.
    /// enemy-design §5 1층(조합 목록/편집). 유효성 경고(교전거리·라벨 모순)는 M2-4에서 이 창에 얹는다.
    /// </summary>
    public sealed class EnemyAuthoringWindow : EditorWindow
    {
        [MenuItem("Window/Void Drift/Enemy Authoring")]
        public static void ShowWindow()
        {
            var win = GetWindow<EnemyAuthoringWindow>();
            win.titleContent = new GUIContent("Enemy Authoring");
            win.minSize = new Vector2(560f, 320f);
        }

        void CreateGUI()
        {
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/Authoring/SoTableEditor.uss");
            if (uss != null) rootVisualElement.styleSheets.Add(uss);
            rootVisualElement.Add(new EnemyTableEditorView());
        }
    }
}
