import { Form, Input, DatePicker, Button, Card, Space, Typography } from "antd";
import { useMutation } from "@apollo/client/react";
import { CREATE_TASK } from "../../graphql/mutations";
import { GET_TASKS } from "../../graphql/queries";
import toast from "react-hot-toast";
import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import type {
  ICreateTaskData,
  ICreateTaskVars,
  ITasksData,
} from "../../helpers/types/taskTypes";

const { TextArea } = Input;
const { Title } = Typography;

export function CreateTaskForm() {
  const [form] = Form.useForm();
  const navigate = useNavigate();

  const textSize = "16px";

  // Cleanup form when component unmounts
  useEffect(() => {
    return () => {
      form.resetFields();
    };
  }, [form]);

  const [createTask, { loading: createLoading, error: createError }] =
    useMutation<ICreateTaskData, ICreateTaskVars>(CREATE_TASK, {
      update(cache, { data }) {
        const newTask = data?.createTask?.task;
        if (!newTask) return;
        try {
          const existing = cache.readQuery<ITasksData>({
            query: GET_TASKS,
          });
          cache.writeQuery<ITasksData>({
            query: GET_TASKS,
            data: { tasks: [newTask, ...(existing?.tasks ?? [])] },
          });
        } catch {}
      },
      onCompleted() {
        toast.success("Task created successfully");
        setTimeout(() => {
          toast.success("An email notification has been sent");
        }, 500);
        form.resetFields();
        navigate("/tasks"); // Navigate back to the task list
      },
    });

  const onFinish = async (values: {
    title: string;
    description: string;
    dueDate?: Date;
  }) => {
    try {
      const input = {
        title: values.title.trim(),
        description: values.description?.trim() ?? "",
        dueDate: values.dueDate ? values.dueDate.toISOString() : null,
      };

      await createTask({
        variables: {
          input,
        },
      });
    } catch (err) {
      // Keep the form mounted in case of error
      return;
    }
  };

  return (
    <Space
      direction="vertical"
      size="large"
      style={{ width: "100%", padding: "24px" }}
    >
      <Card>
        <Title level={2} style={{ color: "#4f4f4f" }}>
          Create New Task
        </Title>
        <Form
          form={form}
          onFinish={onFinish}
          layout="vertical"
          style={{ margin: "0 auto", maxWidth: 800 }}
        >
          <Form.Item
            name="title"
            label={<span style={{ fontSize: textSize }}>Title</span>}
            rules={[{ required: true, message: "Please enter a title" }]}
          >
            <Input placeholder="Enter task title" />
          </Form.Item>

          <Form.Item
            name="description"
            label={<span style={{ fontSize: textSize }}>Description</span>}
          >
            <TextArea placeholder="Enter task description" rows={3} />
          </Form.Item>

          <Form.Item
            name="dueDate"
            label={<span style={{ fontSize: textSize }}>Due Date</span>}
          >
            <DatePicker style={{ width: "100%" }} />
          </Form.Item>

          <Form.Item>
            <Space>
              <Button type="primary" htmlType="submit" loading={createLoading}>
                Create Task
              </Button>
            </Space>
          </Form.Item>

          {createError && (
            <span style={{ color: "#ff4d4f", fontSize: 14 }}>
              {createError.message}
            </span>
          )}
        </Form>
      </Card>
    </Space>
  );
}
