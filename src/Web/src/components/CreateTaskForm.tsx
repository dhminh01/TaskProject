import { Card, Form, Input, DatePicker, Button } from "antd";
import { useMutation } from "@apollo/client/react";
import { CREATE_NEW_TASK } from "../graphql/mutations";
import { GET_TASKS } from "../graphql/queries";
import toast from "react-hot-toast";
import type {
  CreateTaskMutationData,
  CreateTaskMutationVars,
  GetTasksData,
} from "../types";

const { TextArea } = Input;

export function CreateTaskForm() {
  const [form] = Form.useForm();

  const [createTask, { loading, error }] = useMutation<
    CreateTaskMutationData,
    CreateTaskMutationVars
  >(CREATE_NEW_TASK, {
    update(cache, { data }) {
      const newTask = data?.createTask?.task;
      if (!newTask) return;
      try {
        const existing = cache.readQuery<GetTasksData>({ query: GET_TASKS });
        cache.writeQuery<GetTasksData>({
          query: GET_TASKS,
          data: { tasks: [newTask, ...(existing?.tasks ?? [])] },
        });
      } catch {}
    },
    onCompleted() {
      toast.success("Task created successfully");
      // Show email notification toast after a small delay
      setTimeout(() => {
        toast.success("An email notification has been sent");
      }, 500);
      form.resetFields();
    },
    onError(err) {
      toast.error(err.message);
    },
  });

  const onFinish = async (values: {
    title: string;
    description: string;
    dueDate?: Date;
  }) => {
    await createTask({
      variables: {
        input: {
          title: values.title.trim(),
          description: values.description?.trim() ?? "",
          dueDate: values.dueDate ? values.dueDate.toISOString() : null,
        },
      },
    });
  };

  return (
    <Card style={{ marginBottom: 24 }}>
      <Form
        form={form}
        onFinish={onFinish}
        layout="vertical"
        style={{ maxWidth: 440 }}
      >
        <Form.Item
          name="title"
          label="Title"
          rules={[{ required: true, message: "Please enter a title" }]}
        >
          <Input placeholder="Enter task title" />
        </Form.Item>

        <Form.Item name="description" label="Description">
          <TextArea placeholder="Enter task description" rows={3} />
        </Form.Item>

        <Form.Item name="dueDate" label="Due Date">
          <DatePicker style={{ width: "100%" }} />
        </Form.Item>

        <Form.Item>
          <Button type="primary" htmlType="submit" loading={loading}>
            Add Task
          </Button>
        </Form.Item>

        {error && (
          <span style={{ color: "#ff4d4f", fontSize: 12 }}>
            {error.message}
          </span>
        )}
      </Form>
    </Card>
  );
}
