import { Typography } from "antd";
const { Title, Paragraph } = Typography;

export function DashboardPage() {
  return (
    <div>
      <Title level={3}>Welcome to Task Manager</Title>
      <Paragraph>
        This is your task management dashboard. Use the navigation menu above
        to:
      </Paragraph>
      <ul>
        <li>View and manage your tasks</li>
        <li>Create new tasks</li>
        <li>Track your progress</li>
      </ul>
    </div>
  );
}
