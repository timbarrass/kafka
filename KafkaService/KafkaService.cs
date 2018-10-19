using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Topshelf;

namespace KafkaService
{
    class KafkaService
    {
        public void Start()
        {
            var dir = ConfigurationManager.AppSettings["KafkaInstallDir"];
            EventLog.WriteEntry("Kafka and Zookeeper", "Starting Kafka from " + dir);

            var zookeeperStart = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = dir,
                FileName = "cmd.exe",
                Arguments = @"/C bin\windows\zookeeper-server-start.bat config\zookeeper.properties"
            };
            zookeeperStart.EnvironmentVariables["LOG_DIR"] = Path.Combine(dir, "logs");

            var kafkaStart = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = dir,
                FileName = "cmd.exe",
                Arguments = @"/C bin\windows\kafka-server-start.bat config\server.properties"
            };
            kafkaStart.EnvironmentVariables["LOG_DIR"] = Path.Combine(dir, "logs");

            var z = Process.Start(zookeeperStart);
            EventLog.WriteEntry("Kafka and Zookeeper", "Started zookeeper [" + z.Id + "]");
            Thread.Sleep(5000);
            var k = Process.Start(kafkaStart);
            EventLog.WriteEntry("Kafka and Zookeeper", "Started Kafka [" + k.Id + "]");
        }

        public void Stop()
        {
            var dir = ConfigurationManager.AppSettings["KafkaInstallDir"];
            EventLog.WriteEntry("Kafka and Zookeeper", "Starting Kafka from " + dir);

            var zookeeperStop = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = dir,
                FileName = "cmd.exe",
                Arguments = @"/C bin\windows\zookeeper-server-stop.bat config\zookeeper.properties"
            };
            zookeeperStop.EnvironmentVariables["LOG_DIR"] = Path.Combine(dir, "logs");

            var kafkaStop = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = dir,
                FileName = "cmd.exe",
                Arguments = @"/C bin\windows\kafka-server-stop.bat config\server.properties"
            };
            kafkaStop.EnvironmentVariables["LOG_DIR"] = Path.Combine(dir, "logs");

            var k = Process.Start(kafkaStop);
            EventLog.WriteEntry("Kafka and Zookeeper", "Stopped Kafka [" + k.Id + "]");
            Thread.Sleep(5000);
            var z = Process.Start(zookeeperStop);
            EventLog.WriteEntry("Kafka and Zookeeper", "Stopped zookeeper [" + z.Id + "]");
        }

        static void Main(string[] args)
        {
            var rc = HostFactory.Run(x =>
            {
                x.Service<KafkaService>(s =>
                {
                    s.ConstructUsing(name => new KafkaService());
                    s.WhenStarted(ks => ks.Start());
                    s.WhenStopped(ks => ks.Stop());
                });
                x.RunAsLocalSystem();
                x.SetDescription("Kafka and Zookeeper service.");
                x.SetDisplayName("Kafka and Zookeeper");
                x.SetServiceName("Kafka and Zookeeper");
            });
        }
    }
}
