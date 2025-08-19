using TaskService.Domain.Common;

namespace TaskService.Domain.Entities
{
    public class TaskItem : BaseEntity
    {
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public DateTime? DueDate { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateUpdated { get; set; }

        private TaskItem() { }

        public TaskItem(string title, string description, DateTime? dueDate = null)
        {
            Id = Guid.NewGuid();
            Title = title;
            Description = description;
            DueDate = dueDate;
            DateCreated = DateTime.UtcNow;
        }


    }
}