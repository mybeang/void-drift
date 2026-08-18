namespace VD.Core
{
    /// <summary>
    /// 적 아키타입 (enemy-design §2). 4종. 작성자가 SO에서 명시 선택.
    /// 아키타입이 교전거리(range)를 자동 결정한다(파생은 한 곳에서만 — M2-2c 헬퍼 예정):
    /// 탄막→원거리, 돌진·자폭→근거리, 복합→복합.
    /// 프리팹 비주얼 이름(Enemy_Barrage/Charger/Bomber/Heavy)과 대응하되, 모델↔AI는 분리.
    /// </summary>
    public enum Archetype
    {
        Barrage, // 탄막형 — 거리 두고 탄 뿌림 (range 원거리)
        Charger, // 돌진형 — 고체력 몸통 충돌 (range 근거리)
        Bomber,  // 자폭형 — 저체력 고속 범위 폭발 (range 근거리)
        Hybrid,  // 복합형 — 돌진하며 원거리도 사격 (range 복합)
    }
}
