import { Menu } from "antd";
import { useLocation, useNavigate } from "react-router-dom";
import {
  UnorderedListOutlined,
  DashboardOutlined,
  FileAddOutlined,
} from "@ant-design/icons";

export function SideBar() {
  const location = useLocation();
  const navigate = useNavigate();

  const items = [
    {
      key: "/",
      icon: <DashboardOutlined />,
      label: "Dashboard",
    },
    {
      key: "/tasks",
      icon: <UnorderedListOutlined />,
      label: "Tasks",
    },
    {
      key: "/create-task",
      icon: <FileAddOutlined />,
      label: "Create Task",
    },
  ];

  return (
    <Menu
      mode="inline"
      selectedKeys={[location.pathname]}
      onClick={({ key }) => navigate(key)}
      items={items}
      style={{ borderRight: "none" }}
    />
  );
}
