# ToDoList Console Application

A lightweight, terminal-based ToDo manager written in C# (.NET). It allows users to track tasks by project and due date, 
manage task lifecycles, prevent duplicates, and persist data across runs via JSON.

## Features
* CRUD Operations: Add, update, mark as completed, and remove tasks.
* Project Organization: Categorize tasks under custom project tags (defaults to General).
* Flexible Views: View tasks sorted chronologically by due date or grouped by project.
* Duplicate Prevention: Rejects tasks that have identical Title and Project combinations (case-insensitive) during creation and updates.
* Date Validation: Enforces the yyyy-MM-dd date format and prevents setting due dates in the past.
* ANSI TrueColor Output: Full-color terminal table rendering mapped to lifecycle states:
    * ■ Pending: Dark Grey (#6E6E6E)
    * ■ InProgress: True Orange (#FF8C00)
    * ■ Completed: Emerald Green (#2ECC71)
* JSON Persistence: Automatically loads and saves data locally using System.Text.Json.

**File Storage Details**

Task records are saved in indented JSON format to:
Plaintext

tasks.json
Location: The file resides in the application's binary execution directory (e.g., bin/Debug/netX.0/tasks.json or alongside published executables).
Data Model Schema
JSON

[
  {
    "Id": "6ba7b810-9dad-11d1-80b4-00c04fd430c8",
    "Title": "Set up CI pipeline",
    "DueDate": "2026-10-15T00:00:00",
    "Status": "InProgress",
    "Project": "DevOps"
  }
]

## Prerequisites
* .NET SDK (Version 6.0, 7.0, 8.0, or newer).
* A terminal with ANSI color code support (Windows Terminal, PowerShell 7+, macOS Terminal, Linux Bash/Zsh).
Getting Started
1. Clone or Copy the Project. 
  git clone https://github/JohnKYoung/todolist.git   
  cd todolist
 
Ensure your project structure includes a .csproj file configured for a console app:
XML 

<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>

2. Build the Application  
   Bash
   dotnet build

3. Run the Application   
   Bash
   dotnet run

**Navigation & Controls**
When the application runs, the main dashboard provides a task completion summary and 4 primary actions:
Plaintext

>> Welcome to ToDoList
>> You have 2 tasks to do and 1 tasks are done!
>> Tasks will be displayed in the following colours: Pending, InProgress, Completed

>> (1) Show Task List by date or project
>> (2) Add New Task
>> (3) Edit Task (update, mark as done, remove)
>> (4) Save and Quit
========================================
>> Select an option (1-4): 
Option	Action	Notes
1	Show Task List	Prompts to view either sorted by Due Date or grouped by Project.
2	Add New Task	Prompts for Title, Due Date (yyyy-MM-dd), Status (Pending, InProgress, Completed), and Project. Enforces date and duplicate checks.
3	Edit Task	Select a task index from the printed table to: (1) Update specific properties (leaving prompts blank keeps existing values), (2) Mark as completed, or (3) Delete task with a confirmation prompt.
4	Save and Quit	Flushes task state to tasks.json and closes the program.


**Task Model**

Each task is represented by the TodoTask class.
 
Property Type Description
 
Id;  Guid Unique identifier generated automatically for the task
Title:  string Title or summary of the task
DueDate:  DateTime Date by which the task should be completed
Status:  TaskStatus Current task status
The available task statuses are:
Pending
InProgress
Completed
The status is stored as a string when the task is serialized to JSON.

Application Structure
The application is organised around three main types:
TaskStatus
An enumeration defining the lifecycle states of a task:
public enum TaskStatus
{
    Pending,
    InProgress,
    Completed
}
**TodoTask**
A data/model class representing an individual task.
TaskManager
Responsible for task persistence and task operations, including:
•	Loading tasks from the JSON file
•	Saving tasks to the JSON file
•	Adding tasks
•	Editing tasks
•	Finding duplicate tasks
•	Marking tasks as completed
•	Removing tasks
•	Returning tasks for display
•	Sorting tasks by due date
•	Grouping tasks by project

**Program**  
Contains the console user interface and application control flow.
It provides the main menu, input validation, task display, editing
workflow, and status colour formatting.
Main Menu
When the application starts, the main menu provides four options:
>> (1) Show Task List by date or project
>> (2) Add New Task
>> (3) Edit Task (update, mark as done, remove)
>> (4) Save and Quit
1. Show Task List
The application provides two viewing options:
1. View by Due Date
2. View by Project
3. Cancel
View by Due Date
Tasks are ordered chronologically according to their DueDate.
View by Project
Tasks are grouped by project name and the project groups are displayed
alphabetically.
2. Add New Task
The application prompts for:
1.	Title
2.	Due date
3.	Status
4.	Project

Example:
Title: Prepare project report
Due Date (yyyy-MM-dd): 2026-10-15
Select Status: 1. Pending  2. InProgress  3. Completed
Choice (1-3): 1
Project (Default: 'General'): MyProject

Validation
The title cannot be empty.
The due date must:
•	Use the yyyy-MM-dd format.
•	Be today or a future date.
If no project is entered, the project is set to:
General

**Duplicate Prevention**
Before a task is created, the application checks whether another task
already has the same Title and Project.
The comparison is case-insensitive and ignores leading/trailing
whitespace.
If a duplicate is found, the existing task is displayed and the user can
either try again or return to the main menu.
3. Edit Task
The edit menu allows the user to:
1. Update task details
2. Mark as completed
3. Remove task
4. Cancel
Update Task Details
The user can change:
•	Title
•	Due date
•	Status
•	Project
Leaving a field blank retains its current value.
The application also performs duplicate checking when the title or
project is changed.
Mark as Completed
The selected task's status is changed to:
Completed
Remove Task
The application asks the user for confirmation before deleting the task.
4. Save and Quit
The application saves all tasks to the JSON storage file before exiting.
The TaskManager also saves automatically after task changes such as:
•	Adding a task
•	Editing a task
•	Marking a task as completed
•	Removing a task

**Data Storage**  
Tasks are stored in a local JSON file named:
tasks.json  
The file path is resolved relative to the application's binary
directory.   
The JSON is formatted with indentation to make it easier to read.
Example structure:  
[
  {
    "Id": "00000000-0000-0000-0000-000000000000",
    "Title": "Prepare project report",
    "DueDate": "2026-10-15T00:00:00",
    "Status": "Pending",
    "Project": "MyProject"
  }
]   
If the storage file does not exist, the application starts with an empty
task list.
If the file cannot be read or deserialized, the application reports the
error and starts with an empty task list.   

*
Display
Tasks are displayed in a tabular format:  
--------------------------------------------------------------------------  *  
*#   | Title                    | Due Date     | Status       | Project   *
--------------------------------------------------------------------------
1    | Prepare project report   | 2026-10-15   | Pending      | MyProject
--------------------------------------------------------------------------  **

Long titles and project names are truncated to fit the table columns.
Status colours are applied using ANSI 24-bit colour escape sequences.

**Technologies Used:**  
•	C#  
•	.NET  
•	LINQ   
•	System.Text.Json  
•	System.Text.Json.Serialization   
•	JSON serialization/deserialization  
•	Guid  
•	DateTime  
•	Enums  
•	Nullable reference types / nullable values  
•	Console input/output  
•	ANSI terminal colour codes  

**Key Classes and Responsibilities**  
Type Responsibility
 
TaskStatus Defines the available task states
TodoTask Represents an individual task
TaskManager Manages task data, persistence, and task operations
Program Provides the console UI and controls application flow

**Data Flow**
The general application flow is:
Application starts
       |
       v
TaskManager loads tasks.json
       |
       v
Main menu
       |
       +------------------+
       |                  |
       v                  v
Show tasks          Add/Edit/Remove
       |                  |
       |                  v
       |             TaskManager
       |                  |
       +---------> Save tasks
                          |
                          v
                      tasks.json
**Error Handling**
The application handles several common input and storage errors.
Examples include:
•	Empty task titles
•	Invalid menu selections
•	Invalid dates
•	Due dates in the past
•	Invalid status selections
•	Invalid task selections
•	JSON file read errors
•	JSON serialization/file write errors
•	Duplicate title/project combinations

**Notes**
•	Tasks are stored locally; the application does not use a database or
remote service.
•	The task collection is managed in memory while the application is
running.
•	Changes are persisted to tasks.json.
•	Task selection in the edit workflow uses the displayed task number,
which is converted to a zero-based collection index internally.
•	The application uses IReadOnlyList<TodoTask> when exposing the
current task collection to the UI.
Example Usage
A typical workflow might be:
1. Start the application
2. Add a task
3. Enter its due date and project
4. View tasks by due date or project
5. Edit the task when required
6. Mark the task as completed
7. Save and quit
On the next application start, the previously saved tasks are loaded
from tasks.json.

**Source**
This README describes the C# console application supplied with the
project. The application code defines TaskStatus, TodoTask,
TaskManager, and Program, with JSON persistence and a console-based
task-management interface.
, with JSON persistence and a console-based
task-management interface.

**License**


