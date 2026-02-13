namespace Models;


public class Todo {
    public int Id {get; set;}
    public string Data {get; set;} = string.Empty;
    public bool IsComplited = false;
}