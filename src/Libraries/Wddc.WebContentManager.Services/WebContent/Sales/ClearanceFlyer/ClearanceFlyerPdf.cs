using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Fonts;

namespace Wddc.WebContentManager.Services.WebContent.Sales.ClearanceFlyer
{
    /// <summary>
    /// Builds a clearance flyer PDF (2026-10-27) in one standard layout, with the look Bucky sets per flyer:
    ///   - a cover: his picture (if any), the title, the month, the intro and the note (Fire Sale: at or below cost);
    ///   - the items, grouped by product class, four to a row: picture, description, item code, Reg Price, and the
    ///     clearance price with how much off;
    ///   - on every item page the title and "Prices as of" at the top, and his footer notes and the page number at
    ///     the bottom.
    /// PDFsharp/MigraDoc 6.2 (MIT - later versions are AGPL, see the project file).
    /// </summary>
    public static class ClearanceFlyerPdf
    {
        /// <summary>The PDF, and what is in it.</summary>
        public sealed class Result
        {
            public byte[] Pdf { get; init; }
            public int Items { get; init; }
            public int Pages { get; init; }
        }

        private const double PageWidth = 8.5, PageHeight = 11, SideMargin = 0.55;
        private const double ContentWidth = PageWidth - 2 * SideMargin;
        private const int Columns = 4;

        private static readonly object FontLock = new object();
        private static bool _fontsReady;

        private static readonly Color Grey = new Color(105, 115, 125);
        private static readonly Color Dark = new Color(33, 41, 52);
        private static readonly Color Line = new Color(220, 226, 232);

        /// <summary>
        /// The flyer of <paramref name="flyer"/> with <paramref name="items"/> as they are at <paramref name="asOf"/>.
        /// <paramref name="pictures"/> has each item's picture, made small (ClearanceFlyerPictures.Fit); an item
        /// without one gets an empty box.
        /// </summary>
        public static Result Build(ClearanceFlyerDto flyer, ClearanceFlyerPictures.Picture cover, IReadOnlyList<ClearanceFlyerItemDto> items,
            IReadOnlyDictionary<string, ClearanceFlyerPictures.Picture> pictures, DateTime asOf)
        {
            EnsureFonts();

            var accent = ParseColor(flyer.AccentColor);
            var document = new Document();
            document.Info.Title = flyer.Title ?? "Clearance";
            document.Info.Author = "WDDC";

            var normal = document.Styles[StyleNames.Normal];
            normal.Font.Name = "Arial";
            normal.Font.Size = 9;
            normal.Font.Color = Dark;

            AddCover(document, flyer, cover, items.Count, asOf, accent);
            AddItems(document, flyer, items, pictures, asOf, accent);

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();
            var pages = renderer.PdfDocument.PageCount;   // before Save: a saved document can't be read any more
            using var stream = new MemoryStream();
            renderer.PdfDocument.Save(stream, false);

            return new Result { Pdf = stream.ToArray(), Items = items.Count, Pages = pages };
        }

        // Windows' own fonts (Arial) - PDFsharp 6 has to be told once per process.
        private static void EnsureFonts()
        {
            lock (FontLock)
            {
                if (_fontsReady)
                {
                    return;
                }

                if (GlobalFontSettings.FontResolver == null)
                {
                    GlobalFontSettings.UseWindowsFontsUnderWindows = true;
                }

                _fontsReady = true;
            }
        }

        private static Section NewSection(Document document, double top, double bottom)
        {
            var section = document.AddSection();
            var setup = section.PageSetup;
            setup.PageWidth = Unit.FromInch(PageWidth);
            setup.PageHeight = Unit.FromInch(PageHeight);
            setup.LeftMargin = Unit.FromInch(SideMargin);
            setup.RightMargin = Unit.FromInch(SideMargin);
            setup.TopMargin = Unit.FromInch(top);
            setup.BottomMargin = Unit.FromInch(bottom);
            setup.HeaderDistance = Unit.FromInch(0.35);
            setup.FooterDistance = Unit.FromInch(0.3);
            return section;
        }

        // ── The cover ──

