namespace MenuSystem;

public class MenuItem
{
    public string Title { get; set; }
    
    public Func<string>? MethodToRun { get; set; }
    
    public MenuItem(string title, Func<string>? methodToRun)
    {
        Title = title;
        MethodToRun = methodToRun;
    }


    public override string ToString() => Title;
}