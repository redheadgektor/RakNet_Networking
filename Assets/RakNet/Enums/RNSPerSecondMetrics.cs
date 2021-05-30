public enum RNSPerSecondMetrics
{
	/// How many bytes per pushed via a call to sent
	USER_MESSAGE_BYTES_PUSHED,

	/// How many user message bytes were sent
	USER_MESSAGE_BYTES_SENT,

	/// How many user messages sent
	USER_MESSAGES_SENT,

	/// How many user message bytes were resent. A message is resent if it is marked as reliable, and either the message didn't arrive or the message ack didn't arrive.
	USER_MESSAGE_BYTES_RESENT,

	/// How many user messages were resent. A message is resent if it is marked as reliable, and either the message didn't arrive or the message ack didn't arrive.
	USER_MESSAGES_RESENT,

	/// How many user message bytes were received, and returned to the user successfully.
	USER_MESSAGE_BYTES_RECEIVED_PROCESSED,

	/// How many user message bytes were received, but ignored due to data format errors. This will usually be 0.
	USER_MESSAGE_BYTES_RECEIVED_IGNORED,

	/// How many actual bytes were sent, including per-message and per-datagram overhead, and reliable message acks
	ACTUAL_BYTES_SENT,

	/// How many actual messages were sent, including per-message and per-datagram overhead, and reliable message acks
	ACTUAL_MESSAGES_SENT,

	/// How many actual bytes were received, including overead and acks.
	ACTUAL_BYTES_RECEIVED,

	////// How many actual messages were received, including overead and acks.
	ACTUAL_MESSAGES_RECEIVED,
}