        private static void AddCover(Document document, ClearanceFlyerDto flyer, ClearanceFlyerPictures.Picture cover, int itemCount, DateTime asOf, Color accent)
        {
            var section = NewSection(document, 0.6, 0.8);
            AddFooter(section, flyer, withPageNumber: false);

            if (cover != null)
            {
                // His picture, as big as fits above the title.
                var p = section.AddParagraph();
                p.Format.Alignment = ParagraphAlignment.Center;
                p.Format.SpaceAfter = Unit.FromPoint(16);
                FitImage(p, cover, ContentWidth, 6.6);
            }
            else
            {
                // No picture: a band in the flyer's colour.
                var band = section.AddTable();
                band.Borders.Width = 0;
                band.AddColumn(Unit.FromInch(ContentWidth));
                var row = band.AddRow();
                row.Height = Unit.FromInch(3.2);
                row.HeightRule = RowHeightRule.Exactly;
                row.VerticalAlignment = VerticalAlignment.Center;
                row.Cells[0].Shading.Color = accent;
                var p = row.Cells[0].AddParagraph(flyer.Title ?? string.Empty);
                p.Format.Alignment = ParagraphAlignment.Center;
                p.Format.Font.Size = 40;
                p.Format.Font.Bold = true;
                p.Format.Font.Color = Colors.White;
                section.AddParagraph().Format.SpaceAfter = Unit.FromPoint(18);
            }

            if (cover != null)
            {
                var title = section.AddParagraph(flyer.Title ?? string.Empty);
                title.Format.Alignment = ParagraphAlignment.Center;
                title.Format.Font.Size = 32;
                title.Format.Font.Bold = true;
                title.Format.Font.Color = accent;
            }

            if (!string.IsNullOrWhiteSpace(flyer.Subtitle))
            {
                var sub = section.AddParagraph(flyer.Subtitle.Trim());
                sub.Format.Alignment = ParagraphAlignment.Center;
                sub.Format.Font.Size = 18;
                sub.Format.Font.Color = Grey;
                sub.Format.SpaceBefore = Unit.FromPoint(4);
            }

            if (!string.IsNullOrWhiteSpace(flyer.Intro))
            {
                var intro = section.AddParagraph(flyer.Intro.Trim());
                intro.Format.Alignment = ParagraphAlignment.Center;
                intro.Format.Font.Size = 11;
                intro.Format.Font.Italic = true;
                intro.Format.SpaceBefore = Unit.FromPoint(12);
            }

            if (!string.IsNullOrWhiteSpace(flyer.Note))
            {
                var note = section.AddParagraph(flyer.Note.Trim());
                note.Format.Alignment = ParagraphAlignment.Center;
                note.Format.Font.Size = 11;
                note.Format.Font.Bold = true;
                note.Format.Font.Color = accent;
                note.Format.SpaceBefore = Unit.FromPoint(16);
                note.Format.Borders.Width = Unit.FromPoint(1);
                note.Format.Borders.Color = accent;
                note.Format.Borders.Distance = Unit.FromPoint(8);
                note.Format.LeftIndent = Unit.FromInch(0.6);
                note.Format.RightIndent = Unit.FromInch(0.6);
            }

            var count = section.AddParagraph(string.Format(CultureInfo.InvariantCulture, "{0} item{1}  ·  Prices as of {2:MMMM d, yyyy}",
                itemCount, itemCount == 1 ? "" : "s", asOf));
            count.Format.Alignment = ParagraphAlignment.Center;
            count.Format.Font.Size = 9;
            count.Format.Font.Color = Grey;
            count.Format.SpaceBefore = Unit.FromPoint(18);
        }

        // ── The items ──

