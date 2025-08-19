namespace EmailService.Application.Models;

public class EmailSendResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? MessageId { get; set; }

    public static EmailSendResult Success(string messageId) => new()
    {
        IsSuccess = true,
        MessageId = messageId
    };

    public static EmailSendResult Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}
