ToDo List

C# Console Application to Manage a Todo List

The program manages a list of tasks, each with an Id, Title, Due Date, Status and Project.
It saves the list to a file: <tasks.json>

The user is presented with a welcome message
and a brief summary of open and complete Tasks.

The following menu is then displayed
(1) Show task list(by date of project)
(2) Add New Task
(3) Edit Task (update, mark as done, remove)
(4) Save and Quit

Note that whenever the tasks are displayed they will be coloured according to Status:
Pending - Grey
InProgress - Orange
Completed - Green


Option 1:
If the Task List is found
The user is presented with a menu 
(1) Display tasks by due date
(2) Display tasks by Project
(3) Cancel

Option 2:
The user is prompted for entry data to complete the Task creation
Title:
Due Date
Status
Project

Option (3)
The user is presented with the Task List in Id order and prompted
for input of a valid ID number
Once a valid number is entered, the user is offered the opportunity to update the
following, in turn (Enter for no change)
Title
Due Date
Mark Task as Completed
Delete Task

Option (4)

Saves the Task List to <tasks.json>
Prints an information message and exits the program