        private static void AddItems(Document document, ClearanceFlyerDto flyer, IReadOnlyList<ClearanceFlyerItemDto> items,
            IReadOnlyDictionary<string, ClearanceFlyerPictures.Picture> pictures, DateTime asOf, Color accent)
        {
            var section = NewSection(document, string.IsNullOrWhiteSpace(flyer.Note) ? 0.8 : 1.0, 0.8);
            AddHeader(section, flyer, asOf, accent);
            AddFooter(section, flyer, withPageNumber: true);

            if (items.Count == 0)
            {
                var none = section.AddParagraph("No items are on this flyer right now.");
                none.Format.Font.Size = 12;
                none.Format.Font.Color = Grey;
                none.Format.SpaceBefore = Unit.FromInch(1);
                none.Format.Alignment = ParagraphAlignment.Center;
                return;
            }

            var cellWidth = ContentWidth / Columns;

            // The groups in the order the list has them (product class, then description).
            foreach (var group in items.GroupBy(i => string.IsNullOrWhiteSpace(i.GroupName) ? "Other" : i.GroupName.Trim()))
            {
                var table = section.AddTable();
                table.Borders.Width = 0;
                table.LeftPadding = Unit.FromPoint(4);
                table.RightPadding = Unit.FromPoint(4);
                table.TopPadding = Unit.FromPoint(2);
                table.BottomPadding = Unit.FromPoint(5);
                for (var c = 0; c < Columns; c++)
                {
                    table.AddColumn(Unit.FromInch(cellWidth));
                }

                // The heading: kept with the first row, and repeated at the top of a page the group runs onto.
                var head = table.AddRow();
                head.HeadingFormat = true;
                head.KeepWith = 1;
                head.Cells[0].MergeRight = Columns - 1;
                var h = head.Cells[0].AddParagraph(group.Key);
                h.Format.Font.Size = 12;
                h.Format.Font.Bold = true;
                h.Format.Font.Color = accent;
                h.Format.SpaceBefore = Unit.FromPoint(8);
                h.Format.Borders.Bottom.Width = Unit.FromPoint(1);
                h.Format.Borders.Bottom.Color = accent;
                h.Format.Borders.Distance = Unit.FromPoint(2);

                var list = group.ToList();
                for (var k = 0; k < list.Count; k += Columns)
                {
                    var row = table.AddRow();
                    row.VerticalAlignment = VerticalAlignment.Top;
                    for (var c = 0; c < Columns && k + c < list.Count; c++)
                    {
                        pictures.TryGetValue(list[k + c].ItemNumber ?? string.Empty, out var picture);
                        AddCard(row.Cells[c], list[k + c], picture, cellWidth, accent);
                    }

                    // A light line between rows.
                    row.Borders.Bottom.Width = Unit.FromPoint(0.5);
                    row.Borders.Bottom.Color = Line;
                }
            }
        }

        private static void AddCard(Cell cell, ClearanceFlyerItemDto item, ClearanceFlyerPictures.Picture picture, double cellWidth, Color accent)
        {
            var pic = cell.AddParagraph();
            pic.Format.Alignment = ParagraphAlignment.Center;
            pic.Format.SpaceBefore = Unit.FromPoint(2);
            pic.Format.SpaceAfter = Unit.FromPoint(3);
            if (picture != null)
            {
                FitImage(pic, picture, cellWidth - 0.3, 0.95);
            }
            else
            {
                // The same height as a picture, so the rows line up.
                pic.Format.LineSpacingRule = LineSpacingRule.Exactly;
                pic.Format.LineSpacing = Unit.FromInch(0.95);
                pic.AddFormattedText("No picture", new Font { Size = 7, Color = Line });
            }

            var description = cell.AddParagraph(CleanDescription(item.Description, item.ItemNumber));
            description.Format.Font.Size = 8;
            description.Format.Font.Bold = true;
            description.Format.SpaceAfter = Unit.FromPoint(2);

            var code = cell.AddParagraph();
            code.Format.Font.Size = 7.5;
            code.AddFormattedText("Code ", new Font { Color = Grey });
            code.AddFormattedText(item.ItemNumber ?? string.Empty, new Font { Bold = true, Color = accent });

            var hasReg = item.RegularPrice.HasValue && item.ClearancePrice.HasValue && item.RegularPrice.Value > item.ClearancePrice.Value;
            if (hasReg)
            {
                var reg = cell.AddParagraph("Reg Price " + Money(item.RegularPrice.Value));
                reg.Format.Font.Size = 7.5;
                reg.Format.Font.Color = Grey;
                reg.Format.SpaceBefore = Unit.FromPoint(2);
            }

            var price = cell.AddParagraph();
            price.Format.SpaceBefore = Unit.FromPoint(hasReg ? 0 : 2);
            price.AddFormattedText("Clearance ", new Font { Size = 7.5, Color = Grey });
            price.AddFormattedText(item.ClearancePrice.HasValue ? Money(item.ClearancePrice.Value) : "see the website", new Font { Size = 11, Bold = true, Color = accent });
            if (hasReg)
            {
                var off = (int)Math.Round((1 - item.ClearancePrice.Value / item.RegularPrice.Value) * 100m);
                if (off > 0)
                {
                    price.AddFormattedText("  " + off.ToString(CultureInfo.InvariantCulture) + "% off", new Font { Size = 7.5, Bold = true, Color = accent });
                }
            }
        }

        // ── Header and footer ──

