using UnityEngine;

namespace Action002.Core
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Action002/Game Config")]
    public class GameConfigSO : ScriptableObject
    {
        [Header("Player")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private int maxHp = 5;
        [SerializeField] private float attackInterval = 0.2f;
        [SerializeField] private float invincibleDuration = 1f;

        [Header("Combo")]
        [SerializeField] private float comboMultiplierStep = 0.1f;
        [SerializeField] private float comboTimeout = 1.5f;

        [Header("Spawn")]
        [SerializeField] private float baseSpawnInterval = 0.75f;
        [SerializeField] private float minSpawnInterval = 0.2f;
        [SerializeField] private int maxEnemies = 1000;
        [SerializeField] private float spawnRadius = 15f;

        [Header("Player Bullet")]
        [SerializeField] private float playerBulletSpeed = 15f;
        [SerializeField] private float bulletOffscreenMargin = 2f;

        [Header("Gauge")]
        [SerializeField] private float absorbGaugeRate = 0.02f;
        [SerializeField] private float killGaugeRate = 0.05f;

        [Header("Contact")]
        [SerializeField] private float playerContactRadius = 0.3f;
        [SerializeField] private int contactScoreBonus = 10;

        public float MoveSpeed => moveSpeed;
        public int MaxHp => maxHp;
        public float AttackInterval => attackInterval;
        public float InvincibleDuration => invincibleDuration;
        public float ComboMultiplierStep => comboMultiplierStep;
        public float ComboTimeout => comboTimeout;
        public float BaseSpawnInterval => baseSpawnInterval;
        public float MinSpawnInterval => minSpawnInterval;
        public int MaxEnemies => maxEnemies;
        public float SpawnRadius => spawnRadius;
        public float PlayerBulletSpeed => playerBulletSpeed;
        public float BulletOffscreenMargin => bulletOffscreenMargin;
        public float AbsorbGaugeRate => absorbGaugeRate;
        public float KillGaugeRate => killGaugeRate;
        public float PlayerContactRadius => playerContactRadius;
        public int ContactScoreBonus => contactScoreBonus;
    }
}
