namespace RedCell.Devices.LedDisplay.Daktronics
{
    internal enum CommandsV3
    {
        Unsupported = 0x00,
        SendFile = 0x01,
        RetrieveFile = 0x02,
        RenameFile = 0x03,
        DeleteFile = 0x04,
        SendFileBlock = 0x05,
        RetrieveFileBlock = 0x06,
        InsertMessagetoPlaylist = 0x07,
        RemoveMessagefromPlaylist = 0x08,
        MoveaPlaylistEntry = 0x09,
        BlankDisplay = 0x0A,
        ResetDisplay = 0x0B,
        RunSchedule = 0x0C,
        StopSchedule = 0x0D,
        SetDimming = 0x0E,
        SetTime = 0x0F,
        SetDisplayMode = 0x10,
        SendRTD = 0x11,
        StartCounterTimer = 0x12,
        StopCounterTimer = 0x13,
        SendCompressedFile = 0x14,
        RetrieveCompressedFile = 0x15,
        SelectFrame = 0x16,
        GetRunningMessageName = 0x17,
        Authenticate = 0x18,
        UnAuthenticated = 0x19,
        CloseAuthentication = 0x1A,
        CursorControl = 0x1B,
        ManualCalibrate = 0x1C,
        FileCalibrate = 0x1D,
        DeviceBootload = 0x1E
    };
}
