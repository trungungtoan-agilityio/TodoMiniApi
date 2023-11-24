namespace TodoMinimalApi.ViewModels;

public class TodoOutput
{
    public TodoOutput(string? title, bool isCompleted, DateTime createdOn)
    {
        Title = title;
        IsCompleted = isCompleted;
        CreatedOn = createdOn;
    }

    public string? Title { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedOn { get; set; }
}