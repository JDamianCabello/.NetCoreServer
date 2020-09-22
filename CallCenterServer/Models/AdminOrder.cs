using System;

public enum OrderType { GetUserList, SendMessage, DisconnectUser }
namespace SharedNameSpace
{
    [Serializable]
    class AdminOrder
    {
        public OrderType OrderType { get; set; }
        public User UserToSendResponse { get; set; }
        public AdminOrder(OrderType orderType, User u)
        {
            OrderType = orderType;
            UserToSendResponse = u;
        }
    }
}
