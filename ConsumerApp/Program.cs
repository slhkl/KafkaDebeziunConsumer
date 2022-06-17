using Confluent.Kafka;
using Data;
using Data.Model;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Threading;

namespace ConsumerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string data = @"{
                    ""name"": ""mysqldb - connector"",
                    ""config"": {
                    ""connector.class"": ""io.debezium.connector.mysql.MySqlConnector"",
                    ""database.hostname"": ""mysql"",
                    ""database.port"": ""3306"",
                    ""database.user"": ""root"",
                    ""database.allowPublicKeyRetrieval"":""true"",
                    ""database.password"": ""root"",
                    ""database.dbname"" : ""BookAndWriteEntitiyFrameWork"",
                    ""database.server.name"": ""mysqlDbServer"",
                    ""heartbeat.interval.ms"": ""5000"",
                    ""database.history.kafka.bootstrap.servers"": ""kafka:9092"",
                    ""database.history.kafka.topic"": ""HistoricData"",
                    ""include.schema.changes"": ""true"",
                    ""key.converter"": ""org.apache.kafka.connect.json.JsonConverter"",
                    ""key.converter.schemas.enable"": ""false"",
                    ""value.converter"": ""org.apache.kafka.connect.json.JsonConverter"",
                    ""value.converter.schemas.enable"": ""false""
                     }
                  }";

            RestClient pr = new RestClient("http://127.0.0.1:8083/connectors/");
            RestRequest re = new RestRequest(Method.POST) { RequestFormat = DataFormat.Json };
            re.AddJsonBody(data);
            var response = pr.Execute(re);
            Console.WriteLine($"{response.StatusCode}::: {response.Content} istek gönderildi. {response.ErrorMessage}:::{response.ErrorException}");

            Thread.Sleep(1000);

            var conf = new ConsumerConfig
            {
                GroupId = "dbgroup",
                BootstrapServers = "localhost:9093",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                SecurityProtocol = SecurityProtocol.Plaintext
            };

            ThreadPool.QueueUserWorkItem(new WaitCallback(BookConsumer), conf);
            ThreadPool.QueueUserWorkItem(new WaitCallback(WriterConsumer), conf);

            Console.ReadLine();
        }
        static void BookConsumer(Object obj)
        {
            Console.WriteLine("BookConsumer calisti.");
            ConsumerConfig conf = (ConsumerConfig)obj;
            using var consumerBuilder = new ConsumerBuilder<Ignore, string>(conf).Build();
            consumerBuilder.Subscribe("mysqlDbServer.BookAndWriteEntitiyFrameWork.Book");

            try
            {
                while (true)
                {
                    try
                    {
                        var cr = consumerBuilder.Consume();
                        if (cr.Message.Value != null)
                        {
                            var consumeAfter = JsonConvert.DeserializeObject<KafkaModel<Book>>(cr.Message.Value);
                            if (consumeAfter.op.Equals("c"))
                            {
                                using (MongoRepository<Book> uow = new MongoRepository<Book>())
                                {
                                    uow.Add(consumeAfter.after);
                                    Console.WriteLine("Book eklendi");
                                }
                            }
                        }
                    
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Error occured: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ensure the consumer leaves the group cleanly and final offsets are committed.
                consumerBuilder.Close();
            }
        }
        static void WriterConsumer(Object obj)
        {
            Console.WriteLine("Writerconsumer calisti");
            ConsumerConfig conf = (ConsumerConfig)obj;
            using var consumerBuilder = new ConsumerBuilder<Ignore, string>(conf).Build();
            consumerBuilder.Subscribe("mysqlDbServer.BookAndWriteEntitiyFrameWork.Writer");

            try
            {
                while (true)
                {
                    try
                    {
                        var cr = consumerBuilder.Consume();
                        if (cr.Message.Value != null)
                        {
                            var consumeAfter = JsonConvert.DeserializeObject<KafkaModel<Writer>>(cr.Message.Value);
                            if (consumeAfter.op.Equals("c"))
                            {
                                using (MongoRepository<Writer> uow = new MongoRepository<Writer>())
                                {
                                    uow.Add(consumeAfter.after);
                                    Console.WriteLine("Writer eklendi");
                                }
                            }
                        }

                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Error occured: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ensure the consumer leaves the group cleanly and final offsets are committed.
                consumerBuilder.Close();
            }
        }
    }
}
