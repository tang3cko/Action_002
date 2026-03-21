using System;
using Unity.Mathematics;
using Action002.Bullet.Data;

namespace Action002.Bullet.Logic
{
    public static class ShotPatternCalculator
    {
        public static int Calculate(
            Span<BulletState> buffer,
            in ShotPatternSpec pattern,
            float2 origin,
            float2 playerPosition,
            byte polarity,
            float scoreValue)
        {
            return pattern.Kind switch
            {
                ShotPatternKind.Aimed => CalculateAimed(buffer, pattern, origin, playerPosition, polarity, scoreValue),
                ShotPatternKind.NWay => CalculateNWay(buffer, pattern, origin, playerPosition, polarity, scoreValue),
                ShotPatternKind.Ring => CalculateRing(buffer, pattern, origin, polarity, scoreValue),
                _ => 0,
            };
        }

        static int CalculateAimed(
            Span<BulletState> buffer,
            in ShotPatternSpec pattern,
            float2 origin,
            float2 playerPosition,
            byte polarity,
            float scoreValue)
        {
            if (buffer.Length < 1) return 0;

            float2 dir = playerPosition - origin;
            float dist = math.length(dir);
            if (dist < 0.01f) return 0;

            float2 velocity = (dir / dist) * pattern.BulletSpeed;

            buffer[0] = new BulletState
            {
                Position = origin,
                Velocity = velocity,
                ScoreValue = scoreValue,
                Polarity = polarity,
                Faction = BulletFaction.Enemy,
                Damage = 1,
            };
            return 1;
        }

        static int CalculateNWay(
            Span<BulletState> buffer,
            in ShotPatternSpec pattern,
            float2 origin,
            float2 playerPosition,
            byte polarity,
            float scoreValue)
        {
            int count = math.min(pattern.Count, buffer.Length);
            if (count <= 0) return 0;

            float2 dir = playerPosition - origin;
            float dist = math.length(dir);
            if (dist < 0.01f) return 0;

            float centerAngle = math.atan2(dir.y, dir.x);
            float arcRad = math.radians(pattern.ArcDegrees);
            float startAngle = centerAngle - arcRad * 0.5f;
            float step = count > 1 ? arcRad / (count - 1) : 0f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + step * i;
                buffer[i] = new BulletState
                {
                    Position = origin,
                    Velocity = new float2(math.cos(angle), math.sin(angle)) * pattern.BulletSpeed,
                    ScoreValue = scoreValue,
                    Polarity = polarity,
                    Faction = BulletFaction.Enemy,
                    Damage = 1,
                };
            }
            return count;
        }

        static int CalculateRing(
            Span<BulletState> buffer,
            in ShotPatternSpec pattern,
            float2 origin,
            byte polarity,
            float scoreValue)
        {
            int count = math.min(pattern.Count, buffer.Length);
            if (count <= 0) return 0;

            float angleStep = (math.PI * 2f) / count;

            for (int i = 0; i < count; i++)
            {
                float angle = angleStep * i;
                buffer[i] = new BulletState
                {
                    Position = origin,
                    Velocity = new float2(math.cos(angle), math.sin(angle)) * pattern.BulletSpeed,
                    ScoreValue = scoreValue,
                    Polarity = polarity,
                    Faction = BulletFaction.Enemy,
                    Damage = 1,
                };
            }
            return count;
        }
    }
}
