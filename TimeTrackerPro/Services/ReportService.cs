// Services/ReportService.cs
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TimeTrackerPro.Models;

namespace TimeTrackerPro.Services
{
    /// <summary>
    /// Genera informes PDF de proyectos usando QuestPDF.
    /// La API de QuestPDF es "fluent": cada método devuelve el mismo
    /// objeto para poder encadenar llamadas, lo que hace el código
    /// muy legible de arriba a abajo.
    /// </summary>
    public class ReportService
    {
        /// <summary>
        /// Genera un PDF completo del proyecto y lo guarda en la ruta indicada.
        /// </summary>
        public void GenerateProjectReport(Project project, string outputPath)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Configuración de página
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    // Cabecera de página
                    page.Header().Element(ComposeHeader(project));

                    // Contenido principal
                    page.Content().Element(ComposeContent(project));

                    // Pie de página
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("TimeTracker Pro · Generado el ");
                        text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                            .FontColor(Colors.Grey.Medium);
                        text.Span(" · Página ");
                        text.CurrentPageNumber();
                        text.Span(" de ");
                        text.TotalPages();
                    });
                });
            })
            .GeneratePdf(outputPath);
        }

        // ——— Secciones del documento ———

        private Action<IContainer> ComposeHeader(Project project) => container =>
        {
            container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                .PaddingBottom(10)
                .Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(project.Name)
                            .FontSize(22).Bold().FontColor(Colors.Blue.Darken2);

                        if (!string.IsNullOrEmpty(project.Description))
                            col.Item().Text(project.Description)
                                .FontSize(11).FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(160).Column(col =>
                    {
                        col.Item().AlignRight().Text("INFORME DE PROYECTO")
                            .FontSize(11).Bold().FontColor(Colors.Grey.Medium);
                        col.Item().AlignRight()
                            .Text(DateTime.Now.ToString("dd MMMM yyyy"))
                            .FontSize(10).FontColor(Colors.Grey.Medium);
                    });
                });
        };

        private Action<IContainer> ComposeContent(Project project) => container =>
        {
            container.Column(col =>
            {
                col.Spacing(16);

                // Tarjetas de resumen
                col.Item().Element(ComposeSummaryCards(project));

                // Barra de progreso
                col.Item().Element(ComposeProgressBar(project));

                // Secciones
                if (project.Sections.Any())
                    col.Item().Element(ComposeSections(project));

                // Gastos
                if (project.Expenses.Any())
                    col.Item().Element(ComposeExpenses(project));
            });
        };

        private Action<IContainer> ComposeSummaryCards(Project project) => container =>
        {
            container.Row(row =>
            {
                row.Spacing(8);

                SummaryCard(row.RelativeItem(), "Horas estimadas",
                    $"{project.TotalEstimatedHours:F1}h", Colors.Blue.Darken2);

                SummaryCard(row.RelativeItem(), "Horas trabajadas",
                    $"{project.TotalWorkedHours:F1}h", Colors.Teal.Darken1);

                SummaryCard(row.RelativeItem(), "Horas/semana",
                    $"{project.WeeklyHours}h", Colors.Orange.Darken1);

                // Calculamos fecha estimada de fin
                var remaining = project.TotalEstimatedHours - project.TotalWorkedHours;
                var endDate = remaining > 0 && project.WeeklyHours > 0
                    ? DateTime.Now.AddDays((remaining / project.WeeklyHours) * 7)
                        .ToString("dd/MM/yyyy")
                    : "Completado";

                SummaryCard(row.RelativeItem(), "Fin estimado",
                    endDate, Colors.Purple.Darken1);
            });
        };

        private void SummaryCard(IContainer container, string label,
            string value, string color)
        {
            container
                .Border(1).BorderColor(Colors.Grey.Lighten2)
                .CornerRadius(6).Padding(12)
                .Column(col =>
                {
                    col.Item().Text(label)
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                    col.Item().Text(value)
                        .FontSize(16).Bold().FontColor(color);
                });
        }

        private Action<IContainer> ComposeProgressBar(Project project) => container =>
        {
            var percent = project.TotalEstimatedHours > 0
                ? Math.Min(100, (project.TotalWorkedHours / project.TotalEstimatedHours) * 100)
                : 0;

            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("Progreso general")
                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                    row.ConstantItem(50).AlignRight()
                        .Text($"{percent:F0}%").Bold()
                        .FontColor(Colors.Blue.Darken1);
                });

                // Barra de fondo
                col.Item().Height(8).Background(Colors.Grey.Lighten3)
                    .CornerRadius(4);

                // Barra de progreso (superponemos con margen negativo)
                col.Item().TranslateY(-8)
                    .Width((float)(percent / 100 * 515))
                    .Height(8).Background(Colors.Blue.Darken1)
                    .CornerRadius(4);
            });
        };

        private Action<IContainer> ComposeSections(Project project) => container =>
        {
            container.Column(col =>
            {
                col.Item().Text("Secciones")
                    .FontSize(14).Bold().FontColor(Colors.Blue.Darken2);

                col.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2)
                    .PaddingTop(8).Column(sectionsCol =>
                    {
                        sectionsCol.Spacing(8);

                        foreach (var section in project.Sections)
                        {
                            // Fila de la sección
                            sectionsCol.Item()
                                .Background(Colors.Blue.Lighten5)
                                .CornerRadius(4).Padding(10)
                                .Row(row =>
                                {
                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text(section.Name)
                                            .FontSize(11).Bold();

                                        if (section.WorkSessions.Any())
                                        {
                                            c.Item().Text(
                                                $"{section.WorkSessions.Count} sesiones · " +
                                                $"Última: {section.WorkSessions.Max(ws => ws.StartTime):dd/MM/yyyy}")
                                                .FontSize(9).FontColor(Colors.Grey.Medium);
                                        }
                                    });

                                    row.ConstantItem(180).AlignRight().Column(c =>
                                    {
                                        c.Item().AlignRight().Text(
                                            $"{section.TotalWorkedHours:F1}h / {section.EstimatedHours}h")
                                            .FontSize(10);

                                        var pct = section.EstimatedHours > 0
                                            ? Math.Min(100, section.TotalWorkedHours
                                                / section.EstimatedHours * 100)
                                            : 0;

                                        c.Item().AlignRight().Text($"{pct:F0}% completado")
                                            .FontSize(9).FontColor(Colors.Grey.Medium);
                                    });
                                });

                            // Subsecciones
                            foreach (var sub in section.SubSections)
                            {
                                sectionsCol.Item()
                                    .PaddingLeft(20)
                                    .Background(Colors.Grey.Lighten4)
                                    .CornerRadius(4).Padding(8)
                                    .Row(row =>
                                    {
                                        row.RelativeItem().Text($"↳ {sub.Name}")
                                            .FontSize(10).FontColor(Colors.Grey.Darken2);

                                        row.ConstantItem(180).AlignRight()
                                            .Text($"{sub.TotalWorkedHours:F1}h / {sub.EstimatedHours}h")
                                            .FontSize(9).FontColor(Colors.Grey.Medium);
                                    });
                            }
                        }
                    });
            });
        };

        private Action<IContainer> ComposeExpenses(Project project) => container =>
        {
            container.Column(col =>
            {
                col.Item().Text("Gastos")
                    .FontSize(14).Bold().FontColor(Colors.Orange.Darken2);

                col.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2)
                    .PaddingTop(8).Table(table =>
                    {
                        // Definición de columnas
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3); // Descripción
                            cols.RelativeColumn(2); // Categoría
                            cols.RelativeColumn(1); // Importe
                            cols.RelativeColumn(2); // Fecha
                        });

                        // Cabecera de la tabla
                        table.Header(header =>
                        {
                            foreach (var h in new[] { "Descripción", "Categoría", "Importe", "Fecha" })
                                header.Cell().Background(Colors.Orange.Lighten4)
                                    .Padding(6).Text(h).Bold().FontSize(9);
                        });

                        // Filas de gastos
                        foreach (var expense in project.Expenses)
                        {
                            table.Cell().Padding(6).Text(expense.Description).FontSize(9);
                            table.Cell().Padding(6).Text(expense.Category.ToString()).FontSize(9);
                            table.Cell().Padding(6).AlignRight()
                                .Text($"{expense.Amount:C}").FontSize(9);
                            table.Cell().Padding(6)
                                .Text(expense.Date.ToString("dd/MM/yyyy")).FontSize(9);
                        }

                        // Fila de total
                        table.Cell().ColumnSpan(2).Padding(6)
                            .Text("TOTAL").Bold().FontSize(9);
                        table.Cell().Padding(6).AlignRight()
                            .Text($"{project.TotalExpenses:C}").Bold()
                            .FontColor(Colors.Orange.Darken2).FontSize(9);
                        table.Cell();
                    });
            });
        };
    }
}
