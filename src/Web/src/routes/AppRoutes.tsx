import { Routes, Route, Navigate } from "react-router-dom";
import TaskPage from "../pages/task/TaskPage";
import CreateTaskPage from "../pages/task/CreateTaskPage";
import DashboardPage from "../pages/dashboard/DashboardPage";

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
