namespace Server
{
	public interface ITransport
	{
		void Send(object obj);
		T Receive<T>();
	}
}