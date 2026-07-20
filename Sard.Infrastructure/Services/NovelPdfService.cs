using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class NovelPdfService
{
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

                page.DefaultTextStyle(x => x
                    .FontSize(13)
                    .FontFamily("Arial")
                    .FontColor("#4a3428"));

                // Cover Page
                page.Header().ShowOnce().Column(col =>
                {
                    col.Item().Height(180).Background("#6b2d2d")
                        .AlignCenter().AlignMiddle()
                        .Text("سـرد")
                        .FontSize(48)
                        .FontColor("#ffffff")
                        .Bold();

                    col.Item().Height(20);

                    col.Item().AlignCenter().Text(novel.Title)
                        .FontSize(26)
                        .Bold()
                        .FontColor("#6b2d2d");

                    col.Item().Height(8);

                    col.Item().AlignCenter().Text($"بقلم: {novel.AuthorName}")
                        .FontSize(14)
                        .FontColor("#8a7264");

                    if (!string.IsNullOrEmpty(novel.Description))
                    {
                        col.Item().Height(16);
                        col.Item().Background("#fdf3ee")
                            .Padding(16)
                            .Text(novel.Description)
                            .FontSize(12)
                            .FontColor("#6b4f3f")
                            .Italic()
                            .AlignCenter();
                    }

                    col.Item().Height(20);
                    col.Item().LineHorizontal(1.5f).LineColor("#c0785a");
                });

                page.Content().Column(content =>
                {
                    foreach (var chapter in novel.Chapters.OrderBy(c => c.Order))
                    {
                        content.Item().PageBreak();

                        content.Item()
                            .Background("#fdf3ee")
                            .Padding(16)
                            .Column(ch =>
                            {
                                ch.Item().Text($"الفصل {chapter.Order}")
                                    .FontSize(11)
                                    .FontColor("#c0785a")
                                    .Bold();

                                ch.Item().Text(chapter.Title)
                                    .FontSize(18)
                                    .Bold()
                                    .FontColor("#6b2d2d");
                            });

                        content.Item().Height(20);

                        content.Item()
                            .Text(chapter.Content)
                            .FontSize(13)
                            .LineHeight(2f)
                            .FontColor("#4a3428");
                    }
                });

                page.Footer().Row(row =>
                {
                    row.RelativeItem().AlignRight()
                        .Text(x =>
                        {
                            x.Span("سرد — منصة الروايات العربية  |  صفحة ")
                                .FontColor("#b89a8a").FontSize(9);
                            x.CurrentPageNumber()
                                .FontColor("#c0785a").FontSize(9).Bold();
                        });
                });
            });
        }).GeneratePdf();
    }
}