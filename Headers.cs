namespace RedCell.Devices.LedDisplay.Daktronics
{
    internal enum Headers
    {
        Poll = 0x00,
        Ack = 0x01,
        Nak = 0x02,
        Identify = 0x03,
        Poll2 = 0x05,
        Ack2 = 0x06,
        Identify2 = 0x07,
        BroadcastEnable = 0x08,
        BroadcastDisable = 0x09,
        BroadcastStatus = 0x0a,
        GetEnabledBroadcastAddresses = 0x0b,
        SetBaudRate = 0x0c
    }
}
