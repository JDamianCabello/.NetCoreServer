using System;

namespace SharedNameSpace
{
    /// <summary>
    /// Model to end call
    /// </summary>
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
