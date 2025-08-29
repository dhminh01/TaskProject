import { Menu } from "antd";
import { useLocation, useNavigate } from "react-router-dom";
import { UnorderedListOutlined, DashboardOutlined } from "@ant-design/icons";

export function MainMenu() {
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
