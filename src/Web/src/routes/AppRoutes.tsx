import { Routes, Route, Navigate } from "react-router-dom";
import { DashboardPage } from "../pages/DashboardPage";
import { TaskPage } from "../pages/TaskPage";
import { CreateTaskPage } from "../pages/CreateTaskPage";

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
