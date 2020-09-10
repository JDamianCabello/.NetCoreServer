using System;
using System.ComponentModel;

namespace SharedNameSpace
{
    [Serializable]
    public class User : INotifyPropertyChanged
    {
        [field: NonSerializedAttribute()]
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

        public string Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public bool IsAdmin { get => isAdmin; set => isAdmin = value; }
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

        public bool Online { get => online; set => online = value; }

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
            return Name + " | " + Id;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is User))
                return false;
            if (((User)obj).Id == Id && ((User)obj).Name == Name)
                return true;
            return false;
        }

        public static bool operator ==(object o, User u)
        {
            return u.Equals(o);
        }

        public static bool operator !=(object o, User u)
        {
            return !u.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}