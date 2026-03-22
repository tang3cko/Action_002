namespace Action002.Enemy.Data
{
    public enum MovementPattern : byte
    {
        Chase,       // 追尾型
        KeepDistance, // 距離維持型
        Anchor       // 定位置型
    }
}
