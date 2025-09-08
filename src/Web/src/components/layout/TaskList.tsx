import { Table, Button, Space } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useState } from "react";
import { useQuery, useMutation } from "@apollo/client/react";
import { GET_TASKS } from "../../graphql/queries";
import { DELETE_TASK } from "../../graphql/mutations";
import { Modal } from "antd";
import { DeleteOutlined, EditOutlined } from "@ant-design/icons";
import { UpdateTaskModal } from "../modal/UpdateTaskModal";
import toast from "react-hot-toast";
import type { ITask, ITasksData } from "../../helpers/types/taskTypes";

export function TaskList() {
  const { loading, error, data, refetch } = useQuery<ITasksData>(GET_TASKS);
  const [editingTask, setEditingTask] = useState<ITask | undefined>();
  const [isModalOpen, setIsModalOpen] = useState(false);

  const [deleteTask] = useMutation(DELETE_TASK, {
    onCompleted: () => {
      refetch(); // Refresh the task list after successful deletion
    },
  });

  const textSize = 15;

  if (error) return <p>Error: {error.message}</p>;
  if (!data) return null;

  const tasks = (data.tasks ?? [])
    .filter((t): t is ITask => !!t && typeof t.id === "string")
    .sort((a, b) => {
      const da = a.dateCreated ? Date.parse(a.dateCreated) : 0;
      const db = b.dateCreated ? Date.parse(b.dateCreated) : 0;
      return db - da; // newest first
    });

  const handleEdit = (task: ITask) => {
    setEditingTask(task);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setEditingTask(undefined);
    setIsModalOpen(false);
  };

  const handleDelete = (task: ITask) => {
    Modal.confirm({
      title: "Delete Task",
      content: `Are you sure you want to delete "${task.title}"?`,
      okText: "Yes",
      okType: "danger",
      cancelText: "No",
      onOk: async () => {
        try {
          await deleteTask({
            variables: {
              input: {
                id: task.id,
              },
            },
          });
          toast.success("Task deleted successfully!");
        } catch (error) {
          Modal.error({
            title: "Error",
            content: "Failed to delete task. Please try again.",
          });
        }
      },
    });
  };

  const columns: ColumnsType<ITask> = [
    {
      title: "Title",
      dataIndex: "title",
      key: "title",
      render: (text) => (
        <div
          style={{ width: "3rem", textAlign: "justify", fontSize: textSize }}
        >
          {text}
        </div>
      ),
    },
    {
      title: "Description",
      dataIndex: "description",
      key: "description",
      render: (text) => (
        <div
          style={{
            textAlign: "justify",
            paddingRight: "5px",
            fontSize: textSize,
          }}
        >
          {text || <em style={{ color: "#999" }}>—</em>}
        </div>
      ),
    },
    {
      title: "Due Date",
      dataIndex: "dueDate",
      key: "dueDate",
      render: (date) => (
        <div
          style={{
            textAlign: "justify",
            paddingRight: "5px",
            fontSize: textSize,
          }}
        >
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
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record)}
            title="Edit"
          />
          <Button
            type="link"
            danger
            icon={<DeleteOutlined />}
            onClick={() => handleDelete(record)}
            title="Delete"
          />
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
      <UpdateTaskModal
        open={isModalOpen}
        onClose={handleCloseModal}
        taskToEdit={editingTask}
      />
    </>
  );
}
