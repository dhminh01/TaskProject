import { Space } from "antd";
import { CreateTaskForm } from "../../components/form/CreateTaskForm";

const CreateTaskPage = () => {
  return (
    <Space direction="vertical" size="large" style={{ width: "100%" }}>
      <CreateTaskForm />
    </Space>
  );
};

export default CreateTaskPage;
