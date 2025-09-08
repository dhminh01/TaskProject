import { Routes, Route, Navigate } from "react-router-dom";
import { DashboardPage } from "../pages/dashboard/DashboardPage";
import { TaskPage } from "../pages/task/TaskPage";
import { CreateTaskPage } from "../pages/task/CreateTaskPage";

export const AppRoutes = () => {
  return (
    <Routes>
      <Route path="/" element={<DashboardPage />} />
      <Route path="/tasks" element={<TaskPage />} />
      <Route path="/create-task" element={<CreateTaskPage />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};
