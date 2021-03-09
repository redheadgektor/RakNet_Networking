public enum PacketPriority
{
	/// The highest possible priority. These message trigger sends immediately, and are generally not buffered or aggregated into a single datagram.
	IMMEDIATE_PRIORITY,

	/// For every 2 IMMEDIATE_PRIORITY messages, 1 HIGH_PRIORITY will be sent.
	/// Messages at this priority and lower are buffered to be sent in groups at 10 millisecond intervals to reduce UDP overhead and better measure congestion control. 
	HIGH_PRIORITY,

	/// For every 2 HIGH_PRIORITY messages, 1 MEDIUM_PRIORITY will be sent.
	/// Messages at this priority and lower are buffered to be sent in groups at 10 millisecond intervals to reduce UDP overhead and better measure congestion control. 
	MEDIUM_PRIORITY,

	/// For every 2 MEDIUM_PRIORITY messages, 1 LOW_PRIORITY will be sent.
	/// Messages at this priority and lower are buffered to be sent in groups at 10 millisecond intervals to reduce UDP overhead and better measure congestion control. 
	LOW_PRIORITY
};