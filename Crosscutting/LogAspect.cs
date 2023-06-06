using PostSharp.Aspects;
using Serilog;
using System;
using PostSharp.Serialization;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;
using Elastic.Apm.SerilogEnricher;
using Elastic.Apm;
using Elastic.Apm.Api;
using System.Diagnostics;

namespace Crosscutting
{
    [Serializable]
    public class LogAspect : OnMethodBoundaryAspect
    {
        public void InitializeLoggerConfiguration()
        {
            var apm = 
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithElasticApmCorrelationInfo()
                .WriteTo.File("C://logs//myapp.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200") ){
                    DetectElasticsearchVersion = false,
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                    CustomFormatter = new ElasticsearchJsonFormatter(),
                    IndexDecider = (@event,offset) => "test.localhost"
                })
                .CreateLogger();

            Log.Information("Initialize LogAspect");
        }


        public override void OnEntry(MethodExecutionArgs args)
        {
            InitializeLoggerConfiguration();
            var logData = new
            {
                returnValue = args.ReturnValue,
                arguments = args.Arguments,
                methodExecutionTag = args.MethodExecutionTag,
                instance = args.Instance,
                flowBehavior = args.FlowBehavior
            };
            var message = string.Format("Entering: {0}::{1}", args.Instance.GetType().Name, args.Method.Name);

            var transaction = new
            {
                transactionId = Agent.Tracer.CurrentTransaction.Id,
                traceId = Agent.Tracer.CurrentTransaction.TraceId,
                userId = "mbraga"
            };
            Log.Information("{message} {transactionId} {traceId} {userId}", message, transaction.transactionId, transaction.traceId, transaction.userId);
        }

        public override void OnException(MethodExecutionArgs args)
        {
            var message = string.Format("There was an exception in: {0}::{1}", args.Instance.GetType().Name, args.Method.Name);
            Console.WriteLine($"[ERROR] {message}");
            Log.Error($"[ERROR] {message}");
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            var message = string.Format("Exiting: {0}::{1}", args.Instance.GetType().Name, args.Method.Name);
            Console.WriteLine($"[INFO] {message}");
            Log.Debug($"{message}");
        }

        public override void OnSuccess(MethodExecutionArgs args)
        {
            var message = string.Format("Exiting successfully: {0}::{1}", args.Instance.GetType().Name, args.Method.Name);
            Console.WriteLine($"[INFO] {message}");
            Log.Debug($"[DEBUG] {message}");
        }
    }
}
