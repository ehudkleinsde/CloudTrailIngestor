namespace Common.Interfaces
{
    public interface ICloudTrailLoadProvider
    {
        Task Provide(int amount);
    }
}
