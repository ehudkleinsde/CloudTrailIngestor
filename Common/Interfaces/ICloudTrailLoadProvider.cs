namespace Common.Interfaces
{
    public interface ICloudTrailLoadProvider
    {
        Task ProvideAsync(int amount);
    }
}
