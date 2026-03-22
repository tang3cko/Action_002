namespace Action002.Enemy.Data
{
    /// <summary>
    /// Burst 互換の軽量 struct。EnemyMoveJob に NativeArray で渡す。
    /// </summary>
    public struct MovementSpec
    {
        public MovementPattern Pattern;
        public float KeepDistance;
        public float ArrivalThreshold;
    }
}
