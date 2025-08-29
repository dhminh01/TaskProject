import { Space } from "antd";
import { CreateTaskForm } from "../components/CreateTaskForm";
import { TaskList } from "../components/TaskList";

export function TaskPage() {
  return (
    <Space direction="vertical" size="large" style={{ width: "100%" }}>
      <CreateTaskForm />
      <TaskList />
    </Space>
  );
}
