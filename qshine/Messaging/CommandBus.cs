using System;
using System.Collections.Generic;
using qshine.Configuration;

namespace qshine
{
	/// <summary>
	/// Command bus class.
	/// 
	/// The default command bus is built from command bus provider.
	/// 
	/// </summary>
	public class CommandBus:ICommandBus
	{
		/// <summary>
		/// Register all command bus factories.
		/// </summary>
		static Dictionary<string, ICommandBusFactory> _factories = new Dictionary<string, ICommandBusFactory>();
		static object lockobj = new object();
		static Interceptor _intercepter = Interceptor.Register(typeof(CommandBus));

		ICommandBus _bus;

		//default command bus from default bus factory
		public CommandBus()
		{
			var factory = GetFactory("default");
			if (factory == null)
			{
				factory = new DefaultCommandBusFactory();
				_factories.Add("default", factory);
			}
			_bus = factory.Create();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:qshine.CommandBus"/> class by a given bus name.
		/// The bus name specifies a type of command bus configued in environment config file.
		/// </summary>
		/// <param name="busName">Bus name.</param>
		public CommandBus(string busName)
		{
			var factory = GetFactory(busName);
			if (factory == null)
			{
				throw new InvalidProviderException(
					string.Format(Globalization.Resources.InvalidCommandBusFactory, busName));
			}
			_bus = factory.Create();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:qshine.CommandBus"/> by a given bus factory.
		/// </summary>
		/// <param name="busFactory">Bus provider.</param>
		public CommandBus(ICommandBusFactory busFactory)
		{
			_bus = busFactory.Create();
		}

		/// <summary>
		/// Gets command bus factory by factory provider name.
		/// The bus factory must implement ICommandBusFactory
		/// </summary>
		/// <returns>The factory.</returns>
		/// <param name="name">Name.</param>
		public ICommandBusFactory GetFactory(string name)
		{
			if (string.IsNullOrEmpty(name)) return null;

			if (!_factories.ContainsKey(name))
			{
				var factory = EnvironmentManager.GetProvider<ICommandBusFactory>(name);
				if (factory == null)
				{
					return null;
				}
				lock(lockobj)
				{
					_factories[name] = factory;
				}
			}
			return _factories[name];
		}

		/// <summary>
		/// Send the specified command.
		/// </summary>
		/// <returns>The send.</returns>
		/// <param name="command">Command.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void Send<T>(T command) where T : ICommandMessage
		{
			_intercepter.JoinPoint<int>(() =>
			{
				_bus.Send(command);
				return 0;
			}, _bus, "Send", typeof(T), command);
		}

		/// <summary>
		/// Sends the async command message.
		/// </summary>
		/// <param name="command">Command.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void SendAsync<T>(T command) where T : ICommandMessage
		{
			_intercepter.JoinPoint<int>(() =>
			{
				_bus.SendAsync(command);
				return 0;
			}, _bus, "SendAsync", typeof(T), command);
		}

		/// <summary>
		/// Register all command handler
		/// </summary>
		static Dictionary<Type, Type> _commandHandlers = new Dictionary<Type, Type>();
		static object _commandHandlerLock = new object();

		/// <summary>
		/// Get command handler by command type
		/// </summary>
		/// <returns>The handler.</returns>
		/// <param name="commandType">Command type.</param>
		/// <remarks>
		/// The first ommand will load all command handler for later use.
		/// </remarks>
		public static ICommandHandler GetHandler(Type commandType)
		{
			if (!_commandHandlers.ContainsKey(commandType))
			{
				lock (_commandHandlerLock)
				{
					//Try to register all ICommandHandlers
					var types = EnvironmentManager.SafeGetInterfacedTypes(typeof(ICommandHandler));
					foreach (var type in types)
					{
						var typeArguments = type.GetOpenGenericTypes(typeof(ICommandHandler<>));
						if (typeArguments != null && typeArguments.Length == 1)
						{
							var commandMessageType = typeArguments[0];
							//var handlerType = type.GetGenericTypeDefinition();
							if (!_commandHandlers.ContainsKey(commandMessageType))
							{
								_commandHandlers.Add(commandMessageType, type);
							}
						}
					}
				}
			}
			if (_commandHandlers.ContainsKey(commandType))
			{
				var handlerType = _commandHandlers[commandType];
				return (ICommandHandler)Activator.CreateInstance(handlerType);
			}
			return null;
		}
	}
}
