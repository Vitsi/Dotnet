using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Common.Settings;

namespace Common.MongoDB
;
    public static class Extensions{
        public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration configuration){
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));
            

            services.Configure<ServiceSettings>(configuration.GetSection(nameof(ServiceSettings)));
            services.Configure<MongoDbSettings>(configuration.GetSection(nameof(MongoDbSettings)));

            services.AddSingleton<IMongoDatabase>(serviceProvider =>{
                var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
                var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);
                return mongoClient.GetDatabase(serviceSettings.ServiceName);
            });
            return services;
        }
        public static IServiceCollection AddMongoService<T>(this IServiceCollection services, string collectionName)
            where T : IModel
             {
              services.AddSingleton<IService<T>>(serviceProvider =>{
                var database = serviceProvider.GetService<IMongoDatabase>();
                return new MongoService<T>(database, collectionName);
            });
            return services;
            
        }
    }
