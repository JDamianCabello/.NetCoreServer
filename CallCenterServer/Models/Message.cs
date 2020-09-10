using System;

namespace SharedNameSpace
{
    [Serializable]
    public class Message
    {
        public string msg;

        public User userToMessage;


        public Message(string msg)
        {
            this.msg = msg;
            userToMessage = null;
        }

        public Message(string msg, User userToMessage)
        {
            this.msg = msg;
            this.userToMessage = userToMessage;
        }
    }
}
