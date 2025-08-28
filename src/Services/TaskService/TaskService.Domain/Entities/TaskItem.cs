using TaskService.Domain.Common;

namespace TaskService.Domain.Entities
{
    public class TaskItem : BaseEntity
    {
        public string Title { get; private set; } = default!;
        public string Description { get; private set; } = default!;
        public DateTime? DueDate { get; private set; }
        public DateTime DateCreated { get; private set; } = DateTime.UtcNow;
        public DateTime? DateModified { get; private set; }

        private TaskItem() { }

        public TaskItem(string title, string description, DateTime? dueDate = null)
        {
            Id = Guid.NewGuid();
            Title = title;
            Description = description;
            DueDate = dueDate;
            DateCreated = DateTime.UtcNow;
            DateModified = null;
        }

        public void Update(string title, string description, DateTime? dueDate)
        {
            Title = title;
            Description = description;
            DueDate = dueDate;
            DateModified = DateTime.UtcNow;
        }
    }
}