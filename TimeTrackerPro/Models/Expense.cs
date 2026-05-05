// Models/Expense.cs
namespace TimeTrackerPro.Models
{

    public class Expense
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public string Description { get; set; } = string.Empty;

        public double Amount { get; set; }

        public ExpenseCategory Category { get; set; } = ExpenseCategory.Other;

        public DateTime Date { get; set; } = DateTime.Now;

        public string Reference { get; set; } = string.Empty;
    }

    public enum ExpenseCategory
    {
        Software,   // Licencias, suscripciones
        Hardware,   // Equipamiento físico
        Services,   // Servicios externos, freelancers
        Training,   // Cursos, libros
        Other       // Otros
    }
}
