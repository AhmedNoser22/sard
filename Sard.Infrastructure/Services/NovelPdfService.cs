using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class NovelPdfService
{
    // نفس ألوان هوية سرد المستخدمة في الفرونت (novel-settings.scss)
    private const string PrimaryColor = "#7a2e32";      // --sard-primary
    private const string PrimaryDarkColor = "#5e2226";  // --sard-primary-dark
    private const string MaroonColor = "#4e1d20";        // --sard-maroon
    private const string TanColor = "#e0bfbc";            // --sard-tan
    private const string BgColor = "#f9eeec";              // --sard-bg
    private const string TextColor = "#3a1f1f";            // --sard-text
    private const string TextLightColor = "#8a6a68";      // --sard-text-light
    private const string BorderColor = "#eddad7";          // --sard-border

    public byte[] GeneratePdf(NovelDownloadDto novel)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(2.5f, Unit.Centimetre);
                page.MarginVertical(3, Unit.Centimetre);

                // أهم سطر لعرض عربي سليم: يقلب اتجاه الصفحة كلها لليمين لليسار
                page.ContentFromRightToLeft();

                page.DefaultTextStyle(x => x
                    .FontSize(13)
                    .FontFamily("Amiri")
                    .FontColor(TextColor)
                    .DirectionFromRightToLeft());

                // Cover Page
                page.Header().ShowOnce().Column(col =>
                {
                    col.Item().Height(180).Background(PrimaryColor)
                        .AlignCenter().AlignMiddle()
                        .Text("سـرد")
                        .FontSize(48)
                        .FontColor("#ffffff")
                        .Bold();

                    col.Item().Height(20);

                    col.Item().AlignCenter().Text(novel.Title)
                        .FontSize(26)
                        .Bold()
                        .FontColor(MaroonColor)
                        .DirectionFromRightToLeft();

                    col.Item().Height(8);

                    col.Item().AlignCenter().Text($"بقلم: {novel.AuthorName}")
                        .FontSize(14)
                        .FontColor(TextLightColor)
                        .DirectionFromRightToLeft();

                    if (!string.IsNullOrEmpty(novel.Description))
                    {
                        col.Item().Height(16);
                        col.Item().Background(BgColor)
                            .Border(1)
                            .BorderColor(BorderColor)
                            .CornerRadius(10)
                            .Padding(16)
                            .Text(novel.Description)
                            .FontSize(12)
                            .FontColor(TextLightColor)
                            .Italic()
                            .AlignCenter()
                            .DirectionFromRightToLeft();
                    }

                    col.Item().Height(20);
                    col.Item().LineHorizontal(1.5f).LineColor(TanColor);
                });

                page.Content().Column(content =>
                {
                    foreach (var chapter in novel.Chapters.OrderBy(c => c.Order))
                    {
                        content.Item().PageBreak();

                        content.Item()
                            .Background(BgColor)
                            .Border(1)
                            .BorderColor(BorderColor)
                            .CornerRadius(10)
                            .Padding(16)
                            .Column(ch =>
                            {
                                ch.Item().AlignRight().Text($"الفصل {chapter.Order}")
                                    .FontSize(11)
                                    .FontColor(PrimaryColor)
                                    .Bold()
                                    .DirectionFromRightToLeft();

                                ch.Item().AlignRight().Text(chapter.Title)
                                    .FontSize(18)
                                    .Bold()
                                    .FontColor(MaroonColor)
                                    .DirectionFromRightToLeft();
                            });

                        content.Item().Height(20);

                        content.Item()
                            .AlignRight()
                            .Text(chapter.Content)
                            .FontSize(13)
                            .LineHeight(2f)
                            .ParagraphSpacing(10)
                            .FontColor(TextColor)
                            .DirectionFromRightToLeft();
                    }
                });

                page.Footer().Row(row =>
                {
                    row.RelativeItem().AlignRight()
                        .Text(x =>
                        {
                            x.DefaultTextStyle(s => s.DirectionFromRightToLeft());
                            x.Span("سرد — منصة الروايات العربية  |  صفحة ")
                                .FontColor(TextLightColor).FontSize(9);
                            x.CurrentPageNumber()
                                .FontColor(PrimaryColor).FontSize(9).Bold();
                        });
                });
            });
        }).GeneratePdf();
    }
}