using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TodoList
{
    public enum TaskStatus
    {
        Pending,
        InProgress,
        Completed
    }

    public class TodoTask
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        public string Project { get; set; } = "General";
    }

    public class TaskManager
    {
        private readonly string _filePath;
        private List<TodoTask> _tasks;
        private readonly JsonSerializerOptions _jsonOptions;

        public TaskManager(string fileName = "tasks.json")
        {
            _filePath = Path.Combine(AppContext.BaseDirectory, fileName);
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
            _tasks = LoadTasks();
        }

        private List<TodoTask> LoadTasks()
        {
            if (!File.Exists(_filePath))
            {
                return new List<TodoTask>();
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<TodoTask>>(json, _jsonOptions) ?? new List<TodoTask>();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error reading storage file: {ex.Message}. Starting with an empty list.");
                Console.ResetColor();
                return new List<TodoTask>();
            }
        }

        public void SaveTasks()
        {
            try
            {
                string json = JsonSerializer.Serialize(_tasks, _jsonOptions);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error saving tasks: {ex.Message}");
                Console.ResetColor();
            }
        }

        public void AddTask(string title, DateTime dueDate, TaskStatus status, string project)
        {
            var task = new TodoTask
            {
                Title = title,
                DueDate = dueDate,
                Status = status,
                Project = string.IsNullOrWhiteSpace(project) ? "General" : project
            };

            _tasks.Add(task);
            SaveTasks();
        }

        public bool EditTask(int index, string? newTitle, DateTime? newDueDate, TaskStatus? newStatus, string? newProject)
        {
            if (index < 0 || index >= _tasks.Count)
                return false;

            var task = _tasks[index];
            if (!string.IsNullOrWhiteSpace(newTitle)) task.Title = newTitle;
            if (newDueDate.HasValue) task.DueDate = newDueDate.Value;
            if (newStatus.HasValue) task.Status = newStatus.Value;
            if (!string.IsNullOrWhiteSpace(newProject)) task.Project = newProject;

            SaveTasks();
            return true;
        }

        public bool MarkTaskAsCompleted(int index)
        {
            if (index < 0 || index >= _tasks.Count)
                return false;

            _tasks[index].Status = TaskStatus.Completed;
            SaveTasks();
            return true;
        }

        public bool RemoveTask(int index)
        {
            if (index < 0 || index >= _tasks.Count)
                return false;

            _tasks.RemoveAt(index);
            SaveTasks();
            return true;
        }

        public IReadOnlyList<TodoTask> GetTasks() => _tasks.AsReadOnly();

        public IEnumerable<TodoTask> GetTasksSortedByDate() =>
            _tasks.OrderBy(t => t.DueDate);

        public IEnumerable<IGrouping<string, TodoTask>> GetTasksGroupedByProject() =>
            _tasks.GroupBy(t => t.Project, StringComparer.OrdinalIgnoreCase).OrderBy(g => g.Key);
    }

    internal class Program
    {
        private static readonly TaskManager Manager = new TaskManager();

        static void Main()
        {
            bool running = true;
            while (running)
            {
                Console.Clear();
                Console.WriteLine(">> Welcome to ToDoList");
                RenderSummary();
                Console.WriteLine();
                Console.WriteLine(">> (1) Show Task List by date or project");
                Console.WriteLine(">> (2) Add New Task");
                Console.WriteLine(">> (3) Edit Task (update, mark as done, remove)");
                Console.WriteLine(">> (4) Save and Quit");
                Console.WriteLine("========================================");
                Console.Write(">> Select an option (1-4): ");

                switch (Console.ReadLine()?.Trim())
                {
                    case "1":
                        ShowTaskListMenu();
                        break;
                    case "2":
                        CreateTaskView();
                        break;
                    case "3":
                        EditTaskView();
                        break;
                    case "4":
                        Manager.SaveTasks();
                        Console.WriteLine("\nAll tasks saved. Goodbye!");
                        running = false;
                        break;
                    default:
                        PauseMessage("Invalid option. Press any key to try again.");
                        break;
                }
            }
        }

        private static void RenderSummary()
        {
            var tasks = Manager.GetTasks();
            int incompleteCount = tasks.Count(t => t.Status != TaskStatus.Completed);
            int completedCount = tasks.Count(t => t.Status == TaskStatus.Completed);

            Console.WriteLine($">> You have {incompleteCount} tasks to do and {completedCount} tasks are done!");
        }

        private static void CreateTaskView()
        {
            Console.Clear();
            Console.WriteLine("--- Add New Task ---");

            string title = PromptRequired("Title: ");
            DateTime dueDate = PromptDate("Due Date (yyyy-MM-dd): ");
            TaskStatus status = PromptStatus();
            Console.Write("Project (Default: 'General'): ");
            string? project = Console.ReadLine();

            Manager.AddTask(title, dueDate, status, string.IsNullOrWhiteSpace(project) ? "General" : project);
            PauseMessage("Task created successfully!");
        }

        private static void EditTaskView()
        {
            Console.Clear();
            Console.WriteLine("--- Edit Task ---");

            var tasks = Manager.GetTasks();
            if (!tasks.Any())
            {
                PauseMessage("No tasks found to edit.");
                return;
            }

            RenderTable(tasks);

            Console.Write("\nEnter task number to edit (1 to {0}) or 0 to cancel: ", tasks.Count);
            if (!int.TryParse(Console.ReadLine(), out int selection) || selection < 1 || selection > tasks.Count)
            {
                PauseMessage("Operation cancelled or invalid selection.");
                return;
            }

            int targetIndex = selection - 1;
            var current = tasks[targetIndex];

            Console.WriteLine($"\nSelected: \"{current.Title}\"");
            Console.WriteLine("1. Update task details");
            Console.WriteLine("2. Mark as completed");
            Console.WriteLine("3. Remove task");
            Console.WriteLine("4. Cancel");
            Console.Write("Select an action (1-4): ");

            string? action = Console.ReadLine()?.Trim();
            switch (action)
            {
                case "1":
                    UpdateTaskDetails(targetIndex, current);
                    break;
                case "2":
                    Manager.MarkTaskAsCompleted(targetIndex);
                    PauseMessage("Task marked as completed!");
                    break;
                case "3":
                    Console.Write($"Are you sure you want to delete \"{current.Title}\"? (y/n): ");
                    if (Console.ReadLine()?.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        Manager.RemoveTask(targetIndex);
                        PauseMessage("Task removed successfully!");
                    }
                    else
                    {
                        PauseMessage("Deletion cancelled.");
                    }
                    break;
                default:
                    PauseMessage("No changes made.");
                    break;
            }
        }

        private static void UpdateTaskDetails(int index, TodoTask current)
        {
            Console.WriteLine($"\nEditing: \"{current.Title}\" (Leave blank to keep current value)");

            Console.Write($"New Title [{current.Title}]: ");
            string? newTitle = Console.ReadLine();

            Console.Write($"New Due Date (yyyy-MM-dd) [{current.DueDate:yyyy-MM-dd}]: ");
            DateTime? newDate = null;
            string? dateInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(dateInput))
            {
                if (DateTime.TryParseExact(dateInput, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
                {
                    if (parsed.Date < DateTime.Today)
                    {
                        Console.WriteLine("Due date cannot be in the past. Retaining existing date.");
                    }
                    else
                    {
                        newDate = parsed;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid format. Retaining existing date.");
                }
            }

            Console.WriteLine($"Current Status: {current.Status}");
            Console.WriteLine("Change status to: 1. Pending  2. InProgress  3. Completed  (Blank to skip)");
            TaskStatus? newStatus = Console.ReadLine()?.Trim() switch
            {
                "1" => TaskStatus.Pending,
                "2" => TaskStatus.InProgress,
                "3" => TaskStatus.Completed,
                _ => null
            };

            Console.Write($"New Project [{current.Project}]: ");
            string? newProj = Console.ReadLine();

            Manager.EditTask(index, newTitle, newDate, newStatus, newProj);
            PauseMessage("Task updated successfully!");
        }

        private static void ShowTaskListMenu()
        {
            Console.Clear();
            Console.WriteLine("--- Show Task List ---");
            Console.WriteLine("1. View by Due Date");
            Console.WriteLine("2. View by Project");
            Console.WriteLine("3. Cancel");
            Console.Write("Select an option (1-3): ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    DisplayByDateView();
                    break;
                case "2":
                    DisplayByProjectView();
                    break;
                case "3":
                    return;
                default:
                    PauseMessage("Invalid option.");
                    break;
            }
        }

        private static void DisplayByDateView()
        {
            Console.Clear();
            Console.WriteLine("--- Tasks Sorted by Due Date ---");

            var tasks = Manager.GetTasksSortedByDate().ToList();
            if (!tasks.Any())
            {
                PauseMessage("No tasks available.");
                return;
            }

            RenderTable(tasks);
            PauseMessage();
        }

        private static void DisplayByProjectView()
        {
            Console.Clear();
            Console.WriteLine("--- Tasks Grouped by Project ---");

            var grouped = Manager.GetTasksGroupedByProject().ToList();
            if (!grouped.Any())
            {
                PauseMessage("No tasks available.");
                return;
            }

            foreach (var group in grouped)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n[ Project: {group.Key} ]");
                Console.ResetColor();
                RenderTable(group.ToList());
            }

            PauseMessage();
        }

        private static void RenderTable(IEnumerable<TodoTask> tasks)
        {
            Console.WriteLine(new string('-', 74));
            Console.WriteLine($"{"#",-3} | {"Title",-24} | {"Due Date",-12} | {"Status",-12} | {"Project",-14}");
            Console.WriteLine(new string('-', 74));

            int index = 1;
            foreach (var t in tasks)
            {
                string shortTitle = t.Title.Length > 24 ? t.Title[..21] + "..." : t.Title;
                string shortProj = t.Project.Length > 14 ? t.Project[..11] + "..." : t.Project;

                Console.WriteLine($"{index,-3} | {shortTitle,-24} | {t.DueDate,-12:yyyy-MM-dd} | {t.Status,-12} | {shortProj,-14}");
                index++;
            }
            Console.WriteLine(new string('-', 74));
        }

        private static string PromptRequired(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) return input.Trim();
                Console.WriteLine("This field cannot be empty. Please try again.");
            }
        }

        private static DateTime PromptDate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    if (date.Date < DateTime.Today)
                    {
                        Console.WriteLine("Due date cannot be in the past. Please enter today or a future date.");
                        continue;
                    }
                    return date;
                }
                Console.WriteLine("Invalid format. Please enter date as YYYY-MM-DD.");
            }
        }

        private static TaskStatus PromptStatus()
        {
            while (true)
            {
                Console.WriteLine("Select Status: 1. Pending  2. InProgress  3. Completed");
                Console.Write("Choice (1-3): ");
                string? choice = Console.ReadLine()?.Trim();
                switch (choice)
                {
                    case "1": return TaskStatus.Pending;
                    case "2": return TaskStatus.InProgress;
                    case "3": return TaskStatus.Completed;
                    default:
                        Console.WriteLine("Invalid option. Enter 1, 2, or 3.");
                        break;
                }
            }
        }

        private static void PauseMessage(string? message = null)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine($"\n{message}");
            }
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey(true);
        }
    }
}