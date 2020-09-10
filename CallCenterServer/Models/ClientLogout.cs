using System;


namespace SharedNameSpace
{
    [Serializable]
    public class ClienLogout
    {
        public User clientToLogout;

        public ClienLogout(User clientToLogout)
        {
            this.clientToLogout = clientToLogout;
        }
    }
}
