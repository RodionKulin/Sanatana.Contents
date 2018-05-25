using Nest;
using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace Sanatana.Contents.Search.ElasticSearch.Connection
{
    public class ElasticClientFactory<TKey> : IElasticClientFactory
        where TKey : struct
    {
        //fields
        protected ElasticSettings<TKey> _settings;
        protected static ElasticClient _instance;
        protected object _createInstanceLock = new object();


        //init
        public ElasticClientFactory(ElasticSettings<TKey> settings)
        {
            _settings = settings;
        }


        //methods
        public virtual ElasticClient GetClient()
        {
            if (_instance == null)
            {
                lock (_createInstanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = CreateClient();
                    }
                }
            }

            return _instance;
        }

        protected virtual ElasticClient CreateClient()
        {
            ConnectionSettings connection = CreateConnection(_settings);
            return new ElasticClient(connection);
        }

        protected virtual ConnectionSettings CreateConnection(ElasticSettings<TKey> settings)
        {
            ConnectionSettings connection = new ConnectionSettings(settings.NodeUri)
                .DefaultIndex(_settings.DefaultIndexName)   
                .DefaultFieldNameInferrer(input => input)       //remain property names stored same as c# class property names starting with capital letter
                .ThrowExceptions(true)
                ;
            
            string defaultIndexName = _settings.DefaultIndexName.ToLowerInvariant();
            foreach (ElasticEntitySettings entity in _settings.EntitySettings)
            {
                entity.ApplyInferMappingSettings(connection, defaultIndexName);
            }

            if(settings.Username != null && settings.Password != null)
            {
                connection = connection
                    .BasicAuthentication(settings.Username, settings.Password);
            }

            if (settings.IsDebugMode)
            {
                connection = EnableDebugging(connection);
            }

            return connection;
        }

        protected virtual ConnectionSettings EnableDebugging(ConnectionSettings connection)
        {
            return connection
                .PrettyJson()
                .DisableDirectStreaming()
                .OnRequestCompleted(response =>
                {
                    // log out the request
                    if (response.RequestBodyInBytes != null)
                    {
                        Debug.WriteLine(
                            $"{response.HttpMethod} {response.Uri} \n" +
                            $"{Encoding.UTF8.GetString(response.RequestBodyInBytes)}");
                    }
                    else
                    {
                        Debug.WriteLine($"{response.HttpMethod} {response.Uri}");
                    }

                    Debug.WriteLine("");

                    // log out the response
                    if (response.ResponseBodyInBytes != null)
                    {
                        Debug.WriteLine($"Status: {response.HttpStatusCode}\n" +
                                 $"{Encoding.UTF8.GetString(response.ResponseBodyInBytes)}\n" +
                                 $"{new string('-', 30)}\n");
                    }
                    else
                    {
                        Debug.WriteLine($"Status: {response.HttpStatusCode}\n" +
                                 $"{new string('-', 30)}\n");
                    }
                });
        }

    }
}
