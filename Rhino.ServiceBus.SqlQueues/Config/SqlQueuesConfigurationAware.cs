using System;
using System.Reflection;
using Rhino.ServiceBus.Config;
using Rhino.ServiceBus.Hosting;
using Rhino.ServiceBus.Impl;
using Rhino.ServiceBus.Internal;

namespace Rhino.ServiceBus.SqlQueues.Config
{
	public static class QueueConnectionStringContainer
	{
		public static string ConnectionString { get; set; }
	}
	
	public class SqlQueuesConfigurationAware : IBusConfigurationAware
	{
		public void Configure(AbstractRhinoServiceBusConfiguration config, IBusContainerBuilder builder, IServiceLocator locator)
		{
			var busConfig = config as RhinoServiceBusConfiguration;
			if (busConfig == null)
				return;

			if (!config.Endpoint.Scheme.Equals("sql.queues", StringComparison.InvariantCultureIgnoreCase))
				return;

			RegisterSqlQueuesTransport(config, builder, locator);
		}

		private void RegisterSqlQueuesTransport(AbstractRhinoServiceBusConfiguration c, IBusContainerBuilder b, IServiceLocator l)
		{
			var busConfig = c.ConfigurationSection.Bus;

			b.RegisterSingleton<IStorage>(() => new SqlStorage(busConfig.Path ?? QueueConnectionStringContainer.ConnectionString));

			b.RegisterSingleton<ISubscriptionStorage>(() => new GenericSubscriptionStorage(
			                                                	l.Resolve<IStorage>(),
			                                                	c.Endpoint.ToString(),
			                                                	l.Resolve<IMessageSerializer>(),
			                                                	l.Resolve<IReflection>()));

			b.RegisterSingleton<ITransport>(() => new SqlQueuesTransport(
			                                      	c.Endpoint,
			                                      	l.Resolve<IEndpointRouter>(),
			                                      	l.Resolve<IMessageSerializer>(),
			                                      	c.ThreadCount,
													busConfig.Path ?? QueueConnectionStringContainer.ConnectionString,
			                                      	c.NumberOfRetries,
			                                      	l.Resolve<IMessageBuilder<MessagePayload>>()));

			b.RegisterSingleton<IMessageBuilder<MessagePayload>>(() => new SqlQueuesMessageBuilder(
			                                                           	l.Resolve<IMessageSerializer>(),
			                                                           	l.Resolve<IServiceLocator>()));
		}
	}
}