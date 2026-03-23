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
        public float RotationSpeed;
        /// <summary>1ステップの角度（度）。0 = 滑らか回転。</summary>
        public float StepAngle;
        /// <summary>ステップ内でホールドする割合（0..1）。残りの時間でスナップ。</summary>
        public float HoldRatio;
    }
}
