[System.Serializable]
public class User
{

    public string id;
    public string username;
    public string email;

    public User(string _username, string _email, string _id)
    {
        this.username = _username;
        this.email = _email;
        this.id = _id;
    }
}

