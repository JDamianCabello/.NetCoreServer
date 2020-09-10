using System;

namespace SharedNameSpace
{
    [Serializable]
    class EndCall
    {
        public User to;

        public EndCall(User to)
        {
            this.to = to;
        }
    }
}
