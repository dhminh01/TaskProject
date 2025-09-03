import { Button } from "antd";
import { useState } from "react";
import { TaskFormModal } from "./TaskFormModal";

export function CreateTaskForm() {
  const [isModalOpen, setIsModalOpen] = useState(false);

  const showModal = () => setIsModalOpen(true);
  const hideModal = () => setIsModalOpen(false);

  return (
    <>
      <Button type="primary" onClick={showModal}>
        Create New Task
      </Button>
      <TaskFormModal open={isModalOpen} onClose={hideModal} />
    </>
  );
}
