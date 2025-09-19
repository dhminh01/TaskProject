import { Space } from "antd";
import { TaskList } from "../../components/layout/TaskList";

const TaskPage = () => {
  return (
    <Space direction="vertical" size="large" style={{ width: "100%" }}>
      <TaskList />
    </Space>
  );
};

export default TaskPage;
