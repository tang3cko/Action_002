using Action002.Player.Data;

namespace Action002.Player.Logic
{
    public static class DamageCalculator
    {
        public static PlayerState ApplyDamage(PlayerState state, float invincibleDuration)
        {
            if (state.InvincibleTimer > 0f) return state;
            state.Hp--;
            state.InvincibleTimer = invincibleDuration;
            state.ComboCount = 0;
            state.ComboMultiplier = 1f;
            return state;
        }

        public static PlayerState TickInvincibility(PlayerState state, float deltaTime)
        {
            if (state.InvincibleTimer > 0f)
                state.InvincibleTimer -= deltaTime;
            return state;
        }

        public static bool IsInvincible(PlayerState state)
        {
            return state.InvincibleTimer > 0f;
        }

        public static bool IsDead(PlayerState state)
        {
            return state.Hp <= 0;
        }
    }
}
