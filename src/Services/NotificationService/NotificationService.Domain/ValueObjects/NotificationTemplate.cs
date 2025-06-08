using NotificationService.Domain.Entities;

namespace NotificationService.Domain.ValueObjects;

public record NotificationTemplate
{
    public string Name { get; }
    public NotificationType Type { get; }
    public string Subject { get; }
    public string Body { get; }
    public Dictionary<string, string> Variables { get; }

    public NotificationTemplate(string name, NotificationType type, 
        string subject, string body, Dictionary<string, string>? variables = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = type;
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        Body = body ?? throw new ArgumentNullException(nameof(body));
        Variables = variables ?? new Dictionary<string, string>();
    }

    public string RenderSubject(Dictionary<string, object> data)
    {
        return RenderTemplate(Subject, data);
    }

    public string RenderBody(Dictionary<string, object> data)
    {
        return RenderTemplate(Body, data);
    }

    private string RenderTemplate(string template, Dictionary<string, object> data)
    {
        var result = template;
        foreach (var variable in Variables)
        {
            var placeholder = $"{{{variable.Key}}}";
            if (data.TryGetValue(variable.Key, out var value))
            {
                result = result.Replace(placeholder, value?.ToString() ?? string.Empty);
            }
        }
        return result;
    }
}