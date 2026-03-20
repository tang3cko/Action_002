namespace Action002.Bullet.Data
{
    public readonly struct ShotPatternSpec
    {
        public readonly ShotPatternKind Kind;
        public readonly int Count;
        public readonly float ArcDegrees;
        public readonly float BulletSpeed;

        public ShotPatternSpec(ShotPatternKind kind, int count, float arcDegrees, float bulletSpeed)
        {
            Kind = kind;
            Count = count;
            ArcDegrees = arcDegrees;
            BulletSpeed = bulletSpeed;
        }
    }
}
