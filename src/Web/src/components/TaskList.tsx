import { Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { Task } from "../types";
import { useGetTasks } from "../hooks/useGetTasks";

export function TaskList() {
  const { loading, error, data } = useGetTasks();
  if (error) return <p>Error: {error.message}</p>;
  if (!data) return null;

  const tasks = (data.tasks ?? [])
    .filter((t): t is Task => !!t && typeof t.id === "string")
    .sort((a, b) => {
      const da = a.dateCreated ? Date.parse(a.dateCreated) : 0;
      const db = b.dateCreated ? Date.parse(b.dateCreated) : 0;
      return db - da; // newest first
    });

  const columns: ColumnsType<Task> = [
    {
      title: "No.",
      key: "index",
      width: 80,
      render: (_text, _record, index) => index + 1,
    },
    {
      title: "Title",
      dataIndex: "title",
      key: "title",
    },
    {
      title: "Description",
      dataIndex: "description",
      key: "description",
      render: (text) => text || <em style={{ color: "#999" }}>—</em>,
    },
    {
      title: "Due Date",
      dataIndex: "dueDate",
      key: "dueDate",
      render: (date) => (date ? new Date(date).toLocaleDateString() : "—"),
    },
  ];

  return (
    <Table
      dataSource={tasks}
      columns={columns}
      rowKey="id"
      loading={loading}
      locale={{ emptyText: "No tasks found" }}
      pagination={false}
    />
  );
}
