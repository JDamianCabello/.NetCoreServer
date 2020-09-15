using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedNameSpace
{
    [Serializable, Table("Users")]
    public class User : INotifyPropertyChanged
    {
        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private string id;
        private string name;
        private bool isAdmin;
        private bool inCall;
        private bool online;

        [Column]
        public string Id { get => id; set => id = value; }
        [Column]
        public string Name { get => name; set => name = value; }
        [Column]
        public bool IsAdmin { get => isAdmin; set => isAdmin = value; }
        [NotMapped] //The NotMapped attribute is used to specify that an entity or property is not to be mapped to a table or column in the database.
        public bool InCall
        {
            get => inCall;

            set
            {
                if (inCall != value)
                {
                    inCall = value;
                    OnPropertyChanged("InCall");
                }
            }
        }
        [NotMapped] //The NotMapped attribute is used to specify that an entity or property is not to be mapped to a table or column in the database.
        public bool Online { get => online; set => online = value; }

        public User() { }

        public User(string id, string name, bool isAdmin)
        {
            Id = id;
            Name = name;
            IsAdmin = isAdmin;
            inCall = false;
            Online = true;
        }

        public override string ToString()
        {
            string aux = IsAdmin ? " Admin user" : " Normal user";
            return Name + " | " + Id + " | " +  aux;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is User))
                return false;
            if (((User)obj).Id == Id && ((User)obj).Name == Name)
                return true;
            return false;
        }

        public static bool operator == (object o, User u)
        {
            return u.Equals(o);
        }

        public static bool operator != (object o, User u)
        {
            return !u.Equals(o);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 23; //Some prime number to start hash
                int hashSecondPrime = 41; //Other prime to multiply
                hash = hash * hashSecondPrime + Id.GetHashCode();
                hash = hash * hashSecondPrime + Name.GetHashCode();

                return hash;
            }
        }
    }
}