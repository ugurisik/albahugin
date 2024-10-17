namespace Hugin.ExDevice {
    public interface IConnection
    {
        int Timeout { get; set; }

        event OnMessageHandler OnReportLine;

        int Send(byte[] buffer, int offset, int count);

        byte[] Read();
    }
}


