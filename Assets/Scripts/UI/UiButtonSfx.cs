using UnityEngine;
using UnityEngine.EventSystems;
using VD.Core;

namespace VD.UI
{
    /// <summary>
    /// UI 버튼 공용 효과음(M5-5) — 붙이기만 하면 호버 시 <see cref="SfxId.ButtonHover"/>,
    /// 클릭(포인터 다운) 시 <see cref="SfxId.ButtonClick"/>을 2D로 재생. 버튼별 배선 불필요(디커플).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UiButtonSfx : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            AudioManager.Instance?.PlayUi(SfxId.ButtonHover);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            AudioManager.Instance?.PlayUi(SfxId.ButtonClick);
        }
    }
}
