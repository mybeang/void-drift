using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace VD.Enemy
{
    /// <summary>
    /// 적 비주얼(모델) 캐시 (M2-5c). SO별 다른 비주얼을 Addressables로 로드해 재사용한다(enemy-design §2·§6).
    /// <para>시작 시 유니크 비주얼을 <b>한 번씩</b> 로드(비동기 pop-in·매 스폰 async 회피) → 스폰 때 동기 부착.
    /// 로드된 <b>프리팹만</b> 캐시(파괴하지 않음). 스폰 인스턴스의 생성/파괴는 셸(<see cref="Enemy"/>)이 담당
    /// (조립=<see cref="Enemy.AttachVisual"/> / 반납 teardown=<see cref="Enemy.ClearVisual"/>).</para>
    /// 로드 리스트는 스포너의 SO DB에서 온다(M2-5e). 핸들은 씬 종료 시 <see cref="ReleaseAll"/>로 해제.
    /// </summary>
    public sealed class EnemyVisualCache
    {
        readonly Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();
        readonly List<AsyncOperationHandle> _handles = new List<AsyncOperationHandle>();

        /// <summary>유니크 비주얼(AssetGUID 기준)을 한 번씩 로드해 캐시. 중복 GUID·미배정은 스킵.</summary>
        public async UniTask PreloadAsync(IEnumerable<AssetReferenceGameObject> visuals)
        {
            foreach (var v in visuals)
            {
                if (v == null) continue;
                string guid = v.AssetGUID;
                if (string.IsNullOrEmpty(guid) || _prefabs.ContainsKey(guid)) continue;

                var handle = v.LoadAssetAsync<GameObject>();
                _handles.Add(handle);
                await handle.Task;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    _prefabs[guid] = handle.Result;
                else
                    Debug.LogWarning($"[EnemyVisualCache] 비주얼 로드 실패 (guid={guid})");
            }
        }

        /// <summary>미리 로드된 비주얼 프리팹 반환(없으면 null). 인스턴스화는 호출부(<see cref="Enemy.AttachVisual"/>).</summary>
        public GameObject Resolve(AssetReferenceGameObject visual)
        {
            if (visual == null) return null;
            return _prefabs.TryGetValue(visual.AssetGUID, out var p) ? p : null;
        }

        /// <summary>로드 핸들 전부 해제(씬 종료 시). 캐시 비움.</summary>
        public void ReleaseAll()
        {
            foreach (var h in _handles)
                if (h.IsValid()) Addressables.Release(h);
            _handles.Clear();
            _prefabs.Clear();
        }
    }
}
