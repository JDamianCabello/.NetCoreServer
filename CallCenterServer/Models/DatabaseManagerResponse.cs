using System;

namespace SharedNameSpace
{
    [Serializable]
    class DatabaseManagerResponse
    {
        public DatabaseManagerResponse(bool status, User[] updatedUserList)
        {
            Status = status;
            UpdatedUserList = updatedUserList;
        }

        public bool Status { get; set; }
        public User[] UpdatedUserList { get; set; }
    }
}
