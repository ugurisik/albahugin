namespace Hugin.ExDevice {

    public interface ISecureComm
    {
        int ConnTimeout { get; set; }

        bool IsVx675 { get; }

        int BufferSize { get; }

        string ECRVersion { get; }

        bool Connect();

        void SetCommObject(object commObj);

        int GetVersion();

        FPUResponse Send(FPURequest request);
    }

}


