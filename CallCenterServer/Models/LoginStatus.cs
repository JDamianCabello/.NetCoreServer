using System;

namespace SharedNameSpace
{
    [Serializable]
    class LoginStatus
    {
        public bool Logged { get; set; }
        public string Msg { get; set; }

        public bool UserIsAdmin { get; set; }


        public LoginStatus(bool error, string msg)
        {
            Logged = error;
            Msg = msg;
        }

        public LoginStatus(bool error, string msg, bool userIsAdmmin)
        {
            Logged = error;
            Msg = msg;
            UserIsAdmin = userIsAdmmin;
        }
    }
}
