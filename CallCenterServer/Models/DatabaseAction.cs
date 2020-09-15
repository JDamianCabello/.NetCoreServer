using System;


namespace SharedNameSpace
{
    public enum DatabaseActionType { Create, Read, Update, Dalete }
    [Serializable]
    class DatabaseAction
    {
        public DatabaseActionType Action1 { get; set; }
        public User User { get; set; }

        public DatabaseAction(DatabaseActionType action1)
        {
            Action1 = action1;
            User = null;
        }

        public DatabaseAction(DatabaseActionType action, User user)
        {
            Action1 = action;
            User = user;
        }
    }
}