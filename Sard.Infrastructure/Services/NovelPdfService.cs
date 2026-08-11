using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq;

public class NovelPdfService
{
    private const string PrimaryColor = "#7a2e32";
    private const string PrimaryDarkColor = "#5e2226";
    private const string MaroonColor = "#4e1d20";
    private const string TanColor = "#e0bfbc";
    private const string BgColor = "#f9eeec";
    private const string TextColor = "#3a1f1f";
    private const string TextLightColor = "#8a6a68";
    private const string BorderColor = "#eddad7";
    private const string GoldColor = "#c9a15a";

    public byte[] GeneratePdf(NovelDownloadDto novel)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.ContentFromRightToLeft();
                page.DefaultTextStyle(x => x
                    .FontFamily("Amiri")
                    .FontColor(TextColor)
                    .DirectionFromRightToLeft());

                page.Content().Background(BgColor).Column(col =>
                {
                    col.Item().Height(280).Background(PrimaryColor).AlignCenter().AlignMiddle().Column(logoCol =>
                    {
                        logoCol.Item().AlignCenter().Text("سـرد")
                            .FontSize(60).Bold().FontColor("#ffffff");

                        logoCol.Item().Height(8);

                        logoCol.Item().AlignCenter().Text("منصة الروايات العربية")
                            .FontSize(13).FontColor(TanColor);
                    });

                    col.Item().Height(60);

                    col.Item().AlignCenter().PaddingHorizontal(40).Text(novel.Title)
                        .FontSize(32)
                        .Bold()
                        .FontColor(MaroonColor)
                        .DirectionFromRightToLeft();

                    col.Item().Height(14);

                    col.Item().AlignCenter().Text($"بقلم: {novel.AuthorName}")
                        .FontSize(15)
                        .FontColor(TextLightColor)
                        .DirectionFromRightToLeft();

                    col.Item().Height(30);

                    col.Item().AlignCenter().Width(120).LineHorizontal(2).LineColor(GoldColor);

                    if (!string.IsNullOrEmpty(novel.Description))
                    {
                        col.Item().Height(30);

                        col.Item().AlignCenter().Width(420)
                            .Background(Colors.White)
                            .Border(1)
                            .BorderColor(BorderColor)
                            .CornerRadius(12)
                            .Padding(20)
                            .Text(novel.Description)
                            .FontSize(12.5f)
                            .FontColor(TextLightColor)
                            .Italic()
                            .AlignCenter()
                            .LineHeight(1.6f)
                            .DirectionFromRightToLeft();
                    }
                });

                page.Footer().Height(60).AlignCenter().AlignMiddle()
                    .Text($"عدد الفصول: {novel.Chapters.Count()}")
                    .FontSize(11)
                    .FontColor(TextLightColor)
                    .DirectionFromRightToLeft();
            });

            foreach (var chapter in novel.Chapters.OrderBy(c => c.Order))
            {
                var currentChapter = chapter;

                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.MarginHorizontal(2.5f, Unit.Centimetre);
                    page.MarginVertical(2.5f, Unit.Centimetre);
                    page.ContentFromRightToLeft();

                    page.DefaultTextStyle(x => x
                        .FontSize(13)
                        .FontFamily("Amiri")
                        .FontColor(TextColor)
                        .DirectionFromRightToLeft());

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().AlignRight().Text(novel.Title)
                                .FontSize(10)
                                .FontColor(TextLightColor)
                                .DirectionFromRightToLeft();

                            row.RelativeItem().AlignLeft().Text("سرد")
                                .FontSize(10)
                                .Bold()
                                .FontColor(PrimaryColor);
                        });

                        col.Item().Height(6);
                        col.Item().LineHorizontal(1).LineColor(BorderColor);
                        col.Item().Height(16);

                        col.Item()
                            .Background(BgColor)
                            .Border(1)
                            .BorderColor(BorderColor)
                            .CornerRadius(10)
                            .Padding(16)
                            .Column(ch =>
                            {
                                ch.Item().AlignRight().Text($"الفصل {currentChapter.Order}")
                                    .FontSize(11)
                                    .FontColor(PrimaryColor)
                                    .Bold()
                                    .DirectionFromRightToLeft();

                                ch.Item().Height(4);

                                ch.Item().AlignRight().Text(currentChapter.Title)
                                    .FontSize(19)
                                    .Bold()
                                    .FontColor(MaroonColor)
                                    .DirectionFromRightToLeft();
                            });

                        col.Item().Height(20);
                    });

                    page.Content()
                        .AlignRight()
                        .Text(currentChapter.Content)
                        .FontSize(13)
                        .LineHeight(2f)
                        .ParagraphSpacing(10)
                        .FontColor(TextColor)
                        .DirectionFromRightToLeft();

                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(BorderColor);
                        col.Item().Height(6);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().AlignRight().Text(x =>
                            {
                                x.DefaultTextStyle(s => s.DirectionFromRightToLeft());
                                x.CurrentPageNumber().FontColor(PrimaryColor).FontSize(9).Bold();
                                x.Span(" / ").FontColor(TextLightColor).FontSize(9);
                                x.TotalPages().FontColor(TextLightColor).FontSize(9);
                            });

                            row.RelativeItem().AlignLeft().Text("سرد — منصة الروايات العربية")
                                .FontColor(TextLightColor)
                                .FontSize(9);
                        });
                    });
                });
            }
        }).GeneratePdf();
    }
}