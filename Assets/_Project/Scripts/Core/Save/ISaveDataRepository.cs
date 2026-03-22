namespace Action002.Core.Save
{
    public interface ISaveDataRepository
    {
        SaveData Load();
        void Save(SaveData data);
    }
}
