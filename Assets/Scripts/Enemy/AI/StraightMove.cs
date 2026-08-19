using UnityEngine;

namespace VD.Enemy
{
    /// <summary>
    /// 직진 이동(M3-1). 코스 따라 -Z로 곧장 접근(플레이어/카메라 쪽). 무상태 → 싱글톤 공유 가능.
    /// (기존 <see cref="Enemy"/>.Update 하드코딩을 모듈로 이관.)
    /// </summary>
    public sealed class StraightMove : IMoveBehaviour
    {
        public void OnSpawned() { }

        public void Tick(Enemy self, float dt)
        {
            self.transform.position += Vector3.back * (self.MoveSpeed * dt);
        }
    }
}
