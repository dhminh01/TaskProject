import {
  Form,
  Input,
  DatePicker,
  Button,
  Card,
  Space,
  Typography,
  type FormInstance,
} from "antd";
import { LoadingOutlined } from "@ant-design/icons";
import { useMutation, useQuery } from "@apollo/client/react";
import { CREATE_TASK } from "../../graphql/mutations";
import { GET_TASKS } from "../../graphql/queries";
import toast from "react-hot-toast";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import type {
  ICreateTaskData,
  ICreateTaskVars,
  ITasksData,
} from "../../helpers/types/taskTypes";

const { TextArea } = Input;
const { Title } = Typography;

interface SubmitButtonProps {
  form: FormInstance;
  loading?: boolean;
}

const SubmitButton: React.FC<React.PropsWithChildren<SubmitButtonProps>> = ({
  form,
  children,
}) => {
  const [submittable, setSubmittable] = useState<boolean>(false);

  // Watch all values
  const values = Form.useWatch([], form);

  useEffect(() => {
    form
      .validateFields({ validateOnly: true })
      .then(() => setSubmittable(true))
      .catch(() => setSubmittable(false));
  }, [form, values]);

  return (
    <Button type="primary" htmlType="submit" disabled={!submittable}>
      {children}
    </Button>
  );
};

export function CreateTaskForm() {
  const [form] = Form.useForm();
  const navigate = useNavigate();
  const { data: tasksData } = useQuery<ITasksData>(GET_TASKS);

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
        if (!cache || !data?.createTask?.task) return;
        const newTask = data.createTask.task;
        try {
          const existing = cache.readQuery<ITasksData>({
            query: GET_TASKS,
          });
          const existingTasks =
            existing?.tasks?.filter((task) => task != null) ?? [];
          cache.writeQuery<ITasksData>({
            query: GET_TASKS,
            data: { tasks: [newTask, ...existingTasks] },
          });
        } catch (error) {
          console.error("Error updating cache:", error);
        }
      },
      onCompleted() {
        toast.success("Task created successfully");
        setTimeout(() => {
          toast.success("An email notification has been sent");
          form.resetFields();
          navigate("/tasks"); // Navigate back to the task list
        }, 500);
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
    } catch (err: any) {
      // Keep the form mounted in case of error
      console.log(err);
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
            hasFeedback
            name="title"
            label={<span style={{ fontSize: textSize }}>Title</span>}
            validateDebounce={1000}
            rules={[
              { required: true, message: "Please enter a title" },
              { max: 200, message: "Title cannot exceed 200 characters" },
              { whitespace: true, message: "Title cannot be empty spaces" },
              {
                validator: (_, value) => {
                  if (value && (value.startsWith(" ") || value.endsWith(" "))) {
                    return Promise.reject(
                      "Title cannot start or end with spaces"
                    );
                  }
                  // Check for duplicate titles
                  if (value && tasksData?.tasks) {
                    const trimmedValue = value.trim();
                    const isDuplicate = tasksData.tasks.some(
                      (task) =>
                        task.title.trim().toLowerCase() ===
                        trimmedValue.toLowerCase()
                    );
                    if (isDuplicate) {
                      return Promise.reject(
                        "A task with this title already exists"
                      );
                    }
                  }
                  return Promise.resolve();
                },
              },
            ]}
          >
            <Input placeholder="Enter task title" />
          </Form.Item>

          <Form.Item
            hasFeedback
            name="description"
            label={<span style={{ fontSize: textSize }}>Description</span>}
            validateDebounce={1000}
            rules={[
              { required: true, message: "Please enter a description" },
              {
                max: 1000,
                message: "Description cannot exceed 1000 characters",
              },
              {
                whitespace: true,
                message: "Description cannot be empty spaces",
              },
            ]}
          >
            <TextArea placeholder="Enter task description" rows={3} />
          </Form.Item>

          <Form.Item
            name="dueDate"
            label={<span style={{ fontSize: textSize }}>Due Date</span>}
            rules={[
              {
                validator: (_, value) => {
                  if (value && value.isBefore(new Date(), "minute")) {
                    return Promise.reject("Due date cannot be in the past");
                  }
                  return Promise.resolve();
                },
              },
            ]}
          >
            <DatePicker
              showTime={{ format: "HH:mm" }}
              style={{ width: "100%" }}
              format="YYYY-MM-DD HH:mm"
            />
          </Form.Item>

          <Form.Item>
            <SubmitButton form={form} loading={createLoading}>
              {createLoading ? (
                <LoadingOutlined style={{ fontSize: 12 }} />
              ) : null}
              Create Task
            </SubmitButton>
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
