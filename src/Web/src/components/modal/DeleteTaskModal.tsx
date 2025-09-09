import { Modal } from "antd";
import { useMutation } from "@apollo/client/react";
import { DELETE_TASK } from "../../graphql/mutations";
import toast from "react-hot-toast";
import type { ITask } from "../../helpers/types/taskTypes";

interface DeleteTaskModalProps {
  open: boolean;
  onClose: () => void;
  taskToDelete?: ITask;
  onDeleteSuccess: () => void;
}

export function DeleteTaskModal({
  open,
  onClose,
  taskToDelete,
  onDeleteSuccess,
}: DeleteTaskModalProps) {
  const [deleteTask] = useMutation(DELETE_TASK, {
    onCompleted: () => {
      toast.success("Task deleted successfully!");
      onDeleteSuccess();
      onClose();
    },
  });

  const handleDelete = async () => {
    if (!taskToDelete) return;

    try {
      await deleteTask({
        variables: {
          input: {
            id: taskToDelete.id,
          },
        },
      });
    } catch (error) {
      Modal.error({
        title: "Error",
        content: "Failed to delete task. Please try again.",
      });
    }
  };

  return (
    <Modal
      title="Delete Task"
      open={open}
      onCancel={onClose}
      okText="Yes"
      cancelText="No"
      okType="danger"
      onOk={handleDelete}
    >
      {taskToDelete && (
        <p>Are you sure you want to delete "{taskToDelete.title}"?</p>
      )}
    </Modal>
  );
}
