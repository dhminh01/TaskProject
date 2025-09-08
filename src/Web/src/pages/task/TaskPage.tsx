import { Space } from "antd";
import { TaskList } from "../../components/layout/TaskList";

export function TaskPage() {
  return (
    <Space direction="vertical" size="large" style={{ width: "100%" }}>
      <TaskList />
    </Space>
  );
}
