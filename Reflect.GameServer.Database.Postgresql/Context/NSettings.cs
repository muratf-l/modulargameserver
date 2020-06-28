using System.Collections.Generic;
using System.Linq;
using LinqToDB.Configuration;

namespace Reflect.GameServer.Database.Postgresql.Context
{
    public class NSettings : ILinqToDBSettings
    {
        public ConnectionStringSettings Settings { get; set; }

        public IEnumerable<IDataProviderSettings> DataProviders => Enumerable.Empty<IDataProviderSettings>();

        public string DefaultConfiguration => Settings.Name;

        public string DefaultDataProvider => Settings.ProviderName;

        public IEnumerable<IConnectionStringSettings> ConnectionStrings
        {
            get { yield return Settings; }
        }
    }
}