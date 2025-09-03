import { Modal, Form, Input, DatePicker, Button } from "antd";
import { useMutation } from "@apollo/client/react";
import { CREATE_NEW_TASK, UPDATE_TASK } from "../graphql/mutations";
import { GET_TASKS } from "../graphql/queries";
import toast from "react-hot-toast";
import { useEffect } from "react";
import dayjs from "dayjs";
import type {
  CreateTaskMutationData,
  CreateTaskMutationVars,
  UpdateTaskMutationData,
  UpdateTaskMutationVars,
  GetTasksData,
  Task,
} from "../types";

const { TextArea } = Input;

interface TaskFormModalProps {
  open: boolean;
  onClose: () => void;
  taskToEdit?: Task;
}

export function TaskFormModal({
  open,
  onClose,
  taskToEdit,
}: TaskFormModalProps) {
  const [form] = Form.useForm();
  const isEditing = !!taskToEdit;

  // Reset form when modal is closed
  useEffect(() => {
    if (!open) {
      form.resetFields();
    }
  }, [open, form]);

  // Cleanup form when component unmounts
  useEffect(() => {
    return () => {
      form.resetFields();
    };
  }, [form]);

  const [createTask, { loading: createLoading, error: createError }] =
    useMutation<CreateTaskMutationData, CreateTaskMutationVars>(
      CREATE_NEW_TASK,
      {
        update(cache, { data }) {
          const newTask = data?.createTask?.task;
          if (!newTask) return;
          try {
            const existing = cache.readQuery<GetTasksData>({
              query: GET_TASKS,
            });
            cache.writeQuery<GetTasksData>({
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
          onClose();
        },
        onError(err) {
          toast.error(err.message);
        },
      }
    );

  const [updateTask, { loading: updateLoading, error: updateError }] =
    useMutation<UpdateTaskMutationData, UpdateTaskMutationVars>(UPDATE_TASK, {
      update(cache, { data }) {
        const updatedTask = data?.updateTask?.task;
        if (!updatedTask) return;
        try {
          const existing = cache.readQuery<GetTasksData>({ query: GET_TASKS });
          if (!existing?.tasks) return;

          // Sort tasks to maintain the order (newest first)
          const updatedTasks = existing.tasks
            .map((task) => (task.id === updatedTask.id ? updatedTask : task))
            .sort((a, b) => {
              const da = a.dateCreated ? Date.parse(a.dateCreated) : 0;
              const db = b.dateCreated ? Date.parse(b.dateCreated) : 0;
              return db - da;
            });

          cache.writeQuery<GetTasksData>({
            query: GET_TASKS,
            data: {
              tasks: updatedTasks,
            },
          });
        } catch (error) {
          console.error("Error updating cache:", error);
        }
      },
      refetchQueries: [{ query: GET_TASKS }], // Force refresh if cache update fails
      onCompleted() {
        toast.success("Task updated successfully");
        form.resetFields();
        onClose();
      },
      onError(err) {
        toast.error(err.message);
      },
    });

  // Set initial form values when editing
  useEffect(() => {
    if (taskToEdit && open) {
      form.setFieldsValue({
        title: taskToEdit.title,
        description: taskToEdit.description,
        dueDate: taskToEdit.dueDate ? dayjs(taskToEdit.dueDate) : undefined,
      });
    } else {
      form.resetFields();
    }
  }, [taskToEdit, open, form]);

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

      if (isEditing && taskToEdit) {
        await updateTask({
          variables: {
            input: {
              ...input,
              id: taskToEdit.id,
            },
          },
        });
      } else {
        await createTask({
          variables: {
            input,
          },
        });
      }
    } catch (err) {
      // Keep the form mounted in case of error
      return;
    }
  };

  const loading = createLoading || updateLoading;
  const error = createError || updateError;

  return (
    <Modal
      title={isEditing ? "Edit Task" : "Create New Task"}
      open={open}
      onCancel={onClose}
      footer={null}
      destroyOnHidden
    >
      <Form form={form} onFinish={onFinish} layout="vertical">
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
          <Button type="primary" htmlType="submit" loading={loading} block>
            {isEditing ? "Update Task" : "Add Task"}
          </Button>
        </Form.Item>

        {error && (
          <span style={{ color: "#ff4d4f", fontSize: 12 }}>
            {error.message}
          </span>
        )}
      </Form>
    </Modal>
  );
}
