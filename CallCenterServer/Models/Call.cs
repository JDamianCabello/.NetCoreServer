using System;

namespace SharedNameSpace
{
    [Serializable]
    class Call
    {
        public User to;
        public string number;
        public string campaigne;


        public Call(User to, string number, string campaigne)
        {
            this.campaigne = campaigne;
            this.number = number;
            this.to = to;
        }
    }
}