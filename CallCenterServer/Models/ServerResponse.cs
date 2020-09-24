using System;

namespace SharedNameSpace
{
    public enum ResponseContext 
    { 
        ClientsManager,
        DatabaseManager, 
        Login, 
        ServerMessage, 
        Disconnect
    }

    [Serializable]
    class ServerResponse
    {
        public object ResponseObject { get; set; }
        public ResponseContext ResponseContext { get; set; }
        public ResponseContext Disconnect { get; }

        public ServerResponse(ResponseContext responseContext)
        {
            ResponseContext = responseContext;
        }
        public ServerResponse(ResponseContext responseContext, object responseObject)
        {
            ResponseObject = responseObject;
            ResponseContext = responseContext;
        }


    }
}
