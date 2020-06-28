using System;

namespace Reflect.GameServer.Telemetry
{
    [Serializable]
    public class GameResourceUsage
    {
 
        public long CodeTicks;
        public int ServerCpuAborts;
        public int ServerToClientMessagesReceived;
        public int ServerToClientMessagesSent;
        public int ServerToClientReceived;
        public int ServerToClientSent;
        public int ServerToExternalSiteReceived;
        public int ServerToExternalSiteRequests;
        public int ServerToExternalSiteSent;
        public int ServerToWebserviceReceived;
        public int ServerToWebserviceRequests;
        public int ServerToWebserviceRequestsError;
        public int ServerToWebserviceRequestsFailed;
        public int ServerToWebserviceRequestsTime;
        public int ServerToWebserviceSent;
    }
}