namespace Sard.Infrastructure.Templates
{
    public static class EmailTemplates
    {
        public static string GetCodeEmail(string displayName, string code, string type)
        {
            var message = GetMessage(type);

            return @"<!DOCTYPE html>
<html>
<head>
  <meta charset='UTF-8'/>
  <style>
    body { margin:0; padding:0; background:#f5f0eb; font-family:'Segoe UI',sans-serif; }
    .wrapper { max-width:600px; margin:40px auto; background:#fff; border-radius:16px; overflow:hidden; box-shadow:0 4px 24px rgba(0,0,0,0.08); }
    .header { background:linear-gradient(135deg,#c0785a,#e8a98a); padding:40px 32px; text-align:center; }
    .header h1 { margin:0; color:#fff; font-size:42px; letter-spacing:6px; font-weight:300; }
    .heart { font-size:22px; margin:8px 0 0; }
    .body { padding:40px 32px; }
    .greeting { font-size:20px; color:#3d2b1f; font-weight:600; margin-bottom:8px; }
    .message { font-size:15px; color:#6b4f3f; line-height:1.8; margin-bottom:28px; }
    .code-box { background:linear-gradient(135deg,#fdf3ee,#fbe8df); border:2px solid #e8a98a; border-radius:12px; padding:24px; text-align:center; margin:24px 0; }
    .code { font-size:40px; font-weight:700; color:#c0785a; letter-spacing:12px; }
    .code-label { font-size:13px; color:#9e7060; margin-top:8px; }
    .footer { background:#fdf3ee; padding:24px 32px; text-align:center; border-top:1px solid #f0e0d6; }
    .footer p { margin:0; font-size:13px; color:#b08070; }
    .signature { font-size:14px; color:#c0785a; font-weight:600; margin-top:4px; }
  </style>
</head>
<body>
  <div class='wrapper'>
    <div class='header'>
      <h1>سرد</h1>
      <div class='heart'>💗 ✨ 💗</div>
    </div>
    <div class='body'>
      <p class='greeting'>مرحباً " + displayName + @" 💛</p>
      <p class='message'>" + message + @"
      <div class='code-box'>
        <div class='code'>" + code + @"</div>
      </div>
    </div>
    <div class='footer'>
      <p>بكل الحب والاهتمام 💗</p>
      <p class='signature'>Ahmed Noser — فريق سرد </p>
    </div>
  </div>
</body>
</html>";
        }
        private static string GetMessage(string type) => type switch
        {
            "confirm" => "رمز تأكيد بريدك الإلكتروني هو",
            "reset" => "تلقينا طلباً لإعادة تعيين كلمة المرور.<br/>رمز إعادة التعيين هو:",
            _ => "رمز التحقق الخاص بك هو:"
        };
    }
}
