using SharedNameSpace;
using System.Collections.Generic;
using System.Linq;

namespace CallCenterServer
{
    /// <summary>
    /// Util class to manage database operations
    /// </summary>
    public class SqLiteOperator
    {
        /// <summary>
        /// Makes a new user entry in database
        /// </summary>
        /// <param name="u">The new user</param>
        public void CreateNewUser(User u)
        {
            using var db = new SQLiteDBContext();
            db.Users.Add(u);
            db.SaveChangesAsync();
        }

        public void UpdateUser(User u)
        {
            using var db = new SQLiteDBContext();
            //Find the user (Can't be null was controlled in the other side)
            var user = db.Users.FirstOrDefault(x => x.Id == u.Id);

            if (user is null)
                return;

            //Update user
            user.Id = u.Id;
            user.Name = u.Name;

            //Save changes
            db.SaveChangesAsync();
        }

        public void DeleteUser(User u)
        {
            using var db = new SQLiteDBContext();
            db.Users.Remove(u);
            db.SaveChangesAsync();
        }

        /// <summary>
        /// Find if exist the user in the database
        /// </summary>
        /// <param name="userID">User unique id</param>
        /// <returns>User if exist or null if don´t exist</returns>
        public User FindUser(string userID)
        {
            using var db = new SQLiteDBContext();
            var user = db.Users.FirstOrDefault(u => u.Id == userID);
            return user;
        }
        /// <summary>
        /// Validate if the current id and name exist in the database
        /// </summary>
        /// <param name="user">The current user triying to login</param>
        /// <returns></returns>
        public bool ValidateLogin(User user)
        {
            using var db = new SQLiteDBContext();
            var data = db.Users.FirstOrDefault(a => a.Id == user.Id && a.Name == user.Name);
            return !(data is null);
        }

        /// <summary>
        /// Get all database users order by name
        /// </summary>
        /// <returns></returns>
        public List<User> GetAllUsers()
        {
            using var db = new SQLiteDBContext();
            var user = db.Users.OrderBy(n => n.Name).ToList();
            return user;
        }
    }
}