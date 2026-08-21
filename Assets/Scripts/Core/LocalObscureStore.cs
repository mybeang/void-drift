using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 로컬 최고점 저장소(M4-10) — <see cref="Application.persistentDataPath"/>에
    /// <b>AES 암호화 + 무결성 해시</b> 파일로 저장한다. JSON처럼 열어볼 수 없고 파일명도 눈에 안 띄게 하여
    /// 캐주얼한 조회/변조를 막는다.
    /// <para>⚠️ 암호화 키가 빌드 바이너리에 포함되므로 이는 진짜 보안이 아니라 <b>난독화</b>다(작정하면 뚫림).
    /// 로컬 최고점 용도엔 충분하며, 권위 있는 기록은 후일 Firebase(M5-7)가 담당한다.</para>
    /// <para>파일 부재·복호화 실패·해시 불일치(변조) 시 <see cref="LoadBest"/>는 0을 반환한다.</para>
    /// </summary>
    public sealed class LocalObscureStore : IHighScoreStore
    {
        // 난독화 시드(빌드에 포함 — 보안 목적 아님, 캐주얼 변조 억제용).
        const string Secret = "VD::v1::7A3C-void-drift";
        // 눈에 안 띄는 파일명(확장자·의미 없는 형태).
        const string FileName = ".vdsys.dat";

        readonly string _path;
        readonly byte[] _key; // Secret에서 파생한 32B AES-256 키

        public LocalObscureStore()
        {
            _path = Path.Combine(Application.persistentDataPath, FileName);
            using var sha = SHA256.Create();
            _key = sha.ComputeHash(Encoding.UTF8.GetBytes(Secret));
        }

        public int LoadBest()
        {
            try
            {
                if (!File.Exists(_path)) return 0;

                string plain = Decrypt(File.ReadAllBytes(_path));

                // 형식: "score|hash"
                int sep = plain.LastIndexOf('|');
                if (sep <= 0) return 0;

                string valuePart = plain.Substring(0, sep);
                string hashPart = plain.Substring(sep + 1);
                if (Hash(valuePart) != hashPart) return 0; // 변조 감지 → 폴백

                return int.TryParse(valuePart, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) && v >= 0
                    ? v : 0;
            }
            catch
            {
                return 0; // 손상/복호화 실패 → 폴백
            }
        }

        public void SaveBest(int score)
        {
            try
            {
                if (score < 0) score = 0;
                string valuePart = score.ToString(CultureInfo.InvariantCulture);
                string plain = valuePart + "|" + Hash(valuePart);
                File.WriteAllBytes(_path, Encrypt(plain));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LocalObscureStore] 최고점 저장 실패: {e.Message}");
            }
        }

        // 값 무결성용 해시(값 + 시드) → 변조 시 불일치.
        string Hash(string value)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(value + "::" + Secret)));
        }

        // 출력 = IV(16B) ‖ ciphertext. AES-CBC/PKCS7(기본값).
        byte[] Encrypt(string plain)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();
            using var enc = aes.CreateEncryptor();

            byte[] data = Encoding.UTF8.GetBytes(plain);
            byte[] cipher = enc.TransformFinalBlock(data, 0, data.Length);

            byte[] result = new byte[aes.IV.Length + cipher.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipher, 0, result, aes.IV.Length, cipher.Length);
            return result;
        }

        string Decrypt(byte[] blob)
        {
            using var aes = Aes.Create();
            aes.Key = _key;

            byte[] iv = new byte[16];
            Buffer.BlockCopy(blob, 0, iv, 0, 16); // blob이 16B 미만이면 예외 → 호출부에서 폴백
            aes.IV = iv;

            using var dec = aes.CreateDecryptor();
            byte[] plain = dec.TransformFinalBlock(blob, 16, blob.Length - 16);
            return Encoding.UTF8.GetString(plain);
        }
    }
}
