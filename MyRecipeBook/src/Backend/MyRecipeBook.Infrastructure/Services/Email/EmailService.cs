// using MyRecipeBook.Domain.Services.Email;
// using PostmarkDotNet;

// namespace MyRecipeBook.Infrastructure.Services.Email;

// internal sealed class EmailService : IEmailService
// {
//     private readonly PostmarkClient _client;

//     public PostmarkEmailService()
//     {
//         _client = new PostmarkClient("POSTMARK_SERVER_TOKEN");
//     }

//     public async Task SendPasswordRecoveryCode(string email, string code)
//     {
//         var message = new PostmarkMessage
//         {
//             From = "noreply@myrecipebook.com",
//             To = email,
//             Subject = "Password recovery code",
//             TextBody = $"Your password recovery code is: {code}",
//             HtmlBody = $"""
//                 <h2>Password Recovery</h2>
//
//                 <p>Your password recovery code is:</p>
//
//                 <strong>{code}</strong>
//
//                 <p>This code expires in a few minutes.</p>
//                 """
//         };

//         var response = await _client.SendMessageAsync(message);

//         if (response.Status != PostmarkStatus.Success)
//             throw new Exception("Unable to send the password recovery email.");
//     }
// }