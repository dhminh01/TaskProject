export interface ITask {
  id: string;
  title: string;
  description: string;
  dueDate?: string | null;
  dateCreated?: string;
}

export interface ITasksData {
  tasks: ITask[];
}

export interface ICreateTaskData {
  createTask: {
    task: ITask;
  };
}

export interface ICreateTaskVars {
  input: {
    title: string;
    description: string;
    dueDate?: string | null;
  };
}

export interface IUpdateTaskData {
  updateTask: {
    task: ITask;
  };
}

export interface IUpdateTaskVars {
  input: {
    id: string;
    title: string;
    description: string;
    dueDate?: string | null;
  };
}
