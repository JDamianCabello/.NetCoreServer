using System;


namespace SharedNameSpace
{
    public enum OrderType { ChangeClientCall, DisconnectUser, GetUserList, ManageDatabase, SendMessage }
    public enum DatabaseAction { Create, Read, Update, Delete }

    [Serializable]
    class AdminOrder
    {

        public OrderType OrderType { get; set; }

        public DatabaseAction DatabaseAction { get; set; }
        public User UserToDoAction { get; set; }
        public Call OldCall { get; private set; }
        public User UserToSendResponse { get; set; }

        public string Message { get; set; }
        public AdminOrder(OrderType orderType, User u)
        {
            OrderType = orderType;
            UserToDoAction = u;
        }

        public AdminOrder(string messageToSend, User userToSendMessage)
        {
            OrderType = OrderType.SendMessage;
            UserToDoAction = userToSendMessage;
            Message = messageToSend;

        }

        public AdminOrder(Call oldCall, User userToReSendCall)
        {
            OrderType = OrderType.ChangeClientCall;
            UserToDoAction = userToReSendCall;
            OldCall = oldCall;

        }

        public AdminOrder(OrderType orderType, DatabaseAction databaseAction, User userToDoAction)
        {
            OrderType = orderType;
            DatabaseAction = databaseAction;
            UserToDoAction = userToDoAction;
        }

        public AdminOrder(OrderType orderType, DatabaseAction databaseAction, User userToDoAction, User userToSendResponse)
        {
            OrderType = orderType;
            DatabaseAction = databaseAction;
            UserToDoAction = userToDoAction;
            UserToSendResponse = userToSendResponse;
        }
    }
}