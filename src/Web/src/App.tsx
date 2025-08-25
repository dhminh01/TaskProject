import { useQuery } from "@apollo/client/react";
import { GET_TASKS } from "./graphql/queries";
import { useState } from "react";
import { useMutation } from "@apollo/client/react";
import { CREATE_NEW_TASK } from "./graphql/mutations";
import toast, { Toaster } from "react-hot-toast";

// Task types
interface Task {
  id: string;
  title: string;
  description: string;
  dueDate?: string | null;
  dateCreated?: string;
}

interface GetTasksData {
  tasks: Task[];
}

interface CreateTaskMutationData {
  createTask: {
    task: Task;
  };
}

interface CreateTaskMutationVars {
  input: {
    title: string;
    description: string;
    dueDate?: string | null;
  };
}

function useGetTasks() {
  return useQuery<GetTasksData>(GET_TASKS);
}

function TaskList() {
  const { loading, error, data } = useGetTasks();
  if (loading) return <p>Loading tasks...</p>;
  if (error) return <p>Error: {error.message}</p>;
  if (!data) return null;

  const tasks = (data.tasks ?? [])
    .filter((t): t is Task => !!t && typeof t.id === "string")
    .sort((a, b) => {
      const da = a.dateCreated ? Date.parse(a.dateCreated) : 0;
      const db = b.dateCreated ? Date.parse(b.dateCreated) : 0;
      return db - da; // newest first
    });

  if (!tasks.length) return <p>No tasks found.</p>;

  return (
    <div style={{ overflowX: "auto" }}>
      <table
        style={{
          width: "100%",
          borderCollapse: "collapse",
          background: "#fff",
          fontSize: 14,
        }}
      >
        <thead>
          <tr style={{ background: "#f5f5f5" }}>
            <th style={thStyle}>No.</th>
            <th style={thStyle}>Title</th>
            <th style={thStyle}>Description</th>
            <th style={thStyle}>Due Date</th>
          </tr>
        </thead>
        <tbody>
          {tasks.map(({ id, title, description, dueDate }, index) => (
            <tr key={id} style={{ borderBottom: "1px solid #eee" }}>
              <td style={tdStyle}>{index + 1}</td>
              <td style={tdStyle}>{title}</td>
              <td style={{ ...tdStyle, whiteSpace: "pre-line" }}>
                {description || <em style={{ color: "#999" }}>—</em>}
              </td>
              <td style={tdStyle}>
                {dueDate ? new Date(dueDate).toLocaleDateString() : "—"}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function CreateTaskForm() {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [dueDate, setDueDate] = useState<string>("");

  const [createTask, { loading, error }] = useMutation<
    CreateTaskMutationData,
    CreateTaskMutationVars
  >(CREATE_NEW_TASK, {
    update(cache, { data }) {
      const newTask = data?.createTask?.task;
      if (!newTask) return;
      try {
        const existing = cache.readQuery<GetTasksData>({ query: GET_TASKS });
        cache.writeQuery<GetTasksData>({
          query: GET_TASKS,
          data: { tasks: [newTask, ...(existing?.tasks ?? [])] },
        });
      } catch {}
    },
    onCompleted() {
      toast.success("Task created");
    },
    onError(err) {
      toast.error(err.message);
    },
  });

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) {
      toast.error("Title required");
      return;
    }
    await createTask({
      variables: {
        input: {
          title: title.trim(),
          description: description.trim(),
          dueDate: dueDate ? new Date(dueDate).toISOString() : null,
        },
      },
    });
    setTitle("");
    setDescription("");
    setDueDate("");
  };

  return (
    <form
      onSubmit={onSubmit}
      style={{
        display: "flex",
        flexDirection: "column",
        gap: 8,
        maxWidth: 440,
        marginBottom: 24,
      }}
    >
      <input
        required
        placeholder="Title"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        style={inputStyle}
      />
      <textarea
        placeholder="Description"
        value={description}
        onChange={(e) => setDescription(e.target.value)}
        rows={3}
        style={{ ...inputStyle, resize: "vertical" }}
      />
      <input
        type="date"
        value={dueDate}
        onChange={(e) => setDueDate(e.target.value)}
        style={inputStyle}
      />
      <button
        type="submit"
        disabled={loading}
        style={{
          padding: "8px 16px",
          background: "#1677ff",
          color: "#fff",
          border: "none",
          borderRadius: 4,
          cursor: "pointer",
          opacity: loading ? 0.6 : 1,
        }}
      >
        {loading ? "Saving..." : "Add Task"}
      </button>
      {error && (
        <span style={{ color: "#ff4d4f", fontSize: 12 }}>{error.message}</span>
      )}
    </form>
  );
}

export default function App() {
  return (
    <div>
      <Toaster position="top-right" />
      <h2>My first Apollo app 🚀</h2>
      <br />
      <CreateTaskForm />
      <TaskList />
    </div>
  );
}

// Reusable cell styles (placed at bottom to avoid re-creation per render)
const thStyle: React.CSSProperties = {
  textAlign: "left",
  padding: "8px 12px",
  borderBottom: "1px solid #ddd",
  fontWeight: 600,
};

const tdStyle: React.CSSProperties = {
  padding: "8px 12px",
  verticalAlign: "top",
};

const inputStyle: React.CSSProperties = {
  padding: "8px 10px",
  border: "1px solid #ccc",
  borderRadius: 4,
  font: "inherit",
  width: "100%",
  boxSizing: "border-box",
};