        private static void AddHeader(Section section, ClearanceFlyerDto flyer, DateTime asOf, Color accent)
        {
            var table = section.Headers.Primary.AddTable();
            table.Borders.Width = 0;
            table.AddColumn(Unit.FromInch(ContentWidth - 2.3));
            table.AddColumn(Unit.FromInch(2.3));
            var row = table.AddRow();
            row.Borders.Bottom.Width = Unit.FromPoint(1.5);
            row.Borders.Bottom.Color = accent;
            row.BottomPadding = Unit.FromPoint(3);

            var left = row.Cells[0].AddParagraph();
            left.AddFormattedText(flyer.Title ?? string.Empty, new Font { Size = 11, Bold = true, Color = accent });
            if (!string.IsNullOrWhiteSpace(flyer.Subtitle))
            {
                left.AddFormattedText("   " + flyer.Subtitle.Trim(), new Font { Size = 10, Color = Grey });
            }

            var right = row.Cells[1].AddParagraph(string.Format(CultureInfo.InvariantCulture, "Prices as of {0:MMMM d, yyyy}", asOf));
            right.Format.Alignment = ParagraphAlignment.Right;
            right.Format.Font.Size = 8;
            right.Format.Font.Color = Grey;
            row.Cells[1].VerticalAlignment = VerticalAlignment.Bottom;

            if (!string.IsNullOrWhiteSpace(flyer.Note))
            {
                var note = section.Headers.Primary.AddParagraph(flyer.Note.Trim());
                note.Format.Font.Size = 8;
                note.Format.Font.Bold = true;
                note.Format.Font.Color = accent;
                note.Format.SpaceBefore = Unit.FromPoint(4);
            }
        }

        private static void AddFooter(Section section, ClearanceFlyerDto flyer, bool withPageNumber)
        {
            var parts = (flyer.Footer ?? string.Empty).Split('|').Select(p => p.Trim()).Where(p => p.Length > 0).ToList();
            if (parts.Count > 0)
            {
                var notes = section.Footers.Primary.AddParagraph(string.Join("     ·     ", parts));
                notes.Format.Alignment = ParagraphAlignment.Center;
                notes.Format.Font.Size = 8;
                notes.Format.Font.Color = Grey;
            }

            if (withPageNumber)
            {
                var page = section.Footers.Primary.AddParagraph();
                page.Format.Alignment = ParagraphAlignment.Center;
                page.Format.Font.Size = 7.5;
                page.Format.Font.Color = Grey;
                page.Format.SpaceBefore = Unit.FromPoint(2);
                page.AddText("Page ");
                page.AddPageField();
                page.AddText(" of ");
                page.AddNumPagesField();
            }
        }

        // ── Helpers ──

        // A picture as big as fits in maxWidth x maxHeight inches, keeping its shape.
        private static void FitImage(Paragraph paragraph, ClearanceFlyerPictures.Picture picture, double maxWidth, double maxHeight)
        {
            var ratio = picture.Width / (double)picture.Height;
            var width = maxWidth;
            var height = width / ratio;
            if (height > maxHeight)
            {
                height = maxHeight;
                width = height * ratio;
            }

            var image = paragraph.AddImage("base64:" + Convert.ToBase64String(picture.Bytes));
            image.LockAspectRatio = false;
            image.Width = Unit.FromInch(width);
            image.Height = Unit.FromInch(height);
        }

        // GP's status tags at the end of a description - (d), (nr), (so), (tp) - mean nothing to a clinic.
        private static readonly Regex StatusTags = new Regex(@"(\s*\((?:d|nr|so|tp|dc)\))+\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static string CleanDescription(string description, string itemNumber)
        {
            var text = StatusTags.Replace((description ?? string.Empty).Trim(), string.Empty).Trim();
            return text.Length > 0 ? text : itemNumber ?? string.Empty;
        }

        private static string Money(decimal value) => string.Format(CultureInfo.InvariantCulture, "${0:#,0.00}", value);

        // "#RRGGBB"; anything else is WDDC blue.
        private static Color ParseColor(string hex)
        {
            if (!string.IsNullOrWhiteSpace(hex) && Regex.IsMatch(hex.Trim(), "^#[0-9A-Fa-f]{6}$"))
            {
                var v = hex.Trim();
                return new Color(Convert.ToByte(v.Substring(1, 2), 16), Convert.ToByte(v.Substring(3, 2), 16), Convert.ToByte(v.Substring(5, 2), 16));
            }

            return new Color(31, 78, 121);
        }
    }
}
