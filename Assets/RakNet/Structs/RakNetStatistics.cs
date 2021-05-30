/// Store Statistics information related to network usage 
[System.Serializable]
public struct RakNetStatistics
{
	unsafe fixed ulong valueOverLastSecond[11];

	/// <summary>
	/// For each type in RNSPerSecondMetrics, what is the value over the last 1 second?
	/// </summary>
	public unsafe ulong GetStatsLastSecond(RNSPerSecondMetrics metrics)
	{
		unsafe
		{
			return valueOverLastSecond[(int)metrics];
		}
	}

	unsafe fixed ulong runningTotal[11];

	/// <summary>
	/// For each type in RNSPerSecondMetrics, what is the total value over the lifetime of the connection?
	/// </summary>
	public ulong GetStatsTotal(RNSPerSecondMetrics metrics)
	{
		unsafe
		{
			return runningTotal[(int)metrics];
		}
	}

	ulong connectionStartTime;

	/// <summary>
	/// When did the connection start?
	/// </summary>
	public ulong ConnectionStartTime()
	{
		return connectionStartTime;
	}

	ulong connectionTime;

	/// <summary>
	/// How much time has passed since connection?
	/// </summary>
	public ulong ConnectionTime()
	{
		return connectionTime;
	}

	bool isLimitedByCongestionControl;

	/// <summary>
	/// Is our current send rate throttled by congestion control?
	/// This value should be true if you send more data per second than your bandwidth capacity
	/// </summary>
	public bool IsCongestionLimited()
	{
		return isLimitedByCongestionControl;
	}

	ulong BPSLimitByCongestionControl;

	/// <summary>
	/// If \a isLimitedByCongestionControl is true, what is the limit, in bytes per second?
	/// </summary>
	/// <returns></returns>
	public ulong CongestionLimit()
	{
		return BPSLimitByCongestionControl;
	}

	bool isLimitedByOutgoingBandwidthLimit;

	/// <summary>
	/// Is our current send rate throttled by a call to RakServer.SetLimitBandwidth()?
	/// </summary>
	public bool IsBandwidthLimited()
	{
		return isLimitedByOutgoingBandwidthLimit;
	}

	ulong BPSLimitByOutgoingBandwidthLimit;

	/// <summary>
	/// If \a IsBandwidthLimited is true, what is the limit, in bytes per second?
	/// </summary>
	public ulong BandwidthLimit()
	{
		return BPSLimitByOutgoingBandwidthLimit;
	}

	unsafe fixed ulong messageInSendBuffer[4];

	/// <summary>
	/// For each priority level, how many messages are waiting to be sent out?
	/// </summary>
	public ulong GetMessagesInSendBuffer(PacketPriority packetPriority)
	{
		unsafe
		{
			return messageInSendBuffer[(int)packetPriority];
		}
	}

	unsafe fixed ulong bytesInSendBuffer[4];

	/// <summary>
	/// For each priority level, how many bytes are waiting to be sent out?
	/// </summary>
	public ulong GetBytesInSendBuffer(PacketPriority packetPriority)
	{
		unsafe
		{
			return bytesInSendBuffer[(int)packetPriority];
		}
	}

	ulong messagesInResendBuffer;

	/// <summary>
	/// How many messages are waiting in the resend buffer? This includes messages waiting for an ack, so should normally be a small value
	/// If the value is rising over time, you are exceeding the bandwidth capacity.
	/// </summary>
	public ulong MessagesInResendBuffer()
	{
		return messagesInResendBuffer;
	}

	ulong bytesInResendBuffer;

	/// <summary>
	/// How many bytes are waiting in the resend buffer. See also messagesInResendBuffer
	/// </summary>
	public ulong BytesInResendBuffer()
	{
		return bytesInResendBuffer;
	}

	float packetlossLastSecond;

	/// <summary>
	/// What was our packetloss?
	/// </summary>
	public int PacketLoss()
	{
		return (int)(packetlossLastSecond * 100f);
	}

	float packetlossTotal;
}