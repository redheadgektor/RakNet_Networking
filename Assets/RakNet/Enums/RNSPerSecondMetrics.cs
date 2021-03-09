public enum RNSPerSecondMetrics
{
	/// How many bytes per pushed via a call to send
	USER_MESSAGE_BYTES_PUSHED,

	/// How many user message bytes were sent via a call to send. This is less than or equal to USER_MESSAGE_BYTES_PUSHED.
	/// A message would be pushed, but not yet sent, due to congestion control
	USER_MESSAGE_BYTES_SENT,

	/// How many user message bytes were resent. A message is resent if it is marked as reliable, and either the message didn't arrive or the message ack didn't arrive.
	USER_MESSAGE_BYTES_RESENT,

	/// How many user message bytes were received, and returned to the user successfully.
	USER_MESSAGE_BYTES_RECEIVED_PROCESSED,

	/// How many user message bytes were received, but ignored due to data format errors. This will usually be 0.
	USER_MESSAGE_BYTES_RECEIVED_IGNORED,

	/// How many actual bytes were sent, including per-message and per-datagram overhead, and reliable message acks
	ACTUAL_BYTES_SENT,

	/// How many actual bytes were received, including overead and acks.
	ACTUAL_BYTES_RECEIVED
}