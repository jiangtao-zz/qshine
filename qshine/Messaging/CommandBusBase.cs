using System;
using qshine.Configuration;

namespace qshine
{
	public abstract class CommandBusBase : ICommandBus
	{
		/// <summary>
		/// Send the specified command to command handler.
		/// </summary>
		/// <returns>The send.</returns>
		/// <param name="command">Command message.</param>
		/// <typeparam name="T">The type of command message.</typeparam>
		public virtual void Send<T>(T command) where T : ICommandMessage
		{
			//In case of 
			//	var command = message as ICommandMessage;
			//	bus.Send(command);
			if (typeof(T).IsInterface)
            {
                Send(command);
                return;
            }

			//if command and command handler implemented in the same class
			var handler = command as ICommandHandler<T>;
			if (handler != null)
			{
				handler.Using(() =>
				{
					handler.Handle(command);
				});
			}
			else
			{
				Send((ICommandMessage)command);
			}
		}

		/// <summary>
		/// Send the specified command without declare command type explicitly.
		/// </summary>
		/// <returns>The send.</returns>
		/// <param name="command">Command message.</param>
		public virtual void Send(ICommandMessage command)
		{
			var type = command.GetType();

			var handler = CommandBus.GetHandler(type);
			if (handler == null)
			{
				//If the command and command handler implemented in same class, try to execute it if Handler is available
				handler = command as ICommandHandler;
			}
			if (handler != null)
			{
				handler.Using(() =>
				{
					handler.TryCall(new [] {type}, "Handle", command);
				});
			}
			else if (ProcessGenericHandler(command) == false)
			{
				throw new NotImplementedException(
					string.Format("ICommandHandler<{0}> is not implemented.", type.Name));
			}
		}


		public abstract void SendAsync<T>(T command) where T : ICommandMessage;

		/// <summary>
		/// Processes the generic command handler.
		/// </summary>
		/// <returns><c>true</c>, if generic handler was processed, <c>false</c> otherwise.</returns>
		/// <param name="command">Command.</param>
		/// <example>
		/// 	public class CreateEntityHandler[T]:ICommandHandler[T]
		/// 	{
		/// 	}
		/// 
		/// </example>
		private bool ProcessGenericHandler(object command)
		{
			var type = command.GetType();

			if (type.IsGenericType && !type.IsAbstract)
			{
				var genericType = type.GetGenericTypeDefinition();
				//if (_genericCommandHandlers.ContainsKey(genericType))
				{
					var typeArguments = type.GetGenericArguments();
					//var handlerGenericType = _genericCommandHandlers[genericType];
					//var handlerType = handlerGenericType.MakeGenericType(typeArguments);
					//var handler = Activator.CreateInstance(handlerType);

					//handler.CallProcedureIfExists("Handle", type, command);
					return true;
				}
			}
			return false;
		}
	}

}
