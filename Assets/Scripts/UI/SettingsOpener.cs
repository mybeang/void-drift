using UnityEngine;
using UnityEngine.UI;

namespace VD.UI
{
    /// <summary>
    /// 버튼에 붙여 클릭 시 지정한 <see cref="SoundSettingsPanel"/>을 여는 연결 컴포넌트(M5-10).
    /// 타이틀의 "환경설정" 버튼용(인게임은 일시정지 메뉴 <see cref="SettingsPanel"/>의 "환경 설정" 버튼이 담당).
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class SettingsOpener : MonoBehaviour
    {
        [SerializeField] SoundSettingsPanel panel;

        void Awake()
        {
            if (panel != null) GetComponent<Button>().onClick.AddListener(panel.Open);
        }
    }
}
