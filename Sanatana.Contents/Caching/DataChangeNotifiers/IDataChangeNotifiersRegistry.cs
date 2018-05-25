using System.Threading.Tasks;

namespace Sanatana.Contents.Caching.DataChangeNotifiers
{
    public interface IDataChangeNotifiersRegistry
    {
        void HandleChangeNotification(IDataChangeNotifier changeNotifier);
        Task Register(IDataChangeNotifier changeNotifier, string cacheKey);
    }
}