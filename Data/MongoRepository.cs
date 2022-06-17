using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace Data
{
    public class MongoRepository<T> : IDisposable, IRepository<T> where T : class
    {
        private static readonly ReaderWriterLockSlim _databaseLocker = new ReaderWriterLockSlim();
        private static readonly ReaderWriterLockSlim _clientLocker = new ReaderWriterLockSlim();

        private static Dictionary<string, IMongoDatabase> Databases { get; set; } = new Dictionary<string, IMongoDatabase>();
        private static Dictionary<string, MongoClient> MongoClients { get; set; } = new Dictionary<string, MongoClient>();

        public DatabaseSettings DatabaseSettings { get; set; } = new DatabaseSettings()
        {
            CollectionName = typeof(T).Name,
            ConnectionString = "mongodb://root:root@127.0.0.1:27017",
            DatabaseName = "BookAndWriteEntitiyFrameWork"
        };

        private IMongoCollection<T> GetCollection(DatabaseSettings dbSettings)
        {
            return GetDatabase(dbSettings).GetCollection<T>(typeof(T).Name);
        }

        private IMongoDatabase GetDatabase(DatabaseSettings dbSettings)
        {
            IMongoDatabase database = null;
            _databaseLocker.EnterReadLock();
            try
            {
                if (Databases.ContainsKey(dbSettings.DatabaseName))
                {
                    database = Databases[dbSettings.DatabaseName];
                }
                else
                {

                    _databaseLocker.ExitReadLock();
                    _databaseLocker.EnterWriteLock();
                    try
                    {
                        if (!MongoClients.ContainsKey(dbSettings.ConnectionString))
                        {
                            database = GetClient(dbSettings.ConnectionString).GetDatabase(dbSettings.DatabaseName);
                            Databases.Add(dbSettings.ConnectionString, database);
                        }
                        database = Databases[dbSettings.ConnectionString];
                    }
                    finally
                    {
                        _databaseLocker.ExitWriteLock();
                    }
                }
            }
            finally
            {
                if (_databaseLocker.IsReadLockHeld)
                    _databaseLocker.ExitReadLock();
            }
            return database;
        }

        private MongoClient GetClient(string connectionString)
        {
            MongoClient client = null;
            _clientLocker.EnterReadLock();
            try
            {
                if (MongoClients.ContainsKey(connectionString))
                {
                    client = MongoClients[connectionString];
                }
                else
                {
                    _clientLocker.ExitReadLock();
                    _clientLocker.EnterWriteLock();
                    try
                    {
                        if (!MongoClients.ContainsKey(connectionString))
                        {
                            client = new MongoClient(connectionString);
                            MongoClients.Add(connectionString, client);
                        }
                        client = MongoClients[connectionString];
                    }
                    finally
                    {
                        _clientLocker.ExitWriteLock();
                    }
                }
            }
            finally
            {
                if (_clientLocker.IsReadLockHeld)
                    _clientLocker.ExitReadLock();
            }
            return client;
        }

        public IMongoCollection<T> CurrentCollection { get; set; }

        private IMongoDatabase database;

        public IMongoDatabase Database
        {
            get
            {
                if (database == null)
                    database = GetDatabase(DatabaseSettings);
                return database;
            }
            set { database = value; }
        }

        public MongoRepository()
        {
            Database = GetDatabase(DatabaseSettings);
            if (Database.GetCollection<T>(typeof(T).Name) == null)
            {
                Database.CreateCollection(typeof(T).Name);
            }

            CurrentCollection = GetCollection(DatabaseSettings);
        }

        public void Add(T entity)
        {
            CurrentCollection.InsertOne(entity);
        }

        public void Delete(Expression<Func<T, bool>> predicate, bool forceDelete = false)
        {
            CurrentCollection.DeleteOne(predicate);
        }

        public void Delete<TField>(FieldDefinition<T, TField> field, TField date)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Lte(field, date);
            CurrentCollection.DeleteMany(filter);
        }

        public void Update(Expression<Func<T, bool>> predicate, T entity)
        {
            var a = CurrentCollection.ReplaceOne(predicate, entity);

        }

        public int Count(Expression<Func<T, bool>> predicate)
        {
            return (int)CurrentCollection.Find(predicate).CountDocuments();
        }

        public IQueryable<T> GetAll(Expression<Func<T, bool>> predicate)
        {
            return CurrentCollection.Find(predicate).ToEnumerable().AsQueryable();
        }

        public IQueryable<T> GetAll(Expression<Func<T, bool>> predicate, string sortColumnName, int skipCount, int takeCount, OrderType orderByType)
        {
            var findObject = CurrentCollection.Find(predicate);
            var sort = orderByType == OrderType.DESC ? Builders<T>.Sort.Descending(sortColumnName) : Builders<T>.Sort.Ascending(sortColumnName);
            return findObject.Sort(sort).Skip(skipCount).Limit(takeCount).ToEnumerable().AsQueryable();
        }

        public T Get(Expression<Func<T, bool>> predicate)
        {
            return CurrentCollection.Find(predicate).FirstOrDefault();
        }

        public bool Any(Expression<Func<T, bool>> predicate)
        {
            return CurrentCollection.Find(predicate).FirstOrDefault() != null;
        }

        public List<string> GetFieldList(string field)
        {
            return CurrentCollection.Distinct<string>(field, new BsonDocument()).ToList();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
