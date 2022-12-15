using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Amqp.Framing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace WorkerService
{
    public class AzBusService : IAzBusService
    {
        private readonly ServiceBusClient _serviceBusClient;

        public AzBusService(ServiceBusClient service)
        {
            _serviceBusClient = service;
        }
        public async Task GetQueues(CancellationToken stoppingToken)
        {
            int next = 0;
            ServiceBusReceiver receiver = _serviceBusClient.CreateReceiver("myqueue");
            do
            {
                Console.WriteLine(next);
                ServiceBusReceivedMessage receivedMessage = await receiver
                    .ReceiveMessageAsync(TimeSpan.FromMilliseconds(1000), stoppingToken);
                if (receivedMessage != null)
                {
                    var jsonstring = receivedMessage.Body.ToString();
                    Console.WriteLine(jsonstring);
                    if (jsonstring != null)
                    {
                        dynamic json = JsonConvert.DeserializeObject(jsonstring);
                        if (json.Name != null && json.Color != null)
                        {
                            await receiver.CompleteMessageAsync(receivedMessage);
                            string CONNSTR = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultDbConnection"].ConnectionString;

                            SqlConnection conn = new SqlConnection(CONNSTR);
                            conn.Open();
                            DateTime myDateTime = DateTime.Now;
                            string sqlFormattedDate = myDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            string sqlQuery = "INSERT INTO CarsDone(Name, Color, Date) VALUES (@Name, @Color, @Date);";
                            using (SqlCommand command = new SqlCommand(sqlQuery, conn))
                            {
                                command.Parameters.Add("@Name", SqlDbType.VarChar).Value = json.Name;
                                command.Parameters.Add("@Color", SqlDbType.VarChar).Value = json.Color;
                                command.Parameters.Add("@Date", SqlDbType.VarChar).Value = sqlFormattedDate;
                                command.ExecuteNonQuery();
                            }
                        }
                        else next = 1;
                    }
                    else next = 1;
                }
                else next = 1;
            } while (next == 0);
            Console.WriteLine(next);
        }
        public async Task ProcessMessages()
        {
            string queueName = "myqueue";
            // create a processor that we can use to process the messages
            var processor = _serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions());
            // add handler to process messages
            processor.ProcessMessageAsync += MessageHandler;
            // add handler to process any errors
            processor.ProcessErrorAsync += ErrorHandler;
            await processor.StartProcessingAsync();
            // Process messages for 5 minutes 
            await Task.Delay(TimeSpan.FromSeconds(5));
            // stop processing 
            Console.WriteLine("Stopping the receiver...");
            await processor.StopProcessingAsync();
            Console.WriteLine("Stopped receiving messages");
        }
        public Task ErrorHandler(ProcessErrorEventArgs arg)
        {
            // Here you can catch errors;
            return Task.CompletedTask;
        }
        public async Task MessageHandler(ProcessMessageEventArgs args)
        {
            // Do something with the message .e.g deserialize it and insert to SQL
            try
            {
                BinaryData content = args.Message.Body;
                // Here you can use :
                string contentStr = content.ToString(); // This would be your data

                if (contentStr != null)
                {
                    dynamic json = JsonConvert.DeserializeObject(contentStr);

                    if (json.Name != null && json.Color != null)
                    {
                        string CONNSTR = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultDbConnection"].ConnectionString;

                        SqlConnection conn = new SqlConnection(CONNSTR);
                        conn.Open();
                        DateTime myDateTime = DateTime.Now;
                        string sqlFormattedDate = myDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        string sqlQuery = "INSERT INTO CarsDone(Name, Color, Date) VALUES (@Name, @Color, @Date);";
                        using (SqlCommand command = new SqlCommand(sqlQuery, conn))
                        {
                            command.Parameters.Add("@Name", SqlDbType.VarChar).Value = json.Name;
                            command.Parameters.Add("@Color", SqlDbType.VarChar).Value = json.Color;
                            command.Parameters.Add("@Date", SqlDbType.VarChar).Value = sqlFormattedDate;
                            command.ExecuteNonQuery();
                        }
                    }

                }
                await args.CompleteMessageAsync(args.Message);


            }
            catch (Exception e)
            {
                // If something goes wrong you should abandon the message
                await args.AbandonMessageAsync(args.Message);
            }
        }
    }
}
