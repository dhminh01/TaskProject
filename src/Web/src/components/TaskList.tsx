import { Table, Button, Space } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { Task } from "../types";
import { useGetTasks } from "../hooks/useGetTasks";
import { TaskFormModal } from "./TaskFormModal";
import { useState } from "react";

export function TaskList() {
  const { loading, error, data } = useGetTasks();
  const [editingTask, setEditingTask] = useState<Task | undefined>();
  const [isModalOpen, setIsModalOpen] = useState(false);

  if (error) return <p>Error: {error.message}</p>;
  if (!data) return null;

  const tasks = (data.tasks ?? [])
    .filter((t): t is Task => !!t && typeof t.id === "string")
    .sort((a, b) => {
      const da = a.dateCreated ? Date.parse(a.dateCreated) : 0;
      const db = b.dateCreated ? Date.parse(b.dateCreated) : 0;
      return db - da; // newest first
    });

  const handleEdit = (task: Task) => {
    setEditingTask(task);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setEditingTask(undefined);
    setIsModalOpen(false);
  };

  const columns: ColumnsType<Task> = [
    {
      title: "No.",
      key: "index",
      width: 80,
      render: (_text, _record, index) => (
        <div style={{ textAlign: "justify" }}>{index + 1}</div>
      ),
    },
    {
      title: "Title",
      dataIndex: "title",
      key: "title",
      render: (text) => <div style={{ textAlign: "justify" }}>{text}</div>,
    },
    {
      title: "Description",
      dataIndex: "description",
      key: "description",
      render: (text) => (
        <div style={{ textAlign: "justify" }}>
          {text || <em style={{ color: "#999" }}>—</em>}
        </div>
      ),
    },
    {
      title: "Due Date",
      dataIndex: "dueDate",
      key: "dueDate",
      render: (date) => (
        <div style={{ textAlign: "justify" }}>
          {date ? new Date(date).toLocaleDateString() : "—"}
        </div>
      ),
    },
    {
      title: "Actions",
      key: "actions",
      width: 100,
      render: (_, record) => (
        <Space>
          <Button type="link" onClick={() => handleEdit(record)}>
            Edit
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <>
      <Table
        dataSource={tasks}
        columns={columns}
        rowKey="id"
        loading={loading}
        locale={{ emptyText: "No tasks found" }}
        pagination={false}
      />
      <TaskFormModal
        open={isModalOpen}
        onClose={handleCloseModal}
        taskToEdit={editingTask}
      />
    </>
  );
}
