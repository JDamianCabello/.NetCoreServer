using System;

namespace SharedNameSpace
{
    [Serializable]
    class ErrorStatus
    {
        public bool Error { get; set; }
        public string Msg { get; set; }

        public ErrorStatus(bool error, string msg)
        {
            Error = error;
            Msg = msg;
        }
    }
}
