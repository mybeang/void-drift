using UnityEngine;
using UnityEngine.UI;

namespace VD.UI
{
    /// <summary>
    /// 버튼에 붙여 클릭 시 지정한 <see cref="SettingsPanel"/>을 여는 연결 컴포넌트(M5-10).
    /// 타이틀의 "환경설정" 버튼·인게임 기어 버튼 공용.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class SettingsOpener : MonoBehaviour
    {
        [SerializeField] SettingsPanel panel;

        void Awake()
        {
            if (panel != null) GetComponent<Button>().onClick.AddListener(panel.Open);
        }
    }
}
