import { Table, Button, Space, Spin } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useState } from "react";
import { useQuery } from "@apollo/client/react";
import { useNavigate } from "react-router-dom";
import { GET_TASKS } from "../../graphql/queries";
import { DeleteOutlined, EditOutlined } from "@ant-design/icons";
import useDebounce from "../../helpers/hooks/useDebounce";
import { UpdateTaskModal } from "../modal/UpdateTaskModal";
import { DeleteTaskModal } from "../modal/DeleteTaskModal";
import type { ITask, ITasksData } from "../../helpers/types/taskTypes";

export function TaskList() {
  const navigate = useNavigate();
  const {
    loading: queryLoading,
    error,
    data,
    refetch,
  } = useQuery<ITasksData>(GET_TASKS);
  const [editingTask, setEditingTask] = useState<ITask | undefined>();
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [taskToDelete, setTaskToDelete] = useState<ITask | undefined>();
  const loading = useDebounce(queryLoading, 500);

  const handleCreateTask = () => {
    navigate("/create-task");
  };

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
    setIsEditModalOpen(true);
  };

  const handleCloseEditModal = () => {
    setEditingTask(undefined);
    setIsEditModalOpen(false);
  };

  const handleDelete = (task: ITask) => {
    setTaskToDelete(task);
    setIsDeleteModalOpen(true);
  };

  const handleCloseDeleteModal = () => {
    setTaskToDelete(undefined);
    setIsDeleteModalOpen(false);
  };

  const columns: ColumnsType<ITask> = [
    {
      title: "Title",
      dataIndex: "title",
      key: "title",
      render: (text) => (
        <div
          style={{
            maxWidth: "300px",
            textAlign: "left",
            fontSize: textSize,
            whiteSpace: "normal",
            wordBreak: "break-word",
          }}
        >
          {text}
        </div>
      ),
      width: 200,
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
    <div style={{ position: "relative", minHeight: "200px" }}>
      <div style={{ marginBottom: 16, textAlign: "left" }}>
        <Button type="primary" onClick={handleCreateTask}>
          Create New Task
        </Button>
      </div>
      {loading && (
        <div
          style={{
            position: "absolute",
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            background: "rgba(255, 255, 255, 0.7)",
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            zIndex: 1000,
          }}
        >
          <Spin size="large" />
        </div>
      )}
      <Table
        dataSource={tasks}
        columns={columns}
        rowKey="id"
        locale={{ emptyText: "No tasks found" }}
        pagination={false}
      />
      <UpdateTaskModal
        open={isEditModalOpen}
        onClose={handleCloseEditModal}
        taskToEdit={editingTask}
      />
      <DeleteTaskModal
        open={isDeleteModalOpen}
        onClose={handleCloseDeleteModal}
        taskToDelete={taskToDelete}
        onDeleteSuccess={refetch}
      />
    </div>
  );
}
