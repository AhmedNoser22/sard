using System.Linq;

namespace Sard.Infrastructure.Templates
{
    public static class EmailTemplates
    {
        public static string GetCodeEmail(string displayName, string code, string type)
        {
            var message = GetMessage(type);
            var codeDigits = string.Join("", code.Select(c => $"<span class='digit'>{c}</span>"));

            return @"<!DOCTYPE html>
<html lang='ar' dir='rtl'>
<head>
  <meta charset='UTF-8'/>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
  <style>
    @import url('https://fonts.googleapis.com/css2?family=Amiri:wght@400;700&family=Cairo:wght@400;600;700&display=swap');

    body { margin:0; padding:0; background:#f5f0eb; font-family:'Cairo','Segoe UI',sans-serif; }
    .wrapper { max-width:560px; margin:40px auto; background:#ffffff; border-radius:20px; overflow:hidden; box-shadow:0 10px 40px rgba(192,120,90,0.15); }

    .header { background:linear-gradient(135deg,#c0785a 0%,#e8a98a 100%); padding:36px 32px 28px; text-align:center; position:relative; }
    .header h1 { margin:0; color:#fff; font-family:'Amiri',serif; font-size:44px; letter-spacing:8px; font-weight:700; }
    .book-icon { margin:14px auto 0; display:block; }
    @keyframes float {
      0%,100% { transform:translateY(0); }
      50% { transform:translateY(-6px); }
    }
    .book-icon { animation:float 3s ease-in-out infinite; }

    .body { padding:36px 32px 8px; }
    .greeting { font-size:19px; color:#3d2b1f; font-weight:700; margin-bottom:10px; font-family:'Amiri',serif; }
    .message { font-size:15px; color:#6b4f3f; line-height:1.9; margin-bottom:26px; }

    .code-box { background:linear-gradient(135deg,#fdf3ee,#fbe8df); border:2px solid #e8a98a; border-radius:14px; padding:26px 16px; text-align:center; margin:8px 0 28px; }
    .code { display:inline-flex; gap:8px; direction:ltr; }
    @keyframes pulseGlow {
      0%,100% { box-shadow:0 0 0 rgba(192,120,90,0); }
      50% { box-shadow:0 0 14px rgba(192,120,90,0.35); }
    }
    .digit {
      display:inline-block; width:38px; height:48px; line-height:48px;
      background:#ffffff; border-radius:8px; font-size:26px; font-weight:700;
      color:#c0785a; border:1px solid #eecdb9;
      animation:pulseGlow 2.4s ease-in-out infinite;
    }
    .code-label { font-size:13px; color:#9e7060; margin-top:12px; }

    .divider { border:none; border-top:1px dashed #ecdccb; margin:0 32px; }

    .footer { padding:22px 32px 30px; text-align:center; }
    .footer p { margin:0; font-size:13px; color:#b08070; }
    .signature { font-size:14px; color:#c0785a; font-weight:700; margin-top:6px; font-family:'Amiri',serif; }
  </style>
</head>
<body>
  <div class='wrapper'>
    <div class='header'>
      <h1>سرد</h1>
      <svg class='book-icon' width='54' height='40' viewBox='0 0 54 40' fill='none' xmlns='http://www.w3.org/2000/svg'>
        <path d='M27 8C22 4 14 3 6 5V32C14 30 22 31 27 35C32 31 40 30 48 32V5C40 3 32 4 27 8Z' stroke='#ffffff' stroke-width='2' stroke-linejoin='round' fill='rgba(255,255,255,0.12)'/>
        <path d='M27 8V35' stroke='#ffffff' stroke-width='2'/>
        <path d='M11 12H21M11 17H21M11 22H19' stroke='#ffffff' stroke-width='1.4' stroke-linecap='round' opacity='0.85'/>
        <path d='M33 12H43M33 17H43M35 22H43' stroke='#ffffff' stroke-width='1.4' stroke-linecap='round' opacity='0.85'/>
      </svg>
    </div>
    <div class='body'>
      <p class='greeting'>مرحباً " + displayName + @"</p>
      <p class='message'>" + message + @"</p>
      <div class='code-box'>
        <div class='code'>" + codeDigits + @"</div>
        <div class='code-label'>صالح لفترة محدودة، برجاء عدم مشاركته مع أحد</div>
      </div>
    </div>
    <hr class='divider'/>
    <div class='footer'>
      <p>بكل الحب والاهتمام 💗</p>
      <p class='signature'>فريق سرد</p>
    </div>
  </div>
</body>
</html>";
        }

        private static string GetMessage(string type) => type switch
        {
            "confirm" => "رمز تأكيد بريدك الإلكتروني هو",
            "reset" => "تلقينا طلباً لإعادة تعيين كلمة المرور. رمز إعادة التعيين هو:",
            _ => "رمز التحقق الخاص بك هو:"
        };
    }
}