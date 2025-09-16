import { Modal, Form, Input, DatePicker, Button } from "antd";
import { useMutation, useQuery } from "@apollo/client/react";
import { UPDATE_TASK } from "../../graphql/mutations";
import { GET_TASKS } from "../../graphql/queries";
import toast from "react-hot-toast";
import { useEffect } from "react";
import dayjs from "dayjs";
import type {
  ITask,
  ITasksData,
  IUpdateTaskData,
  IUpdateTaskVars,
} from "../../helpers/types/taskTypes";

const { TextArea } = Input;

interface UpdateTaskModalProps {
  open: boolean;
  onClose: () => void;
  taskToEdit?: ITask;
}

export function UpdateTaskModal({
  open,
  onClose,
  taskToEdit,
}: UpdateTaskModalProps) {
  const [form] = Form.useForm();
  const isEditing = !!taskToEdit;
  const { data: tasksData } = useQuery<ITasksData>(GET_TASKS);

  // // Reset form when modal is closed
  // useEffect(() => {
  //   if (!open) {
  //     form.resetFields();
  //   }
  // }, [open, form]);

  // // Cleanup form when component unmounts
  // useEffect(() => {
  //   return () => {
  //     form.resetFields();
  //   };
  // }, [form]);

  const [updateTask, { loading: updateLoading, error: updateError }] =
    useMutation<IUpdateTaskData, IUpdateTaskVars>(UPDATE_TASK, {
      update(cache, { data }) {
        const updatedTask = data?.updateTask?.task;
        if (!updatedTask) return;
        try {
          const existing = cache.readQuery<ITasksData>({ query: GET_TASKS });
          if (!existing?.tasks) return;

          // Sort tasks to maintain the order (newest first)
          const updatedTasks = existing.tasks
            .map((task) => (task.id === updatedTask.id ? updatedTask : task))
            .sort((a, b) => {
              const da = a.dateCreated ? Date.parse(a.dateCreated) : 0;
              const db = b.dateCreated ? Date.parse(b.dateCreated) : 0;
              return db - da;
            });

          cache.writeQuery<ITasksData>({
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
      onError() {
        // Keep the form mounted in case of error
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
      // Clear form when not editing
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
      }
    } catch (err) {
      // Keep the form mounted in case of error
      return;
    }
  };

  const loading = updateLoading;
  const error = updateError;

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
          rules={[
            { required: true, message: "Please enter a title" },
            {
              validator: (_, value) => {
                if (value && tasksData?.tasks) {
                  const trimmedValue = value.trim();
                  const isDuplicate = tasksData.tasks.some(
                    (task) =>
                      task.id !== taskToEdit?.id && // Exclude current task
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
          name="description"
          label="Description"
          rules={[{ required: true, message: "Please enter a description" }]}
        >
          <TextArea placeholder="Enter task description" rows={3} />
        </Form.Item>

        <Form.Item name="dueDate" label="Due Date">
          <DatePicker
            showTime={{ format: "HH:mm" }}
            style={{ width: "100%" }}
            format="YYYY-MM-DD HH:mm"
          />
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
