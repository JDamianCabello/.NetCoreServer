using System;


namespace SharedNameSpace
{
    [Serializable]
    public class Logout
    {
        public User userToLogout;

        public Logout(User userToLogout)
        {
            this.userToLogout = userToLogout;
        }
    }
}
