export interface Task {
  id: string;
  title: string;
  description: string;
  dueDate?: string | null;
  dateCreated?: string;
}

export interface GetTasksData {
  tasks: Task[];
}

export interface CreateTaskMutationData {
  createTask: {
    task: Task;
  };
}

export interface CreateTaskMutationVars {
  input: {
    title: string;
    description: string;
    dueDate?: string | null;
  };
}
